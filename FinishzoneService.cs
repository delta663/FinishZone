using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BepInEx;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FinishZone.Services;

internal class FinishzoneService
{
    private static bool _initialized;
    private static bool _loaded;

    private static readonly string CONFIG_DIR = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    private static readonly string CONFIG_FILE = Path.Combine(CONFIG_DIR, "finishzones.json");
    private static readonly string LOG_FILE = Path.Combine(CONFIG_DIR, "finish_log.csv");
    private static readonly TimeSpan CooldownDuration = TimeSpan.FromMinutes(10);

    private const float DEFAULT_LOOP_INTERVAL = 1f;
    private const float DEFAULT_VERTICAL_LIMIT = 1f;
    private const string DEFAULT_MESSAGE = "";

    public static readonly PrefabGUID DefaultRewardPrefab = new(576389135);
    public const string DefaultRewardName = "Greater Stygian Shards";
    public const int DefaultRewardAmount = 1000;

    private static readonly Dictionary<string, FinishZoneModel> _zones = new();
    private static readonly HashSet<string> _finished = new();
    private static readonly Dictionary<string, DateTime> _cooldown = new();

    private static bool _modEnabled = true;
    private static float _loopIntervalSeconds = DEFAULT_LOOP_INTERVAL;
    private static Coroutine _loopCoroutine;

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        EnsureZoneFileExists();
        LoadZones();
        LoadLog();
        _loaded = true;
        RefreshLoopState();

        Core.Log.LogInfo("[FinishZoneService] Initialized.");
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
            return;

        EnsureZoneFileExists();
        LoadZones();
        LoadLog();
        _loaded = true;
        RefreshLoopState();
    }

    public static void ReloadZones()
    {
        EnsureZoneFileExists();
        LoadZones();
        LoadLog();
        _loaded = true;
        RefreshLoopState();
        Core.Log.LogInfo("[FinishZoneService] Reloaded zones from file.");
    }

    public static bool TryUpsertZone(string id, float3 position, float radius, out string error)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(id))
        {
            error = "<color=red>Zone id is required.</color>";
            return false;
        }

        if (radius <= 0f)
        {
            error = "<color=red>Radius must be greater than 0.</color>";
            return false;
        }

        if (radius > 30f)
        {
            error = "<color=red>Radius must be less than 30.</color>";
            return false;
        }

        if (_zones.ContainsKey(id))
        {
            error = $"<color=red>Finish zone {id} already exists. Use <color=green>.finish update {id} {radius}</color> to update.</color>";
            return false;
        }

        _zones[id] = new FinishZoneModel
        {
            Position = position,
            Radius = radius,
            VerticalLimit = DEFAULT_VERTICAL_LIMIT,
            Message = DEFAULT_MESSAGE,
            RewardPrefab = DefaultRewardPrefab,
            RewardName = DefaultRewardName,
            RewardAmount = DefaultRewardAmount,
            ZoneEnabled = true
        };

        try
        {
            SaveZones();
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[FinishZoneService] SaveZones failed: {ex}");
            error = "<color=red>Failed to save finishzones.json</color>";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TryUpdateZone(string id, float3 position, float radius, out string error)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(id))
        {
            error = "<color=red>Zone id is required.</color>";
            return false;
        }

        if (radius <= 0f)
        {
            error = "<color=red>Radius must be greater than 0.</color>";
            return false;
        }

        if (radius > 30f)
        {
            error = "<color=red>Radius must be less than 30.</color>";
            return false;
        }

        if (!_zones.TryGetValue(id, out var existing))
        {
            error = $"<color=red>Finish zone {id} not found.</color>";
            return false;
        }

        _zones[id] = new FinishZoneModel
        {
            Position = position,
            Radius = radius,
            VerticalLimit = existing.VerticalLimit > 0f ? existing.VerticalLimit : DEFAULT_VERTICAL_LIMIT,
            Message = existing.Message ?? DEFAULT_MESSAGE,
            RewardPrefab = existing.RewardPrefab,
            RewardName = string.IsNullOrWhiteSpace(existing.RewardName) ? DefaultRewardName : existing.RewardName,
            RewardAmount = existing.RewardAmount,
            ZoneEnabled = existing.ZoneEnabled
        };

        try
        {
            SaveZones();
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[FinishZoneService] SaveZones failed: {ex}");
            error = "<color=red>Failed to save finishzones.json</color>";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TrySetZoneEnabled(string id, bool enabled, out string error)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(id))
        {
            error = "<color=red>Zone id is required.</color>";
            return false;
        }

        if (!_zones.TryGetValue(id, out var zone))
        {
            error = $"<color=red>Finish zone {id} not found.</color>";
            return false;
        }

        zone.ZoneEnabled = enabled;

        try
        {
            SaveZones();
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[FinishZoneService] SaveZones failed: {ex}");
            error = "<color=red>Failed to save finishzones.json</color>";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TrySetModEnabled(bool enabled, out string error)
    {
        EnsureLoaded();

        _modEnabled = enabled;

        try
        {
            SaveZones();
            RefreshLoopState();
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[FinishZoneService] SaveZones failed: {ex}");
            error = "<color=red>Failed to save finishzones.json</color>";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static bool TryRemoveZone(string id, out string error)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(id))
        {
            error = "<color=red>Zone id is required.</color>";
            return false;
        }

        if (!_zones.Remove(id))
        {
            error = $"<color=red>Finish zone {id} not found.</color>";
            return false;
        }

        try
        {
            SaveZones();
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[FinishZoneService] SaveZones failed: {ex}");
            error = "<color=red>Failed to save finishzones.json</color>";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static IReadOnlyDictionary<string, FinishZoneModel> GetZonesSnapshot()
    {
        EnsureLoaded();
        return new Dictionary<string, FinishZoneModel>(_zones);
    }

    public static bool IsModEnabled()
    {
        EnsureLoaded();
        return _modEnabled;
    }

    public static float GetLoopInterval()
    {
        EnsureLoaded();
        return _loopIntervalSeconds;
    }

    private static void RefreshLoopState()
    {
        if (_modEnabled)
        {
            if (_loopCoroutine == null)
            {
                _loopCoroutine = Core.StartCoroutine(FinishLoop());
                Core.Log.LogInfo($"[FinishZoneService] Loop started. Interval: {_loopIntervalSeconds:0.##}s");
            }
        }
        else if (_loopCoroutine != null)
        {
            Core.StopCoroutine(_loopCoroutine);
            _loopCoroutine = null;
            Core.Log.LogInfo("[FinishZoneService] Loop stopped.");
        }
    }

    private static System.Collections.IEnumerator FinishLoop()
    {
        while (true)
        {
            try
            {
                UpdateZones();
            }
            catch (Exception ex)
            {
                Core.Log.LogError($"[FinishZoneService] Loop error: {ex}");
            }

            var waitSeconds = _loopIntervalSeconds > 0f ? _loopIntervalSeconds : DEFAULT_LOOP_INTERVAL;
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    private static void UpdateZones()
    {
        if (!_modEnabled)
            return;

        if (_zones.Count == 0)
            return;

        var zonesSnapshot = _zones
            .Where(kv => kv.Value.ZoneEnabled)
            .ToArray();

        if (zonesSnapshot.Length == 0)
            return;

        var users = Helper.GetEntitiesByComponentTypes<User, LocalToWorld>();

        try
        {
            foreach (var userEnt in users)
            {
                var user = userEnt.Read<User>();
                if (!user.IsConnected || user.IsAdmin)
                    continue;

                var charEnt = user.LocalCharacter._Entity;
                if (!TryGetCharacterPosition(charEnt, out var pos))
                    continue;

                var sid = user.PlatformId;
                var name = user.CharacterName.ToString();

                foreach (var (zoneId, zone) in zonesSnapshot)
                {
                    float3 diff = pos - zone.Position;

                    if (math.abs(diff.y) > zone.VerticalLimit)
                        continue;

                    float2 horiz = new(diff.x, diff.z);
                    float dist2 = math.lengthsq(horiz);
                    if (dist2 > zone.Radius * zone.Radius)
                        continue;

                    var key = $"{zoneId}:{sid}";
                    if (!CanTrigger(key))
                        continue;

                    _cooldown[key] = DateTime.UtcNow;

                    bool firstTime = !_finished.Contains(key);
                    if (firstTime)
                        _finished.Add(key);

                    string message = zone.Message ?? string.Empty;
                    var rewardPrefab = zone.RewardPrefab;
                    var rewardName = string.IsNullOrWhiteSpace(zone.RewardName) ? DefaultRewardName : zone.RewardName;
                    var rewardAmount = zone.RewardAmount;

                    Core.Log.LogInfo($"[FinishZoneService] {name} reached finish zone {zoneId} first time: {firstTime.ToString().ToLower()}");

                    string rewardLineGame = string.Empty;
                    if (firstTime && rewardAmount > 0)
                        rewardLineGame = $" <color=yellow>Received:</color> <color=#87CEFA>{rewardAmount} {rewardName}</color>. ";

                    var broadcastMsg =
                        $"<color=white>{name}</color> <color=green>has completed {zoneId}.</color>" +
                        rewardLineGame +
                        $"<color=yellow>{message}</color>";

                    var msg = new FixedString512Bytes(broadcastMsg);
                    ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, ref msg);

                    if (firstTime && rewardAmount > 0)
                    {
                        try
                        {
                            Helper.AddItemToInventory(charEnt, rewardPrefab, rewardAmount);
                            Core.Log.LogInfo($"[FinishZoneService] Gave {rewardAmount} {rewardName} ({rewardPrefab.GuidHash}) to {name} successfully.");
                        }
                        catch (Exception e)
                        {
                            Core.Log.LogError($"[FinishZoneService] Failed to give reward: {e.Message}");
                        }
                    }

                    LogFinish(zoneId, sid, name, firstTime);
                }
            }
        }
        finally
        {
            users.Dispose();
        }
    }

    private static bool TryGetCharacterPosition(Entity charEnt, out float3 pos)
    {
        pos = float3.zero;

        if (charEnt == Entity.Null || !Core.EntityManager.Exists(charEnt))
            return false;

        if (!charEnt.Has<LocalToWorld>())
            return false;

        pos = charEnt.Read<LocalToWorld>().Position;
        return true;
    }

    private static bool CanTrigger(string key)
    {
        if (_cooldown.TryGetValue(key, out var last))
            return (DateTime.UtcNow - last) > CooldownDuration;

        return true;
    }

    private static void EnsureZoneFileExists()
    {
        Directory.CreateDirectory(CONFIG_DIR);

        if (File.Exists(CONFIG_FILE))
            return;

        var emptyConfig = new FinishZonesConfigDto();
        var json = JsonSerializer.Serialize(emptyConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(CONFIG_FILE, json);
        Core.Log.LogInfo("[FinishZoneService] Created default finishzones.json");
    }

    private static void LoadZones()
    {
        try
        {
            if (!File.Exists(CONFIG_FILE))
            {
                Core.Log.LogWarning("[FinishZoneService] No finishzones.json found.");
                return;
            }

            var json = File.ReadAllText(CONFIG_FILE);
            var config = JsonSerializer.Deserialize<FinishZonesConfigDto>(json) ?? new FinishZonesConfigDto();

            _modEnabled = config.ModEnabled;
            _loopIntervalSeconds = config.LoopIntervalSeconds > 0f ? config.LoopIntervalSeconds : DEFAULT_LOOP_INTERVAL;

            _zones.Clear();
            foreach (var kv in config.Zones)
            {
                var dto = kv.Value ?? new ZoneDto();
                var arr = dto.Position ?? new float[3];

                var verticalLimit = dto.VerticalLimit > 0f ? dto.VerticalLimit : DEFAULT_VERTICAL_LIMIT;
                var rewardPrefab = dto.RewardPrefab != 0 ? new PrefabGUID(dto.RewardPrefab) : DefaultRewardPrefab;
                var rewardName = string.IsNullOrWhiteSpace(dto.RewardName) ? DefaultRewardName : dto.RewardName;
                var rewardAmount = dto.RewardAmount != 0 ? dto.RewardAmount : DefaultRewardAmount;

                _zones[kv.Key] = new FinishZoneModel
                {
                    Position = new float3(arr.Length > 0 ? arr[0] : 0f, arr.Length > 1 ? arr[1] : 0f, arr.Length > 2 ? arr[2] : 0f),
                    Radius = dto.Radius,
                    VerticalLimit = verticalLimit,
                    Message = dto.Message ?? string.Empty,
                    RewardPrefab = rewardPrefab,
                    RewardName = rewardName,
                    RewardAmount = rewardAmount,
                    ZoneEnabled = dto.ZoneEnabled
                };
            }

            Core.Log.LogInfo($"[FinishZoneService] Loaded {_zones.Count} zones. ModEnabled: {_modEnabled} LoopInterval: {_loopIntervalSeconds:0.##}s");
        }
        catch (Exception e)
        {
            Core.Log.LogError($"[FinishZoneService] LoadZones error: {e}");
        }
    }

    private static void SaveZones()
    {
        Directory.CreateDirectory(CONFIG_DIR);

        var config = new FinishZonesConfigDto
        {
            ModEnabled = _modEnabled,
            LoopIntervalSeconds = _loopIntervalSeconds > 0f ? _loopIntervalSeconds : DEFAULT_LOOP_INTERVAL,
            Zones = new Dictionary<string, ZoneDto>(_zones.Count)
        };

        foreach (var kv in _zones)
        {
            var z = kv.Value;
            config.Zones[kv.Key] = new ZoneDto
            {
                Position = new[] { z.Position.x, z.Position.y, z.Position.z },
                Radius = z.Radius,
                VerticalLimit = z.VerticalLimit > 0f ? z.VerticalLimit : DEFAULT_VERTICAL_LIMIT,
                Message = z.Message ?? string.Empty,
                RewardPrefab = z.RewardPrefab.GuidHash,
                RewardName = z.RewardName ?? string.Empty,
                RewardAmount = z.RewardAmount,       
                ZoneEnabled = z.ZoneEnabled
            };
        }

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(CONFIG_FILE, json);
    }

    private static void LoadLog()
    {
        try
        {
            if (!File.Exists(LOG_FILE))
            {
                Core.Log.LogInfo("[FinishZoneService] No finish_log.csv found.");
                return;
            }

            _finished.Clear();
            var lines = File.ReadAllLines(LOG_FILE);
            foreach (var line in lines)
            {
                if (line.StartsWith("time"))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 5)
                    continue;

                if (parts[4].Equals("true", StringComparison.OrdinalIgnoreCase))
                    _finished.Add($"{parts[1]}:{parts[2]}");
            }

            Core.Log.LogInfo($"[FinishZoneService] Loaded {_finished.Count} completed entries.");
        }
        catch (Exception e)
        {
            Core.Log.LogError($"[FinishZoneService] LoadLog error: {e}");
        }
    }

    private static void LogFinish(string zoneId, ulong steamId, string name, bool firstTime)
    {
        try
        {
            Directory.CreateDirectory(CONFIG_DIR);
            bool header = !File.Exists(LOG_FILE);

            using var w = new StreamWriter(LOG_FILE, append: true);
            if (header)
                w.WriteLine("servertime,finishzone,steamid,playername,firsttime");

            w.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{zoneId},{steamId},{name},{firstTime.ToString().ToLower()}");
        }
        catch (Exception e)
        {
            Core.Log.LogError($"[FinishZoneService] Log error: {e}");
        }
    }

    internal class FinishZoneModel
    {
        public float3 Position { get; set; }
        public float Radius { get; set; }
        public float VerticalLimit { get; set; } = DEFAULT_VERTICAL_LIMIT;
        public string Message { get; set; } = string.Empty;
        public PrefabGUID RewardPrefab { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public int RewardAmount { get; set; }

        public bool ZoneEnabled { get; set; } = true;
    }

    private sealed class FinishZonesConfigDto
    {
        public bool ModEnabled { get; set; } = true;
        public float LoopIntervalSeconds { get; set; } = DEFAULT_LOOP_INTERVAL;
        public Dictionary<string, ZoneDto> Zones { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class ZoneDto
    {
        public float[] Position { get; set; } = new float[3];
        public float Radius { get; set; }
        public float VerticalLimit { get; set; } = DEFAULT_VERTICAL_LIMIT;
        public string Message { get; set; } = string.Empty;
        public int RewardPrefab { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public int RewardAmount { get; set; }
        public bool ZoneEnabled { get; set; } = true;
    }
}
