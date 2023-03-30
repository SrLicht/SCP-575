using System.IO;
using System.Linq;
using System.Reflection;
using MapGeneration;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Helpers;
using UnityEngine;

namespace SCP575.Resources
{
    public static class Extensions
    {
        /// <summary>
        /// Checks for .ogg files in the sounds folder
        /// </summary>
        /// <returns></returns>
        public static bool AudioFileExist()
        {
            var files = Directory.GetFiles(Scp575.Instance.AudioPath);
            return files?.Length > 0 && files.FirstOrDefault(a => a.EndsWith(".ogg")) != null;
        }

        /// <summary>
        /// Checks if the player is in a room that is on the blacklist.
        /// </summary>
        public static bool IsInInvalidRoom(this Player ply)
        {
            if (ply.Room is null) return false;

            return Scp575.Instance.Config.BlackOut.BlackListRooms != null &&
                   Scp575.Instance.Config.BlackOut.BlackListRooms.Contains(ply.Room.Name);
        }

        /// <summary>
        /// Gets the audio files from the folder, if there is more than one it will take one at random.
        /// </summary>
        /// <returns></returns>
        public static string GetAudioFilePath()
        {
            var files = Directory.GetFiles(Scp575.Instance.AudioPath);
            var audios = files.Where(a => a.EndsWith(".ogg"));

            return audios.Any() ? audios.ElementAtOrDefault(Random.Range(0, audios.Count())) : null;
        }

        /// <summary>
        /// Create the Sound Directory if it does not exist
        /// </summary>
        public static void CreateDirectory()
        {
            if (!Directory.Exists(Scp575.Instance.AudioPath))
            {
                Directory.CreateDirectory(Scp575.Instance.AudioPath);
            }
        }

        /// <summary>
        /// Check if the ReferenceHub is a listed dummy
        /// </summary>
        public static bool IsDummy(ReferenceHub hub)
        {
            return Scp575.Dummies.Contains(hub);
        }

        /// <summary>
        /// Turns off the lights in the specified zone, for a period of time.
        /// </summary>
        /// <param name="duration">The duration in seconds of the blackout</param>
        public static void FlickerLights(float duration, bool antiScp173 = false)
        {
            var flickerControllerInstances = FlickerableLightController.Instances;

            if (SCP575.Scp575.Instance.Config.ActiveInHeavy)
            {
                foreach (var controller in flickerControllerInstances)
                {
                    if (controller.Room.Zone != FacilityZone.HeavyContainment ||
                        Scp575.Instance.Config.BlackOut.BlackListRooms.Count > 0 &&
                        Scp575.Instance.Config.BlackOut.BlackListRooms.Contains(controller.Room.Name)) continue;
                    controller.ServerFlickerLights(duration);
                }
            }

            if (SCP575.Scp575.Instance.Config.ActiveInLight)
            {
                if (!antiScp173)
                {
                    foreach (var controller in flickerControllerInstances)
                    {
                        if (controller.Room.Zone != FacilityZone.LightContainment ||
                            Scp575.Instance.Config.BlackOut.BlackListRooms.Count > 0 &&
                            Scp575.Instance.Config.BlackOut.BlackListRooms.Contains(controller.Room.Name)) continue;
                        controller.ServerFlickerLights(duration);
                    }
                }
            }

            if (SCP575.Scp575.Instance.Config.ActiveInEntrance)
            {
                foreach (var controller in flickerControllerInstances)
                {
                    if (controller.Room.Zone != FacilityZone.Entrance ||
                        Scp575.Instance.Config.BlackOut.BlackListRooms.Count > 0 &&
                        Scp575.Instance.Config.BlackOut.BlackListRooms.Contains(controller.Room.Name)) continue;
                    controller.ServerFlickerLights(duration);
                }
            }
        }

        /// <summary>
        /// Check if the current room is with the lights on.
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public static bool IsRoomIlluminated(RoomIdentifier roomId)
        {
            var lightController = roomId.GetComponentInChildren<FlickerableLightController>();

            return lightController != null && lightController.NetworkLightsEnabled;
        }

        #region SpawnMessage

        private static MethodInfo _sendSpawnMessage;

        /// <summary>
        /// Gets the cached <see cref="SendSpawnMessage"/> <see cref="MethodInfo"/>.
        /// </summary>
        public static MethodInfo SendSpawnMessage => _sendSpawnMessage ??=
            typeof(NetworkServer).GetMethod("SendSpawnMessage", BindingFlags.NonPublic | BindingFlags.Static);

        #endregion
    }
}