using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Helpers;
using Respawning;
using SCP575.Resources;
using SCPSLAudioApi.AudioCore;
using UnityEngine;
using VoiceChat;
using Extensions = SCP575.Resources.Extensions;
using Random = System.Random;

namespace SCP575
{
    public class Scp575
    {
        /// <summary>
        /// Plugin instance
        /// </summary>
        public static Scp575 Instance;

        /// <summary>
        /// Path to folder of audios.
        /// </summary>
        public readonly string AudioPath = Path.Combine($"{Paths.Plugins}", "Scp575Sounds");

        /// <summary>
        /// List of dummies.
        /// </summary>
        public static List<ReferenceHub> Dummies = new();

        /// <summary>
        /// Harmony Instance.
        /// </summary>
        private Harmony _harmonyInstance;

        private static Random _rng = new();
        private CoroutineHandle _blackoutHandler;
        [PluginConfig] public Config Config;

        /// <summary>
        /// Plugin version
        /// </summary>
        private const string Version = "1.0.5";

        [PluginEntryPoint("SCP-575", Version, "Add SCP-575 to SCP:SL", "SrLicht")]
        private void OnLoadPlugin()
        {
            try
            {
                SCPSLAudioApi.Startup.SetupDependencies();
                Instance = this;
                if (!Config.IsEnabled) return;
                Extensions.CreateDirectory();
                PluginAPI.Events.EventManager.RegisterEvents(this);
            }
            catch (Exception e)
            {
                Log.Error($"Error loading plugin: {e}");
            }

            try
            {
                _harmonyInstance = new Harmony($"SrLicht.{DateTime.UtcNow.Ticks}");
                _harmonyInstance.PatchAll();
            }
            catch (Exception e)
            {
                Log.Error($"Error on patch harmony: {e.Data} -- {e.StackTrace}");
            }
        }

        [PluginUnload]
        void UnLoadPlugin()
        {
            _harmonyInstance.UnpatchAll();
            _harmonyInstance = null;
            _rng = null;
            DestroyAllDummies();
            Dummies.Clear();
            Dummies = null;
            Instance = null;
        }

        // Events

        [PluginEvent(ServerEventType.RoundStart)]
        private void OnRoundStart()
        {
            if (!Config.IsEnabled || _rng.Next(100) >= Config.SpawnChance) return;
            Log.Info($"SCP-575 will spawn in this round");

            _blackoutHandler = Timing.RunCoroutine(Blackout());
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        private void OnWaitingForPlayers()
        {
            if (_blackoutHandler.IsRunning)
                Timing.KillCoroutines(_blackoutHandler);
        }

        /// <summary>
        /// Coroutine that is responsible for causing blackouts
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> Blackout()
        {
            yield return Timing.WaitForSeconds(Config.BlackOut.InitialDelay);

            while (Round.IsRoundStarted)
            {
                // Obtains the blackout duration by calculating between the minimum and maximum duration.
                var blackoutDuration =
                    (float)_rng.NextDouble() * (Config.BlackOut.MaxDuration - Config.BlackOut.MinDuration) +
                    Config.BlackOut.MinDuration;
                // Send Cassie's message to everyone
                RespawnEffectsController.PlayCassieAnnouncement(Config.BlackOut.CassieMessage, false, true);
                // Wait for Cassie to finish speaking
                yield return Timing.WaitForSeconds(Config.BlackOut.DelayAfterCassie);

                // Spawn SCP-575
                Spawn575(blackoutDuration);

                // Turn off the lights in the area
                Extensions.FlickerLights(blackoutDuration);

                // Decide the delay by calculating between the minimum and the maximum value.
                yield return Timing.WaitForSeconds(_rng.Next(Config.BlackOut.MinDelay, Config.BlackOut.MaxDelay) +
                                                   blackoutDuration);
            }

            DestroyAllDummies();
        }

        /// <summary>
        /// Spawns a SCP-575 that will chase a player until it kills him or the blackout ends
        /// </summary>
        /// <param name="duration">How long will the SCP-575 live</param>
        private void Spawn575(float duration)
        {
            try
            {
                var victim = GetVictim();

                if (victim == null)
                {
                    Log.Debug("Victim player is null, skipping code", Config.Debug);
                    return;
                }

                Log.Debug("Creating Dummy", Config.Debug);

                #region Create Dummy

                var newPlayer =
                    UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
                int id = Dummies.Count;
                var fakeConnection = new FakeConnection(id++);
                var hubPlayer = newPlayer.GetComponent<ReferenceHub>();

                #endregion

                Log.Debug("Adding dummy to the list of dummies", Config.Debug);
                Dummies.Add(hubPlayer);
                Log.Debug("Spawning dummy", Config.Debug);
                NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);

                Log.Debug("Setting the UserId of the dummy", Config.Debug);
                hubPlayer.characterClassManager._privUserId = $"SCP-575-{id}@server";
                hubPlayer.characterClassManager.InstanceMode = ClientInstanceMode.Unverified;

                try
                {
                    Log.Debug("Applying nickname", Config.Debug);
                    hubPlayer.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;
                    hubPlayer.nicknameSync.ViewRange = Config.Scp575.ViewRange;
                    // SetNick it will always give an error but will apply it anyway.
                    hubPlayer.nicknameSync.SetNick(Config.Scp575.Nickname);
                }
                catch (Exception)
                {
                    // ignored
                }

                Log.Debug("Changing role of dummy to SCP-106", Config.Debug);
                hubPlayer.roleManager.ServerSetRole(RoleTypeId.Scp106, RoleChangeReason.RemoteAdmin);
                hubPlayer.characterClassManager.GodMode = true;

                Timing.CallDelayed(0.3f, () =>
                {
                    Log.Debug("Moving dummy to the player's room", Config.Debug);

                    var room = victim.Room;

                    if (room.Name == RoomName.Lcz173)
                    {
                        hubPlayer.TryOverridePosition(room.ApiRoom.Position + new Vector3(0f, 13.5f, 0f), Vector3.zero);
                    }
                    else if (room.Name == RoomName.HczTestroom)
                    {
                        if (DoorVariant.DoorsByRoom.TryGetValue(room, out var hashSet))
                        {
                            var door = hashSet.FirstOrDefault();
                            if (door != null) hubPlayer.TryOverridePosition(door.transform.position, Vector3.zero);
                        }
                    }
                    else
                    {
                        hubPlayer.TryOverridePosition(room.ApiRoom.Position + new Vector3(0f, 1.3f, 0f), Vector3.zero);
                    }
                });

                Log.Debug("Adding SCP-575 component", Config.Debug);
                hubPlayer.gameObject.AddComponent<Resources.Components.Scp575Component>().Victim = victim;

                if (hubPlayer.gameObject.TryGetComponent<Resources.Components.Scp575Component>(out var comp))
                {
                    comp.Destroy(duration);
                }

                if (!Config.Scp575.PlaySounds) return;
                if (!Extensions.AudioFileExist()) Log.Error($"There is no .ogg file in the folder {AudioPath}");
                var audioPlayer = AudioPlayerBase.Get(hubPlayer);
                var audioFile = Extensions.GetAudioFilePath();
                audioPlayer.Enqueue(audioFile, -1);
                audioPlayer.LogDebug = Config.AudioDebug;
                //This will cause only the victim to be able to hear the music.
                audioPlayer.BroadcastTo.Add(victim.PlayerId);
                audioPlayer.Volume = Config.Scp575.SoundVolume;
                audioPlayer.Play(0);
                Log.Debug($"Playing sound {audioFile}", Config.Debug);
            }
            catch (Exception e)
            {
                Log.Error($"Error on {nameof(Spawn575)}: {e.Data} -- {e.StackTrace}");
            }
        }

        /// <summary>
        /// You get a random player from the area where the blackout occurred.
        /// </summary>
        /// <returns></returns>
        private Player GetVictim()
        {
            var players = new List<Player>();
            var playerList = Player.GetPlayers();

            if (Config.ActiveInLight)
            {
                foreach (var player in playerList)
                {
                    if (player.IsAlive && !player.IsSCP && !player.IsTutorial &&
                        player.Zone == FacilityZone.LightContainment
                        && !player.IsInInvalidRoom())

                    {
                        players.Add(player);
                    }
                }
            }

            if (Config.ActiveInHeavy)
            {
                foreach (var player in playerList)
                {
                    if (player.IsAlive && !player.IsSCP && !player.IsTutorial &&
                        player.Zone == FacilityZone.HeavyContainment
                        && !player.IsInInvalidRoom())
                    {
                        players.Add(player);
                    }
                }
            }

            if (Config.ActiveInEntrance)
            {
                foreach (var player in playerList)
                {
                    if (player.IsAlive && !player.IsSCP && !player.IsTutorial && player.Zone == FacilityZone.Entrance
                        && !player.IsInInvalidRoom())
                    {
                        players.Add(player);
                    }
                }
            }

            return players.Any() ? players.ElementAtOrDefault(UnityEngine.Random.Range(0, players.Count)) : null;
        }

        /// <summary>
        /// Destroy all dummies in the list.
        /// </summary>
        private void DestroyAllDummies()
        {
            foreach (var dummy in Dummies)
            {
                NetworkServer.Destroy(dummy.gameObject);
            }
        }
    }
}