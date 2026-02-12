using Exiled.API.Features;
using GameCore;
using GateSummonSCP;
using System;
using static RoundSummary;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;


namespace UnifiedSCPPlugin
{
    public class RoleplayPlugin : Plugin<Config>
    {
        public static RoleplayPlugin Instance { get; private set; }

        public override string Name => "BRP - Better Roleplay Plugin";
        public override string Author => "Konoara";
        public override string Prefix => "BRP - Better Roleplay Plugin";
        public override System.Version Version => new(1, 1, 0);
        public override System.Version RequiredExiledVersion => new(9, 6, 0);

        public override void OnEnabled()
        {
            Instance = this;

            PlayerEvents.InteractingDoor += DoorHandler.OnInteractingDoor;
            CustomSpawnHandler.Register();
            PlayerEvents.Spawned += PlayerHandler.OnSpawned;
            PlayerEvents.ChangingRole += PlayerHandler.OnChangingRole;
            PlayerEvents.Died += PlayerHandler.OnDied;
            ServerEvents.RoundStarted += RoundHandler.OnRoundStarted;
            Door106Handler.Register();
            Scp3114FlashBlind.RegisterEvents();
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            PlayerEvents.Spawning += Scp106Handler.OnSpawning;
            PlayerEvents.Spawned += Scp106Handler.OnSpawned;
            PlayerEvents.Died += Scp106Handler.OnDied;
            ServerEvents.RoundStarted += Scp106Handler.OnRoundStarted;
            Exiled.Events.Handlers.Scp106.Teleporting += Scp106Handler.OnTeleporting;
            Exiled.Events.Handlers.Scp106.Stalking += Scp106Handler.OnStalking;
            PlayerEvents.InteractingDoor += DoorHandler.OnInteractingDoor;
            Exiled.Events.Handlers.Server.RoundStarted += HackCommand.OnRoundStart;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += BlackoutCommand.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundStarted += BlackoutCommand.OnRoundStart;
            UnifiedSCPSystem.Enable();
            TokenManagerHandler.Register();
            SpeedBadgePlugin.Register();
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            ScpToOverwatchHandler.Register();
            RpCreditsHandler.Register();







            Exiled.API.Features.Log.Info("[BRP] Plugin enable");
        }

        public override void OnDisabled()
        {
            PlayerEvents.InteractingDoor -= DoorHandler.OnInteractingDoor;
            PlayerEvents.Spawned -= PlayerHandler.OnSpawned;
            CustomSpawnHandler.Unregister();
            PlayerEvents.ChangingRole -= PlayerHandler.OnChangingRole;
            PlayerEvents.Died -= PlayerHandler.OnDied;
            ServerEvents.RoundStarted -= RoundHandler.OnRoundStarted;
            Door106Handler.Unregister();
            Scp3114FlashBlind.UnregisterEvents();
            SharedState.KillAllCoroutines();
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            SharedState.ClearAllState();
            ContainCommand.StopAll();
            PlayerEvents.Spawning -= Scp106Handler.OnSpawning;
            PlayerEvents.Spawned -= Scp106Handler.OnSpawned;
            PlayerEvents.Died -= Scp106Handler.OnDied;
            ServerEvents.RoundStarted -= Scp106Handler.OnRoundStarted;
            Exiled.Events.Handlers.Scp106.Teleporting -= Scp106Handler.OnTeleporting;
            Exiled.Events.Handlers.Scp106.Stalking -= Scp106Handler.OnStalking;
            PlayerEvents.InteractingDoor -= DoorHandler.OnInteractingDoor;
            Exiled.Events.Handlers.Server.RoundStarted -= HackCommand.OnRoundStart;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= BlackoutCommand.OnRoundStart;
            UnifiedSCPSystem.Disable();
            TokenManagerHandler.Unregister();
            Exiled.Events.Handlers.Server.RoundStarted -= BlackoutCommand.OnRoundStart;
            TokenManagerHandler.Unregister();
            SpeedBadgePlugin.Unregister();
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            ScpToOverwatchHandler.Unregister();
            RpCreditsHandler.Unregister();



            Scp106Handler.StopAll();

            Instance = null;

            Exiled.API.Features.Log.Info("[BRP] Plugin disable");
        }

        private void OnWaitingForPlayers()
        {
            SharedState.ClearAllState();
            PlayerHandler.ClearRoleChangeTimes();
        }

        private void OnRoundStarted()
        {

        }
    }
}
