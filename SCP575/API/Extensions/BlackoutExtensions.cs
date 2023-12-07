using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceChat;

namespace SCP575.API.Extensions
{
    /// <summary>
    /// Extension methods for handling blackout in the game.
    /// </summary>
    public static class BlackoutExtensions
    {
        /// <summary>
        /// List of room light controllers affected during the last blackout.
        /// </summary>
        private static List<RoomLightController> _roomLightsAffected = new();

        /// <summary>
        /// Initiates a blackout, causing flickering lights in specified zones.
        /// </summary>
        /// <param name="duration">Duration of the blackout in seconds.</param>
        /// <param name="scp173">Flag indicating if SCP-173 behavior should be considered.</param>
        public static void StartBlackout(float duration, bool scp173 = false)
        {
            // Retrieve all room light controllers in the game.
            var roomLightControllers = RoomLightController.Instances;

            // Clear the list of affected lights from the previous blackout.
            _roomLightsAffected.Clear();

            // Get the count of blacklisted rooms.
            var listCount = EntryPoint.Instance.Config.BlackOut.BlackListRooms.Count;

            // Check and initiate blackout in Heavy Containment zone.
            if (EntryPoint.Instance.Config.BlackOut.ActiveZones.Contains(MapGeneration.FacilityZone.HeavyContainment) && !scp173)
            {
                InitiateBlackoutInZone(roomLightControllers.Where(l => l.Room != null && l.Room.Zone == FacilityZone.HeavyContainment), duration);
            }

            // Check and initiate blackout in Light Containment zone.
            if (EntryPoint.Instance.Config.BlackOut.ActiveZones.Contains(MapGeneration.FacilityZone.LightContainment))
            {
                InitiateBlackoutInZone(roomLightControllers.Where(l => l.Room != null && l.Room.Zone == FacilityZone.LightContainment), duration);
            }

            // Check and initiate blackout in Entrance zone.
            if (EntryPoint.Instance.Config.BlackOut.ActiveZones.Contains(MapGeneration.FacilityZone.Entrance))
            {
                InitiateBlackoutInZone(roomLightControllers.Where(l => l.Room != null && l.Room.Zone == FacilityZone.Entrance), duration);
            }

            // Check and initiate blackout in Surface zone.
            if (EntryPoint.Instance.Config.BlackOut.ActiveZones.Contains(MapGeneration.FacilityZone.Surface))
            {
                InitiateBlackoutInZone(roomLightControllers.Where(l => l.Room != null && l.Room.Zone == FacilityZone.Surface), duration);
            }
        }

        /// <summary>
        /// Initiates a blackout in the specified zone, causing flickering lights in all room light controllers.
        /// </summary>
        /// <param name="roomLightControllers">The collection of room light controllers in the game.</param>
        /// <param name="duration">Duration of the blackout in seconds.</param>
        private static void InitiateBlackoutInZone(IEnumerable<RoomLightController> roomLightControllers, float duration)
        {
            // Get the count of blacklisted rooms.
            var listCount = EntryPoint.Instance.Config.BlackOut.BlackListRooms.Count;

            // Iterate through each room light controller in the specified zone.
            foreach (var light in roomLightControllers)
            {
                // Skip the room if it is blacklisted.
                if (listCount > 0 && EntryPoint.Instance.Config.BlackOut.BlackListRooms.Contains(light.Room.Name))
                    continue;

                // Flicker the lights in the room.
                light.ServerFlickerLights(duration);

                // Add the affected light to the list.
                _roomLightsAffected.Add(light);
            }
        }


        /// <summary>
        /// Ends the blackout, restoring lights to their normal state.
        /// </summary>
        public static void EndBlackout()
        {
            // Iterate through each room light controller affected during the blackout.
            foreach (var roomLight in _roomLightsAffected)
            {
                // Turn off flickering lights, restoring them to their normal state.
                roomLight.ServerFlickerLights(0);
            }

            // Clear the list of room light controllers affected during the blackout.
            _roomLightsAffected.Clear();
        }

        /// <summary>
        /// Checks if the specified room is illuminated.
        /// </summary>
        /// <param name="roomId">The identifier of the room to check.</param>
        /// <returns>
        /// True if the room is illuminated; otherwise, false.
        /// </returns>
        public static bool IsRoomIlluminated(RoomIdentifier roomId)
        {
            // Attempt to find the RoomLightController component in the child objects of the specified room.
            var lightController = roomId.GetComponentInChildren<RoomLightController>();

            // Check if a RoomLightController is found and its network lights are enabled.
            // Assumes that the state of network lights determines the illumination of the room.
            return lightController != null && lightController.NetworkLightsEnabled;
        }

        /// <summary>
        /// Checks if the victim is in a blacklisted room based on the current configuration.
        /// </summary>
        /// <param name="player">The victim to check.</param>
        /// <returns>
        /// True if the victim is in a blacklisted room; otherwise, false.
        /// </returns>
        public static bool InInvalidRoom(this Player player)
        {
            // If the victim or room is null, or the client instance is not ready, consider it not in an invalid room.
            if (player is null || player.Room is null || player.ReferenceHub.authManager.InstanceMode != CentralAuth.ClientInstanceMode.ReadyClient)
            {
                return false;
            }

            // Check if the current room is blacklisted.
            return EntryPoint.Instance.Config.BlackOut.BlackListRooms?.Contains(player.Room.Name) == true;
        }

        /// <summary>
        /// Handler the spawn of a SCP-575 instance.
        /// </summary>
        /// <param name="victim"></param>
        /// <param name="duration"></param>
        public static void SpawnScp575(Player victim, float duration)
        {
            try
            {
                if (victim is null)
                {
                    Log.Debug($"{nameof(SpawnScp575)}: victim victim not found.", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                    return;
                }

                var scp575 = Dummies.CreateDummy("Scp575", EntryPoint.Instance.Config.Scp575.Nickname, EntryPoint.Instance.Config.Scp575.RoleType);

                if (scp575 is null)
                {
                    Log.Error($"{nameof(SpawnScp575)}: Dummy victim is null", EntryPoint.Prefix);
                    return;
                }

                scp575.ReferenceHub.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;

                scp575.ReferenceHub.nicknameSync.ViewRange = EntryPoint.Instance.Config.Scp575.ViewRange;

                scp575.IsGodModeEnabled = true;

                Timing.CallDelayed(0.3f, () =>
                {
                    Log.Debug("Moving dummy to the victim's room", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);

                    var room = victim.Room;

                    if (room.Name == RoomName.Lcz173)
                    {
                        scp575.Position = room.ApiRoom.Position + new Vector3(0f, 13.5f, 0f);
                    }
                    else if (room.Name == RoomName.HczTestroom)
                    {
                        if (DoorVariant.DoorsByRoom.TryGetValue(room, out var hashSet))
                        {
                            var door = hashSet.FirstOrDefault();
                            if (door != null) scp575.Position = door.transform.position + Vector3.up;
                        }
                    }
                    else
                    {
                        if (room.Zone == FacilityZone.Surface)
                        {
                            scp575.Position = victim.Position + Vector3.back;
                        }
                        else
                        {
                            scp575.Position = room.ApiRoom.Position + Vector3.up;
                        }
                    }
                });

                Log.Debug("Adding SCP-575 component", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                var comp = scp575.GameObject.AddComponent<API.Components.ChaseComponent>();
                comp.Player = victim;
                comp.Disable(duration);

                if (!EntryPoint.Instance.Config.Scp575.PlaySounds)
                    return;

                if (!AudioExtensions.AudioFilesExist())
                {
                    Log.Error($"There is no .ogg file in the folder {EntryPoint.Instance.Config.PathToAudios}", EntryPoint.Prefix);
                    return;
                }

                var audioFile = AudioExtensions.GetRandomAudioFile();

                if (audioFile is null)
                    return;

                scp575.PlayAudio(audioFile, channel: VoiceChatChannel.Proximity, volume: EntryPoint.Instance.Config.Scp575.SoundVolume, player: victim);

                Log.Debug($"{nameof(SpawnScp575)}: Playing sound {audioFile}", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(SpawnScp575)}: {e}");
            }
        }
    }
}
