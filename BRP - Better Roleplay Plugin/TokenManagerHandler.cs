using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using Respawning.Waves.Generic;
using System.Collections.Generic;

namespace UnifiedSCPPlugin
{
    public static class TokenManagerHandler
    {
        private static CoroutineHandle tokenLoop;

        private enum SpamProfile
        {
            None,
            AfterMtfRespawn,
            AfterChaosRespawn
        }

        private static SpamProfile currentProfile = SpamProfile.None;

        private static InfiniteRespawnSettings Cfg => RoleplayPlugin.Instance.Config.InfiniteRespawns;

        public static void Register()
        {
            if (!Cfg.Enable)
            {
                Log.Warn("[TokenManager] Disabled in config.");
                return;
            }

            Log.Info("[TokenManager] Initialized");
            Exiled.Events.Handlers.Server.RespawningTeam += OnTeamRespawn;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Server.RespawningTeam -= OnTeamRespawn;

            if (tokenLoop.IsRunning)
                Timing.KillCoroutines(tokenLoop);

            tokenLoop = default;
            Log.Info("[TokenManager] Stopped");
        }

        private static void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            if (!Cfg.Enable)
                return;

            if (tokenLoop.IsRunning)
                Timing.KillCoroutines(tokenLoop);

            if (ev.NextKnownTeam == Faction.FoundationEnemy)
            {
                currentProfile = SpamProfile.AfterChaosRespawn;
                if (Cfg.Debug)
                    Log.Info($"[TokenManager] Chaos respawn → MTF={Cfg.MtfTokensAfterChaos}, Chaos={Cfg.MiniWaveTokens}");
            }
            else if (ev.NextKnownTeam == Faction.FoundationStaff)
            {
                currentProfile = SpamProfile.AfterMtfRespawn;
                if (Cfg.Debug)
                    Log.Info($"[TokenManager] MTF respawn → Chaos={Cfg.ChaosTokensAfterMtf}, MTF={Cfg.MiniWaveTokens}");
            }
            else
            {
                currentProfile = SpamProfile.None;
            }

            if (currentProfile != SpamProfile.None)
                tokenLoop = Timing.RunCoroutine(TokenManagerLoop());
        }

        private static IEnumerator<float> TokenManagerLoop()
        {
            while (true)
            {
                int ntfTokensTarget = 0;
                int chaosTokensTarget = 0;

                switch (currentProfile)
                {
                    case SpamProfile.AfterMtfRespawn:
                        ntfTokensTarget = Cfg.MiniWaveTokens;
                        chaosTokensTarget = Cfg.ChaosTokensAfterMtf;
                        break;

                    case SpamProfile.AfterChaosRespawn:
                        ntfTokensTarget = Cfg.MtfTokensAfterChaos;
                        chaosTokensTarget = Cfg.MiniWaveTokens;
                        break;
                }

                foreach (var wave in WaveManager.Waves)
                {
                    if (wave is ILimitedWave limited)
                    {
                        if (wave is NtfSpawnWave && limited.RespawnTokens != ntfTokensTarget)
                            limited.RespawnTokens = ntfTokensTarget;

                        if (wave is ChaosSpawnWave && limited.RespawnTokens != chaosTokensTarget)
                            limited.RespawnTokens = chaosTokensTarget;

                        if ((wave is NtfMiniWave || wave is ChaosMiniWave) &&
                            limited.RespawnTokens != Cfg.MiniWaveTokens)
                        {
                            limited.RespawnTokens = Cfg.MiniWaveTokens;
                        }
                    }
                }

                yield return Timing.WaitForSeconds(Cfg.EnforcementInterval);
            }
        }
    }
}
