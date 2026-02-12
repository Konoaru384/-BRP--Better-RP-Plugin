using Cassie;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using GameCore;
using MEC;
using NorthwoodLib;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnifiedSCPPlugin
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class HackCommand : ICommand
    {
        public string Command => "hack";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Allows Chaos or Guards to hack the Foundation servers.";

        private static readonly Dictionary<Player, HackData> ActiveHacks = new();
        private static bool waitingForNextSpectator = false;
        private static CoroutineHandle waitingCoroutine = default;
        private static bool scp079AlreadyAssigned = false;
        private static CoroutineHandle corridorChaosHandle = default;

        public static bool IsCassieDeconfined => scp079AlreadyAssigned;

        private static HackSettings Cfg => RoleplayPlugin.Instance.Config.Hack;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player;
            try { player = Player.Get(sender); }
            catch { player = null; }

            if (player == null)
            {
                response = "Player not found.";
                return false;
            }

            if (player.CurrentRoom?.Type != RoomType.HczServerRoom)
            {
                response = "You must be inside the HCZ Server Room.";
                return false;
            }

            if (!(player.Role.Team == Team.ChaosInsurgency || player.Role == RoleTypeId.FacilityGuard))
            {
                response = "Access denied.";
                return false;
            }

            if (!player.Items.Any(it => it.Type == ItemType.KeycardChaosInsurgency))
            {
                response = "You must hold a Chaos keycard to start the hack.";
                return false;
            }

            if (ActiveHacks.ContainsKey(player))
            {
                response = "A hack is already in progress.";
                return false;
            }

            if (scp079AlreadyAssigned)
            {
                response = "SCP-079 is already active.";
                return false;
            }

            var hack = new HackData(player);
            ActiveHacks[player] = hack;
            hack.Coroutine = Timing.RunCoroutine(HackRoutine(hack));

            response = Cfg.HackStartMessage;
            return true;
        }

        public static void OnRoundStart()
        {
            scp079AlreadyAssigned = false;
            waitingForNextSpectator = false;

            if (waitingCoroutine.IsRunning)
                Timing.KillCoroutines(waitingCoroutine);

            if (corridorChaosHandle.IsRunning)
                Timing.KillCoroutines(corridorChaosHandle);
        }

        private static IEnumerator<float> HackRoutine(HackData hack)
        {
            var player = hack.Player;
            Vector3 startPos = player.Position;
            float corruption = 0f;

            while (hack.Progress < 100)
            {
                if (!player.IsAlive)
                {
                    player.ShowHint(Cfg.HackDeathMessage, 5);
                    ActiveHacks.Remove(player);
                    yield break;
                }

                float distance = Vector3.Distance(startPos, player.Position);

                if (distance > Cfg.ResetDistance)
                {
                    player.ShowHint(Cfg.HackTooFarMessage, 5);
                    ActiveHacks.Remove(player);
                    yield break;
                }

                if (distance > Cfg.CorruptionRange)
                {
                    corruption += Cfg.CorruptionRate;
                    if (corruption >= Cfg.MaxCorruption)
                    {
                        player.ShowHint(Cfg.HackCorruptedMessage, 6);
                        ActiveHacks.Remove(player);
                        yield break;
                    }
                }
                else
                {
                    corruption = Mathf.Max(0, corruption - Cfg.CorruptionRecovery);
                }

                hack.Progress++;

                player.ShowHint(
                    string.Format(Cfg.HackProgressHint, hack.Progress, Mathf.RoundToInt(corruption)),
                    Cfg.HackTickDelay + 0.1f
                );

                yield return Timing.WaitForSeconds(Cfg.HackTickDelay);
            }

            player.ShowHint(Cfg.HackCompleteMessage, 5);
            ActiveHacks.Remove(player);

            if (scp079AlreadyAssigned)
                yield break;

            var candidate = Player.List.FirstOrDefault(p => p.IsOverwatchEnabled || p.Role.Team == Team.Dead);
            if (candidate != null)
            {
                Assign079To(candidate);
                yield break;
            }

            waitingForNextSpectator = true;

            if (!waitingCoroutine.IsRunning)
                waitingCoroutine = Timing.RunCoroutine(WaitAndAssignCoroutine());

            player.Broadcast(12, Cfg.NoCandidateMessage);
        }

        private static void Assign079To(Player candidate)
        {
            if (candidate == null || scp079AlreadyAssigned) return;

            try
            {
                candidate.Role.Set(RoleTypeId.Scp079, RoleSpawnFlags.UseSpawnpoint);
                candidate.Broadcast(8, Cfg.Become079Message);

                LabApi.Features.Wrappers.Cassie.Message(
                    new CassieTtsPayload(customAnnouncement: Cfg.Cassie079Message, autoGenerateSubtitles: false, playBackground: false),
                    glitchScale: 0
                );

                scp079AlreadyAssigned = true;
                waitingForNextSpectator = false;

                if (!corridorChaosHandle.IsRunning)
                    corridorChaosHandle = Timing.RunCoroutine(CorridorChaosRoutine());
            }
            catch (Exception ex)
            {
                Log.Error($"[Assign079To] Exception: {ex}");
            }
        }

        private static IEnumerator<float> WaitAndAssignCoroutine()
        {
            while (waitingForNextSpectator && !scp079AlreadyAssigned)
            {
                var candidate = Player.List.FirstOrDefault(p => p.IsOverwatchEnabled || p.Role.Team == Team.Dead);
                if (candidate != null)
                {
                    Assign079To(candidate);
                    yield break;
                }
                yield return Timing.WaitForSeconds(2f);
            }
        }

        private static bool IsCorridorDoor(Door door)
        {
            if (door?.Base == null) return false;

            string goName = door.Base.gameObject.name ?? string.Empty;

            foreach (var id in Cfg.ExcludedDoorIds)
                if (!string.IsNullOrEmpty(id) && goName.Contains(id, StringComparison.OrdinalIgnoreCase))
                    return false;

            return true;
        }

        private static IEnumerator<float> CorridorChaosRoutine()
        {
            yield return Timing.WaitForSeconds(5f);

            float duration = Cfg.CorridorChaosDuration;
            float elapsed = 0f;
            float nextOverchargeAt = Cfg.OverchargeInterval;

            var doors = Door.List.ToList();

            while (elapsed < duration)
            {
                float progress = Mathf.Clamp01(elapsed / duration);

                float baseInterval = Mathf.Lerp(Cfg.CorridorChaosStartInterval, Cfg.CorridorChaosEndInterval, progress);
                float jitter = Random.Range(-Cfg.CorridorChaosJitter, Cfg.CorridorChaosJitter);
                float interval = Mathf.Max(0.05f, baseInterval + jitter);

                float toggleProbability = Mathf.Lerp(Cfg.CorridorChaosStartProbability, Cfg.CorridorChaosEndProbability, progress);

                foreach (var door in doors.OrderBy(_ => Random.value))
                {
                    if (!IsCorridorDoor(door)) continue;
                    if (Random.value <= toggleProbability)
                        door.IsOpen = !door.IsOpen;
                }

                if (elapsed >= nextOverchargeAt - 0.001f)
                {
                    try
                    {
                        Map.TurnOffAllLights(3f);
                        Timing.CallDelayed(2.8f, () => Map.TurnOffAllLights(0.4f));
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[Overcharge] Lights error: {ex}");
                    }

                    nextOverchargeAt += Cfg.OverchargeInterval;
                }

                yield return Timing.WaitForSeconds(interval);
                elapsed += interval;
            }

            foreach (var door in Door.List)
            {
                if (!IsCorridorDoor(door)) continue;
                if (door.IsOpen) door.IsOpen = false;
            }
        }

        private class HackData
        {
            public Player Player { get; }
            public int Progress { get; set; }
            public CoroutineHandle Coroutine { get; set; }

            public HackData(Player player)
            {
                Player = player;
                Progress = 0;
            }
        }
    }
}
