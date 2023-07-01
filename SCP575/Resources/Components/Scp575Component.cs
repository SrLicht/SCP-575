using InventorySystem.Items.Firearms;
using InventorySystem.Items.Flashlight;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP575.Resources.Components
{
    [RequireComponent(typeof(ReferenceHub))]
    public class Scp575Component : MonoBehaviour
    {
        /// <summary>
        /// SCP-575 ReferenceHub.
        /// </summary>
        private ReferenceHub ReferenceHub { get; set; }

        /// <summary>
        /// The victim that SCP-575 will be pursuing
        /// </summary>
        public Player Victim { get; set; }

        /// <summary>
        /// MEc coroutine that handles follow of the victim.
        /// </summary>
        private CoroutineHandle _followCoroutine;

        /// <summary>
        /// MEc coroutine that handles checks.
        /// </summary>
        private CoroutineHandle _checksCoroutine;

        private void Awake()
        {
            var _hub = gameObject.GetComponent<ReferenceHub>();

            if (_hub == null)
            {
                Destroy(this);
            }

            ReferenceHub = _hub;
            _firstSpawn = true;
            _fpcScp575 = (IFpcRole)ReferenceHub.roleManager.CurrentRole;
            _delayChase = Scp575.Instance.Config.Scp575.DelayOnChase;
            _followCoroutine = Timing.RunCoroutine(Follow().CancelWith(this).CancelWith(gameObject));
            _checksCoroutine = Timing.RunCoroutine(Checks());
        }

        /// <summary>
        /// Move SCP-575 camera.
        /// </summary>
        private void Update()
        {
            var mouseLook = ((IFpcRole)ReferenceHub.roleManager.CurrentRole).FpcModule.MouseLook;
            var eulerAngles = Quaternion.LookRotation(Victim.Position - Position, Vector3.up).eulerAngles;
            mouseLook.CurrentHorizontal = eulerAngles.y;
            mouseLook.CurrentVertical = eulerAngles.x;
        }

        private IEnumerator<float> Follow()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(0.1f);

                // If the room where the SCP-575 is located is illuminated, it disappears.
                if (_roomIsIlluminated) Destroy();

                if (_delayChase)
                {
                    yield return Timing.WaitForSeconds(Scp575.Instance.Config.Scp575.DelayChase);
                    _delayChase = false;
                }

                // If the player has a flashlight or a weapon with a flashlight, light points are earned.
                if (Victim.CurrentItem != null && Victim.CurrentItem is FlashlightItem { IsEmittingLight: true } ||
                    Victim.CurrentItem != null && Victim.CurrentItem is Firearm { IsEmittingLight: true })
                {
                    if (Physics.Raycast(Victim.Camera.position, Victim.Camera.transform.forward, out var hit) &&
                        hit.collider.transform.root.gameObject == ReferenceHub.gameObject)
                    {
                        _lightPoints++;
                        if (_lightPoints >= Scp575.Instance.Config.Scp575.LightPoints)
                        {
                            Victim.ReceiveHint(Scp575.Instance.Config.Scp575.LightPointKillMessage, 5f);
                            Destroy();
                        }
                    }
                }

                if (_firstSpawn)
                {
                    // Wait for SCP-575 to spawn completely and be in the player's room.
                    yield return Timing.WaitForSeconds(0.8f);
                    _firstSpawn = false;
                }

                var distance = Vector3.Distance(Victim.Position, Position);

                if (_fpcScp575 != null)
                {
                    if (distance >= Scp575.Instance.Config.Scp575.MaxDistance) Destroy();
                    else if (distance >= Scp575.Instance.Config.Scp575.MediumDistance)
                    {
                        var directionFast = Victim.Position - Position;
                        directionFast = directionFast.normalized;
                        var velocityFast = directionFast * Scp575.Instance.Config.Scp575.MovementSpeedFast;
                        _fpcScp575.FpcModule.CharController.Move(velocityFast * Time.deltaTime);
                    }
                    else if (distance > Scp575.Instance.Config.Scp575.MinDistance)
                    {
                        var directionNormal = Victim.Position - Position;
                        directionNormal = directionNormal.normalized;
                        var velocityNormal = directionNormal * Scp575.Instance.Config.Scp575.MovementSpeed;
                        _fpcScp575.FpcModule.CharController.Move(velocityNormal * Time.deltaTime);
                    }
                    else if (distance <= Scp575.Instance.Config.Scp575.KillDistance)
                    {
                        Victim.Kill(Scp575.Instance.Config.Scp575.KillFeed);
                        if (Scp575.Instance.Config.Scp575.BroadcastDuration > 0)
                            Victim.SendBroadcast(Scp575.Instance.Config.Scp575.BroadcastKill,
                                Scp575.Instance.Config.Scp575.BroadcastDuration);

                        Log.Info($"SCP-575 kill player {Victim.Nickname} ({Victim.UserId})");
                        Destroy();
                    }
                }
                else
                {
                    Destroy();
                }
            }
        }

        /// <summary>
        /// It is in charge of updating variables and checking if the player that is being followed is alive.
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> Checks()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(5.0f);

                if (!Victim.IsAlive) Destroy();

                cachedScp575Room = Scp575Room;
                _roomIsIlluminated = Extensions.IsRoomIlluminated(cachedScp575Room);
            }
        }

        /// <summary>
        /// Called when destroying the component
        /// </summary>
        private void OnDestroy()
        {
            var dummyPlayer = Dummies.DummiesPlayers.FirstOrDefault(d => d.ReferenceHub == ReferenceHub);

            dummyPlayer?.StopAudio();

            Timing.CallDelayed(0.5f, () =>
            {
                Dummies.DestroyDummy(ReferenceHub);
            });
        }

        /// <summary>
        /// Public method to destroy this component.
        /// </summary>
        public void Destroy()
        {
            Destroy(this, 0.1f);
        }

        /// <summary>
        /// Method that destroys the component after a certain period of time.
        /// </summary>
        /// <param name="destroyTimer">Duration of active component (in seconds)</param>
        public void Destroy(float destroyTimer)
        {
            Log.Debug($"Calling Destroy in {destroyTimer}", Scp575.Instance.Config.Debug);
            Destroy(this, destroyTimer);
        }

        #region API and private variables

        // I dont want to use NWAPI for the NCP soo..

        private int _lightPoints = 0;

        private bool _firstSpawn = false;

        private IFpcRole _fpcScp575;

        private bool _delayChase = false;

        /// <summary>
        /// Get or set SCP-575 nickname.
        /// </summary>
        public string Nickname
        {
            get => ReferenceHub.nicknameSync._myNickSync;
            set => ReferenceHub.nicknameSync.MyNick = value;
        }

        /// <summary>
        /// Obtain the current room of SCP-575
        /// </summary>
        private RoomIdentifier Scp575Room => RoomIdUtils.RoomAtPosition(Position);

        /// <summary>
        /// To avoid killing the server by making calls every Frame, I will save the last room in a cache that refreshes every 5 seconds.
        /// </summary>
        public RoomIdentifier cachedScp575Room;

        /// <summary>
        /// Every 5 seconds I will update if the current room has the lights on
        /// </summary>
        private bool _roomIsIlluminated = false;

        /// <summary>
        /// Get or set SCP-575 god mode.
        /// </summary>
        public bool GodMode
        {
            get => ReferenceHub.characterClassManager.GodMode;
            set => ReferenceHub.characterClassManager.GodMode = value;
        }

        /// <summary>
        /// Get or set SCP-575 current scale.
        /// </summary>
        public Vector3 Scale
        {
            get => ReferenceHub.transform.localScale;
            set
            {
                try
                {
                    ReferenceHub.transform.localScale = value;
                    foreach (Player target in Player.GetPlayers())
                    {
                        Extensions.SendSpawnMessage?.Invoke(null,
                            new object[] { ReferenceHub.networkIdentity, target.Connection });
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"{nameof(Scale)} error: {e}");
                }
            }
        }

        /// <summary>
        /// Get or set SCP-575 position.
        /// </summary>
        public Vector3 Position
        {
            get => ReferenceHub.gameObject.transform.position;
            set => ReferenceHub.TryOverridePosition(value, Vector3.zero);
        }

        /// <summary>
        /// Get or set SCP-575 role.
        /// </summary>
        public RoleTypeId Role
        {
            get => ReferenceHub.GetRoleId();

            set => ReferenceHub.roleManager.ServerSetRole(value, RoleChangeReason.RemoteAdmin);
        }

        #endregion
    }
}