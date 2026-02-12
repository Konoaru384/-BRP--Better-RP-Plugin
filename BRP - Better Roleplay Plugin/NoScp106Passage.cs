using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace UnifiedSCPPlugin
{
    public static class Scp106Handler
    {
        private static CoroutineHandle _runtimeLoop;
        private static readonly HashSet<int> Tracked106 = new();
        private static bool _doorsUnlocked = false;

        private static Scp106BreachHandlerSettings Cfg => RoleplayPlugin.Instance.Config.Breach106;

        public static void OnSpawning(SpawningEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp106)
                Tracked106.Add(ev.Player.Id);
        }

        public static void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp106)
                Tracked106.Add(ev.Player.Id);
            else
                Tracked106.Remove(ev.Player.Id);
        }

        public static void OnDied(DiedEventArgs ev)
        {
            Tracked106.Remove(ev.Player.Id);
        }

        public static void OnRoundStarted()
        {
            if (!Cfg.Enable)
                return;

            _doorsUnlocked = false;
            ForceLockDoors();
            StartRuntimeLoop();
        }

        public static void OnTeleporting(TeleportingEventArgs ev)
        {
            if (!Cfg.Enable) return;
            if (IsIn106Room(ev.Player))
                ev.IsAllowed = false;
        }

        public static void OnStalking(StalkingEventArgs ev)
        {
            if (!Cfg.Enable) return;
            if (IsIn106Room(ev.Player))
                ev.IsAllowed = false;
        }

        public static void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (!Cfg.Enable) return;
            if (ev.Door == null) return;

            if (ev.Door.Type != DoorType.Scp106Primary && ev.Door.Type != DoorType.Scp106Secondary)
                return;

            if (!_doorsUnlocked)
            {
                ev.IsAllowed = false;
                ev.Player?.ShowHint(string.Format(Cfg.LockedDoorHint, Cfg.GeneratorsRequired), 2);
                ev.Door.PlaySound(DoorBeepType.InteractionDenied);
            }
        }

        private static void StartRuntimeLoop()
        {
            if (_runtimeLoop.IsRunning)
                return;

            _runtimeLoop = Timing.RunCoroutine(RuntimeLoop());
        }

        private static IEnumerator<float> RuntimeLoop()
        {
            while (Round.IsStarted)
            {
                int activeCount = Generator.List.Count(g => g.IsEngaged);

                if (!_doorsUnlocked && activeCount >= Cfg.GeneratorsRequired)
                {
                    _doorsUnlocked = true;
                    ForceUnlockDoors();
                    Log.Info("[Scp106Handler] Required generators activated → SCP-106 doors unlocked.");
                }

                foreach (var player in Player.List)
                {
                    if (player is null || !player.IsAlive || player.Role != RoleTypeId.Scp106)
                        continue;

                    if (IsIn106Room(player))
                    {
                        player.Broadcast(2, Cfg.InsideRoomHint, Broadcast.BroadcastFlags.Normal, true);
                    }
                    else
                    {
                        player.ClearBroadcasts();
                    }
                }

                foreach (var scp106 in Player.List)
                {
                    if (scp106 is null || !scp106.IsAlive || scp106.Role != RoleTypeId.Scp106)
                        continue;

                    PreventPassThrough106Doors(scp106);
                }

                yield return Timing.WaitForSeconds(Cfg.RuntimeTickDelay);
            }
        }

        private static void PreventPassThrough106Doors(Player scp106)
        {
            foreach (var door in Door.List)
            {
                if (door.Type != DoorType.Scp106Primary && door.Type != DoorType.Scp106Secondary)
                    continue;

                if (door.IsOpen)
                    continue;

                var doorPos = door.Position;
                var forward = door.GameObject.transform.forward.normalized;

                float centerDist = Vector3.Distance(scp106.Position, doorPos);
                if (centerDist > Cfg.MaxDistanceToCenter)
                    continue;

                Vector3 toPlayer = scp106.Position - doorPos;
                float signedDist = Vector3.Dot(toPlayer, forward);

                if (Mathf.Abs(signedDist) <= Cfg.MaxDistanceToPlane)
                {
                    Vector3 pushDir = forward * Mathf.Sign(signedDist == 0f ? 1f : signedDist);
                    if (signedDist < 0f)
                        pushDir = -forward;

                    scp106.Position = doorPos + pushDir * Cfg.PushDistance;
                    scp106.ShowHint(Cfg.BlockedDoorHint, 2);
                }
            }
        }

        private static void ForceLockDoors()
        {
            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.Scp106Primary || door.Type == DoorType.Scp106Secondary)
                {
                    if (door.IsOpen)
                        door.IsOpen = false;

                    door.Lock(DoorLockType.AdminCommand);
                }
            }
        }

        private static void ForceUnlockDoors()
        {
            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.Scp106Primary || door.Type == DoorType.Scp106Secondary)
                    door.Unlock();
            }
        }

        private static bool IsIn106Room(Player player)
        {
            return player.CurrentRoom != null && player.CurrentRoom.Type == RoomType.Hcz106;
        }

        public static void StopAll()
        {
            if (_runtimeLoop.IsRunning)
                Timing.KillCoroutines(_runtimeLoop);

            Tracked106.Clear();
            _doorsUnlocked = false;
        }
    }
}
