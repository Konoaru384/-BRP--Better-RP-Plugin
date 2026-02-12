using Cassie;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;
using PlayerRoles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnifiedSCPPlugin
{
    public static class CheckpointHackCommand
    {
        private static bool checkpointHackDone = false;
        private static CoroutineHandle openLoopCoroutine;

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
            public string Description => "Piratage des checkpoints LCZ/HCZ/Entrance";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                var player = Player.Get(sender);

                if (player == null)
                {
                    response = "Commande réservée aux joueurs.";
                    return false;
                }

                if (checkpointHackDone)
                {
                    response = "⚠ Le piratage des checkpoints a déjà été effectué ce round.";
                    return false;
                }
                if (!(player.Role.Type == RoleTypeId.FacilityGuard || player.Role.Team == Team.ChaosInsurgency))
                {
                    response = "❌ Seuls les Gardes traîtres et les Chaos peuvent effectuer cette commande.";
                    return false;
                }
                var room914 = Room.List.FirstOrDefault(r => r.Type == RoomType.Lcz914);
                if (room914 == null)
                {
                    response = "Salle 914 introuvable.";
                    return false;
                }

                float distance = Vector3.Distance(player.Position, room914.Position);
                if (distance > 15f)
                {
                    response = "Vous devez être proche de la salle des checkpoints a 914.";
                    return false;
                }
                float heightDiff = player.Position.y - room914.Position.y;
                if (heightDiff <= 1f)
                {
                    response = "Vous devez être dans la salle des checkpoints juste en haut !.";
                    return false;
                }
                if (!(player.CurrentItem is Exiled.API.Features.Items.Keycard card &&
                      card.Type.ToString().Contains("Chaos")))
                {
                    response = "❌ Vous devez tenir une carte Chaos en main.";
                    return false;
                }
                Vector3 hackStartPosition = player.Position;

                Timing.RunCoroutine(HackRoutine(player, hackStartPosition, room914));
                response = "✅ Piratage des checkpoints lancé...";
                return true;
            }
        }

        private static IEnumerator<float> HackRoutine(Player player, Vector3 hackStartPosition, Room room914)
        {
            const float duration = 100f;
            float elapsed = 0f;

            float corruption = 0f;
            const float corruptionRate = 5f;
            const float corruptionDecay = 2f;

            while (elapsed < duration)
            {
                float distance = Vector3.Distance(player.Position, hackStartPosition);
                float heightDiff = player.Position.y - room914.Position.y;

                bool hasChaosCard = player.CurrentItem is Exiled.API.Features.Items.Keycard card &&
                                    card.Type.ToString().Contains("Chaos");
                if (distance > 15f || heightDiff <= 1f || !hasChaosCard)
                {
                    corruption += corruptionRate;
                }
                else
                {
                    corruption = Math.Max(0f, corruption - corruptionDecay);
                }

                if (corruption >= 100f)
                {
                    player.ShowHint("<color=red><b>❌ Le piratage a échoué ! Corruption maximale atteinte.</b></color>", 5);
                    yield break;
                }

                float percent = (elapsed / duration) * 100f;
                player.ShowHint(
                    $"<b><color=yellow>⚡ Piratage des checkpoints en cours...</color></b>\n" +
                    $"Progression: <color=green>{percent:0}%</color>\n" +
                    $"<color=orange>Corruption: {corruption:0}%</color>\n" +
                    $"<i>⚠ Ne vous éloignez pas !</i>",
                    1);

                elapsed += 1f;
                yield return Timing.WaitForSeconds(1f);
            }

            checkpointHackDone = true;
            LabApi.Features.Wrappers.Cassie.Message(new CassieTtsPayload(customAnnouncement: "CUSTOMCASSIE checkpoint", autoGenerateSubtitles: false, playBackground: false), glitchScale: 0);
            openLoopCoroutine = Timing.RunCoroutine(ForceOpenCheckpointsLoop());

            Map.Broadcast(10, "<color=red><b>⚠ PIRATAGE DES CHECKPOINTS ACTIVÉ !</b></color>\n<color=yellow>Toutes les checkpoints sont compromis.</color>");
        }

        private static IEnumerator<float> ForceOpenCheckpointsLoop()
        {
            try
            {
                var cpLczA = Door.Get(DoorType.CheckpointLczA);
                if (cpLczA != null)
                {
                    cpLczA.IsOpen = true;
                    cpLczA.ChangeLock(DoorLockType.Warhead); 
                }

                var cpLczB = Door.Get(DoorType.CheckpointLczB);
                if (cpLczB != null)
                {
                    cpLczB.IsOpen = true;
                    cpLczB.ChangeLock(DoorLockType.Warhead);
                }

                var cpEzHczA = Door.Get(DoorType.CheckpointEzHczA);
                if (cpEzHczA != null)
                {
                    cpEzHczA.IsOpen = true;
                    cpEzHczA.ChangeLock(DoorLockType.Warhead);
                }

                var cpEzHczB = Door.Get(DoorType.CheckpointEzHczB);
                if (cpEzHczB != null)
                {
                    cpEzHczB.IsOpen = true;
                    cpEzHczB.ChangeLock(DoorLockType.Warhead);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Erreur lors de l'ouverture forcée des checkpoints : {ex.Message}");
            }
            while (checkpointHackDone)
            {
                yield return Timing.WaitForSeconds(5f);
            }
            foreach (var type in new[] { DoorType.CheckpointLczA, DoorType.CheckpointLczB, DoorType.CheckpointEzHczA, DoorType.CheckpointEzHczB })
            {
                var door = Door.Get(type);
                if (door != null)
                    door.ChangeLock(DoorLockType.None);
            }
        }
    }
}