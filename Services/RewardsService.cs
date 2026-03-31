using BepInEx;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using VampireCommandFramework;

namespace FinishZone.Services;

internal static class RewardsService
{
    private static bool _initialized;
    private static readonly object _sync = new();

    private static readonly string CONFIG_DIR = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    private static readonly string CONFIG_FILE = Path.Combine(CONFIG_DIR, "rewards.json");
    private static readonly string LOG_FILE = Path.Combine(CONFIG_DIR, "rewards_log.csv");

    public sealed class RewardEntry
    {
        public int Item { get; set; }
        public int Amount { get; set; }
    }

    private static readonly Dictionary<string, List<RewardEntry>> _sets = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<int, string> _labelCache = new();
    private static readonly object _labelLock = new();

    public static void Initialize()
    {
        ReloadFromDisk(out _);
    }

    public static Dictionary<string, List<RewardEntry>> GetAllSetsDetailed() => new(_sets);

    public static bool TryGetSet(string name, out List<RewardEntry> entries)
    {
        if (_sets.TryGetValue(name, out var list) && list is { Count: > 0 })
        {
            entries = list;
            return true;
        }

        entries = new List<RewardEntry>();
        return false;
    }

    public static void AddSet(string name, int itemGuidHash, int amount)
    {
        if (!_sets.TryGetValue(name, out var list))
        {
            list = new List<RewardEntry>();
            _sets[name] = list;
        }

        bool merged = false;
        for (int i = 0; i < list.Count; i++)
        {
            var entry = list[i];
            if (entry.Item != itemGuidHash)
                continue;

            long sum = (long)entry.Amount + amount;
            entry.Amount = (int)Math.Clamp(sum, int.MinValue, int.MaxValue);
            list[i] = entry;
            merged = true;
            break;
        }

        if (!merged)
        {
            list.Add(new RewardEntry
            {
                Item = itemGuidHash,
                Amount = amount
            });
        }

        SaveToDisk();
    }

    public static bool RemoveSet(string name)
    {
        bool removed = _sets.Remove(name);
        if (removed)
            SaveToDisk();

        return removed;
    }

    public static bool ReloadFromDisk(out int setCount)
    {
        try
        {
            Directory.CreateDirectory(CONFIG_DIR);
            EnsureConfigFileExists();
            EnsureLogFileExists();

            string json = File.ReadAllText(CONFIG_FILE, new UTF8Encoding(false));
            var parsed = JsonSerializer.Deserialize<Dictionary<string, List<RewardEntry>>>(json);

            _sets.Clear();
            if (parsed != null)
            {
                foreach (var kv in parsed)
                    _sets[kv.Key] = kv.Value ?? new List<RewardEntry>();
            }

            setCount = _sets.Count;
            return true;
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[RewardsService] Failed to reload rewards.json: {ex}");
            setCount = 0;
            return false;
        }
    }

    public static bool GiveRewardsToPlayer(ChatCommandContext ctx, string setName, string playerToken = "")
    {
        if (string.IsNullOrWhiteSpace(setName))
        {
            ctx.Reply("<color=red>Set name is required.</color>");
            return false;
        }

        if (!TryGetSet(setName.Trim(), out var entries) || entries.Count == 0)
        {
            ctx.Reply($"<color=yellow>Set not found:</color> {setName}");
            return false;
        }

        var em = Core.EntityManager;

        Entity targetUserEntity;
        Entity targetCharacter;
        string displayName;

        if (string.IsNullOrWhiteSpace(playerToken))
        {
            targetUserEntity = ctx.Event.SenderUserEntity;
            targetCharacter = ctx.Event.SenderCharacterEntity;

            if (targetUserEntity == Entity.Null || !em.Exists(targetUserEntity))
            {
                ctx.Reply("<color=red>Could not find your player entity.</color>");
                return false;
            }

            if (targetCharacter == Entity.Null || !em.Exists(targetCharacter))
            {
                ctx.Reply("<color=red>Could not find your character.</color>");
                return false;
            }

            var senderUser = em.GetComponentData<User>(targetUserEntity);
            displayName = Helper.ReadUserName(senderUser, targetUserEntity);
        }
        else
        {
            if (!TryFindPlayer(em, playerToken.Trim(), out targetUserEntity, out targetCharacter, out displayName))
            {
                ctx.Reply($"<color=red>Player not found:</color> {playerToken}");
                return false;
            }
        }

        int successCount = 0;
        foreach (var entry in entries)
        {
            if (entry.Amount <= 0)
                continue;

            try
            {
                Helper.AddItemToInventory(targetCharacter, new PrefabGUID(entry.Item), entry.Amount);
                successCount++;
            }
            catch (Exception ex)
            {
                Core.Log.LogError($"[RewardsService] Failed to give item {entry.Item} x{entry.Amount} to {displayName}: {ex}");
            }
        }

        if (successCount <= 0)
        {
            ctx.Reply($"<color=red>Failed to give reward set</color> <color=white>{setName}</color> <color=red>to</color> <color=white>{displayName}</color>.");
            return false;
        }

        string adminName;
        try
        {
            var adminUser = em.GetComponentData<User>(ctx.Event.SenderUserEntity);
            adminName = Helper.ReadUserName(adminUser, ctx.Event.SenderUserEntity);
        }
        catch
        {
            adminName = $"User#{ctx.Event.SenderUserEntity.Index}";
        }

        ctx.Reply($"Gave reward set <color=white>{setName}</color> to <color=white>{displayName}</color>.");
        Helper.NotifyUser(em, targetUserEntity, $"You received reward set <color=white>{setName}</color>.");

        CsvLogger.LogRow(setName.Trim(), displayName, adminName);
        return true;
    }

    private static bool TryFindPlayer(EntityManager em, string needle, out Entity targetUserEntity, out Entity targetCharacter, out string displayName)
    {
        targetUserEntity = Entity.Null;
        targetCharacter = Entity.Null;
        displayName = string.Empty;

        string query = needle?.Trim() ?? string.Empty;
        if (query.Length == 0)
            return false;

        string queryLower = query.ToLowerInvariant();
        bool queryIsIndex = int.TryParse(queryLower, NumberStyles.Integer, CultureInfo.InvariantCulture, out int wantedIndex);

        NativeArray<Entity> users = Helper.GetEntitiesByComponentType<User>();
        int bestScore = -1;
        Entity bestUserEntity = Entity.Null;
        Entity bestCharacter = Entity.Null;
        string bestName = string.Empty;

        try
        {
            foreach (var userEntity in users)
            {
                if (!em.HasComponent<User>(userEntity))
                    continue;

                var user = em.GetComponentData<User>(userEntity);
                var character = Helper.GetCharacterFromUser(user);
                if (character == Entity.Null)
                    continue;

                string name = Helper.ReadUserName(user, userEntity);
                string nameLower = name.ToLowerInvariant();

                int score = -1;
                if (nameLower.StartsWith(queryLower))
                    score = 2;
                else if (nameLower.Contains(queryLower))
                    score = 1;
                else if (queryIsIndex && userEntity.Index == wantedIndex)
                    score = 1;

                if (score < 0)
                    continue;

                if (score > bestScore || (score == bestScore && name.Length < bestName.Length))
                {
                    bestScore = score;
                    bestUserEntity = userEntity;
                    bestCharacter = character;
                    bestName = name;
                }
            }
        }
        finally
        {
            if (users.IsCreated)
                users.Dispose();
        }

        if (bestScore < 0)
            return false;

        targetUserEntity = bestUserEntity;
        targetCharacter = bestCharacter;
        displayName = bestName;
        return true;
    }

    public static string ResolveItemLabel(PrefabGUID guid, string fallbackInput)
    {
        int key = guid.GuidHash;

        lock (_labelLock)
        {
            if (_labelCache.TryGetValue(key, out var cached))
                return cached;
        }

        string found = string.IsNullOrWhiteSpace(fallbackInput)
            ? guid.GuidHash.ToString(CultureInfo.InvariantCulture)
            : fallbackInput.Trim();

        lock (_labelLock)
            _labelCache[key] = found;

        return found;
    }

    private static void EnsureConfigFileExists()
    {
        if (File.Exists(CONFIG_FILE))
            return;

        File.WriteAllText(CONFIG_FILE, GetDefaultJson(), new UTF8Encoding(false));
    }

    private static void EnsureLogFileExists()
    {
        if (_initialized)
            return;

        lock (_sync)
        {
            if (_initialized)
                return;

            Directory.CreateDirectory(CONFIG_DIR);
            if (!File.Exists(LOG_FILE))
                File.WriteAllText(LOG_FILE, "time,setname,playername,adminname" + Environment.NewLine, new UTF8Encoding(false));

            _initialized = true;
        }
    }

    private static void SaveToDisk()
    {
        try
        {
            Directory.CreateDirectory(CONFIG_DIR);
            string json = JsonSerializer.Serialize(_sets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CONFIG_FILE, json, new UTF8Encoding(false));
        }
        catch (Exception ex)
        {
            Core.Log.LogError($"[RewardsService] Failed to save rewards.json: {ex}");
        }
    }

    private static string GetDefaultJson()
    {
        var defaults = new Dictionary<string, List<RewardEntry>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Winner"] = new List<RewardEntry>
            {
                new() { Item = -1461326411, Amount = 20 },
                new() { Item = -1021407417, Amount = 20 }
            },
            ["Potion"] = new List<RewardEntry>
            {
                new() { Item = 429052660, Amount = 10 },
                new() { Item = 800879747, Amount = 10 }
            },
            ["Buff"] = new List<RewardEntry>
            {
                new() { Item = 1510182325, Amount = 1 },
                new() { Item = -1568756102, Amount = 1 },
                new() { Item = 541321301, Amount = 1 },
                new() { Item = -38051433, Amount = 1 },
                new() { Item = 970650569, Amount = 1 }
            }
        };

        return JsonSerializer.Serialize(defaults, new JsonSerializerOptions { WriteIndented = true });
    }

    private static class CsvLogger
    {
        internal static void LogRow(string setName, string playerName, string adminName)
        {
            try
            {
                EnsureLogFileExists();
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                string line = string.Join(",", new[]
                {
                    Csv(time),
                    Csv(setName),
                    Csv(playerName),
                    Csv(adminName)
                });

                lock (_sync)
                    File.AppendAllText(LOG_FILE, line + Environment.NewLine, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Core.Log.LogError($"[RewardsService] Failed to write rewards_log.csv: {ex}");
            }
        }

        private static string Csv(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            bool needQuote = s.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
            return needQuote ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }
    }
}
