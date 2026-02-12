using Cassie;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnifiedSCPPlugin
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ContainCommand : ICommand
    {
        public string Command => "contain";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Déclenche le confinement du SCP qui l’utilise.";

        private static readonly List<CoroutineHandle> Coroutines = new();

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = string.Empty;

            var player = Player.Get(sender);
            if (player == null || player.Role.Team != Team.SCPs)
            {
                response = "Tu dois être un SCP pour utiliser cette commande.";
                return false;
            }

            var cfg = RoleplayPlugin.Instance.Config.Containment;
            var role = player.Role.Type;
            string roleKey = role.ToString();

            if (role == RoleTypeId.Scp0492)
            {
                player.Kill("Containment Protocol");
                response = "SCP-049-2 a été neutralisé.";
                return true;
            }

            string cassieMsg = cfg.CassieMessages.TryGetValue(roleKey, out var msg)
                ? msg
                : cfg.DefaultCassieMessage;

            LabApi.Features.Wrappers.Cassie.Message(
                new CassieTtsPayload(customAnnouncement: cassieMsg, autoGenerateSubtitles: false, playBackground: false),
                glitchScale: 0
            );

            player.Kill("Containment Protocol");

            var doorsToLock = new List<Door>();

            if (cfg.StandardDoors.TryGetValue(roleKey, out var standardDoorTypes))
            {
                foreach (var dt in standardDoorTypes)
                {
                    var door = Door.Get(dt);
                    if (door != null)
                        doorsToLock.Add(door);
                }
            }

            if (cfg.CustomDoorIds.TryGetValue(roleKey, out var customIds))
            {
                foreach (var map in MapUtils.LoadedMaps.Values)
                {
                    foreach (var obj in map.SpawnedObjects)
                    {
                        if (customIds.Contains(obj.Id))
                        {
                            var door = Door.List.FirstOrDefault(x => x.GameObject == obj.gameObject);
                            if (door != null)
                                doorsToLock.Add(door);
                        }
                    }
                }
            }

            if (doorsToLock.Count > 0)
            {
                var handle = Timing.RunCoroutine(LockLoop(doorsToLock));
                Coroutines.Add(handle);
            }

            response = "SCP contenu avec succès.";
            return true;
        }

        private static IEnumerator<float> LockLoop(IEnumerable<Door> doors)
        {
            while (true)
            {
                foreach (var door in doors)
                    door.Lock(DoorLockType.AdminCommand);

                yield return Timing.WaitForSeconds(0.25f);
            }
        }

        public static void StopAll()
        {
            foreach (var h in Coroutines)
                Timing.KillCoroutines(h);

            Coroutines.Clear();
        }
    }
}
