using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;

namespace GateSummonSCP
{
    public class SpeedBadgePlugin
    {
        public static void Register()
        {
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
        }

        private static void OnPlayerVerified(VerifiedEventArgs ev)
        {
            Player player = ev.Player;
            string nickname = player.Nickname.ToUpperInvariant();

            if (nickname.Contains("SPEED"))
            {
                if (!string.IsNullOrEmpty(player.RankName))
                {
                    player.RankName = $"{player.RankName} | TEAM SPEED";
                }
                else
                {
                    player.RankName = "TEAM SPEED";
                    player.RankColor = "blue";
                }
            }
        }
    }
}
