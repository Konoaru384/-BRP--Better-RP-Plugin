using Cassie;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnifiedSCPPlugin
{
    public static class CheckpointHackCommand
    {
        private static bool checkpointHackDone = false;
        private static CoroutineHandle openLoopCoroutine;

        private static CheckpointHackSettings Cfg => RoleplayPlugin.Instance.Config.CheckpointHack;

        public static void OnRoundStart()
        {
            checkpointHackDone = false;

            if (openLoopCoroutine.IsRunning)
                Timing.KillCoroutines(openLoopCoroutine);
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class CheckpointHack : ICommand
        {
            public string Command => "checkpointhack";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Hack LCZ/HCZ/Entrance checkpoints.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (!Cfg.Enable)
                {
                    response = "Checkpoint hack is disabled.";
                    return false;
                }

                var player = Player.Get(sender);

                if (player == null)
                {
                    response = "Player not found.";
                    return false;
                }

                if (checkpointHackDone)
                {
                    response = "⚠ Checkpoint hack already performed this round.";
                    return false;
                }

                if (!(player.Role.Type == RoleTypeId.FacilityGuard || player.Role.Team == Team.ChaosInsurgency))
                {
                    response = "❌ Only traitor Guards or Chaos can use this command.";
                    return false;
                }

                var room914 = Room.List.FirstOrDefault(r => r.Type == RoomType.Lcz914);
                if (room914 == null)
                {
                    response = "914 room not found.";
                    return false;
                }

                float distance = Vector3.Distance(player.Position, room914.Position);
                if (distance > Cfg.MaxDistance)
                {
                    response = "You must be near the checkpoint control room above 914.";
                    return false;
                }

                float heightDiff = player.Position.y - room914.Position.y;
                if (heightDiff <= Cfg.MinHeightDifference)
                {
                    response = "You must be in the checkpoint control room above 914.";
                    return false;
                }

                if (!(player.CurrentItem is Exiled.API.Features.Items.Keycard card &&
                      card.Type.ToString().Contains("Chaos")))
                {
                    response = "❌ You must hold a Chaos keycard.";
                    return false;
                }

                Vector3 hackStartPosition = player.Position;

                Timing.RunCoroutine(HackRoutine(player, hackStartPosition, room914));
                response = "✅ Checkpoint hack started...";
                return true;
            }
        }

        private static IEnumerator<float> HackRoutine(Player player, Vector3 hackStartPosition, Room room914)
        {
            float elapsed = 0f;
            float corruption = 0f;

            while (elapsed < Cfg.HackDuration)
            {
                float distance = Vector3.Distance(player.Position, hackStartPosition);
                float heightDiff = player.Position.y - room914.Position.y;

                bool hasChaosCard = player.CurrentItem is Exiled.API.Features.Items.Keycard card &&
                                    card.Type.ToString().Contains("Chaos");

                if (distance > Cfg.MaxDistance || heightDiff <= Cfg.MinHeightDifference || !hasChaosCard)
                    corruption += Cfg.CorruptionRate;
                else
                    corruption = Math.Max(0f, corruption - Cfg.CorruptionDecay);

                if (corruption >= 100f)
                {
                    player.ShowHint("<color=red><b>❌ Hack failed! Maximum corruption reached.</b></color>", 5);
                    yield break;
                }

                float percent = (elapsed / Cfg.HackDuration) * 100f;
                player.ShowHint(
                    $"<b><color=yellow>⚡ Checkpoint hack in progress...</color></b>\n" +
                    $"Progress: <color=green>{percent:0}%</color>\n" +
                    $"<color=orange>Corruption: {corruption:0}%</color>\n" +
                    $"<i>⚠ Stay close!</i>",
                    1);

                elapsed += 1f;
                yield return Timing.WaitForSeconds(1f);
            }

            checkpointHackDone = true;

            LabApi.Features.Wrappers.Cassie.Message(
                new CassieTtsPayload(
                    customAnnouncement: Cfg.CassieSuccessMessage,
                    autoGenerateSubtitles: false,
                    playBackground: false),
                glitchScale: 0);

            openLoopCoroutine = Timing.RunCoroutine(ForceOpenCheckpointsLoop());

            Map.Broadcast(Cfg.SuccessBroadcastDuration, Cfg.SuccessBroadcast);
        }

        private static IEnumerator<float> ForceOpenCheckpointsLoop()
        {
            try
            {
                foreach (var type in Cfg.CheckpointDoors)
                {
                    var door = Door.Get(type);
                    if (door != null)
                    {
                        door.IsOpen = true;
                        door.ChangeLock(DoorLockType.Warhead);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error while forcing checkpoint doors open: {ex.Message}");
            }

            while (checkpointHackDone)
                yield return Timing.WaitForSeconds(Cfg.ForceOpenInterval);

            foreach (var type in Cfg.CheckpointDoors)
            {
                var door = Door.Get(type);
                if (door != null)
                    door.ChangeLock(DoorLockType.None);
            }
        }
    }
}
