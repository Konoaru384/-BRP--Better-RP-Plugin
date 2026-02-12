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

        private static RoundStartSettings Cfg => RoleplayPlugin.Instance.Config.RoundStart;

        public static void OnRoundStarted()
        {
            Log.Info("[ROUND] OnRoundStarted triggered");

            if (!Cfg.Enable)
            {
                Log.Warn("[ROUND] Disabled in config");
                return;
            }

            bool isHRP = Player.List.Count < Cfg.MinPlayersForRP;
            Log.Info($"[ROUND] Players: {Player.List.Count}, HRP: {isHRP}");

            Timing.CallDelayed(2f, () =>
            {
                bool specialEventTriggered = false;

                if (isHRP)
                {
                    Log.Warn("[ROUND] HRP mode active");

                    Map.Broadcast(Cfg.HrpBroadcastDuration, Cfg.HrpBroadcast);

                    if (Cfg.AutoOpenClassDCells)
                    {
                        foreach (var door in Door.List.Where(d => d.Room?.Type == RoomType.LczClassDSpawn))
                            door.IsOpen = true;
                    }

                    if (rng.NextDouble() <= Cfg.SpecialEventChance)
                    {
                        specialEventTriggered = true;
                        TriggerSpecialEvent();
                    }

                    if (!specialEventTriggered)
                    {
                        Timing.CallDelayed(Cfg.InitialCassieDelay, () =>
                        {
                            LabApi.Features.Wrappers.Cassie.Message(
                                new CassieTtsPayload(
                                    customAnnouncement: Cfg.CassieStartHRP,
                                    autoGenerateSubtitles: false,
                                    playBackground: false),
                                glitchScale: 0);
                        });
                    }

                    SharedState.RoomCheckCoroutine = Timing.RunCoroutine(PlayerHandler.CheckRoomChanges());

                    if (Cfg.EnableRPTransition)
                        SharedState.HrpCoroutine = Timing.RunCoroutine(WaitForRPBroadcastTransition());
                }
                else
                {
                    Log.Info("[ROUND] Normal RP mode");

                    if (rng.NextDouble() <= Cfg.SpecialEventChance)
                    {
                        specialEventTriggered = true;
                        TriggerSpecialEvent();
                    }

                    if (!specialEventTriggered)
                    {
                        Timing.CallDelayed(Cfg.InitialCassieDelay, () =>
                        {
                            LabApi.Features.Wrappers.Cassie.Message(
                                new CassieTtsPayload(
                                    customAnnouncement: Cfg.CassieStartRP,
                                    autoGenerateSubtitles: false,
                                    playBackground: false),
                                glitchScale: 0);
                        });
                    }
                }
            });

            if (Cfg.BreakPlantWindows)
            {
                foreach (Window w in Window.Get(x => x.Type == GlassType.Plants))
                    w.BreakWindow();
            }
        }

        private static void TriggerSpecialEvent()
        {
            Log.Info("[ROUND] Special event triggered");

            foreach (var door in Door.List.Where(d => d.Room?.Type == RoomType.LczClassDSpawn))
                door.IsOpen = true;

            Timing.CallDelayed(2f, () =>
            {
                var classDPlayers = Player.List.Where(p => p.Role.Type == RoleTypeId.ClassD).ToList();

                if (classDPlayers.Count > 0)
                {
                    var chosen = classDPlayers[rng.Next(classDPlayers.Count)];
                    chosen.AddItem(ItemType.GunCOM18);

                    bool atLeastOneCard = false;

                    foreach (var cd in classDPlayers)
                    {
                        if (rng.NextDouble() <= Cfg.ClassDKeycardChance)
                        {
                            cd.AddItem(ItemType.KeycardGuard);
                            atLeastOneCard = true;
                        }
                    }

                    if (!atLeastOneCard)
                    {
                        var forced = classDPlayers[rng.Next(classDPlayers.Count)];
                        forced.AddItem(ItemType.KeycardGuard);
                    }

                    Timing.CallDelayed(Cfg.InitialCassieDelay, () =>
                    {
                        LabApi.Features.Wrappers.Cassie.Message(
                            new CassieTtsPayload(
                                customAnnouncement: Cfg.CassieSpecialEvent,
                                autoGenerateSubtitles: false,
                                playBackground: false),
                            glitchScale: 0);
                    });
                }
            });
        }

        private static IEnumerator<float> WaitForRPBroadcastTransition()
        {
            while (!SharedState.RpAnnounced)
            {
                if (Player.List.Count >= Cfg.MinPlayersForRP)
                {
                    SharedState.RpAnnounced = true;
                    Map.ClearBroadcasts();

                    yield return Timing.WaitForSeconds(Cfg.RpTransitionDelay);

                    Map.Broadcast(5, Cfg.RpTransitionBroadcast);
                    yield break;
                }

                yield return Timing.WaitForSeconds(5f);
            }
        }
    }
}
