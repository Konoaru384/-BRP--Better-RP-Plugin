using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnifiedSCPPlugin
{
    public class Scp3114FlashBlind
    {
        private static readonly HashSet<Player> ActiveBlinds = new();
        private static CoroutineHandle _checkHandle;

        private static Scp3114FlashSettings Cfg => RoleplayPlugin.Instance.Config.Scp3114Flash;

        public static void RegisterEvents()
        {
            if (!Cfg.Enable)
                return;

            Exiled.Events.Handlers.Player.TogglingFlashlight += OnFlashlightToggle;
            _checkHandle = Timing.RunCoroutine(CheckFlashlights());
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.TogglingFlashlight -= OnFlashlightToggle;

            if (_checkHandle.IsRunning)
                Timing.KillCoroutines(_checkHandle);
        }

        private static void OnFlashlightToggle(TogglingFlashlightEventArgs ev)
        {
            if (Cfg.Debug)
                Log.Info($"[3114-FLASH] Flashlight toggled by {ev.Player.Nickname}");
        }

        private static IEnumerator<float> CheckFlashlights()
        {
            while (true)
            {
                HashSet<Player> currentlyLit = new();

                foreach (var player in Player.List)
                {
                    if (player.CurrentItem is Flashlight flashlight && flashlight.IsEmittingLight)
                    {
                        var target = GetLookedAtPlayer(player, Cfg.MaxDistance);
                        if (target != null && target.Role.Type == RoleTypeId.Scp3114 && target.IsAlive)
                        {
                            currentlyLit.Add(target);
                        }
                    }
                }

                foreach (var scp in currentlyLit)
                {
                    scp.EnableEffect(EffectType.Flashed, Cfg.BlindDuration, false);

                    if (!ActiveBlinds.Contains(scp))
                        ActiveBlinds.Add(scp);
                }

                foreach (var scp in ActiveBlinds.ToList())
                {
                    if (!currentlyLit.Contains(scp))
                    {
                        scp.DisableEffect(EffectType.Flashed);
                        ActiveBlinds.Remove(scp);
                    }
                }

                yield return Timing.WaitForSeconds(Cfg.CheckInterval);
            }
        }

        private static Player GetLookedAtPlayer(Player source, float maxDistance)
        {
            if (Physics.Raycast(
                source.CameraTransform.position,
                source.CameraTransform.forward,
                out var hit,
                maxDistance,
                Cfg.RaycastMask))
            {
                return Player.Get(hit.collider.gameObject);
            }

            return null;
        }
    }
}
