using InventorySystem.Items;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using PluginAPI.Core;
using SCPSLAudioApi.AudioCore;
using System;
using UnityEngine;
using VoiceChat;

namespace SCP575.API.Features
{
    /// <summary>
    /// Dummy player.
    /// </summary>
    public class DummyPlayer
    {
        private ReferenceHub _hub;
        private int _id;

        /// <summary>
        /// Initialize a new instance of <see cref="DummyPlayer"/>.
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="id"></param>
        public DummyPlayer(ReferenceHub hub, int id)
        {
            _hub = hub;
            _id = id;
            AudioPlayerBase = AudioPlayerBase.Get(ReferenceHub);

            if (AudioPlayerBase is null)
                Log.Warning($"AudioPlayerBase is null at creating DummyPlayer");

            //Dummies.AllDummyPlayers.Add(this);
        }

        /// <summary>
        /// Gets dummy ReferenceHub.
        /// </summary>
        public ReferenceHub ReferenceHub => _hub;

        /// <summary>
        /// Gets the <see cref="SCPSLAudioApi.AudioCore.AudioPlayerBase"/> of the dummy.
        /// </summary>
        public AudioPlayerBase? AudioPlayerBase;

        /// <summary>
        /// Get dummy id.
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Gets dummy UserId
        /// </summary>
        public string UserId
        {
            get => ReferenceHub.authManager._privUserId;
        }

        /// <summary>
        /// Gets or sets if the dummy is visible in the player list.
        /// </summary>
        public bool VisibleOnPlayerList
        {
            get => ReferenceHub.authManager.NetworkSyncedUserId != null;
            set => ReferenceHub.authManager.NetworkSyncedUserId = value ? ReferenceHub.authManager._privUserId : null;
        }

        /// <summary>
        /// Gets dummy <see cref="UnityEngine.GameObject"/>
        /// </summary>
        public GameObject GameObject => ReferenceHub.gameObject;

        /// <summary>
        /// Gets dummy <see cref="PlayerRoleManager"/>
        /// </summary>
        public PlayerRoleManager RoleManager => ReferenceHub.roleManager;

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
        /// Gets dummy network id.
        /// </summary>
        public uint NetworkId => ReferenceHub.characterClassManager.netId;

        /// <summary>
        /// Gets dummy <see cref="PlayerRoleBase"/>.
        /// </summary>
        public PlayerRoleBase RoleBase => ReferenceHub.roleManager.CurrentRole;

        /// <summary>
        /// Gets or sets the dummy current health;
        /// </summary>
        public float Health
        {
            get => ReferenceHub.playerStats.GetModule<HealthStat>().CurValue;
            set => ReferenceHub.playerStats.GetModule<HealthStat>().CurValue = value;
        }

        /// <summary>
        /// Gets the dummy current maximum health;
        /// </summary>
        public float MaxHealth => ReferenceHub.playerStats.GetModule<HealthStat>().MaxValue;

        /// <summary>
		/// Gets or sets the dummiy current artificial health.
		/// </summary>
		public float ArtificialHealth
        {
            get => IsSCP ? ReferenceHub.playerStats.GetModule<HumeShieldStat>().CurValue : ReferenceHub.playerStats.GetModule<AhpStat>().CurValue;
            set
            {
                if (IsSCP)
                {
                    ReferenceHub.playerStats.GetModule<HumeShieldStat>().CurValue = value;
                    return;
                }

                ReferenceHub.playerStats.GetModule<AhpStat>().CurValue = value;
            }
        }

        /// <summary>
		/// Gets or sets the item in the dummy hand, returns the default value if empty.
		/// </summary>
		public ItemBase? CurrentItem
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
		/// Gets dummy current room.
		/// </summary>
		public RoomIdentifier Room => RoomIdUtils.RoomAtPosition(GameObject.transform.position);

        /// <summary>
		/// Get dummy current zone.
		/// </summary>
		public FacilityZone Zone => Room?.Zone ?? FacilityZone.None;

        /// <summary>
		/// Gets or sets dummy group role color.
		/// </summary>
		public string RoleColor
        {
            get => ReferenceHub.serverRoles.Network_myColor;
            set => ReferenceHub.serverRoles.SetColor(value);
        }

        /// <summary>
        /// Gets or sets dummy group role text.
        /// </summary>
        public string RoleName
        {
            get => ReferenceHub.serverRoles.Network_myText;
            set => ReferenceHub.serverRoles.SetText(value);
        }

        /// <summary>
		/// Gets whether or not the dummy has god mode enabled.
		/// </summary>
		public bool IsGodModeEnabled
        {
            get => ReferenceHub.characterClassManager.GodMode;
            set => ReferenceHub.characterClassManager.GodMode = value;
        }

        /// <summary>
        /// Gets whether or not the dummy has noclip enabled.
        /// </summary>
        public bool IsNoclipEnabled
        {
            get => ReferenceHub.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip);
            set => ReferenceHub.playerStats.GetModule<AdminFlagsStat>().SetFlag(AdminFlags.Noclip, value);
        }

        /// <summary>
		/// Gets dummy role team.
		/// </summary>
		public Team Team => Role.GetTeam();

        /// <summary>
        /// Gets if the dummy is SCP.
        /// </summary>
        public bool IsSCP => Role.GetTeam() is Team.SCPs;

        /// <summary>
        /// Gets whether or not the dummy is human.
        /// </summary>
        public bool IsHuman => ReferenceHub.IsHuman();

        /// <summary>
		/// Gets or sets the player's position.
		/// </summary>
		public Vector3 Position
        {
            get => GameObject.transform.position;
            set => ReferenceHub.TryOverridePosition(value, Vector3.zero);
        }

        /// <summary>
        /// Gets or sets player's rotation.
        /// </summary>
        public Vector3 Rotation
        {
            get => GameObject.transform.eulerAngles;
            set => ReferenceHub.TryOverridePosition(Position, value);
        }

        /// <summary>
        /// Stop playing any audio.
        /// </summary>
        public void StopAudio()
        {
            AudioPlayerBase?.Stoptrack(true);
        }

        /// <summary>
        /// Add an audio to the audio queue
        /// </summary>
        public void QueueAudio(string filepath)
        {
            AudioPlayerBase?.Enqueue(filepath, -1);
        }

        /// <summary>
        /// Play an audio through the dummy using voice chat
        /// </summary>
        /// <param name="filepath">Audio to be played</param>
        /// <param name="channel">On which channel the dummy will speak</param>
        /// <param name="volume">The volume of the audio to be played by the dummy.</param>
        /// <param name="player">If this is not null, only this player will be able to hear the audio played.</param>
        /// <param name="audioIsLooped"></param>
        /// <param name="clearQueue"></param>
        public void PlayAudio(string filepath, VoiceChatChannel channel = VoiceChatChannel.Proximity, float volume = 85, Player? player = null, bool audioIsLooped = false, bool clearQueue = true)
        {
            AudioPlayerBase ??= AudioPlayerBase.Get(ReferenceHub);

            StopAudio(); // just in case

            AudioPlayerBase.BroadcastChannel = channel;
            AudioPlayerBase.Volume = volume;

            if (clearQueue)
                AudioPlayerBase.BroadcastTo.Clear();

            if (player != null)
                AudioPlayerBase.BroadcastTo.Add(player.PlayerId);

            AudioPlayerBase.Loop = audioIsLooped;
            AudioPlayerBase.Enqueue(filepath, 0);
            AudioPlayerBase.Play(0);
        }
    }
}
