using MapGeneration;
using MEC;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCP575.API.Extensions;
using SCP575.API.Features;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCP575.API.Components
{
    /// <summary>
    /// SCP-575 main componente that manage chasing the player and kill it.
    /// </summary>
    public class ChaseComponent : MonoBehaviour
    {
        private ReferenceHub? ReferenceHub;

        private DummyPlayer DummyPlayer = null!;

        /// <summary>
        /// Gets the current chased player.
        /// </summary>
        public Player Player = null!;

        /// <summary>
        /// Coroutine Handlers.
        /// </summary>
        private CoroutineHandle _followCoroutine, _checkCoroutine;

        private void Start()
        {
            try
            {
                ReferenceHub = GetComponent<ReferenceHub>();

                if (ReferenceHub is null)
                {
                    Log.Debug($"{nameof(Start)}: Reference hub is null", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                    Destroy(this);
                    return;
                }

                if (Player is null)
                {
                    Destroy(this);
                    return;
                }

                var dummy = Dummies.GetDummy(ReferenceHub);

                if (dummy is null)
                {
                    Log.Debug($"{nameof(Start)}: Dummy player is null", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                    Destroy(this);
                    return;
                }

                DummyPlayer = dummy;

                _followCoroutine = Timing.RunCoroutine(Chase().CancelWith(this).CancelWith(gameObject));
                _checkCoroutine = Timing.RunCoroutine(Checks().CancelWith(this).CancelWith(gameObject));
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(Start)}: {e}");
                Destroy(this);
            }
        }

        /// <summary>
        /// Called when destroying the component
        /// </summary>
        private void OnDestroy()
        {
            if (EntryPoint.Instance.Config.BlackOut.EndBlackoutWhenDisappearing)
                BlackoutExtensions.EndBlackout();

            Dummies.DestroyDummy(DummyPlayer);

            Log.Debug($"{nameof(OnDestroy)}: SCP-575 fully destroyed.", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
        }

        /// <summary>
        /// Move SCP-575 camera.
        /// </summary>
        private void Update()
        {
            if (ReferenceHub is null)
                return;

            ((IFpcRole)ReferenceHub.roleManager.CurrentRole).FpcModule.MouseLook.LookAtDirection(Player.Camera.position - DummyPlayer.Position);
        }

        /// <summary>
        /// Disables the MonoBehaviour by scheduling its destruction after a specified delay.
        /// </summary>
        /// <param name="delay">The delay, in seconds, before the MonoBehaviour is destroyed.</param>
        public void Disable(float delay = 0f)
        {

            if (delay > 0f)
            {
                // Log a debug message if debug mode is enabled.
                Log.Debug($"Calling Destroy in {delay} seconds", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);

                Timing.CallDelayed(delay, () =>
                {
                    _destroyed = true;
                    // Schedule the destruction of the MonoBehaviour after the specified delay.
                    Destroy(this);
                });

                return;
            }

            _destroyed = true;

            Destroy(this);
        }

        private IEnumerator<float> Chase()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(.1f);

                if (_destroyed)
                {
                    yield break;
                }

                if (_roomIsIlluminated)
                {
                    Log.Debug($"{nameof(Chase)}: Room is illuminated, destroying SCP575.", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                    // If the room is illuminated, disable SCP-575.
                    Disable();
                    yield break;
                }

                // If there's a delayed chase, wait for the specified delay.
                if (_delayedChase)
                {
                    Log.Debug($"{nameof(Chase)}: delay chase is true, delaying chase for {EntryPoint.Instance.Config.Scp575.DelayChase} seconds.", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                    yield return Timing.WaitForSeconds(EntryPoint.Instance.Config.Scp575.DelayChase);
                    _delayedChase = false;
                }

                // Initialize roles and other properties on the first spawn.
                InitializeOnFirstSpawn();

                // Calculate the distance between the player and the dummy.
                float distance = Vector3.Distance(Player.Position, DummyPlayer.Position);

                // Call private methods based on the calculated distance.
                HandleChase(distance);
            }
        }

        private IEnumerator<float> Checks()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(5.0f);


                if (_destroyed)
                {
                    yield break;
                }

                if (!Player.IsAlive)
                    Disable();

                cachedScp575Room = DummyPlayer.Room;
                _roomIsIlluminated = BlackoutExtensions.IsRoomIlluminated(cachedScp575Room);
            }
        }

        /// <summary>
        /// Initializes roles and other properties on the first spawn of the SCP-575.
        /// </summary>
        private void InitializeOnFirstSpawn()
        {
            if (_firstSpawn)
            {
                Log.Debug($"{nameof(InitializeOnFirstSpawn)}: Caching fpc roles for the victim and the SCP-575.", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                victimFpc = (IFpcRole)Player.ReferenceHub.roleManager.CurrentRole;
                dummyFpc = (IFpcRole)DummyPlayer.ReferenceHub.roleManager.CurrentRole;
                _firstSpawn = false;
                Timing.WaitForSeconds(0.8f);
            }
        }

        /// <summary>
        /// Handles the SCP-575 chase logic based on the distance between the player and the dummy.
        /// </summary>
        /// <param name="distance">The distance between the player and the dummy.</param>
        private void HandleChase(float distance)
        {
            if (distance >= EntryPoint.Instance.Config.Scp575.MaxDistance)
            {
                Log.Debug($"{nameof(HandleChase)}: Max distance reached, destroying SCP-575.", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                // If the dummy is too far, stop chasing.
                Disable(3);
            }
            else if (distance >= EntryPoint.Instance.Config.Scp575.MediumDistance)
            {
                if (EntryPoint.Instance.Config.Scp575.ChangeMovementSpeedIfRun && victimFpc != null && victimFpc.FpcModule.CurrentMovementState == PlayerMovementState.Sprinting)
                {
                    MoveTowardsDummy(EntryPoint.Instance.Config.Scp575.MovementSpeedRunning);

                    return;
                }

                MoveTowardsDummy(EntryPoint.Instance.Config.Scp575.MovementSpeedFast);
            }
            else if (distance > EntryPoint.Instance.Config.Scp575.MinDistance)
            {
                MoveTowardsDummy(EntryPoint.Instance.Config.Scp575.MovementSpeed);
            }
            else if (distance <= EntryPoint.Instance.Config.Scp575.KillDistance)
            {
                Log.Debug($"{nameof(HandleChase)}: Kill distance reached, deboning the victim.", EntryPoint.Instance.Config.DebugMode, EntryPoint.Prefix);
                KillPlayer();
            }
        }

        /// <summary>
        /// Moves SCP-575 towards the dummy with the specified movement speed.
        /// </summary>
        /// <param name="movementSpeed">The movement speed of SCP-575.</param>
        private void MoveTowardsDummy(float movementSpeed)
        {
            var direction = Player.Position - DummyPlayer.Position;
            direction = direction.normalized;
            var velocity = direction * movementSpeed;
            dummyFpc.FpcModule.CharController.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Kills the player and performs associated actions.
        /// </summary>
        private void KillPlayer()
        {
            // Kill the player.
            Player.Kill(EntryPoint.Instance.Config.Scp575.KillFeed);

            // Broadcast kill message if configured.
            if (EntryPoint.Instance.Config.Scp575.BroadcastDuration > 0)
            {
                Player.SendBroadcast(EntryPoint.Instance.Config.Scp575.BroadcastKill, EntryPoint.Instance.Config.Scp575.BroadcastDuration);
            }

            // Log the kill information.
            Log.Info($"SCP-575 killed player {Player.LogName}", EntryPoint.Prefix);

            // Destroy SCP-575 or perform any other cleanup actions.
            Disable();
        }

        /// <summary>
        /// To avoid killing the server by making calls every Frame, I will save the last room in a cache that refreshes every 5 seconds.
        /// </summary>
        private RoomIdentifier? cachedScp575Room;

        /// <summary>
        /// Every 5 seconds I will update if the current room has the lights on
        /// </summary>
        private bool _roomIsIlluminated = false;

        private bool _firstSpawn = true, _delayedChase = EntryPoint.Instance.Config.Scp575.DelayOnChase, _destroyed = false;
        IFpcRole victimFpc = null!;
        IFpcRole dummyFpc = null!;
    }
}
