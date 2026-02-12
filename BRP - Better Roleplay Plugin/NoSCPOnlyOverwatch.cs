using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System.Linq;
using UnityEngine;

namespace UnifiedSCPPlugin
{
    public static class ScpToOverwatchHandler
    {
        private static ScpToOverwatchSettings Cfg => RoleplayPlugin.Instance.Config.ScpToOverwatch;

        public static void Register()
        {
            if (!Cfg.Enable)
                return;

            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        }

        private static void OnRoundStarted()
        {
            if (!Cfg.Enable)
                return;

            var allPlayers = Player.List.ToList();
            int playerCount = allPlayers.Count;

            var scps = allPlayers.Where(p => p.Role.Team == Team.SCPs);

            foreach (var scp in scps)
            {
                if (playerCount < Cfg.MinPlayersForSCP)
                {
                    scp.Role.Set(Cfg.LowPlayerRole);

                    Timing.CallDelayed(1.5f, () =>
                    {
                        var room = Room.Get(Cfg.LowPlayerSpawnRoom);
                        if (room != null)
                            scp.Position = room.Position + Cfg.LowPlayerSpawnOffset;
                    });

                    continue;
                }

                scp.ClearBroadcasts();
                scp.Role.Set(RoleTypeId.Overwatch);

                scp.Broadcast(
                    Cfg.OverwatchBroadcastDuration,
                    Cfg.OverwatchBroadcast
                );
            }
        }
    }
}
