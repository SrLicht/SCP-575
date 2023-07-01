using MapGeneration;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using YamlDotNet.Serialization;

namespace SCP575
{
    public class Config
    {
        [Description("Is the plugin enabled ?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enable the Logs.Debug of light points and other logs.")]
        public bool Debug { get; set; } = false;

        [Description("Path to the folder where the audios are located")]
        public string PathToAudios { get; set; } = Path.Combine(ConfigPath, "Audios");

        [Description("Enable the Logs.Debug of SCPSLAudioApi, warning can be very spammy.")]
        public bool AudioDebug { get; set; } = false;

        [Description("Does the blackout affect Entrance Zone ?")]
        public bool ActiveInEntrance { get; set; } = false;

        [Description("Does the blackout affect Heavy Contaiment ?")]
        public bool ActiveInHeavy { get; set; } = false;

        [Description("Does the blackout affect Light Contaiment ?")]
        public bool ActiveInLight { get; set; } = true;

        [Description("The per-round probability of SCP-575 appearing")]
        public int SpawnChance { get; set; } = 40;

        [Description("If there is an SCP-173 in the round, SCP-575 will deactivate for that round.")]
        public bool DisableForScp173 { get; set; } = false;

        [Description("An alternative to the above configuration, if there is a SCP-173 in the light contaiment zone round it will never suffer a blackout. DO NOT ACTIVATE BOTH AT THE SAME TIME")]
        public bool DisableBlackoutForScp173 { get; set; } = false;

        [Description("All blackout related configuration")]
        public BlackoutConfig BlackOut { get; set; } = new BlackoutConfig();

        [Description("All configuration related to the SCP-575")]
        public Scp575Config Scp575 { get; set; } = new Scp575Config();

        [Description("Here you can translate the responses given by the command when executed, unfortunately due to NWAPI limitations I cannot give a configuration to change the command description.")]
        public ResponseCommandConfig CommandResponses { get; set; } = new ResponseCommandConfig();

        [YamlIgnore]
        private static string ConfigPath => Path.Combine(PluginAPI.Helpers.Paths.LocalPlugins.Plugins, "SCP-575");
    }

    public class BlackoutConfig
    {
        [Description("After this time, the constant blackouts will begin to be executed.")]
        public float InitialDelay { get; set; } = 300f;

        [Description("If this value is true initial_delay will be ignored and a calculation will be made between initial_max_delay and initial_min_delay which will result in the delay")]
        public bool RandomInitialDelay { get; set; } = false;

        [Description("The maximum time that the main delay can have")]
        public float InitialMaxDelay { get; set; } = 250f;

        [Description("The minimun time that the main delay can have")]
        public float InitialMinDelay { get; set; } = 190f;

        [Description("The minimum duration of a blackout")]
        public float MinDuration { get; set; } = 30f;

        [Description("The maximum duration of a blackout")]
        public float MaxDuration { get; set; } = 90f;

        [Description("The minimum duration of a delay after a blackout")]
        public int MinDelay { get; set; } = 180;

        [Description("The minimum duration of a delay after a blackout")]
        public int MaxDelay { get; set; } = 400;

        [Description("Before starting the blackout Cassie will say this message")]
        public string CassieMessage { get; set; } =
            "facility power system failure in 3 . pitch_.80 2 . pitch_.60 1 . pitch_.49 . .g1 pitch_.42  .g2 pitch_.31  .g5";

        [Description("I have no idea what it does")]
        public bool CassieIsHold { get; set; } = false;

        [Description("Enable o disable bells in cassie announcement")]
        public bool CassieIsNoise { get; set; } = true;

        [Description(
            "After making Cassie's announcement the blackout will start after these seconds, perfect to turn off the lights just when the announcement ends.")]
        public float DelayAfterCassie { get; set; } = 8.5f;

        [Description(
            "List of rooms where the light will not turn off, the SCP-575 will disappear if you touch these rooms for 5 seconds. If you want a list of Rooms see the Readme of the plugin repository")]
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

    public class Scp575Config
    {
        [Description("Enabling this will activate the patch that prevents the server from making the SCP-575 not float, causing a rather strange movement.  I thought it was fun to leave it as an option")]
        public bool WeirdMovement { get; set; } = false;

        [Description("The name the dummy will have")]
        public string Nickname { get; set; } = "SCP-575-B";

        [Description("The distance at which players can see the name of the SCP-575 | The game default value is 10")]
        public int ViewRange { get; set; } = 12;

        [Description("Set the SCP-575 role, by default is SCP-106")]
        public RoleTypeId RoleType { get; set; } = RoleTypeId.Scp106;

        [Description("The death message that will appear when players are killed by SCP-575")]
        public string KillFeed { get; set; } = "Devoured by SCP-575";

        [Description("The broadcast that will be sent to the player when killed by SCP-575")]
        public string BroadcastKill { get; set; } = "You were eaten by SCP-575, aim with a lit flashlight next time";

        [Description("The duration of the broadcast, if you want to disable it, set the duration to 0")]
        public ushort BroadcastDuration { get; set; } = 10;

        [Description(
            "Should SCP-575 play the sounds files found in its folder? | The sound file must be .ogg need to be mono channel and have a frequency of 48000 Hz")]
        public bool PlaySounds { get; set; } = false;

        [Description("The audio track replayed by the SCP-575 will loop until it is destroyed.")]
        public bool AudioIsLooped { get; set; } = false;

        [Description("The volume of the sound to be reproduced by the SCP-575, high values violate the VSR.")]
        public float SoundVolume { get; set; } = 85f;

        [Description(
            "Activating this will cause the SCP-575 to spawn with a delay where it will not be able to move or kill.")]
        public bool DelayOnChase { get; set; } = true;

        [Description("Delay duration where SCP-575 will not be able to do anything")]
        public float DelayChase { get; set; } = 1.5f;

        [Description(
            "The maximum distance that SCP-575 can be from its victim, remember that it must be greater than 16")]
        public float MaxDistance { get; set; } = 28f;

        [Description("If the distance is equal to or greater than this value, the speed that is movement_speed_fast will be applied to the SCP-575.")]
        public float MediumDistance { get; set; } = 16f;

        [Description(
            "If the distance is greater than this value the value of movement_speed will be applied to the SCP-575.")]
        public float MinDistance { get; set; } = 0.8f;

        [Description(
            "If the distance between the target and the SCP-575 is less than this value, the target will die. Note that if you modify this value you will have to do it with min_distance as well.")]
        public float KillDistance { get; set; } = 0.8f;

        [Description(
            "If the distance between SCP-575 and its victim is equal to or greater than 16, it will have this movement speed")]
        public float MovementSpeedFast { get; set; } = 29;

        [Description(
            "If the distance between SCP-575 and its victim is equal to or greater than 5, it will have this movement speed")]
        public float MovementSpeed { get; set; } = 22;

        [Description("Enabling this setting if the victim is running and is in the MediumDistance range the 575 will move faster.")]
        public bool ChangeMovementSpeedIfRun { get; set; } = false;

        [Description("At what speed will the SCP-575 move if the target is running if ChangeMovementSpeedIfRun is false this will not be used.")]
        public float MovementSpeedRunning { get; set; } = 25;

        [Description(
            "This is complicated to explain, so I'll just tell you what I do in the code. If a player has a flashlight on and points it at SCP-575 I fire a ray of light that if it touches SCP-575 adds a point of light, when it reaches a certain point of light SCP-575 disappears. The coroutine that checks these points is executed every 0.1s.")]
        public int LightPoints { get; set; } = 85;

        [Description(
            "When a player makes SCP-575 disappear using the LightPoints, this message will be sent to the player.")]
        public string LightPointKillMessage { get; set; } = "SCP-575 disappears for now";
    }

    public class ResponseCommandConfig
    {
        public string RoundHasNotStarted { get; set; } = "You cannot use this command if the round has not started.";

        public string InvalidPlayerId { get; set; } = "{0} is not an valid player id";
        public string PlayerNotFound { get; set; } = "Player not found";

        public string InvalidDuration { get; set; } = "{0} is not an valid duration";

        public string Spawning { get; set; } = "Spawning a SCP-575 to hunt {0} for {1} seconds";

        [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        public string HelpResponse { get; set; } = "Correct use of the command {0}\nPlayer ID | It is a numerical ID that changes with each new round and each time someone connects to the server again.\nDuration | The time (in seconds) that the SCP-575 will hunt someone";
    }
}