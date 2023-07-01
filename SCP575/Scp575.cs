using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Helpers;
using Respawning;
using SCP575.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// Harmony Instance.
        /// </summary>
        private Harmony _harmonyInstance;

        private static Random _rng = new();
        private CoroutineHandle _blackoutHandler;
        [PluginConfig] public Config Config;

        /// <summary>
        /// Plugin version
        /// </summary>
        private const string Version = "1.1.0";

        [PluginEntryPoint("SCP-575", Version, "Add SCP-575 to SCP:SL", "SrLicht")]
        private void OnLoadPlugin()
        {
            try
            {
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
            Dummies.ClearAllDummies();
            Instance = null;
        }

        // Events

        [PluginEvent(ServerEventType.RoundStart)]
        private void OnRoundStart()
        {
            if (!Config.IsEnabled || _rng.Next(100) >= Config.SpawnChance) return;
            Log.Info($"SCP-575 will spawn in this round");

            _blackoutHandler = Timing.RunCoroutine(Blackout());

            if (Config.DisableForScp173)
            {
                Timing.CallDelayed(10, () =>
                {
                    foreach (var player in Player.GetPlayers())
                    {
                        if (player.Role == RoleTypeId.Scp173)
                        {
                            Timing.KillCoroutines(_blackoutHandler);
                        }
                    }
                });
            }
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
            if (Config.BlackOut.RandomInitialDelay)
            {
                var delay = _rng.Next((int)Config.BlackOut.InitialMinDelay, (int)Config.BlackOut.InitialMaxDelay);

                Log.Debug($"Random delay activated, waiting for {delay} seconds", Config.Debug);
                yield return Timing.WaitForSeconds(delay);
            }
            else
            {
                Log.Debug($"Waiting for {Config.BlackOut.InitialDelay} seconds", Config.Debug);
                yield return Timing.WaitForSeconds(Config.BlackOut.InitialDelay);
            }

            while (Round.IsRoundStarted)
            {
                // Obtains the blackout duration by calculating between the minimum and maximum duration.
                var blackoutDuration =
                    (float)_rng.NextDouble() * (Config.BlackOut.MaxDuration - Config.BlackOut.MinDuration) +
                    Config.BlackOut.MinDuration;

                // Send Cassie's message to everyone
                RespawnEffectsController.PlayCassieAnnouncement(Config.BlackOut.CassieMessage, Config.BlackOut.CassieIsHold, Config.BlackOut.CassieIsNoise);

                // Wait for Cassie to finish speaking
                yield return Timing.WaitForSeconds(Config.BlackOut.DelayAfterCassie);

                try
                {
                    // Spawn SCP-575
                    Spawn575(blackoutDuration);
                }
                catch (Exception e)
                {
                    Log.Error($"Error on {nameof(Spawn575)}: {e}");
                }

                var antiScp173 = false;
                if (Config.DisableBlackoutForScp173)
                {
                    antiScp173 = GetScp173();
                }

                // Turn off the lights in the area
                Extensions.FlickerLights(blackoutDuration, antiScp173);

                // Decide the delay by calculating between the minimum and the maximum value.
                yield return Timing.WaitForSeconds(_rng.Next(Config.BlackOut.MinDelay, Config.BlackOut.MaxDelay) +
                                                   blackoutDuration);
            }

            Dummies.ClearAllDummies();
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

                var scp575 = Dummies.CreateDummy("Scp575", "SCP-575");

                try
                {
                    Log.Debug("Applying nickname", Config.Debug);
                    scp575.ReferenceHub.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;

                    scp575.ReferenceHub.nicknameSync.ViewRange = Config.Scp575.ViewRange;
                    // SetNick it will always give an error but will apply it anyway.
                    scp575.Nickname = Config.Scp575.Nickname;
                }
                catch (Exception)
                {
                    // ignored
                }

                Log.Debug($"Changing role of dummy to {Config.Scp575.RoleType}", Config.Debug);

                try
                {
                    scp575.Role = Config.Scp575.RoleType;
                }
                catch (Exception e)
                {
                    Log.Error($"Error on {nameof(Spawn575)}: Error on set dummy role {e}");
                }

                scp575.IsGodModeEnabled = true;

                Timing.CallDelayed(0.3f, () =>
                {
                    Log.Debug("Moving dummy to the player's room", Config.Debug);

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
                        scp575.Position = room.ApiRoom.Position + Vector3.up;
                    }
                });

                Log.Debug("Adding SCP-575 component", Config.Debug);
                var comp = scp575.GameObject.AddComponent<Resources.Components.Scp575Component>();
                comp.Victim = victim;
                comp.Destroy(duration);

                if (!Config.Scp575.PlaySounds) return;
                if (!Extensions.AudioFileExist()) Log.Error($"There is no .ogg file in the folder {AudioPath}");
                var audioFile = Extensions.GetAudioFilePath();
                scp575.PlayAudio(audioFile, channel: VoiceChatChannel.RoundSummary, volume: Config.Scp575.SoundVolume);
                scp575.AudioPlayerBase.LogDebug = Config.AudioDebug;
                Log.Debug($"Playing sound {audioFile}", Config.Debug);
            }
            catch (Exception e)
            {
                Log.Error($"Error on {nameof(Spawn575)}: {e} -- {e.Message}");
            }
        }

        /// <summary>
        /// You get a random player from the area where the blackout occurred.
        /// </summary>
        /// <returns></returns>
        private Player GetVictim()
        {
            try
            {
                var players = new List<Player>();
                var playerList = Player.GetPlayers();

                if (Config.ActiveInLight)
                {
                    foreach (var player in playerList)
                    {
                        if (player?.Room is null) continue;

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
                        if (player?.Room is null) continue;

                        if (player.IsAlive && !player.IsSCP && !player.IsTutorial &&
                            player.Zone == FacilityZone.HeavyContainment
                            && !player.IsInInvalidRoom())
                        {
                            players.Add(player);
                        }
                    }
                }

                if (!Config.ActiveInEntrance)
                    return players.Any()
                        ? players.ElementAtOrDefault(UnityEngine.Random.Range(0, players.Count))
                        : null;
                {
                    foreach (var player in playerList)
                    {
                        if (player?.Room is null) continue;

                        if (player.IsAlive && !player.IsSCP && !player.IsTutorial && player.Zone == FacilityZone.Entrance
                            && !player.IsInInvalidRoom())
                        {
                            players.Add(player);
                        }
                    }
                }

                return players.Any() ? players.ElementAtOrDefault(UnityEngine.Random.Range(0, players.Count)) : null;
            }
            catch (Exception e)
            {
                Log.Error($"Error on {nameof(GetVictim)}: {e} -- {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get if in the round exist a SCP-173
        /// </summary>
        /// <returns>boolean indicating if exist a SCP-173 in the round</returns>
        private bool GetScp173()
        {
            var value = false;

            foreach (var player in Player.GetPlayers())
            {
                if (player.Role == RoleTypeId.Scp173)
                {
                    value = true;
                }
            }

            return value;
        }
    }
}