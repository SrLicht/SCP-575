using HarmonyLib;
using MapGeneration;
using MEC;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using PluginAPI.Helpers;
using Respawning;
using SCP575.API.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SCP575
{
    /// <summary>
    /// Plugin main class
    /// </summary>
    public class EntryPoint
    {
        /// <summary>
        /// Gets the singleton instance of the plugin.
        /// </summary>
        public static EntryPoint Instance { get; private set; } = null!;

        /// <summary>
        /// Gets the plugin config.
        /// </summary>
        [PluginConfig]
        public Config Config = null!;

        /// <summary>
        /// Gets the plugin version.
        /// </summary>
        public const string Version = "2.0.1";

        /// <summary>
        /// Private HarmonyID for unpatching assembly patches.
        /// </summary>
        private static string HarmonyId = "";

        /// <summary>
        /// Harmony instance used to patch code.
        /// </summary>
        public Harmony Harmony_Instance { get; private set; } = null!;

        /// <summary>
        /// Gets the random instance for the probability.
        /// </summary>
        public static System.Random Random = new();

        /// <summary>
        /// Blackout coroutine handler.
        /// </summary>
        private CoroutineHandle _blackoutHandler;

        /// <summary>
        /// Gets plugin prefix for loggin.
        /// </summary>
        public const string Prefix = "SCP575";

        [PluginPriority(LoadPriority.High)]
        [PluginEntryPoint("SCP-575", Version, "Add SCP-575 as an NPC that pursues players.", "SrLicht")]
        private void OnLoad()
        {
            Instance = this;

            if (!Config.IsEnabled)
            {
                Log.Warning($"Scp575 was disabled through configuration.");
                return;
            }

            SCPSLAudioApi.Startup.SetupDependencies();

            try
            {
                HarmonyId = $"{DateTime.Now.Ticks}.SCP575.{Version}";
                Harmony_Instance = new(HarmonyId);
                Harmony_Instance.PatchAll();
            }
            catch (HarmonyException e)
            {
                Log.Error($"Error on Patching: {e.Message} || {e.StackTrace}");
            }

            try
            {
                CreateDirectory();
                EventManager.RegisterEvents(this);
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(OnLoad)}: {e}");
            }

            Log.Info($"SCP-575 &3{Version}&r fully loaded.");
        }

        [PluginUnload]
        void UnLoad()
        {
            Harmony_Instance.UnpatchAll(HarmonyId);
            Dummies.ClearAllDummies();
        }

        [PluginEvent]
        private void OnMapGenerated(MapGeneratedEvent _)
        {
            if (_blackoutHandler.IsRunning)
                Timing.KillCoroutines(_blackoutHandler);
        }

        [PluginEvent]
        private void OnRoundStart(RoundStartEvent _)
        {
            if (!Config.IsEnabled || Random.Next(100) >= Config.SpawnChance)
                return;

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

        private IEnumerator<float> Blackout()
        {
            if (Config.BlackOut.RandomInitialDelay)
            {
                var delay = Random.Next((int)Config.BlackOut.InitialMinDelay, (int)Config.BlackOut.InitialMaxDelay);

                Log.Debug($"{nameof(Blackout)}: Random delay activated, waiting for {delay} seconds", Config.DebugMode);
                yield return Timing.WaitForSeconds(delay);
            }
            else
            {
                Log.Debug($"{nameof(Blackout)}: Waiting for {Config.BlackOut.InitialDelay} seconds", Config.DebugMode);
                yield return Timing.WaitForSeconds(Config.BlackOut.InitialDelay);
            }

            while (Round.IsRoundStarted)
            {
                // Obtains the blackout duration by calculating between the minimum and maximum duration.
                var blackoutDuration =
                    (float)Random.NextDouble() * (Config.BlackOut.MaxDuration - Config.BlackOut.MinDuration) +
                    Config.BlackOut.MinDuration;

#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                if (!string.IsNullOrEmpty(Config.BlackOut.Cassie?.Message) && Config.BlackOut.Cassie != null)
                {
                    // Send Cassie's message to everyone

                    RespawnEffectsController.PlayCassieAnnouncement(Config.BlackOut.Cassie?.Message, Config.BlackOut.Cassie.IsHeld, Config.BlackOut.Cassie.IsNoisy, Config.BlackOut.Cassie.IsSubtitle);

                }
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.

                // Wait for Cassie to finish speaking
                yield return Timing.WaitForSeconds(Config.BlackOut.DelayAfterCassie);

                var antiScp173 = false;
                if (Config.DisableForScp173)
                {
                    antiScp173 = GetScp173();
                }

                // Turn off the lights in the area
                BlackoutExtensions.StartBlackout(blackoutDuration, antiScp173);

                try
                {
                    // Spawn SCP-575

                    var victim = GetVictim();

                    if (victim != null)
                    {
                        BlackoutExtensions.SpawnScp575(victim, blackoutDuration);
                    }

                    if (victim is null)
                        Log.Debug($"{nameof(BlackoutExtensions.SpawnScp575)}: victim player is null.", Config.DebugMode);

                }
                catch (Exception e)
                {
                    Log.Error($"Error on {nameof(BlackoutExtensions.SpawnScp575)}: {e}");
                }

                // Decide the delay by calculating between the minimum and the maximum value.
                yield return Timing.WaitForSeconds(Random.Next(Config.BlackOut.MinDelay, Config.BlackOut.MaxDelay) +
                                                   blackoutDuration);
            }

            Dummies.ClearAllDummies();
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
                    break;
                }
            }

            return value;
        }

        /// <summary>
        /// Obtains a potential victim player based on specified blackout zones and certain conditions.
        /// </summary>
        /// <returns>A potential victim player or null if no valid player is found.</returns>
        private Player? GetVictim()
        {
            try
            {
                // Get all players and filter based on specific conditions.
                var players = Player.GetPlayers()
                    .Where(player =>
                        player.IsAlive &&
                        !player.IsSCP &&
                        !player.IsTutorial &&
                        !player.InInvalidRoom());

                // Get the active blackout zones from the configuration.
                var activeZones = Config.BlackOut.ActiveZones;

                // Filter players based on active blackout zones.
                if (activeZones.Count > 0)
                {
                    players = players.Where(player => activeZones.Contains(player.Zone));
                }

                if(activeZones.Count == 0)
                {
                    Log.Error($"{nameof(GetVictim)}: Config.BlackOut.ActiveZones is 0");
                    return null;
                }

                var filteredPlayers = players.ToList();

                // Return a potential victim player if any, otherwise, return null.
                return filteredPlayers.Any()
                    ? filteredPlayers.ElementAtOrDefault(UnityEngine.Random.Range(0, filteredPlayers.Count))
                    : null;
            }
            catch (Exception e)
            {
                // Log any errors that occur during the process.
                Log.Error($"Error on {nameof(GetVictim)}: {e} -- {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates the audio folder if not exist.
        /// </summary>
        private void CreateDirectory()
        {
            var audioPath = Path.Combine(Paths.LocalPlugins.Plugins, "SCP-575", "Audios");

            if (!Directory.Exists(audioPath))
            {
                Directory.CreateDirectory(audioPath);
            }
        }

    }
}
