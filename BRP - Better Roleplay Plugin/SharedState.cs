using System.Collections.Generic;
using MEC;
using Exiled.API.Features;
using DoorEnum = Exiled.API.Enums.DoorType;

namespace UnifiedSCPPlugin
{
    public static class SharedState
    {
        public static readonly List<CoroutineHandle> CoroutineHandles = new();
        public static CoroutineHandle RoomCheckCoroutine;
        public static CoroutineHandle HrpCoroutine;
        public static bool RpAnnounced;
        public static readonly HashSet<string> UsedMerDoors = new();
        public static readonly HashSet<DoorEnum> UsedVanillaDoors = new();
        public static readonly Dictionary<int, Room> LastKnownRooms = new();
        public static readonly HashSet<int> TriggeredPlayers = new();
        public static readonly HashSet<int> Active066 = new();
        public static readonly HashSet<int> Active999 = new();
        public static readonly HashSet<int> Active008 = new();
        public static readonly HashSet<int> Active3114 = new();
        public static readonly HashSet<int> Active049 = new();
        public static readonly HashSet<int> Active0492 = new();
        public static readonly HashSet<int> Active096 = new();
        public static readonly HashSet<int> Active106 = new();
        public static readonly HashSet<int> Active173 = new();
        public static readonly HashSet<int> Active939 = new();
        public static readonly HashSet<int> Active079 = new();
        public static void KillAllCoroutines()
        {
            foreach (var h in CoroutineHandles)
                Timing.KillCoroutines(h);
            CoroutineHandles.Clear();

            Timing.KillCoroutines(RoomCheckCoroutine);
            Timing.KillCoroutines(HrpCoroutine);
        }
        public static void ClearAllState()
        {
            UsedMerDoors.Clear();
            UsedVanillaDoors.Clear();
            LastKnownRooms.Clear();
            TriggeredPlayers.Clear();

            Active066.Clear();
            Active999.Clear();
            Active008.Clear();
            Active3114.Clear();

            Active049.Clear();
            Active0492.Clear();
            Active096.Clear();
            Active106.Clear();
            Active173.Clear();
            Active939.Clear();
            Active079.Clear();

            RpAnnounced = false;
        }
    }
}
