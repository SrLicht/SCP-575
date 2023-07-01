using InventorySystem.Items;
using MapGeneration;
using PlayerRoles.FirstPersonControl;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoiceChat;

namespace SCP575.Resources
{
    public class DummyPlayer
    {
        private ReferenceHub _hub;
        private int _id;

        public DummyPlayer(ReferenceHub hub, int id)
        {
            _hub = hub;
            _id = id;
            AudioPlayerBase = AudioPlayerBase.Get(ReferenceHub);
            Dummies.DummiesPlayers.Add(this);
        }

        /// <summary>
        /// Gets dummy ReferenceHub
        /// </summary>
        public ReferenceHub ReferenceHub => _hub;


        /// <summary>
        /// Gets <see cref="SCPSLAudioApi.AudioCore.AudioPlayerBase"/> of the dummy.
        /// </summary>
        public readonly AudioPlayerBase AudioPlayerBase;

        /// <summary>
        /// Get dummy id.
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Gets dummy <see cref="UnityEngine.GameObject"/>
        /// </summary>
        public GameObject GameObject => ReferenceHub.gameObject;

        /// <summary>
        /// Gets dummy <see cref="PlayerRoleManager"/>
        /// </summary>
        public PlayerRoleManager RoleManager => ReferenceHub.roleManager;

        /// <summary>
        /// Gets dummy transform
        /// </summary>
        public Transform Transform => ReferenceHub.transform;

        /// <summary>
        /// Gets or sets dummy nickname.
        /// </summary>
        public string Nickname
        {
            get => ReferenceHub.nicknameSync.MyNick;
            set
            {
                try
                {
                    ReferenceHub.nicknameSync.SetNick(value);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Gets or sets the dummy display name.
        /// </summary>
        public string DisplayNickname
        {
            get => ReferenceHub.nicknameSync.DisplayName;
            set => ReferenceHub.nicknameSync.DisplayName = value;
        }


        /// <summary>
        /// Gets or sets the dummy current role.
        /// </summary>
        public RoleTypeId Role
        {
            get => ReferenceHub.GetRoleId();
            set => ReferenceHub.roleManager.ServerSetRole(value, RoleChangeReason.RemoteAdmin);
        }


        /// <summary>
        /// Gets dummy <see cref="PlayerRoleBase"/>.
        /// </summary>
        public PlayerRoleBase RoleBase => ReferenceHub.roleManager.CurrentRole;


        /// <summary>
        /// Gets or sets the dummy current health;
        /// </summary>
        public float Health
        {
            get => ((HealthStat)ReferenceHub.playerStats.StatModules[0]).CurValue;
            set => ((HealthStat)ReferenceHub.playerStats.StatModules[0]).CurValue = value;
        }


        /// <summary>
        /// Gets the dummy current maximum health;
        /// </summary>
        public float MaxHealth => ((HealthStat)ReferenceHub.playerStats.StatModules[0]).MaxValue;


        /// <summary>
        /// Gets or sets the item in the dummy hand, returns the default value if empty.
        /// </summary>
        public ItemBase CurrentItem
        {
            get => ReferenceHub.inventory.CurInstance;
            set
            {
                if (value == null || value.ItemTypeId == ItemType.None)
                    ReferenceHub.inventory.ServerSelectItem(0);
                else
                    ReferenceHub.inventory.ServerSelectItem(value.ItemSerial);
            }
        }

        /// <summary>
		/// Get dummy current room.
		/// </summary>
		public RoomIdentifier Room => RoomIdUtils.RoomAtPosition(GameObject.transform.position);


        /// <summary>
		/// Get player current zone.
		/// </summary>
		public FacilityZone Zone => Room?.Zone ?? FacilityZone.None;


        /// <summary>
        /// Gets whether or not the player has god mode.
        /// </summary>
        public bool IsGodModeEnabled
        {
            get => ReferenceHub.characterClassManager.GodMode;
            set => ReferenceHub.characterClassManager.GodMode = value;
        }


        /// <summary>
        /// Gets whether or not the dummy has noclip.
        /// </summary>
        public bool IsNoclipEnabled
        {
            get => ReferenceHub.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip);
            set => ReferenceHub.playerStats.GetModule<AdminFlagsStat>().SetFlag(AdminFlags.Noclip, value);
        }


        /// <summary>
        /// Get dummy team.
        /// </summary>
        public Team Team => Role.GetTeam();

        /// <summary>
		/// Gets or sets the dummy position.
		/// </summary>
		public Vector3 Position
        {
            get => GameObject.transform.position;
            set => ReferenceHub.TryOverridePosition(value, Vector3.zero);
        }


        /// <summary>
        /// Gets or sets dummy rotation.
        /// </summary>
        public Vector3 Rotation
        {
            get => GameObject.transform.eulerAngles;
            set => ReferenceHub.TryOverridePosition(Position, value);
        }


        /// <summary>
        /// Gets or sets dummy remaining stamina (min = 0, max = 1).
        /// </summary>
        public float StaminaRemaining
        {
            get => ReferenceHub.playerStats.StatModules[2].CurValue;
            set => ReferenceHub.playerStats.StatModules[2].CurValue = value;
        }

        /// <summary>
        /// Stop playing any audio.
        /// </summary>
        public void StopAudio()
        {
            AudioPlayerBase.Stoptrack(true);
        }

        /// <summary>
        /// Add an audio to the audio queue
        /// </summary>
        public void QueueAudio(string filepath)
        {
            AudioPlayerBase.Enqueue(filepath, -1);
        }

        /// <summary>
        /// Play an audio through the dummy using voice chat
        /// </summary>
        /// <param name="filepath">Audio to be played</param>
        /// <param name="channel">On which channel the dummy will speak</param>
        /// <param name="volume">The volume of the audio to be played by the dummy.</param>
        /// <param name="player">If this is not null, only this player will be able to hear the audio played.</param>
        public void PlayAudio(string filepath, VoiceChatChannel channel = VoiceChatChannel.Proximity, float volume = 85, Player player = null)
        {
            StopAudio(); // just in case
            AudioPlayerBase.BroadcastChannel = channel;
            AudioPlayerBase.Volume = volume;
            if (player != null)
                AudioPlayerBase.BroadcastTo.Add(player.PlayerId);

            AudioPlayerBase.Enqueue(filepath, 0);
            AudioPlayerBase.Play(0);
        }
    }
}
