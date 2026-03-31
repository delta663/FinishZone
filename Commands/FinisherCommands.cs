using VampireCommandFramework;
using FinishZone.Services;

namespace FinishZone.Commands;

internal static class FinisherCommands
{
    [Command("finishers", "winners", description: "Show the finishers of each event.", adminOnly: false)]
    public static void ShowWinners(ChatCommandContext ctx)
    {
        if (!FinisherService.TryGetWinnerMessages(out var messages, out var error))
        {
            ctx.Reply(error);
            return;
        }

        foreach (var message in messages)
            ctx.Reply(message);
    }
}
