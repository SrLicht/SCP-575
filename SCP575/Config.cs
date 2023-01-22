using System.Collections.Generic;
using System.ComponentModel;
using MapGeneration;

namespace SCP575
{
    public class Config
    {
        [Description("Is the plugin enabled ?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enable the Logs.Debug of light points and other logs.")]
        public bool Debug { get; set; } = false;

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
        
        [Description("All blackout related configuration")]
        public BlackoutConfig BlackOut { get; set; } = new BlackoutConfig();
        
        [Description("All configuration related to the SCP-575")]
        public Scp575Config Scp575 { get; set; } = new Scp575Config();
    }

    public class BlackoutConfig
    {
        [Description("After this time, the constant blackouts will begin to be executed.")]
        public float InitialDelay { get; set; } = 300f;

        [Description("The minimum duration of a blackout")]
        public float MinDuration { get; set; } = 30f;
        
        [Description("The maximum duration of a blackout")]
        public  float MaxDuration { get; set; } = 90f;
        
        [Description("The minimum duration of a delay after a blackout")]
        public int MinDelay { get; set; } = 180;

        [Description("The minimum duration of a delay after a blackout")]
        public int MaxDelay { get; set; } = 400;

        [Description("Before starting the blackout Cassie will say this message")]
        public string CassieMessage { get; set; } = "facility power system failure in 3 . pitch_.80 2 . pitch_.60 1 . pitch_.49 . .g1 pitch_.42  .g2 pitch_.31  .g5";
        
        [Description("After making Cassie's announcement the blackout will start after these seconds, perfect to turn off the lights just when the announcement ends.")]
        public float DelayAfterCassie { get; set; } = 8.5f;

        [Description("List of rooms where the light will not turn off, the SCP-575 will disappear if you touch these rooms for 5 seconds. If you want a list of Rooms see the Readme of the plugin repository")]
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
        [Description("The name that SCP-575 will have as a player")]
        public string Nickname { get; set; } = "SCP-575-B";
        
        [Description("The information players will see when approaching SCP-575")]
        public string CustomInfo { get; set; } = "SCP-575";

        [Description("The distance at which players can see the name of the SCP-575 | The game default value is 10")]
        public int ViewRange { get; set; } = 12;

        [Description("The death message that will appear when players are killed by SCP-575")]
        public string KillFeed { get; set; } = "Devoured by SCP-575";
        
        [Description("The broadcast that will be sent to the player when killed by SCP-575")]
        public string BroadcastKill { get; set; } = "You were eaten by SCP-575, aim with a lit flashlight next time";
        
        [Description("The duration of the broadcast, if you want to disable it, set the duration to 0")]
        public ushort BroadcastDuration { get; set; } = 10;
        
        [Description("Should SCP-575 play the sounds files found in its folder? | The sound file must be .ogg need to be mono channel and have a frequency of 48000 Hz")]
        public bool PlaySounds { get; set; } = false;

        [Description("The volume of the sound to be reproduced by the SCP-575, high values violate the VSR.")]
        public float SoundVolume { get; set; } = 85f;

        [Description("Activating this will cause the SCP-575 to spawn with a delay where it will not be able to move or kill.")]
        public bool DelayOnChase { get; set; } = true;

        [Description("Delay duration where SCP-575 will not be able to do anything")]
        public float DelayChase { get; set; } = 1.5f;

        [Description("The maximum distance that SCP-575 can be from its victim, remember that it must be greater than 16")]
        public float MaxDistance { get; set; } = 28f;

        [Description("If the distance is equal to or greater than this value, the speed that is movement_speed_fast will be applied to the SCP-575.")]
        public float MediumDistance { get; set; } = 16f;

        [Description("If the distance is greater than this value the value of movement_speed will be applied to the SCP-575.")]
        public float MinDistance { get; set; } = 0.8f;

        [Description("If the distance between the target and the SCP-575 is less than this value, the target will die. Note that if you modify this value you will have to do it with min_distance as well.")]
        public float KillDistance { get; set; } = 0.8f;

        [Description("If the distance between SCP-575 and its victim is equal to or greater than 16, it will have this movement speed")]
        public float MovementSpeedFast { get; set; } = 29;
        
        [Description("If the distance between SCP-575 and its victim is equal to or greater than 5, it will have this movement speed")]
        public float MovementSpeed { get; set; } = 22;

        [Description("This is complicated to explain, so I'll just tell you what I do in the code. If a player has a flashlight on and points it at SCP-575 I fire a ray of light that if it touches SCP-575 adds a point of light, when it reaches a certain point of light SCP-575 disappears. The coroutine that checks these points is executed every 0.1s.")]
        public int LightPoints { get; set; } = 85;

        [Description("When a player makes SCP-575 disappear using the LightPoints, this message will be sent to the player.")]
        public string LightPointKillMessage { get; set; } = "SCP-575 disappears for now";
    }
}