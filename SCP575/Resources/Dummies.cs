using InventorySystem.Items;
using MapGeneration;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using PluginAPI.Core;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceChat;

namespace SCP575.Resources
{
    public class Dummies
    {
        public static HashSet<ReferenceHub> AllDummies = new();
        public static HashSet<DummyPlayer> DummiesPlayers = new();

        public static DummyPlayer CreateDummy()
        {
            var newPlayer =
                    UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            int id = AllDummies.Count;
            var fakeConnection = new FakeConnection(id++);
            var hubPlayer = newPlayer.GetComponent<ReferenceHub>();

            AllDummies.Add(hubPlayer);

            NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
            hubPlayer.characterClassManager._privUserId = $"Dummy-{id}@server";
            hubPlayer.characterClassManager.InstanceMode = ClientInstanceMode.Unverified;

            try
            {
                // SetNick it will always give an error but will apply it anyway.
                hubPlayer.nicknameSync.SetNick($"Dummy #{id}");
            }
            catch (Exception)
            {
                // ignored
            }

            return new(hubPlayer, id);
        }

        public static DummyPlayer CreateDummy(string userId, string nickname)
        {
            var newPlayer =
                    UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            int id = AllDummies.Count;
            var hubPlayer = newPlayer.GetComponent<ReferenceHub>();

            AllDummies.Add(hubPlayer);
            hubPlayer.characterClassManager._privUserId = $"{userId}-{id}@server";
            hubPlayer.characterClassManager.InstanceMode = ClientInstanceMode.Unverified;
            NetworkServer.AddPlayerForConnection(new FakeConnection(id++), newPlayer);

            foreach (var target in ReferenceHub.AllHubs.Where(x => x != ReferenceHub.HostHub))
                NetworkServer.SendSpawnMessage(hubPlayer.networkIdentity, target.connectionToClient);

            try
            {
                // SetNick it will always give an error but will apply it anyway.
                hubPlayer.nicknameSync.SetNick($"{nickname}");
            }
            catch (Exception)
            {
                // ignored
            }

            return new(hubPlayer, id);
        }

        /// <summary>
        /// Destroy all dummies.
        /// </summary>
        public static void ClearAllDummies()
        {
            if (AllDummies.Count > 0)
            {
                foreach (var hub in AllDummies)
                {
                    var dummy = DummiesPlayers.FirstOrDefault(d => d.ReferenceHub == hub);
                    dummy.StopAudio();
                    DummiesPlayers.Remove(dummy);

                    Timing.CallDelayed(0.2f, () =>
                    {
                        AllDummies.Remove(hub);
                        NetworkServer.RemovePlayerForConnection(hub.connectionToClient, true);
                    });
                }

                AllDummies.Clear();
            }
        }

        public static void DestroyDummy(ReferenceHub hub)
        {
            if (!AllDummies.Contains(hub))
                throw new ArgumentOutOfRangeException("hub", "Dummy player is not on the Dummies list");
            var dummy = DummiesPlayers.FirstOrDefault(d => d.ReferenceHub == hub);
            dummy.StopAudio();
            DummiesPlayers.Remove(dummy);

            Timing.CallDelayed(0.2f, () =>
            {
                AllDummies.Remove(hub);
                NetworkServer.RemovePlayerForConnection(hub.connectionToClient, true);
            });
        }

        public static void DestroyDummy(DummyPlayer hub)
        {
            if (!AllDummies.Contains(hub.ReferenceHub))
                throw new ArgumentOutOfRangeException("hub", "Dummy player is not on the Dummies list");
            hub.StopAudio();

            Timing.CallDelayed(0.2f, () =>
            {
                AllDummies.Remove(hub.ReferenceHub);
                NetworkServer.RemovePlayerForConnection(hub.ReferenceHub.connectionToClient, true);
            });

        }
    }
}
