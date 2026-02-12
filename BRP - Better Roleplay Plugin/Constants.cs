using System.Collections.Generic;
using DoorEnum = Exiled.API.Enums.DoorType;
using ExiledPlayer = Exiled.API.Features.Player;
using PlayerRoles;

namespace UnifiedSCPPlugin
{
    public static class Constants
    {
        private static Config Cfg => RoleplayPlugin.Instance.Config;

        public static string Scp939DoorId => Cfg.ScpDoors.Scp939DoorId;
        public static string Scp999DoorId => Cfg.ScpDoors.Scp999DoorId;
        public static string Scp173MerDoorId => Cfg.ScpDoors.Scp173MerDoorId;

        public static string Scp096DoorId => Cfg.ScpDoors.Scp096DoorId;
        public static string Scp096TeleportId => Cfg.ScpDoors.Teleport096;

        public static readonly HashSet<RoleTypeId> TrackedScpRoles = new()
        {
            RoleTypeId.Scp173, RoleTypeId.Scp049, RoleTypeId.Scp106,
            RoleTypeId.Scp096, RoleTypeId.Scp939, RoleTypeId.Scp3114, RoleTypeId.Scp079
        };

        public static readonly Dictionary<DoorEnum, RoleTypeId> DoorToScp = new()
        {
            [DoorEnum.Scp049Gate] = RoleTypeId.Scp049,
            [DoorEnum.GR18Inner] = RoleTypeId.Scp3114,
            [DoorEnum.Scp106Primary] = RoleTypeId.Scp106,
            [DoorEnum.Scp106Secondary] = RoleTypeId.Scp106
        };

        public static bool IsTracked(ExiledPlayer p)
        {
            return TrackedScpRoles.Contains(p.Role.Type)
                   || SharedState.Active066.Contains(p.Id)
                   || SharedState.Active999.Contains(p.Id)
                   || SharedState.Active008.Contains(p.Id);
        }

        public static string GetScpKey(ExiledPlayer p)
        {
            if (SharedState.Active066.Contains(p.Id)) return "Scp066";
            if (SharedState.Active999.Contains(p.Id)) return "Scp999";
            if (SharedState.Active008.Contains(p.Id)) return "Scp008";
            return p.Role.Type.ToString();
        }
    }
}
