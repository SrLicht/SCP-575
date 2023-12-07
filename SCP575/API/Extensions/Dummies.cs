using MEC;
using Mirror;
using PlayerRoles;
using SCP575.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP575.API.Extensions
{
    /// <summary>
    /// Collection methods for easly create dummies.
    /// </summary>
    public static class Dummies
    {
        /// <summary>
        /// Gets all dummies created.
        /// </summary>
        public static HashSet<ReferenceHub> AllDummies = new();

        /// <summary>
        /// Gets all <see cref="DummyPlayer"/> created.
        /// </summary>
        public static HashSet<DummyPlayer> DummiesPlayers = new();


        /// <summary>
        /// Creates a dummy player.
        /// </summary>
        /// <param name="userid">The unique identifier for the dummy player.</param>
        /// <param name="nickname">The nickname for the dummy player.</param>
        /// <param name="role">The role type ID for the dummy player.</param>
        /// <returns>A DummyPlayer instance if successful, otherwise null.</returns>
        public static DummyPlayer? CreateDummy(string userid, string nickname, RoleTypeId role)
        {
            // Instantiate the player prefab from the NetworkManager singleton.
            var prefab = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);

            // Get the ReferenceHub component from the instantiated prefab.
            var referenceHub = prefab.GetComponent<ReferenceHub>();

            // Check if ReferenceHub is null.
            if (referenceHub is null)
            {
                PluginAPI.Core.Log.Error($"{nameof(CreateDummy)}: referenceHub is null");
                return null;
            }

            // Create an EmptyConnection with a unique ID.
            int id = AllDummies.Count + 1;
            var fakeConnection = new EmptyConnection(id);

            // Add the dummy player for the fake connection to the NetworkServer.
            NetworkServer.AddPlayerForConnection(fakeConnection, prefab);

            // Set the authentication manager's instance mode to Unverified.
            referenceHub.authManager.InstanceMode = CentralAuth.ClientInstanceMode.Unverified;

            // Start the character class manager for the dummy player.
            referenceHub.characterClassManager.Start();

            try
            {
                // Attempt to set the nickname for the dummy player.
                referenceHub.nicknameSync.MyNick = nickname;
            }
            catch (Exception)
            {
                // Ignore exception.
            }

            // Create a DummyPlayer instance with the ReferenceHub and ID.
            var dummy = new DummyPlayer(referenceHub, id)
            {
                Role = role,
            };

            AllDummies.Add(referenceHub);
            DummiesPlayers.Add(dummy);
            return dummy;
        }

        /// <summary>
        /// Retrieves the associated DummyPlayer for a given ReferenceHub, if one exists.
        /// </summary>
        /// <param name="hub">The ReferenceHub for which to retrieve the DummyPlayer.</param>
        /// <returns>
        /// The DummyPlayer associated with the specified ReferenceHub, or null if not found.
        /// </returns>
        public static DummyPlayer? GetDummy(ReferenceHub hub)
        {
            // Attempt to find the DummyPlayer whose ReferenceHub matches the provided hub.
            return DummiesPlayers.FirstOrDefault(p => p.ReferenceHub == hub);
        }


        /// <summary>
        /// Destroys all dummy players and clears their associated data.
        /// </summary>
        public static void ClearAllDummies()
        {
            // Check if there are any dummy players to destroy.
            if (AllDummies.Count > 0)
            {
                // Iterate through all dummy players.
                foreach (var hub in AllDummies)
                {
                    // Find the corresponding DummyPlayer instance.
                    var dummy = DummiesPlayers.FirstOrDefault(d => d.ReferenceHub == hub);

                    // Stop any ongoing audio for the dummy player.
                    dummy.StopAudio();

                    // Remove the dummy player from the list of DummyPlayers.
                    DummiesPlayers.Remove(dummy);

                    // Delay the removal of the dummy player from the network.
                    Timing.CallDelayed(0.2f, () =>
                    {
                        // Remove the dummy player from the list of all dummies.
                        AllDummies.Remove(hub);

                        // Remove the player for the connection from the NetworkServer.
                        NetworkServer.RemovePlayerForConnection(hub.connectionToClient, true);
                    });
                }

                // Clear the lists of dummy players and dummies.
                AllDummies.Clear();
                DummiesPlayers.Clear();
            }
        }

        /// <summary>
        /// Destroys a specific dummy player and removes it from the associated data.
        /// </summary>
        /// <param name="hub">The DummyPlayer instance to destroy.</param>
        public static void DestroyDummy(DummyPlayer? hub)
        {
            // Check if the DummyPlayer instance is null.
            if (hub is null)
                return;

            // Check if the dummy player is in the list of all dummies.
            if (!AllDummies.Contains(hub.ReferenceHub))
                throw new ArgumentOutOfRangeException("hub", "Dummy player is not on the Dummies list");

            // Stop any ongoing audio for the dummy player.
            hub.StopAudio();

            // Delay the removal of the dummy player from the network.
            Timing.CallDelayed(0.3f, () =>
            {
                // Remove the dummy player from the list of all dummies.
                AllDummies.Remove(hub.ReferenceHub);

                // Remove the player for the connection from the NetworkServer.
                NetworkServer.RemovePlayerForConnection(hub.ReferenceHub.connectionToClient, true);
            });
        }
    }
}
