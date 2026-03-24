using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;

namespace FinishZone.Services;

internal static class FinisherService
{
    private static readonly string CONFIG_DIR = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    private static readonly string LOG_FILE = Path.Combine(CONFIG_DIR, "finish_log.csv");
    private const int LINE_LIMIT = 200;

    public static bool TryGetWinnerMessages(out List<string> messages, out string error)
    {
        messages = new List<string>();
        error = string.Empty;

        try
        {
            if (!File.Exists(LOG_FILE))
            {
                error = "<color=red>File not found:</color> finish_log.csv";
                return false;
            }

            var lines = File.ReadAllLines(LOG_FILE)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Skip(1)
                .ToList();

            if (lines.Count == 0)
            {
                error = "<color=yellow>No data found in finish_log.csv</color>";
                return false;
            }

            var records = new List<FinishRecord>();

            foreach (var line in lines)
            {
                var parts = line.Split(',', 5);
                if (parts.Length < 5)
                    continue;

                string zone = parts[1].Trim();
                string name = parts[3].Trim();
                string firstFlag = parts[4].Trim();

                if (!firstFlag.Equals("true", StringComparison.OrdinalIgnoreCase))
                    continue;

                records.Add(new FinishRecord(zone, name));
            }

            if (records.Count == 0)
            {
                error = "<color=yellow>No winners found.</color>";
                return false;
            }

            messages.Add("<color=yellow>Event Winners List</color>");

            var grouped = records
                .GroupBy(r => r.Zone)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                string prefix = $"<color=green>{group.Key}</color>: ";
                string currentLine = prefix;

                foreach (var name in group.Select(g => g.PlayerName))
                {
                    string next = (currentLine == prefix ? string.Empty : ", ") + $"<color=white>{name}</color>";

                    if ((currentLine + next).Length > LINE_LIMIT)
                    {
                        messages.Add(currentLine);
                        currentLine = $"<color=white>{name}</color>";
                    }
                    else
                    {
                        currentLine += next;
                    }
                }

                if (!string.IsNullOrWhiteSpace(currentLine))
                    messages.Add(currentLine);
            }

            return true;
        }
        catch (Exception ex)
        {
            error = $"<color=red>Error reading finish_log.csv:</color> {ex.Message}";
            return false;
        }
    }

    private sealed record FinishRecord(string Zone, string PlayerName);
}
