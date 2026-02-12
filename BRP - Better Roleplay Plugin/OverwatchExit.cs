using CommandSystem;
using Exiled.API.Features;
using PlayerRoles;
using RemoteAdmin;
using System;

namespace UnifiedSCPPlugin
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class OverwatchExitCommand : ICommand
    {
        public string Command => "overwatchexit";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Exit the Overwatch role and become another role.";

        private static OverwatchExitSettings Cfg => RoleplayPlugin.Instance.Config.OverwatchExit;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Cfg.Enable)
            {
                response = Cfg.DisabledMessage;
                return false;
            }

            if (sender is not PlayerCommandSender playerSender)
            {
                response = Cfg.NotPlayerMessage;
                return false;
            }

            Player player = Player.Get(playerSender);

            if (player.Role.Type != RoleTypeId.Overwatch)
            {
                response = Cfg.NotOverwatchMessage;
                return false;
            }

            player.Role.Set(Cfg.ExitRole);

            response = Cfg.SuccessMessage;
            return true;
        }
    }
}
