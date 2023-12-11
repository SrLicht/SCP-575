using MapGeneration;
using PlayerRoles;
using PluginAPI.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using YamlDotNet.Serialization;

namespace SCP575
{
    /// <summary>
    /// Plugin configuration class.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Gets or sets whether the plugin is enabled.
        /// </summary>
        [Description("Set if the plugin is enabled. If 'false', the plugin will not load any events.")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the plugin is in debug mode.
        /// </summary>
        [Description("Set if the plugin is in debug mode. Enabling this activates log debugs in the code, useful for identifying issues and reporting them on GitHub.")]
        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// Gets the plugin folder where the audio file will be stored.
        /// </summary>
        [YamlIgnore]
        private static string ConfigPath => Path.Combine(Paths.LocalPlugins.Plugins, "SCP-575");

        /// <summary>
        /// Gets or sets the path to the audio folder.
        /// </summary>
        [Description("Path to the folder where the audios are located. Delete to regenerate.")]
        public string PathToAudios { get; set; } = Path.Combine(ConfigPath, "Audios");

        /// <summary>
        /// Gets or sets the spawn chance of SCP-575.
        /// </summary>
        [Description("The per-round probability of SCP-575 appearing.")]
        public int SpawnChance { get; set; } = 40;

        /// <summary>
        /// Gets or sets whether SCP-575 should not appear if there is an SCP-173 in the game.
        /// </summary>
        [Description("If there is an SCP-173 in the round, SCP-575 will deactivate for that round.")]
        public bool DisableForScp173 { get; set; } = false;

        /// <summary>
        /// Gets or sets all blackout-related configuration.
        /// </summary>
        [Description("All blackout-related configuration.")]
        public BlackoutConfig BlackOut { get; set; } = new BlackoutConfig();

        /// <summary>
        /// Gets or sets all configuration related to the SCP-575.
        /// </summary>
        [Description("All configuration related to the SCP-575.")]
        public Scp575Config Scp575 { get; set; } = new Scp575Config();

        /// <summary>
        /// Gets or sets all responses for the commands.
        /// </summary>
        [Description("Here you can translate the responses given by the command when executed. Unfortunately, due to NWAPI limitations, I cannot provide a configuration to change the command description.")]
        public ResponseCommandConfig CommandResponses { get; set; } = new ResponseCommandConfig();
    }

    /// <summary>
    /// All configuration related to the blackout.
    /// </summary>
    public class BlackoutConfig
    {
        /// <summary>
        /// Gets or sets all facility zones where a blackout can occur.
        /// </summary>
        [Description("All facility zones where a blackout can occur.")]
        public List<FacilityZone> ActiveZones { get; set; } = new()
        {
            FacilityZone.LightContainment,
            FacilityZone.HeavyContainment,
            FacilityZone.Entrance,
        };

        /// <summary>
        /// Gets or sets the time delay before the constant blackouts begin to be executed.
        /// </summary>
        [Description("After this time, the constant blackouts will begin to be executed.")]
        public float InitialDelay { get; set; } = 300f;

        /// <summary>
        /// Gets or sets whether to ignore the initial delay and calculate a delay between the initial max and min delays.
        /// </summary>
        [Description("If this value is true, initial_delay will be ignored, and a calculation will be made between initial_max_delay and initial_min_delay, resulting in the delay.")]
        public bool RandomInitialDelay { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum time that the main delay can have.
        /// </summary>
        [Description("The maximum time that the main delay can have.")]
        public float InitialMaxDelay { get; set; } = 250f;

        /// <summary>
        /// Gets or sets the minimum time that the main delay can have.
        /// </summary>
        [Description("The minimum time that the main delay can have.")]
        public float InitialMinDelay { get; set; } = 190f;

        /// <summary>
        /// Gets or sets the minimum duration of a blackout.
        /// </summary>
        [Description("The minimum duration of a blackout.")]
        public float MinDuration { get; set; } = 30f;

        /// <summary>
        /// Gets or sets the maximum duration of a blackout.
        /// </summary>
        [Description("The maximum duration of a blackout.")]
        public float MaxDuration { get; set; } = 90f;

        /// <summary>
        /// Gets or sets the minimum duration of a delay after a blackout.
        /// </summary>
        [Description("The minimum duration of a delay after a blackout.")]
        public int MinDelay { get; set; } = 180;

        /// <summary>
        /// Gets or sets the minimum duration of a delay after a blackout.
        /// </summary>
        [Description("The minimum duration of a delay after a blackout.")]
        public int MaxDelay { get; set; } = 400;

        /// <summary>
        /// Gets or sets whether the SCP-575 disappears before the duration of the blackout should the blackout end.
        /// </summary>
        [Description("If the SCP-575 disappears before the duration of the blackout, should the blackout end?")]
        public bool EndBlackoutWhenDisappearing { get; set; } = false;


        /// <summary>
        /// Gets or sets the configuration for C.A.S.S.I.E messages before starting the blackout.
        /// </summary>
        [Description("Configuration for C.A.S.S.I.E messages before starting the blackout.")]
        public CassieMessage Cassie { get; set; } = new CassieMessage(
            "facility power system failure in 3 . pitch_.80 2 . pitch_.60 1 . pitch_.49 . .g1 pitch_.42  .g2 pitch_.31  .g5",
            false, true, false);

        /// <summary>
        /// Gets or sets the message Cassie will say before starting the blackout.
        /// </summary>
        [Description("This configuration is not being used and will be deleted in a future patch please use cassie instead.")]
        public string CassieMessage { get; set; } =
            "facility power system failure in 3 . pitch_.80 2 . pitch_.60 1 . pitch_.49 . .g1 pitch_.42  .g2 pitch_.31  .g5";

        /// <summary>
        /// Gets or sets the delay after Cassie's announcement before the blackout starts.
        /// </summary>
        [Description("After making Cassie's announcement, the blackout will start after these seconds, perfect to turn off the lights just when the announcement ends.")]
        public float DelayAfterCassie { get; set; } = 8.5f;

        /// <summary>
        /// Gets or sets the list of rooms where the light will not turn off, and SCP-575 will disappear if touched for 5 seconds.
        /// </summary>
        [Description("List of rooms where the light will not turn off. SCP-575 will disappear if touched in these rooms for 5 seconds. For a list of rooms, see the Readme of the plugin repository.")]
        public List<RoomName> BlackListRooms { get; set; } = new()
        {
            RoomName.Lcz914,
            RoomName.LczArmory,
            RoomName.LczCheckpointA,
            RoomName.LczCheckpointB,
            RoomName.HczArmory,
            RoomName.HczCheckpointA,
            RoomName.HczCheckpointB,
            RoomName.EzGateA,
            RoomName.EzGateB,
        };
    }

    /// <summary>
    /// All configuration related to the SCP-575.
    /// </summary>
    public class Scp575Config
    {
        /// <summary>
        /// Enabling this will activate the patch that prevents the server from making SCP-575 not float, causing a rather strange movement.
        /// I thought it was fun to leave it as an option.
        /// </summary>
        [Description("Enabling this will activate the patch that prevents the server from making SCP-575 not float, causing a rather strange movement. I thought it was fun to leave it as an option.")]
        public bool WeirdMovement { get; set; } = false;

        /// <summary>
        /// The name the dummy will have.
        /// </summary>
        [Description("The name the dummy will have.")]
        public string Nickname { get; set; } = "SCP-575-B";

        /// <summary>
        /// The distance at which players can see the name of SCP-575. The game default value is 10.
        /// </summary>
        [Description("The distance at which players can see the name of SCP-575. The game default value is 10.")]
        public int ViewRange { get; set; } = 12;

        /// <summary>
        /// Set the SCP-575 role, by default, it is SCP-106.
        /// </summary>
        [Description("Set the SCP-575 role, by default, it is SCP-106.")]
        public RoleTypeId RoleType { get; set; } = RoleTypeId.Scp106;

        /// <summary>
        /// The death message that will appear when players are killed by SCP-575.
        /// </summary>
        [Description("The death message that will appear when players are killed by SCP-575.")]
        public string KillFeed { get; set; } = "Devoured by SCP-575";

        /// <summary>
        /// The broadcast that will be sent to the player when killed by SCP-575.
        /// </summary>
        [Description("The broadcast that will be sent to the player when killed by SCP-575.")]
        public string BroadcastKill { get; set; } = "You were eaten by SCP-575, aim with a lit flashlight next time";

        /// <summary>
        /// The duration of the broadcast. If you want to disable it, set the duration to 0.
        /// </summary>
        [Description("The duration of the broadcast. If you want to disable it, set the duration to 0.")]
        public ushort BroadcastDuration { get; set; } = 10;

        /// <summary>
        /// Should SCP-575 play the sound files found in its folder? The sound file must be .ogg, need to be mono channel, and have a frequency of 48000 Hz.
        /// </summary>
        [Description("Should SCP-575 play the sound files found in its folder? The sound file must be .ogg, need to be mono channel, and have a frequency of 48000 Hz.")]
        public bool PlaySounds { get; set; } = false;

        /// <summary>
        /// The audio track replayed by SCP-575 will loop until it is destroyed.
        /// </summary>
        [Description("The audio track replayed by SCP-575 will loop until it is destroyed.")]
        public bool AudioIsLooped { get; set; } = false;

        /// <summary>
        /// The volume of the sound to be reproduced by SCP-575. High values violate the VSR.
        /// </summary>
        [Description("The volume of the sound to be reproduced by SCP-575. High values violate the VSR.")]
        public float SoundVolume { get; set; } = 85f;

        /// <summary>
        /// Activating this will cause SCP-575 to spawn with a delay where it will not be able to move or kill.
        /// </summary>
        [Description("Activating this will cause SCP-575 to spawn with a delay where it will not be able to move or kill.")]
        public bool DelayOnChase { get; set; } = true;

        /// <summary>
        /// Delay duration where SCP-575 will not be able to do anything.
        /// </summary>
        [Description("Delay duration where SCP-575 will not be able to do anything.")]
        public float DelayChase { get; set; } = 1.5f;

        /// <summary>
        /// The maximum distance that SCP-575 can be from its victim, remember that it must be greater than 16.
        /// </summary>
        [Description("The maximum distance that SCP-575 can be from its victim, remember that it must be greater than 16.")]
        public float MaxDistance { get; set; } = 28f;

        /// <summary>
        /// If the distance is equal to or greater than this value, the speed that is movement_speed_fast will be applied to SCP-575.
        /// </summary>
        [Description("If the distance is equal to or greater than this value, the speed that is movement_speed_fast will be applied to SCP-575.")]
        public float MediumDistance { get; set; } = 16f;

        /// <summary>
        /// If the distance is greater than this value, the value of movement_speed will be applied to SCP-575.
        /// </summary>
        [Description("If the distance is greater than this value, the value of movement_speed will be applied to SCP-575.")]
        public float MinDistance { get; set; } = 0.8f;

        /// <summary>
        /// If the distance between the target and SCP-575 is less than this value, the target will die.
        /// Note that if you modify this value, you will have to do it with min_distance as well.
        /// </summary>
        [Description("If the distance between the target and SCP-575 is less than this value, the target will die. Note that if you modify this value, you will have to do it with min_distance as well.")]
        public float KillDistance { get; set; } = 0.8f;

        /// <summary>
        /// If the distance between SCP-575 and its victim is equal to or greater than 16, it will have this movement speed.
        /// </summary>
        [Description("If the distance between SCP-575 and its victim is equal to or greater than 16, it will have this movement speed.")]
        public float MovementSpeedFast { get; set; } = 29;

        /// <summary>
        /// If the distance between SCP-575 and its victim is equal to or greater than 5, it will have this movement speed.
        /// </summary>
        [Description("If the distance between SCP-575 and its victim is equal to or greater than 5, it will have this movement speed.")]
        public float MovementSpeed { get; set; } = 22;

        /// <summary>
        /// Enabling this setting if the victim is running and is in the MediumDistance range, SCP-575 will move faster.
        /// </summary>
        [Description("Enabling this setting if the victim is running and is in the MediumDistance range, SCP-575 will move faster.")]
        public bool ChangeMovementSpeedIfRun { get; set; } = false;

        /// <summary>
        /// At what speed will SCP-575 move if the target is running if ChangeMovementSpeedIfRun is false this will not be used.
        /// </summary>
        [Description("At what speed will SCP-575 move if the target is running if ChangeMovementSpeedIfRun is false this will not be used.")]
        public float MovementSpeedRunning { get; set; } = 25;

        /// <summary>
        /// This is complicated to explain, so I'll just tell you what I do in the code.
        /// If a player has a flashlight on and points it at SCP-575 I fire a ray of light that if it touches SCP-575 adds a point of light,
        /// when it reaches a certain point of light SCP-575 disappears. The coroutine that checks these points is executed every 0.1s.
        /// </summary>
        [Description("This is complicated to explain, so I'll just tell you what I do in the code. If a player has a flashlight on and points it at SCP-575 I fire a ray of light that if it touches SCP-575 adds a point of light, when it reaches a certain point of light SCP-575 disappears. The coroutine that checks these points is executed every 0.1s.")]
        public int LightPoints { get; set; } = 85;

        /// <summary>
        /// When a player makes SCP-575 disappear using the LightPoints, this message will be sent to the player.
        /// </summary>
        [Description("When a player makes SCP-575 disappear using the LightPoints, this message will be sent to the player.")]
        public string LightPointKillMessage { get; set; } = "SCP-575 disappears for now";
    }

    /// <summary>
    /// All responses for the commands.
    /// </summary>
    public class ResponseCommandConfig
    {
        /// <summary>
        /// Gets or sets the response when the round has not started.
        /// </summary>
        [Description("Response when the round has not started.")]
        public string RoundHasNotStarted { get; set; } = "You cannot use this command if the round has not started.";

        /// <summary>
        /// Gets or sets the response for an invalid player ID.
        /// </summary>
        [Description("Response for an invalid player ID.")]
        public string InvalidPlayerId { get; set; } = "{0} is not a valid player id.";

        /// <summary>
        /// Gets or sets the response when a player is not found.
        /// </summary>
        [Description("Response when a player is not found.")]
        public string PlayerNotFound { get; set; } = "Player not found.";

        /// <summary>
        /// Gets or sets the response for an invalid duration.
        /// </summary>
        [Description("Response for an invalid duration.")]
        public string InvalidDuration { get; set; } = "{0} is not a valid duration.";

        /// <summary>
        /// Gets or sets the response when spawning SCP-575 for a specific duration.
        /// </summary>
        [Description("Response when spawning SCP-575 for a specific duration.")]
        public string Spawning { get; set; } = "Spawning SCP-575 to hunt {0} for {1} seconds.";

        /// <summary>
        /// Gets or sets the help response for the command.
        /// </summary>
        [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        [Description("Help response for the command.")]
        public string HelpResponse { get; set; } = "Correct use of the command {0}\nPlayer ID | It is a numerical ID that changes with each new round and each time someone connects to the server again.\nDuration | The time (in seconds) that SCP-575 will hunt someone\n\nNote that this command does not turn off the lights, so if SCP-575 is in a lit room for more than 5 seconds, it will disappear.";
    }

    /// <summary>
    /// Represents a message to be played by C.A.S.S.I.E.
    /// </summary>
    public class CassieMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CassieMessage"/> class with default values.
        /// </summary>
        public CassieMessage() : this(string.Empty, false, true, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CassieMessage"/> class with specified parameters.
        /// </summary>
        /// <param name="message">The message to be reproduced.</param>
        /// <param name="isHeld">Indicates whether C.A.S.S.I.E has to hold the message.</param>
        /// <param name="isNoisy">Indicates whether C.A.S.S.I.E has to make noises during the message.</param>
        /// <param name="isSubtitle">Indicates whether C.A.S.S.I.E has to display subtitles.</param>
        public CassieMessage(string message, bool isHeld, bool isNoisy, bool isSubtitle)
        {
            Message = message;
            IsHeld = isHeld;
            IsNoisy = isNoisy;
            IsSubtitle = isSubtitle;
        }

        /// <summary>
        /// Gets or sets the message to be reproduced.
        /// </summary>
        [Description("The message to be reproduced.")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets whether C.A.S.S.I.E has to hold the message.
        /// </summary>
        [Description("Indicates whether C.A.S.S.I.E has to hold the message.")]
        public bool IsHeld { get; set; }

        /// <summary>
        /// Gets or sets whether C.A.S.S.I.E has to make noises during the message.
        /// </summary>
        [Description("Indicates whether C.A.S.S.I.E has to make noises during the message.")]
        public bool IsNoisy { get; set; }

        /// <summary>
        /// Gets or sets whether C.A.S.S.I.E has to display subtitles.
        /// </summary>
        [Description("Indicates whether C.A.S.S.I.E has to display subtitles.")]
        public bool IsSubtitle { get; set; }
    }
}