using CommandSystem;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using PluginAPI.Core;
using SCP575.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCP575.Command
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class Scp575Command : ICommand, IUsageProvider
    {
        public string Command => "scp575";

        public string[] Aliases => new string[] { "575" };

        public string Description => "This command allows you to spawn instances of SCP-575.";

        public string[] Usage { get; } = {"Player ID", "Duration"};

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if(sender != null)
            {
                if (!Round.IsRoundStarted)
                {
                    response = Scp575.Instance.Config.CommandResponses.RoundHasNotStarted;
                    return false;
                }
                // Display help response.
                if(arguments.Count < 1 || arguments.IsEmpty() || arguments.Count > 2)
                {
                    var text = string.Format(Scp575.Instance.Config.CommandResponses.HelpResponse, this.DisplayCommandUsage());
                    response = text;
                    return false;
                }

                if(!int.TryParse(arguments.At(0), out int playerId))
                {
                    response = string.Format(Scp575.Instance.Config.CommandResponses.InvalidPlayerId, arguments.At(0));
                    return false;
                }

                var victim = Player.Get(playerId);
                if(victim is null)
                {
                    response = Scp575.Instance.Config.CommandResponses.PlayerNotFound;
                    return false;
                }

                if(!int.TryParse(arguments.At(1), out var duration))
                {
                    response = string.Format(Scp575.Instance.Config.CommandResponses.InvalidDuration, arguments.At(1));
                    return false;
                }

                SpawnScp575(victim, duration);

                response = string.Format(Scp575.Instance.Config.CommandResponses.Spawning, victim.Nickname);
                return true;
            }
            else
            {
                response = "Sender is null";
                return false;
            }
        }

        public void SpawnScp575(Player victim, float duration)
        {
            var scp575 = Dummies.CreateDummy("Scp575", "SCP-575");

            scp575.ReferenceHub.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;

            scp575.ReferenceHub.nicknameSync.ViewRange = Scp575.Instance.Config.Scp575.ViewRange;

            scp575.Nickname = Scp575.Instance.Config.Scp575.Nickname;

            scp575.Role = Scp575.Instance.Config.Scp575.RoleType;

            scp575.IsGodModeEnabled = true;

            Timing.CallDelayed(0.3f, () =>
            {
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

            var comp = scp575.GameObject.AddComponent<Resources.Components.Scp575Component>();
            comp.Victim = victim;
            comp.Destroy(duration);

            if (!Scp575.Instance.Config.Scp575.PlaySounds) return;
            if (!Extensions.AudioFileExist()) Log.Error($"There is no .ogg file in the folder {Scp575.Instance.AudioPath}");
            var audioPlayer = DummyAudioPlayer.Get(scp575.ReferenceHub);
            var audioFile = Extensions.GetAudioFilePath();
            audioPlayer.Enqueue(audioFile, -1);
            audioPlayer.LogDebug = Scp575.Instance.Config.AudioDebug;
            //This will cause only the victim to be able to hear the music.
            audioPlayer.BroadcastTo.Add(victim.PlayerId);
            audioPlayer.Volume = Scp575.Instance.Config.Scp575.SoundVolume;
            audioPlayer.Play(0);
            Log.Debug($"Playing sound {audioFile}", Scp575.Instance.Config.Debug);
        }
    }
}
