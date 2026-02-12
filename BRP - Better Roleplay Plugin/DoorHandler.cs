using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using System;
using System.Linq;
using UnityEngine;
using ExiledPlayer = Exiled.API.Features.Player;

namespace UnifiedSCPPlugin
{
    public static class DoorHandler
    {
        public static void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            try
            {
                var cfg = RoleplayPlugin.Instance.Config;

                if (!cfg.GateSummon.Enable || !ev.IsAllowed || ev.Door.IsOpen)
                    return;

                if (HandleMerDoor(ev, cfg.ScpDoors.Scp173MerDoorId, () => SpawnScp(RoleTypeId.Scp173, ev.Player))) return;
                if (HandleMerDoor(ev, cfg.ScpDoors.Scp939DoorId, () => SpawnScp(RoleTypeId.Scp939, ev.Player))) return;
                if (HandleMerDoor(ev, cfg.ScpDoors.Scp096DoorId, () => GiveScp096(ev.Player))) return;


                if (Constants.DoorToScp.TryGetValue(ev.Door.Type, out var scpRole) && scpRole != RoleTypeId.Scp173)
                {
                    Timing.CallDelayed(0.2f, () =>
                    {
                        if (ev.Door.IsOpen &&
                            !SharedState.UsedVanillaDoors.Contains(ev.Door.Type) &&
                            SpawnScp(scpRole, ev.Player))
                        {
                            SharedState.UsedVanillaDoors.Add(ev.Door.Type);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[DoorHandler] Exception in OnInteractingDoor: {ex}");
            }
        }

        private static bool HandleMerDoor(InteractingDoorEventArgs ev, string merId, Func<bool> action)
        {
            if (SharedState.UsedMerDoors.Contains(merId))
                return false;

            foreach (var obj in MapUtils.LoadedMaps.Values.SelectMany(m => m.SpawnedObjects))
            {
                if (obj.Id == merId && obj.gameObject == ev.Door.GameObject)
                {
                    Timing.CallDelayed(0.2f, () =>
                    {
                        if (ev.Door.IsOpen &&
                            !SharedState.UsedMerDoors.Contains(merId) &&
                            action())
                        {
                            SharedState.UsedMerDoors.Add(merId);
                        }
                    });
                    return true;
                }
            }

            return false;
        }

        private static bool GiveScp096(ExiledPlayer source)
        {
            var cfg = RoleplayPlugin.Instance.Config.ScpDoors;
            var candidate = GetEligibleCandidate();

            if (candidate == null)
            {
                source.Broadcast(10, "<color=#ff5555>❌ No available player for SCP-096.</color>");
                return false;
            }

            candidate.Role.Set(RoleTypeId.Scp096, RoleSpawnFlags.UseSpawnpoint);
            TeleportToPrimitive(candidate, cfg.Teleport096);

            candidate.Broadcast(8, "<color=#ffa657>You have become SCP-096.</color>");
            Log.Info($"[DoorHandler] {candidate.Nickname} became SCP-096.");
            return true;
        }


        private static bool SpawnScp(RoleTypeId scp, ExiledPlayer source)
        {
            var candidate = GetEligibleCandidate();

            if (candidate == null)
            {
                source.Broadcast(10, "<color=#ff5555>❌ No available player to become this SCP.</color>");
                return false;
            }

            candidate.Role.Set(scp, RoleSpawnFlags.UseSpawnpoint);
            candidate.Broadcast(8, $"<color=#ffa657>You have become {scp}.</color>");
            Log.Info($"[DoorHandler] {candidate.Nickname} became {scp}.");
            return true;
        }

        private static ExiledPlayer GetEligibleCandidate()
        {
            var overwatch = ExiledPlayer.List.Where(p => p.IsOverwatchEnabled && p.Role.Team == Team.Dead).ToList();
            if (overwatch.Count > 0)
                return overwatch[UnityEngine.Random.Range(0, overwatch.Count)];

            var spectators = ExiledPlayer.List.Where(p => p.Role.Team == Team.Dead).ToList();
            if (spectators.Count > 0)
                return spectators[UnityEngine.Random.Range(0, spectators.Count)];

            return null;
        }


        private static void TeleportToPrimitive(ExiledPlayer player, string primitiveId)
        {
            var obj = MapUtils.LoadedMaps.Values
                .SelectMany(m => m.SpawnedObjects)
                .FirstOrDefault(o => o.Id == primitiveId);

            if (obj != null)
            {
                player.Position = obj.transform.position;
                player.Rotation = Quaternion.Euler(obj.transform.eulerAngles);
            }
            else
            {
                Log.Warn($"[TeleportToPrimitive] Primitive {primitiveId} not found for player {player.Id}");
            }
        }
    }
}
