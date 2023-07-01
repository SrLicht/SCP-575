![Github All Releases](https://img.shields.io/github/downloads/SrLicht/SCP-575/total.svg)   <a href="https://github.com/SrLicht/SCP-575/releases"><img src="https://img.shields.io/github/v/release/SrLicht/SCP-575?include_prereleases&label=Last Release" alt="Releases"></a> 

# What does this do?
This plugin inserts the SCP-575 in SCP:SL, every round there is a probability that the SCP-575 spawns, if it spawns in LCZ, HCZ and EZ (configurable) there will be blackouts, the players that are in the blackout zones are likely to meet the SCP-575, the SCP-575 will start chasing them, if the 575 touches them the players will be devoured and the SCP-575 will disappear for now. It is possible to make SCP-575 disappear by pointing a flashlight at it or by going to a lighted area.

# Features
* Allows the SCP-575 to play sounds. This is explained in [# Sounds](https://github.com/SrLicht/SCP-575#sounds)
* Spawn an NPC (dummy) that will chase the player, the dummy is fully customizable in terms of mobility, name and killing range.
* If the SCP-575 is in a lighted room for 5 seconds, it will disappear until the next blackout.
* If the player being chased points the SCP-575 with a lighted flashlight or with a weapon that has a lighted flashlight it will add light points, when a certain number of points is reached the SCP-575 will stop chasing the player (this is configurable).

# Requirements
This plugin only works for [NWAPI](https://github.com/northwood-studios/NwPluginAPI) 12.0 or higher.

This plugin uses as dependency [SCPSLAudioApi](https://github.com/CedModV2/SCPSLAudioApi)

# SCPSLAudioApi dependency problem
Because of how NWAPI loads the plugin dependencies if more than one plugin installed uses SCPSLAudioApi it will give error and will not start the server, to solve this you will have to put ``NVorbis.dll`` in the dependencies folder.
**NVorbis.dll is included in a separate .zip file in the latest versions.**

# Sounds
This plugin allows you to make the SCP-575 play sounds of your choice, the sound files must be placed in the ``%appdata%/SCP Secret Laboratory/PluginAPI/plugins/Scp575Sounds`` folder.

**Important**
* Sound files must be in .ogg format.
* The sound file must be mono channel
* The sound frequency should be 48000 Hz
* If there are several sound files, one will be chosen at random.
* Only the player being chased hears the sounds.

 I recommend using the website https://convertio.co/mp3-ogg/

# Known problems
* AI is dumb and can sometimes get stuck depending on the location.
* SCP-575 appears in RA as a player
* Lazy programmer

# Gameplay videos
Don't expect high quality, I compressed the videos.

**SCP-575 disappears due to being in a lighted room**

https://user-images.githubusercontent.com/36207738/213876283-ebdc666a-b313-421a-be9a-a52a524b667c.mp4

**SCP-575 chasing and killing his victim**

https://user-images.githubusercontent.com/36207738/213876270-b5333790-a3ed-462a-9e48-809bb9b982d5.mp4

**SCP-575 disappears due to being pointed at with a flashlight for too long**

https://user-images.githubusercontent.com/36207738/213876508-e25d35c8-0a54-4613-8634-2ecb53d6b7e2.mp4

# Configuration

Default configuration

```yml
# Is the plugin enabled ?
is_enabled: true
# Enable the Logs.Debug of light points and other logs.
debug: false
# Enable the Logs.Debug of SCPSLAudioApi, warning can be very spammy.
audio_debug: false
# Does the blackout affect Entrance Zone ?
active_in_entrance: false
# Does the blackout affect Heavy Contaiment ?
active_in_heavy: false
# Does the blackout affect Light Contaiment ?
active_in_light: true
# The per-round probability of SCP-575 appearing
spawn_chance: 40
# If there is an SCP-173 in the round, SCP-575 will deactivate for that round.
disable_for_scp173: false
# An alternative to the above is this, that when there is an SCP-173 in the Light Containment zone round it will never blackout. YOU CANNOT ACTIVATE BOTH OPTIONS THE PLUGIN WILL BE BROKEN
disable_blackout_for_scp173: false
# All blackout related configuration
black_out:
# After this time, the constant blackouts will begin to be executed.
  initial_delay: 300
  # If this value is true initial_delay will be ignored and a calculation will be made between initial_max_delay and initial_min_delay which will result in the delay
  random_initial_delay: false
  # The maximum time that the main delay can have
  initial_max_delay: 250
  # The minimun time that the main delay can have
  initial_min_delay: 190
  # The minimum duration of a blackout
  min_duration: 30
  # The maximum duration of a blackout
  max_duration: 90
  # The minimum duration of a delay after a blackout
  min_delay: 180
  # The minimum duration of a delay after a blackout
  max_delay: 400
  # Before starting the blackout Cassie will say this message
  cassie_message: facility power system failure in 3 . pitch_.80 2 . pitch_.60 1 . pitch_.49 . .g1 pitch_.42  .g2 pitch_.31  .g5
  # I have no idea what it does
  cassie_is_hold: false
  # Enable o disable bells in cassie announcement
  cassie_is_noise: true
  # After making Cassie's announcement the blackout will start after these seconds, perfect to turn off the lights just when the announcement ends.
  delay_after_cassie: 8.5
  # List of rooms where the light will not turn off, the SCP-575 will disappear if you touch these rooms for 5 seconds. If you want a list of Rooms see the Readme of the plugin repository
  black_list_rooms:
  - Lcz914
  - LczArmory
  - LczCheckpointA
  - LczCheckpointB
  - HczArmory
  - HczCheckpointA
  - HczCheckpointB
  - EzGateA
  - EzGateB
# All configuration related to the SCP-575
scp575:
# The name the dummy will have
  nickname: SCP-575-B
  # The distance at which players can see the name of the SCP-575 | The game default value is 10
  view_range: 12
  # Set the SCP-575 role, by default is SCP-106
  role_type: Scp106
  # The death message that will appear when players are killed by SCP-575
  kill_feed: Devoured by SCP-575
  # The broadcast that will be sent to the player when killed by SCP-575
  broadcast_kill: You were eaten by SCP-575, aim with a lit flashlight next time
  # The duration of the broadcast, if you want to disable it, set the duration to 0
  broadcast_duration: 10
  # Should SCP-575 play the sounds files found in its folder? | The sound file must be .ogg need to be mono channel and have a frequency of 48000 Hz
  play_sounds: false
  # The volume of the sound to be reproduced by the SCP-575, high values violate the VSR.
  sound_volume: 85
  # Activating this will cause the SCP-575 to spawn with a delay where it will not be able to move or kill.
  delay_on_chase: true
  # Delay duration where SCP-575 will not be able to do anything
  delay_chase: 1.5
  # The maximum distance that SCP-575 can be from its victim, remember that it must be greater than 16
  max_distance: 28
  # If the distance is equal to or greater than this value, the speed that is movement_speed_fast will be applied to the SCP-575.
  medium_distance: 16
  # If the distance is greater than this value the value of movement_speed will be applied to the SCP-575.
  min_distance: 0.800000012
  # If the distance between the target and the SCP-575 is less than this value, the target will die. Note that if you modify this value you will have to do it with min_distance as well.
  kill_distance: 0.800000012
  # If the distance between SCP-575 and its victim is equal to or greater than 16, it will have this movement speed
  movement_speed_fast: 29
  # If the distance between SCP-575 and its victim is equal to or greater than 5, it will have this movement speed
  movement_speed: 22
  # This is complicated to explain, so I'll just tell you what I do in the code. If a player has a flashlight on and points it at SCP-575 I fire a ray of light that if it touches SCP-575 adds a point of light, when it reaches a certain point of light SCP-575 disappears. The coroutine that checks these points is executed every 0.1s.
  light_points: 85
  # When a player makes SCP-575 disappear using the LightPoints, this message will be sent to the player.
  light_point_kill_message: SCP-575 disappears for now

```

**Rooms**

```md
- Unnamed
- LczClassDSpawn
- LczComputerRoom
- LczCheckpointA
- LczCheckpointB
- LczToilets
- LczArmory
- Lcz173
- LczGlassroom
- Lcz330
- Lcz914
- LczGreenhouse
- LczAirlock
- HczCheckpointToEntranceZone
- HczCheckpointA
- HczCheckpointB
- HczWarhead
- Hcz049
- Hcz079
- Hcz096
- Hcz106
- Hcz939
- HczMicroHID
- HczArmory
- HczServers
- HczTesla
- EzCollapsedTunnel
- EzGateA
- EzGateB
- EzRedroom
- EzEvacShelter
- EzIntercom
- EzOfficeStoried
- EzOfficeLarge
- EzOfficeSmall
- Outside
- Pocket
- HczTestroom

```

# Credits

The image used belongs to https://lselden.github.io/scp-to-epub/pixel-art-collab/scp-575.xhtml

The transpilers belong to [Jesus-QC](https://github.com/Jesus-QC)

# If you use any part of the code to create your own version of this plugin or pets, please remember to credit the original author :)
