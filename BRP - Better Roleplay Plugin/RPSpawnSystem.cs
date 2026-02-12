using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System.Linq;
using UnityEngine;

namespace UnifiedSCPPlugin
{
    public static class CustomSpawnHandler
    {
        private static Vector3? guardSpawnpoint;

        private static CustomSpawnSettings Cfg => RoleplayPlugin.Instance.Config.CustomSpawns;

        public static void Register()
        {
            if (!Cfg.Enable)
                return;

            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Spawned += OnPlayerSpawned;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Spawned -= OnPlayerSpawned;
        }

        private static void OnRoundStarted()
        {
            if (!Cfg.Enable)
                return;

            if (Cfg.GuardSpawn)
            {
                var room = Room.List.FirstOrDefault(r => r.Type == RoomType.LczTCross);
                if (room != null)
                    guardSpawnpoint = room.WorldPosition(Cfg.SpawnOffsets[RoleTypeId.FacilityGuard]);
            }
        }

        private static void OnPlayerSpawned(SpawnedEventArgs ev)
        {
            if (!Cfg.Enable)
                return;

            if (!ev.SpawnFlags.HasFlag(RoleSpawnFlags.UseSpawnpoint))
                return;

            var role = ev.Player.Role.Type;

            if (role == RoleTypeId.FacilityGuard && Cfg.GuardSpawn && guardSpawnpoint != null)
            {
                ev.Player.Teleport(guardSpawnpoint.Value + Cfg.GlobalOffset);
                return;
            }

            if (Cfg.SpawnRooms.TryGetValue(role, out var roomType))
            {
                bool enabled = role switch
                {
                    RoleTypeId.Scientist => Cfg.ScientistSpawn,
                    RoleTypeId.Scp096 => Cfg.Scp096Spawn,
                    RoleTypeId.Scp3114 => Cfg.Scp3114Spawn,
                    RoleTypeId.Scp173 => Cfg.Scp173Spawn,
                    _ => false
                };

                if (!enabled)
                    return;

                var room = Room.List.FirstOrDefault(r => r.Type == roomType);
                if (room == null)
                    return;

                Vector3 offset = Cfg.SpawnOffsets.TryGetValue(role, out var off)
                    ? off
                    : Vector3.zero;

                ev.Player.Teleport(room.WorldPosition(offset + Cfg.GlobalOffset));
            }
        }
    }
}
