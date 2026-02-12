using Cassie;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Waves;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using HarmonyLib;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using Respawning.Waves.Generic;
using Subtitles;
using System;
using System.Collections.Generic;
using System.Linq;
using ExiledLog = Exiled.API.Features.Log;
using ExiledMap = Exiled.API.Features.Map;
using ExiledPlayer = Exiled.API.Features.Player;

namespace UnifiedSCPPlugin
{
    public static class UnifiedSCPSystem
    {
        private static CoroutineHandle timerHandle;
        private static CoroutineHandle scpDetectHandle;
        private static CoroutineHandle spectatorPulseHandle;
        private static CoroutineHandle hintLoop;

        private static bool chaosForced;
        private static float endTime;
        private static float roundStartTime;

        private static bool suppressNextAnnouncement;
        private static bool isAnnouncementPatched;
        private static Harmony harmony;

        private static UnifiedSystemSettings Cfg => RoleplayPlugin.Instance.Config.UnifiedSystem;

        public static void Enable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawningTeam;
        }

        public static void Disable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawningTeam;
            StopAll();
        }

        private static void OnRoundStarted()
        {
            chaosForced = false;
            endTime = UnityEngine.Time.time + Cfg.EarlyWindowDuration;
            roundStartTime = UnityEngine.Time.time;

            int playerCount = ExiledPlayer.List.Count;
            if (playerCount < Cfg.MinPlayersForEarlyWindow)
            {
                ForceChaosRespawn("Not enough players at round start");
                RegisterHintLoop();
                return;
            }

            DisableRespawns();
            timerHandle = Timing.RunCoroutine(EarlyTimerLoop());
            scpDetectHandle = Timing.RunCoroutine(ScpDetectionLoop());
            spectatorPulseHandle = Timing.RunCoroutine(SpectatorLoop());
            RegisterHintLoop();
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            StopAll();
            EnableRespawns();
            suppressNextAnnouncement = false;
            isAnnouncementPatched = false;
            harmony = null;
        }

        private static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!chaosForced && UnityEngine.Time.time < endTime && IsScp(ev.NewRole))
                ForceChaosRespawn("SCP role detected during early window");
        }

        private static void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (UnityEngine.Time.time < endTime && !chaosForced)
            {
                ev.IsAllowed = false;
                return;
            }
        }

        private static IEnumerator<float> EarlyTimerLoop()
        {
            while (UnityEngine.Time.time < endTime && !chaosForced)
                yield return Timing.WaitForSeconds(0.5f);

            if (!chaosForced)
                ForceChaosRespawn("Early window expired");
        }

        private static IEnumerator<float> ScpDetectionLoop()
        {
            yield return Timing.WaitForSeconds(Cfg.ScpDetectionDelay);

            while (UnityEngine.Time.time < endTime && !chaosForced)
            {
                foreach (var p in ExiledPlayer.List)
                {
                    if (p.IsAlive && IsScp(p.Role.Type))
                    {
                        ForceChaosRespawn("SCP detected during early window");
                        yield break;
                    }
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private static IEnumerator<float> SpectatorLoop()
        {
            while (UnityEngine.Time.time < endTime && !chaosForced)
            {
                int converted = 0;

                foreach (var p in ExiledPlayer.List)
                {
                    if (p.Role.Type == RoleTypeId.Spectator)
                    {
                        p.Role.Set(RoleTypeId.ClassD);
                        converted++;
                    }
                }

                if (converted > 0)
                    ExiledMap.Broadcast(Cfg.SpectatorBroadcastDuration, Cfg.SpectatorBroadcastText);

                yield return Timing.WaitForSeconds(Cfg.SpectatorPulseInterval);
            }
        }

        private static void ForceChaosRespawn(string reason)
        {
            chaosForced = true;

            foreach (var wave in WaveManager.Waves)
            {
                if (wave is ILimitedWave limited)
                {
                    if (wave is ChaosSpawnWave)
                        limited.RespawnTokens = Cfg.ChaosTokens;
                    else if (wave is NtfSpawnWave)
                        limited.RespawnTokens = Cfg.NtfTokens;
                    else if (wave is ChaosMiniWave or NtfMiniWave)
                        limited.RespawnTokens = 0;
                }
            }

            StopEarlyCoroutines();
            RegisterHintLoop();
        }

        private static void RegisterHintLoop()
        {
            if (!hintLoop.IsRunning)
                hintLoop = Timing.RunCoroutine(HintLoop());
        }

        private static IEnumerator<float> HintLoop()
        {
            while (Exiled.API.Features.Round.InProgress)
            {
                yield return Timing.WaitForSeconds(1f);

                TimeSpan timeLeft = TimeSpan.Zero;
                string teamName = "<color=#808080>Unknown</color>";
                bool paused = false, isArrival = false, isChaos = false;

                TimeSpan ntfTime = TimeSpan.MaxValue;
                TimeSpan chaosTime = TimeSpan.MaxValue;
                int ntfTokens = 0, chaosTokens = 0;

                if (TimedWave.TryGetTimedWaves(Faction.FoundationStaff, out var ntf) && ntf.Count > 0)
                {
                    ntfTime = ntf[0].Timer.TimeLeft;
                    paused |= ntf[0].Timer.IsPaused;
                    if (ntf[0].Base is ILimitedWave l1) ntfTokens = l1.RespawnTokens;
                }

                if (TimedWave.TryGetTimedWaves(Faction.FoundationEnemy, out var chaos) && chaos.Count > 0)
                {
                    chaosTime = chaos[0].Timer.TimeLeft;
                    paused |= chaos[0].Timer.IsPaused;
                    if (chaos[0].Base is ILimitedWave l2) chaosTokens = l2.RespawnTokens;
                }

                float elapsed = UnityEngine.Time.time - roundStartTime;

                if (paused && elapsed < Cfg.EarlyWindowDuration && !chaosForced)
                {
                    double cycle = Cfg.SpectatorPulseInterval;
                    double remaining = cycle - (elapsed % cycle);
                    timeLeft = TimeSpan.FromSeconds(remaining);
                    teamName = Cfg.SpectatorTeamName;
                }
                else
                {
                    if (ntfTokens > chaosTokens || (ntfTokens == chaosTokens && ntfTime <= chaosTime))
                    {
                        timeLeft = ntfTime;
                        teamName = Cfg.NtfTeamName;
                        if (timeLeft.TotalSeconds <= 0) isArrival = true;
                    }
                    else
                    {
                        timeLeft = chaosTime;
                        teamName = Cfg.ChaosTeamName;
                        if (timeLeft.TotalSeconds <= 0) isArrival = true;
                        isChaos = true;
                    }
                }

                string hintText;

                if (isArrival)
                {
                    hintText = isChaos ? Cfg.ChaosArrivalHint : Cfg.NtfArrivalHint;
                }
                else
                {
                    string countdown = $"{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
                    hintText =
                        $"{Cfg.RespawnHeader}\n" +
                        $"{Cfg.TeamLabel} {teamName}\n" +
                        $"{Cfg.CountdownLabel} <color=#FFD700>{countdown}</color>";
                }

                foreach (var p in ExiledPlayer.List)
                    if (p.Role.Type == RoleTypeId.Spectator)
                        p.ShowHint(hintText, 1f);
            }
        }

        private static bool IsScp(RoleTypeId role) =>
            role is RoleTypeId.Scp049 or RoleTypeId.Scp079 or RoleTypeId.Scp096 or
                   RoleTypeId.Scp106 or RoleTypeId.Scp173 or RoleTypeId.Scp939 or RoleTypeId.Scp3114;

        private static void StopAll()
        {
            if (timerHandle.IsRunning) Timing.KillCoroutines(timerHandle);
            if (scpDetectHandle.IsRunning) Timing.KillCoroutines(scpDetectHandle);
            if (spectatorPulseHandle.IsRunning) Timing.KillCoroutines(spectatorPulseHandle);
            if (hintLoop.IsRunning) Timing.KillCoroutines(hintLoop);
        }

        private static void StopEarlyCoroutines()
        {
            if (timerHandle.IsRunning) Timing.KillCoroutines(timerHandle);
            if (scpDetectHandle.IsRunning) Timing.KillCoroutines(scpDetectHandle);
            if (spectatorPulseHandle.IsRunning) Timing.KillCoroutines(spectatorPulseHandle);
        }

        private static void DisableRespawns()
        {
            foreach (var wave in WaveManager.Waves)
                if (wave is ILimitedWave limited)
                    limited.RespawnTokens = 0;
        }

        private static void EnableRespawns()
        {
            foreach (var wave in WaveManager.Waves)
            {
                if (wave is ChaosSpawnWave chaos)
                    chaos.RespawnTokens = Cfg.ChaosTokens;
                if (wave is NtfSpawnWave ntf)
                    ntf.RespawnTokens = Cfg.NtfTokens;
            }
        }
    }
}
