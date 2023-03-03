using SCPSLAudioApi.AudioCore;
using VoiceChat;

namespace SCP575.Resources
{
    public class Scp575AudioPlayer : AudioPlayerBase
    {
        public static Scp575AudioPlayer Get(ReferenceHub hub)
        {
            if (AudioPlayers.TryGetValue(hub, out AudioPlayerBase player))
            {
                if (player is Scp575AudioPlayer scp575Player1)
                    return scp575Player1;
            }

            var scp575Player = hub.gameObject.AddComponent<Scp575AudioPlayer>();
            scp575Player.Owner = hub;
            scp575Player.BroadcastChannel = VoiceChatChannel.Proximity;

            AudioPlayers.Add(hub, scp575Player);
            return scp575Player;
        }
    }
}