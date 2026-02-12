using Cassie;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnifiedSCPPlugin
{
    public static class BlackoutCommand
    {
        private static bool blackoutDone = false;
        private static bool isHacking = false;
        private static CoroutineHandle hackCoroutine;

        public static void OnRoundStart()
        {
            blackoutDone = false;
            isHacking = false;
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class Blackout : ICommand
        {
            public string Command => "blackout";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Start hacking the generators";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                var cfg = RoleplayPlugin.Instance.Config.Blackout;

                if (!cfg.Enable)
                {
                    response = "This command is disabled.";
                    return false;
                }

                var player = Player.Get(sender);

                if (player == null)
                {
                    response = "Only players can use this command.";
                    return false;
                }

                if (blackoutDone)
                {
                    response = "A blackout has already occurred this round.";
                    return false;
                }

                if (isHacking)
                {
                    response = "A hacking attempt is already in progress.";
                    return false;
                }

                if (player.CurrentRoom == null || player.CurrentRoom.Type != cfg.RequiredRoom)
                {
                    response = $"You must be in the {cfg.RequiredRoom} room.";
                    return false;
                }

                if (!cfg.AllowedTeams.Contains(player.Role.Team))
                {
                    response = "You are not allowed to start the hacking process.";
                    return false;
                }

                if (!(player.CurrentItem is Exiled.API.Features.Items.Keycard keycard &&
                      cfg.RequiredKeycardKeywords.Exists(k => keycard.Type.ToString().Contains(k))))
                {
                    response = "You must hold a valid Chaos keycard.";
                    return false;
                }

                isHacking = true;
                hackCoroutine = Timing.RunCoroutine(HackRoutine(player));
                response = "Hacking started...";
                return true;
            }
        }

        private static IEnumerator<float> HackRoutine(Player player)
        {
            var cfg = RoleplayPlugin.Instance.Config.Blackout;

            float duration = cfg.HackDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (player.CurrentRoom == null ||
                    player.CurrentRoom.Type != cfg.RequiredRoom ||
                    Vector3.Distance(player.Position, player.CurrentRoom.Position) > cfg.MaxDistance ||
                    !(player.CurrentItem is Exiled.API.Features.Items.Keycard keycard &&
                      cfg.RequiredKeycardKeywords.Exists(k => keycard.Type.ToString().Contains(k))))
                {
                    player.ShowHint(cfg.HackCancelledHint, 5);
                    isHacking = false;
                    yield break;
                }

                float percent = (elapsed / duration) * 100f;
                player.ShowHint(string.Format(cfg.HackProgressHint, percent), 1);

                elapsed += 1f;
                yield return Timing.WaitForSeconds(1f);
            }

            blackoutDone = true;
            isHacking = false;

            Map.TurnOffAllLights(cfg.BlackoutDuration);

            LabApi.Features.Wrappers.Cassie.Message(
                new CassieTtsPayload(customAnnouncement: cfg.CassieMessage, autoGenerateSubtitles: false, playBackground: false),
                glitchScale: 0);

            Map.Broadcast(cfg.BlackoutBroadcastDuration, cfg.BlackoutBroadcastMessage);

            Log.Info("[Roleplay-Event] Blackout triggered successfully.");
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class LightOn : ICommand
        {
            public string Command => "lighton";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Restore power after blackout";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                var cfg = RoleplayPlugin.Instance.Config.Blackout;

                if (!cfg.Enable)
                {
                    response = "This command is disabled.";
                    return false;
                }

                var player = Player.Get(sender);

                if (player == null)
                {
                    response = "Only players can use this command.";
                    return false;
                }

                if (!blackoutDone)
                {
                    response = "No blackout has occurred.";
                    return false;
                }

                if (player.CurrentRoom == null || player.CurrentRoom.Type != cfg.RequiredRoom)
                {
                    response = $"You must be in the {cfg.RequiredRoom} room.";
                    return false;
                }

                if (!cfg.RestoreAllowedTeams.Contains(player.Role.Team))
                {
                    response = "You are not allowed to restore the lights.";
                    return false;
                }

                foreach (var gen in Generator.List)
                {
                    if (!gen.IsEngaged)
                    {
                        response = "All generators must be activated.";
                        return false;
                    }
                }

                Timing.RunCoroutine(LightOnRoutine(player));
                response = "Restoring power...";
                return true;
            }
        }

        private static IEnumerator<float> LightOnRoutine(Player player)
        {
            var cfg = RoleplayPlugin.Instance.Config.Blackout;

            player.ShowHint(cfg.RestoreHint, cfg.RestoreDuration);
            Vector3 pos = player.Position;

            yield return Timing.WaitForSeconds(cfg.RestoreDuration);

            if (Vector3.Distance(player.Position, pos) < 1f)
            {
                Map.TurnOnAllLights(cfg.RestoreZones);
                Map.Broadcast(cfg.RestoreBroadcastDuration, cfg.RestoreBroadcastMessage);

                Log.Info("[Roleplay-Event] Lights restored.");
            }
            else
            {
                player.ShowHint(cfg.RestoreCancelledHint, 5);
            }
        }
    }
}
