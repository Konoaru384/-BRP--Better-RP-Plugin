using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;

namespace UnifiedSCPPlugin
{
    public static class Gate173AutoOpen
    {
        public static void Register()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        }

        private static void OnRoundStarted()
        {
            var cfg = RoleplayPlugin.Instance.Config.Gate173;

            if (!cfg.Enable)
                return;

            Timing.CallDelayed(cfg.OpenDelay, () =>
            {
                var door = Door.Get(cfg.DoorName);

                if (door != null)
                {
                    door.ChangeLock(Exiled.API.Enums.DoorLockType.None);
                    door.IsOpen = true;

                    Log.Info("[Roleplay-Event] SCP-173 gate has been automatically opened.");
                }
                else
                {
                    Log.Warn($"[Roleplay-Event] Could not find the door named '{cfg.DoorName}'.");
                }
            });
        }
    }
}
