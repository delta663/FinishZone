using System.Text;
using FinishZone.Services;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace FinishZone.Commands;

[CommandGroup("finish")]
internal static class FinishzoneCommands
{
    [Command("add", shortHand: "a", adminOnly: true, description: "Add a new finish zone.")]
    public static void Add(ChatCommandContext ctx, string id, float radius)
    {
        if (ctx.Event.SenderCharacterEntity == Entity.Null)
        {
            ctx.Reply("<color=red>Character not ready.</color>");
            return;
        }

        if (!TryGetCommandPosition(ctx.Event.SenderCharacterEntity, out var pos))
        {
            ctx.Reply("<color=red>Could not read your position.</color>");
            return;
        }

        if (!FinishzoneService.TryUpsertZone(id, pos, radius, out var error))
        {
            ctx.Reply(error);
            return;
        }

        ctx.Reply($"<color=green>Added finish zone</color> <color=white>{id}</color> <color=yellow>Radius: {radius}</color>");

        Core.Log.LogInfo($"[FinishZoneCommands] Added zone {id} {radius} at ({pos.x:0.##}, {pos.y:0.##}, {pos.z:0.##})");
    }

    [Command("update", shortHand: "u", adminOnly: true, description: "Update an existing finish zone.")]
    public static void Update(ChatCommandContext ctx, string id, float radius)
    {
        if (ctx.Event.SenderCharacterEntity == Entity.Null)
        {
            ctx.Reply("<color=red>Character not ready.</color>");
            return;
        }

        if (!TryGetCommandPosition(ctx.Event.SenderCharacterEntity, out var pos))
        {
            ctx.Reply("<color=red>Could not read your position.</color>");
            return;
        }

        if (!FinishzoneService.TryUpdateZone(id, pos, radius, out var error))
        {
            ctx.Reply(error);
            return;
        }

        ctx.Reply($"<color=green>Updated finish zone</color> <color=white>{id}</color> <color=yellow>Radius: {radius}</color>");

        Core.Log.LogInfo($"[FinishZoneCommands] Updated zone {id} {radius} at ({pos.x:0.##}, {pos.y:0.##}, {pos.z:0.##})");
    }
    
    [Command("remove", shortHand: "rm", adminOnly: true, description: "Remove a finish zone.")]
    public static void Remove(ChatCommandContext ctx, string id)
    {
        if (!FinishzoneService.TryRemoveZone(id, out var error))
        {
            ctx.Reply(error);
            return;
        }

        ctx.Reply($"<color=yellow>Removed finish zone </color><color=white>{id}</color>");

        Core.Log.LogInfo($"[FinishZoneCommands] Removed zone {id}");
    }

    [Command("enable", shortHand: "en", adminOnly: true, description: "Enable FinishZone mod.")]
    public static void EnableMod(ChatCommandContext ctx)
    {
        if (!FinishzoneService.TrySetModEnabled(true, out var error))
        {
            ctx.Reply(error);
            return;
        }

        ctx.Reply("<color=green>FinishZone mod enabled.</color>");
    }

    [Command("disable", shortHand: "dis", adminOnly: true, description: "Disable FinishZone mod.")]
    public static void DisableMod(ChatCommandContext ctx)
    {
        if (!FinishzoneService.TrySetModEnabled(false, out var error))
        {
            ctx.Reply(error);
            return;
        }

        ctx.Reply("<color=yellow>FinishZone mod disabled.</color>");
    }

    [Command("on", adminOnly: true, description: "Enable a finish zone.")]
    public static void On(ChatCommandContext ctx, string id)
    {
        if (!FinishzoneService.TrySetZoneEnabled(id, true, out var error))
        {
            ctx.Reply(error);
            return;
        }

        ctx.Reply($"<color=green>Enabled finish zone </color><color=white>{id}</color>");
    }

    [Command("off", adminOnly: true, description: "Disable a finish zone.")]
    public static void Off(ChatCommandContext ctx, string id)
    {
        if (!FinishzoneService.TrySetZoneEnabled(id, false, out var error))
        {
            ctx.Reply(error);
            return;
        }

        ctx.Reply($"<color=yellow>Disabled finish zone </color><color=white>{id}</color>");
    }

    [Command("reload", shortHand: "rl", adminOnly: true, description: "Reload finish zones from file.")]
    public static void Reload(ChatCommandContext ctx)
    {
        FinishzoneService.ReloadZones();
        ctx.Reply("<color=green>Finish zones reloaded successfully.</color>");
    }

    [Command("list", shortHand: "l", adminOnly: true, description: "Show all finish zones.")]
    public static void List(ChatCommandContext ctx)
    {
        var zones = FinishzoneService.GetZonesSnapshot();
        var modStatus = FinishzoneService.IsModEnabled() ? "enabled" : "disabled";
        var loopInterval = FinishzoneService.GetLoopInterval();

        if (zones.Count == 0)
        {
            ctx.Reply($"<color=yellow>No finish zones found.</color> <color=white>Mod: {modStatus}, Loop Interval: {loopInterval:0.##}</color>");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"<color=yellow>Finish zones list</color> <color=white>Mod: {modStatus}, Loop Interval: {loopInterval:0.##}</color>");

        int index = 1;
        foreach (var kv in zones)
        {
            var id = kv.Key;
            var z = kv.Value;
            var rewardText = z.RewardAmount > 0
                ? $"{z.RewardAmount} {z.RewardName} ({z.RewardPrefab.GuidHash})"
                : "none";
            var status = z.ZoneEnabled ? "enabled" : "disabled";

            sb.AppendLine($"[{index}] <color=white>{id}</color> Status: {status}, Radius: {z.Radius}, Vertical Limit: {z.VerticalLimit}, Reward: {rewardText}, Message: {z.Message}");
            index++;
        }

        ctx.Reply(sb.ToString());
    }

    private static bool TryGetCommandPosition(Entity charEnt, out float3 pos)
    {
        pos = default;

        if (charEnt == Entity.Null || !Core.EntityManager.Exists(charEnt))
            return false;

        if (!charEnt.Has<LocalToWorld>())
            return false;

        pos = charEnt.Read<LocalToWorld>().Position;
        return true;
    }
}
