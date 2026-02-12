using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Features.Items;

namespace UnifiedSCPPlugin
{
    public static class ClassDCellHandler
    {
        private static ClassDCellAccessSettings Cfg => RoleplayPlugin.Instance.Config.ClassDCellAccess;

        public static void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (!Cfg.Enable)
                return;

            if (ev.Door.Type != DoorType.PrisonDoor)
                return;

            if (ev.Door.IsLocked || ev.Player.IsBypassModeEnabled)
                return;

            ItemType[] allowed = Cfg.AllowedKeycards.ToArray();

            bool hasValidKeycard;

            if (Cfg.RequireHeldKeycard)
            {
                hasValidKeycard =
                    ev.Player.CurrentItem?.IsKeycard == true &&
                    allowed.Contains(ev.Player.CurrentItem.Type);
            }
            else
            {
                hasValidKeycard =
                    ev.Player.Items.Any(item => item.IsKeycard && allowed.Contains(item.Type));
            }

            if (!hasValidKeycard)
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint(Cfg.DeniedHint, 3f);
                return;
            }

            ev.IsAllowed = true;
        }
    }
}
