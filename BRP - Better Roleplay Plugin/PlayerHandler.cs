using Cassie;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System;
using System.Collections.Generic;
using ExiledPlayer = Exiled.API.Features.Player;

namespace UnifiedSCPPlugin
{
    public static class PlayerHandler
    {
        private static readonly Dictionary<int, DateTime> LastRoleChangeTime = new();

        private static RMCH Cfg => RoleplayPlugin.Instance.Config.RMCH;

        public static void OnSpawned(SpawnedEventArgs ev)
        {
            if (!Constants.IsTracked(ev.Player))
                return;

            SharedState.LastKnownRooms[ev.Player.Id] = ev.Player.CurrentRoom;
            SharedState.TriggeredPlayers.Remove(ev.Player.Id);
        }

        public static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            LastRoleChangeTime[ev.Player.Id] = DateTime.Now;

            if (Constants.IsTracked(ev.Player))
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    string scpKey = Constants.GetScpKey(ev.Player);

                    if (Cfg.CassieBreachMessages.TryGetValue(scpKey, out var msg)
                        && !SharedState.TriggeredPlayers.Contains(ev.Player.Id))
                    {
                        PlayCassie(msg);
                        SharedState.TriggeredPlayers.Add(ev.Player.Id);
                    }

                    SharedState.LastKnownRooms[ev.Player.Id] = ev.Player.CurrentRoom;
                });
            }
            else
            {
                SharedState.LastKnownRooms.Remove(ev.Player.Id);
                SharedState.TriggeredPlayers.Remove(ev.Player.Id);
            }
        }

        public static void OnDied(DiedEventArgs ev)
        {
            string scpKey = Constants.GetScpKey(ev.Player);

            if (scpKey == "Scp106") return;

            if (Cfg.CassieTerminationMessages.TryGetValue(scpKey, out var message)
                && !string.IsNullOrWhiteSpace(message))
            {
                LabApi.Features.Wrappers.Cassie.Clear();
                PlayCassie(message);
            }

            SharedState.LastKnownRooms.Remove(ev.Player.Id);
            SharedState.TriggeredPlayers.Remove(ev.Player.Id);
            SharedState.Active066.Remove(ev.Player.Id);
            SharedState.Active999.Remove(ev.Player.Id);
            SharedState.Active008.Remove(ev.Player.Id);
            LastRoleChangeTime.Remove(ev.Player.Id);
        }

        public static IEnumerator<float> CheckRoomChanges()
        {
            yield return Timing.WaitForSeconds(2f);

            while (Round.IsStarted)
            {
                foreach (var player in ExiledPlayer.List)
                {
                    bool isTracked = Constants.IsTracked(player);
                    if (!isTracked) continue;

                    if (LastRoleChangeTime.TryGetValue(player.Id, out var lastChange)
                        && (DateTime.Now - lastChange).TotalSeconds < 2.0)
                        continue;

                    if (!SharedState.LastKnownRooms.TryGetValue(player.Id, out var lastRoom))
                    {
                        SharedState.LastKnownRooms[player.Id] = player.CurrentRoom;
                        continue;
                    }

                    if (player.CurrentRoom != lastRoom && !SharedState.TriggeredPlayers.Contains(player.Id))
                    {
                        string scpKey = Constants.GetScpKey(player);

                        if (Cfg.CassieBreachMessages.TryGetValue(scpKey, out var breachMsg)
                            && !string.IsNullOrWhiteSpace(breachMsg))
                        {
                            PlayCassie(breachMsg);
                            SharedState.TriggeredPlayers.Add(player.Id);
                        }
                    }

                    SharedState.LastKnownRooms[player.Id] = player.CurrentRoom;
                }

                yield return Timing.WaitForSeconds(Cfg.CheckInterval);
            }
        }

        public static void ClearRoleChangeTimes()
        {
            LastRoleChangeTime.Clear();
        }
        private static void PlayCassie(string message)
        {
            var payload = new CassieTtsPayload(
                customAnnouncement: message,
                autoGenerateSubtitles: Cfg.CassieAutoSubtitles,
                playBackground: Cfg.CassiePlayBackground
            );


            LabApi.Features.Wrappers.Cassie.Message(payload, glitchScale: Cfg.CassieGlitchScale);
        }
    }
}
