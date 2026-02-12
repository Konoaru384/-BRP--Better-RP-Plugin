// Destroying or modifying this file without Konoara's consent (with or without payment)
// is prohibited and may lead to legal action for copyright infringement
// as well as claims with Northwood.
// Contact me for more information.

using CommandSystem;
using Exiled.API.Features;
using System;

namespace UnifiedSCPPlugin
{
    public static class RpCreditsHandler
    {
        private const string StartupBroadcast =
            "<size=40><color=yellow>YOU PLAYED IN A SERVER WHO USE</color></size>\n" +
            "<size=50><color=orange>BRP — A PLUGIN FROM KONOARA !!!</color></size>";

        private const ushort StartupBroadcastDuration = 8;

        private const string CreditsMessage =
            "<color=yellow>BRP — Better RP Plugin</color>\n" +
            "<color=orange>Made by Konoara</color>\n" +
            "<color=cyan>GitHub:</color> https://github.com/Konoaru384/-BRP--Better-RP-Plugin/tree/main";

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
            Map.Broadcast(StartupBroadcastDuration, StartupBroadcast);
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class RpCreditsCommand : ICommand
        {
            public string Command => "rpcredits";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Shows BRP plugin credits.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                response = CreditsMessage;
                return true;
            }
        }
    }
}

