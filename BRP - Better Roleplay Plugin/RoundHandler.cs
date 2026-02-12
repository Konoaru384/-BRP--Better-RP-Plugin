using Cassie;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Items;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnifiedSCPPlugin
{
    public static class RoundHandler
    {
        private static readonly System.Random rng = new();

        public static void OnRoundStarted()
        {
            Log.Info("[DEBUG-ROUND] OnRoundStarted déclenché");

            if (!RoleplayPlugin.Instance.Config.ScpRoomEntry.Enable)
            {
                Log.Warn("[DEBUG-ROUND] Config désactivée, sortie de OnRoundStarted");
                return;
            }

            bool isHRP = Player.List.Count < 7;
            Log.Info($"[DEBUG-ROUND] Nombre de joueurs: {Player.List.Count}, HRP: {isHRP}");

            Timing.CallDelayed(2f, () =>
            {
                Log.Info("[DEBUG-ROUND] CallDelayed exécuté");

                bool specialEventTriggered = false;

                if (isHRP)
                {
                    Log.Warn("[DEBUG-ROUND] Mode HRP activé");
                    Map.Broadcast(9999, "<color=red><b>❗ Partie en HRP — attention les cellules de class D se sont donc ouvertes... !</b></color>");

                    foreach (var door in Door.List.Where(d => d.Room?.Type == RoomType.LczClassDSpawn))
                        door.IsOpen = true;

                    if (rng.NextDouble() <= 0.10)
                    {
                        Log.Info("[DEBUG-ROUND] Événement spécial déclenché (HRP)");
                        specialEventTriggered = true;
                        TriggerSpecialEvent();
                    }

                    if (!specialEventTriggered)
                    {
                        Log.Info("[DEBUG-ROUND] Cassie OGGSTART1 dans 5 secondes (HRP)");
                        Timing.CallDelayed(15f, () =>
                        {
                            LabApi.Features.Wrappers.Cassie.Message(new CassieTtsPayload(customAnnouncement: "CUSTOMCASSIE OGGSTART1", autoGenerateSubtitles: false, playBackground: false), glitchScale: 0);
                            Log.Info("[DEBUG-ROUND] Cassie OGGSTART1 lancé (HRP)");
                        });
                    }

                    SharedState.RoomCheckCoroutine = Timing.RunCoroutine(PlayerHandler.CheckRoomChanges());
                    SharedState.HrpCoroutine = Timing.RunCoroutine(WaitForRPBroadcastTransition());
                }
                else
                {
                    Log.Info("[DEBUG-ROUND] Mode RP normal");

                    if (rng.NextDouble() <= 0.10)
                    {
                        Log.Info("[DEBUG-ROUND] Événement spécial déclenché (RP)");
                        specialEventTriggered = true;
                        TriggerSpecialEvent();
                    }

                    if (!specialEventTriggered)
                    {
                        Log.Info("[DEBUG-ROUND] Cassie OGGSTART1 dans 5 secondes (RP)");
                        Timing.CallDelayed(15f, () =>
                        {
                            LabApi.Features.Wrappers.Cassie.Message(new CassieTtsPayload(customAnnouncement: "CUSTOMCASSIE OGGSTART1", autoGenerateSubtitles: false, playBackground: false), glitchScale: 0);
                            Log.Info("[DEBUG-ROUND] Cassie OGGSTART1 lancé (RP)");
                        });
                    }
                }
            });

            foreach (Window w in Window.Get(x => x.Type == GlassType.Plants))
                w.BreakWindow();
        }

        private static void TriggerSpecialEvent()
        {
            Log.Info("[DEBUG-ROUND] TriggerSpecialEvent appelé");

            foreach (var door in Door.List.Where(d => d.Room?.Type == RoomType.LczClassDSpawn))
                door.IsOpen = true;

            Timing.CallDelayed(2f, () =>
            {
                var classDPlayers = Player.List.Where(p => p.Role.Type == RoleTypeId.ClassD).ToList();
                Log.Info($"[DEBUG-ROUND] Nombre de ClassD: {classDPlayers.Count}");

                if (classDPlayers.Count > 0)
                {
                    var chosen = classDPlayers[rng.Next(classDPlayers.Count)];
                    chosen.AddItem(ItemType.GunCOM18);
                    Log.Warn($"[DEBUG-ROUND] {chosen.Nickname} a reçu un COM18");

                    bool atLeastOneCard = false;
                    foreach (var cd in classDPlayers)
                    {
                        if (rng.NextDouble() <= 0.45)
                        {
                            cd.AddItem(ItemType.KeycardGuard);
                            atLeastOneCard = true;
                            Log.Info($"[DEBUG-ROUND] {cd.Nickname} a reçu une Keycard Guard");
                        }
                    }

                    if (!atLeastOneCard)
                    {
                        var forced = classDPlayers[rng.Next(classDPlayers.Count)];
                        forced.AddItem(ItemType.KeycardGuard);
                        Log.Warn($"[DEBUG-ROUND] {forced.Nickname} a reçu une Keycard Guard forcée");
                    }

                    Log.Info("[DEBUG-ROUND] Cassie OGGSTART2 dans 5 secondes");
                    Timing.CallDelayed(15f, () =>
                    {
                        LabApi.Features.Wrappers.Cassie.Message(new CassieTtsPayload(customAnnouncement: "CUSTOMCASSIE OGGSTART2", autoGenerateSubtitles: false, playBackground: false), glitchScale: 0);
                        Log.Info("[DEBUG-ROUND] Cassie OGGSTART2 lancé");
                    });
                }
                else
                {
                    Log.Warn("[DEBUG-ROUND] Aucun ClassD trouvé pour l’événement spécial");
                }
            });
        }

        private static IEnumerator<float> WaitForRPBroadcastTransition()
        {
            Log.Info("[DEBUG-ROUND] Coroutine WaitForRPBroadcastTransition démarrée");

            while (!SharedState.RpAnnounced)
            {
                if (Player.List.Count >= 7)
                {
                    SharedState.RpAnnounced = true;
                    Map.ClearBroadcasts();
                    Log.Info("[DEBUG-ROUND] Transition vers RP détectée");
                    yield return Timing.WaitForSeconds(5f);
                    Map.Broadcast(5, "<color=red><b>💬 La partie devient RP</b></color>");
                    yield break;
                }
                yield return Timing.WaitForSeconds(5f);
            }
        }
    }
}
