using CommandSystem;
using PluginAPI.Core;
using SCP575.API.Extensions;
using System;

namespace SCP575.Command
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal sealed class MainCommand : ICommand, IUsageProvider
    {
        public string Command => "scp575";

        public string[] Aliases => new string[] { "575" };

        public string Description => "This command allows you to spawn instances of SCP-575.";

        public string[] Usage { get; } = { "Player ID", "Duration" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender != null)
            {
                if (!Round.IsRoundStarted)
                {
                    response = EntryPoint.Instance.Config.CommandResponses.RoundHasNotStarted;
                    return false;
                }
                // Display help response.
                if (arguments.Count < 1 || arguments.IsEmpty() || arguments.Count > 2)
                {
                    var text = string.Format(EntryPoint.Instance.Config.CommandResponses.HelpResponse, this.DisplayCommandUsage());
                    response = text;
                    return false;
                }

                if (!int.TryParse(arguments.At(0), out int playerId))
                {
                    response = string.Format(EntryPoint.Instance.Config.CommandResponses.InvalidPlayerId, arguments.At(0));
                    return false;
                }

                var victim = Player.Get(playerId);
                if (victim is null)
                {
                    response = EntryPoint.Instance.Config.CommandResponses.PlayerNotFound;
                    return false;
                }

                if (!int.TryParse(arguments.At(1), out var duration))
                {
                    response = string.Format(EntryPoint.Instance.Config.CommandResponses.InvalidDuration, arguments.At(1));
                    return false;
                }

                BlackoutExtensions.SpawnScp575(victim, duration);

                response = string.Format(EntryPoint.Instance.Config.CommandResponses.Spawning, victim.Nickname, duration);
                return true;
            }
            else
            {
                response = "Sender is null";
                return false;
            }
        }
    }
}
