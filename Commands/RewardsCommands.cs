using System.Globalization;
using FinishZone.Services;
using Stunlock.Core;
using VampireCommandFramework;

namespace FinishZone.Commands;

[CommandGroup("rewards")]
internal static class RewardsCommands
{
    [Command("add", shortHand: "a", adminOnly: true, description: "Add an item to a reward set.")]
    public static void Add(ChatCommandContext ctx, string setname = "", string prefabGuidToken = "", string amountToken = "")
    {
        if (string.IsNullOrWhiteSpace(setname) || string.IsNullOrWhiteSpace(prefabGuidToken) || string.IsNullOrWhiteSpace(amountToken))
        {
            ShowUsage(ctx);
            return;
        }

        if (!int.TryParse(prefabGuidToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out int prefabGuid))
        {
            ctx.Reply("<color=red>PrefabGUID must be a valid integer.</color>");
            return;
        }

        if (!int.TryParse(amountToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out int amount) || amount <= 0)
        {
            ctx.Reply("<color=red>Amount must be >= 1.</color>");
            return;
        }

        var guid = new PrefabGUID(prefabGuid);
        string label = RewardsService.ResolveItemLabel(guid, prefabGuidToken);
        RewardsService.AddSet(setname.Trim(), prefabGuid, amount);

        ctx.Reply($"Added <color=#87CEFA>{label}</color> × <color=#FFD700>{amount}</color> to reward set <color=white>{setname}</color>.");
    }

    [Command("remove", shortHand: "rm", adminOnly: true, description: "Remove a reward set.")]
    public static void Remove(ChatCommandContext ctx, string setname = "")
    {
        if (string.IsNullOrWhiteSpace(setname))
        {
            ShowUsage(ctx);
            return;
        }

        if (RewardsService.RemoveSet(setname.Trim()))
            ctx.Reply($"Removed reward set <color=white>{setname}</color>.");
        else
            ctx.Reply($"<color=yellow>Set not found:</color> {setname}");
    }
    
    [Command("give", shortHand: "g", adminOnly: true, description: "Give a reward set to a player or yourself.")]
    public static void Give(ChatCommandContext ctx, string setname = "", string player = "")
    {
        if (string.IsNullOrWhiteSpace(setname))
        {
            ShowUsage(ctx);
            return;
        }

        RewardsService.GiveRewardsToPlayer(ctx, setname, player);
    }

    [Command("reload", shortHand: "rl", adminOnly: true, description: "Reload rewards.json from disk.")]
    public static void Reload(ChatCommandContext ctx)
    {
        if (RewardsService.ReloadFromDisk(out int setCount))
            ctx.Reply($"Reloaded <color=white>{setCount}</color> reward sets.");
        else
            ctx.Reply("<color=red>Failed to reload rewards.json</color>");
    }

    [Command("list", shortHand: "l", adminOnly: true, description: "List all reward sets.")]
    public static void List(ChatCommandContext ctx)
    {
        var all = RewardsService.GetAllSetsDetailed();
        if (all.Count == 0)
        {
            ctx.Reply("<color=yellow>No reward sets found.</color>");
            return;
        }

        ctx.Reply("<color=yellow>Available reward sets:</color>");

        foreach (var kv in all)
        {
            string line = $"<color=#87CEFA>{kv.Key}</color>: ";
            bool hasAny = false;

            foreach (var entry in kv.Value)
            {
                var guid = new PrefabGUID(entry.Item);
                string label = RewardsService.ResolveItemLabel(guid, guid.GuidHash.ToString());

                if (hasAny)
                    line += ", ";

                line += $"{label} × {entry.Amount}";
                hasAny = true;
            }

            if (!hasAny)
                line += " (empty)";

            ctx.Reply(line);
        }
    }

    [Command("help", shortHand: "h", adminOnly: true, description: "Show usage for .rewards")]
    public static void Help(ChatCommandContext ctx)
    {
        ShowUsage(ctx);
    }

    private static void ShowUsage(ChatCommandContext ctx)
    {
        ctx.Reply("<color=yellow>Usage:</color>\n" +
                  ".rewards add <setname> <prefabguid> <amount>\n" +
                  ".rewards remove <setname>\n" +
                  ".rewards give <setname> <player>\n" +
                  ".rewards reload\n" +
                  ".rewards list");
    }
}
