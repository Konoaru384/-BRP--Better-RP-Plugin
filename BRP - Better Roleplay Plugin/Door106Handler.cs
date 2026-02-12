using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Server;
using System;
using System.Linq;

namespace UnifiedSCPPlugin
{
    public static class Door106Handler
    {
        public static void Register()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        private static void OnWaitingForPlayers()
        {
            var cfg = RoleplayPlugin.Instance.Config.Scp106;
            if (!cfg.Enable) return;

            foreach (Door door in Door.List)
            {
                if (cfg.BlockedDoors.Contains(door.Name, StringComparer.OrdinalIgnoreCase))
                {
                    door.AllowsScp106 = false;

                    if (RoleplayPlugin.Instance.Config.Debug)
                        Log.Info($"[ROLEPLAY PLUGIN] SCP-106 bloqued on {door.Name}");
                }
            }
        }
    }
}
