using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Perception.Engine
{

	public class PlayerController : Controller, ICharacterController
	{

		public KinematicCharacterMotor Motor;
		private ControlService Control;
		private Camera PlayerCamera;
		private Player _player;

		[Header("Movement")]
		public float MaxStableMoveSpeed = 4f;
		public float StableMovementSharpness = 15f;
		public float OrientationSharpness = 10f;

		[Header("Running")]
		public bool CanRun = true;
		public bool IsRunning = false;
		public float MaxRunSpeed = 6f;

		[Header("Crouching")]
		public float MaxCrouchSpeed = 2f;
		public bool IsCrouching = false;
		private bool _shouldBeCrouching = false;
		public bool _crouchToggled = false;
		public float CrouchedCameraHeight = 1f;
		private bool _inCrouchVolume = false;
		public SoundObject CrouchSound;
		public float CrouchVignetteIntensity = 1f;
		//private CrouchVignetteEffect _crouchVignette;

		// Use these over the Motor's initial Height/Radius variables.
		[Header("Capsule Dimensions")]
		public float StandingCapsuleHeight = 2f;
		public float CrouchedCapsuleHeight = 1f;
		public float CapsuleRadius = 0.5f;

		[Header("Air")]
		public float MaxAirMoveSpeed = 15f;
		public float AirAccelerationSpeed = 15f;
		public float Drag = 0.1f;

		[Header("Misc")]
		public Vector3 Gravity = new Vector3(0, -30f, 0);
		public Transform MeshRoot;

		public Transform CameraFollowPoint;
		public float CameraStandingFollowHeight = 2f;
		public float CameraCrouchFollowHeight = 1f;
		public float AdditionalCrouchHeight = 0f; // Custom height adjustments for vents/desks.
		private float CameraFollowSpeed = 8f; // This just handles the transition for local position Y on the camera target.
		private float _targetCameraFollowHeight;
		private float _currentCameraFollowHeight;

		public Vector3 _lookInputVector;
		private PlayerInputHelper _inputs;
		private Vector3 _moveInputVector;
		private Vector3 _internalVelocityAdd = Vector3.zero;
		private Collider[] _probedColliders = new Collider[8];
		private RaycastHit[] _probedHits = new RaycastHit[8];

		private bool _inputDisabled = false;

		[Header("Void Recovery")]
		public bool DisableVoidRecovery = false; // Allows you to temp disable this option in case the level has a massive player y displacement.
		public bool InstantKillOnVoidFall = false; // Option to just kill the player immediately if they reach a certain -y displacement.
		private Queue<Vector3> safePositionHistory = new Queue<Vector3>();
		private float positionCheckInterval = 0.5f;
		private float timeSinceLastCheck;
		private float voidYThreshold = -500f; // Adjust magic number as needed.

		[Header("Debug Settings")]
		public bool NoClip = false;
		public float NoClipSpeed = 10f;
		private Vector3 _noClipVelocity;

		[HideInInspector]
		public float SpeedPenalty = 1f;

		public override void Awake()
		{
			base.Awake();

			_player = this.GetComponent<Player>();
			Motor = this.GetComponent<KinematicCharacterMotor>();
			Control = GameManager.GetService<ControlService>();
			PlayerCamera = GameManager.GetService<CameraService>().Camera;
			//_crouchVignette = PlayerCamera.GetComponent<CrouchVignetteEffect>();

			if (Motor == null)
			{
				Debug.LogError("KinematicCharacterMotor was not found on the same GameObject as PlayerController. This script cannot function without it.");
				enabled = false;
				return;
			}

			Motor.CharacterController = this;
			_inputs = new PlayerInputHelper();

			_targetCameraFollowHeight = CameraStandingFollowHeight;
			_currentCameraFollowHeight = _targetCameraFollowHeight;

		}

		void Start()
		{
			Motor.SetCapsuleDimensions(CapsuleRadius, StandingCapsuleHeight, StandingCapsuleHeight * 0.5f);
			CameraFollowPoint.localPosition = new Vector3(0f, _currentCameraFollowHeight, 0f);
		}

		public override void BuildInput()
		{
			if (_inputDisabled) return;
			base.BuildInput();

			Vector2 _movementInput = Control.PlayerMovementAction.ReadValue<Vector2>();
			_movementInput = _movementInput.normalized * Mathf.Clamp01(_movementInput.magnitude);

			_inputs.Horizontal = _movementInput.x;
			_inputs.Vertical = _movementInput.y;
			_inputs.Run = Control.PlayerRunAction.IsPressed();
			_inputs.Crouch = Control.PlayerCrouchAction.IsPressed();
			_inputs.CrouchToggle = Control.PlayerToggleCrouchAction.WasPressedThisFrame();
			_inputs.Jump = Control.PlayerJumpAction.IsPressed();

			this.Log(_movementInput);

			var _cameraRotation = PlayerCamera.transform.rotation;
			Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(_cameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
			if (cameraPlanarDirection.sqrMagnitude == 0f)
			{
				cameraPlanarDirection = Vector3.ProjectOnPlane(_cameraRotation * Vector3.up, Motor.CharacterUp).normalized;
			}
			Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

			Vector3 moveInputVector = new Vector3(_inputs.Horizontal, 0f, _inputs.Vertical);
			moveInputVector = Vector3.ClampMagnitude(moveInputVector, 1f);

			_lookInputVector = cameraPlanarDirection;
			_moveInputVector = cameraPlanarRotation * moveInputVector;

			IsRunning = CanRun && _inputs.Run;

			if (_inputs.Jump) { }

			CheckCrouch();

			// if (Control.PlayerNoClipAction.WasPressedThisFrame())
			// {
			// 	ToggleNoClip();
			// }

		}

		void Update()
		{
			_currentCameraFollowHeight = Mathf.Lerp(_currentCameraFollowHeight, _targetCameraFollowHeight, Time.deltaTime * CameraFollowSpeed);
			CameraFollowPoint.localPosition = new Vector3(0f, _currentCameraFollowHeight, 0f);
		}

		public void Simulate(Player player)
		{

		}

		/// <summary>
		/// This handles all input logic between crouch toggling and hold to crouch logic between bindings.
		/// </summary>
		private void CheckCrouch()
		{
			if (NoClip) return;

			// Hold to crouch
			if (!IsRunning && _inputs.Crouch)
			{
				_shouldBeCrouching = true;
				if (!IsCrouching)
				{
					IsCrouching = true;
					Crouch();
				}
			}
			else if (!_crouchToggled)
			{
				_shouldBeCrouching = false;
			}

			// Toggle crouch
			if (!IsRunning && _inputs.CrouchToggle)
			{
				if (!_crouchToggled && !IsCrouching)
				{
					_crouchToggled = true;
					_shouldBeCrouching = true;
					IsCrouching = true;
					Crouch();
				}
				else
				{
					_crouchToggled = false;
				}
			}

			// If the player tries to run or release hold to crouch while it's toggled, untoggle.
			// if (IsRunning || Control.PlayerCrouchAction.WasReleasedThisFrame())
			// {
			// 	_crouchToggled = false;
			// }
		}

		private void Crouch()
		{
			Motor.SetCapsuleDimensions(CapsuleRadius, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
			MeshRoot.localScale = new Vector3(1f, CrouchedCapsuleHeight / StandingCapsuleHeight, 1f);
			_targetCameraFollowHeight = CameraCrouchFollowHeight;
			if (CrouchSound != null)
			{
				PerceptionAudio.FromGameObject(CrouchSound, this.gameObject);
			}
			//if(_crouchVignette != null)
			//{
			//	_crouchVignette.SetVignetteIntensity(CrouchVignetteIntensity);
			//}
		}

		private void Uncrouch()
		{
			if (_inCrouchVolume) return;

			// First, set the capsule to standing dimensions
			Motor.SetCapsuleDimensions(CapsuleRadius, StandingCapsuleHeight, StandingCapsuleHeight * 0.5f);

			// Check for obstructions
			if (Motor.CharacterOverlap(
				Motor.TransientPosition,
				Motor.TransientRotation,
				_probedColliders,
				Motor.CollidableLayers,
				QueryTriggerInteraction.Ignore) > 0)
			{
				// If obstructions, revert to crouching dimensions
				Motor.SetCapsuleDimensions(CapsuleRadius, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
				//Debug.Log("Obstruction detected, staying crouched");
			}
			else
			{
				// If no obstructions, complete the uncrouch
				MeshRoot.localScale = Vector3.one;
				_targetCameraFollowHeight = CameraStandingFollowHeight;
				IsCrouching = false;
				//Debug.Log("No obstruction, uncrouched successfully");
				if (CrouchSound != null)
				{
					PerceptionAudio.FromGameObject(CrouchSound, this.gameObject);
				}

			}


		}

		public void ApplyModifiedCrouch(float heightReduction, float cameraAdjustment = 0f)
		{
			float modifiedHeight = CrouchedCapsuleHeight + heightReduction;
			Motor.SetCapsuleDimensions(CapsuleRadius, modifiedHeight, modifiedHeight * 0.5f);
			MeshRoot.localScale = new Vector3(1f, modifiedHeight / StandingCapsuleHeight, 1f);
			_targetCameraFollowHeight = CameraCrouchFollowHeight + cameraAdjustment;
			_inCrouchVolume = true;
		}

		public void ResetToNormalCrouch()
		{
			Motor.SetCapsuleDimensions(CapsuleRadius, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
			MeshRoot.localScale = new Vector3(1f, CrouchedCapsuleHeight / StandingCapsuleHeight, 1f);
			_targetCameraFollowHeight = CameraCrouchFollowHeight;
			_inCrouchVolume = false;
		}

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			if (NoClip)
			{
				NoClipMove(ref currentVelocity, deltaTime);
				return;
			}

			// Ground movement
			if (Motor.GroundingStatus.IsStableOnGround)
			{
				float currentVelocityMagnitude = currentVelocity.magnitude;

				Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

				// Reorient velocity on slope
				currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

				// Calculate target velocity
				Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
				Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;

				// Set movement speed
				float _targetSpeed = IsRunning ? MaxRunSpeed : MaxStableMoveSpeed;
				if (IsCrouching) _targetSpeed = MaxCrouchSpeed;


				Vector3 targetMovementVelocity = reorientedInput * _targetSpeed * SpeedPenalty;
				SpeedPenalty = 1f;

				// Smooth movement Velocity
				currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
			}
			// Air movement
			else
			{
				// Add move input
				if (_moveInputVector.sqrMagnitude > 0f)
				{
					Vector3 addedVelocity = _moveInputVector * AirAccelerationSpeed * deltaTime;

					Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

					// Limit air velocity from inputs
					if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
					{
						// clamp addedVel to make total vel not exceed max vel on inputs plane
						Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
						addedVelocity = newTotal - currentVelocityOnInputsPlane;
					}
					else
					{
						// Make sure added vel doesn't go in the direction of the already-exceeding velocity
						if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
						{
							addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
						}
					}

					// Prevent air-climbing sloped walls
					if (Motor.GroundingStatus.FoundAnyGround)
					{
						if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
						{
							Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
							addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
						}
					}

					// Apply added velocity
					currentVelocity += addedVelocity;
				}

				// Gravity
				currentVelocity += Gravity * deltaTime;

				// Drag
				currentVelocity *= (1f / (1f + (Drag * deltaTime)));
			}

			// Take into account additive velocity
			if (_internalVelocityAdd.sqrMagnitude > 0f)
			{
				currentVelocity += _internalVelocityAdd;
				_internalVelocityAdd = Vector3.zero;
			}

			// Attach motor velocity to Actor velocity.
			Velocity = currentVelocity;
		}

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{

		}

		public void BeforeCharacterUpdate(float deltaTime)
		{

		}

		public void PostGroundingUpdate(float deltaTime)
		{

		}

		public void AfterCharacterUpdate(float deltaTime)
		{
			if (IsCrouching && !_shouldBeCrouching)
			{
				Uncrouch();
			}
			else if (!IsCrouching && _shouldBeCrouching)
			{
				Crouch();
			}

		}

		public bool IsColliderValidForCollisions(Collider coll)
		{
			return true;
		}

		public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
		{

		}

		public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
		{

		}

		public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
		{

		}

		public void OnDiscreteCollisionDetected(Collider hitCollider)
		{

		}

		protected void OnLanded()
		{
		}

		protected void OnLeaveStableGround()
		{
		}





		private void NoClipMove(ref Vector3 currentVelocity, float deltaTime)
		{
			Vector3 _forward = PlayerCamera.transform.forward;
			Vector3 _right = PlayerCamera.transform.right;
			Vector3 _up = PlayerCamera.transform.up;

			Vector3 _dir = (_forward * _inputs.Vertical + _right * _inputs.Horizontal).normalized;

			float currentSpeed = _inputs.Run ? NoClipSpeed * 2f : NoClipSpeed;
			currentVelocity = _dir * currentSpeed;

			if (_inputs.Jump)
			{
				currentVelocity += Vector3.up * currentSpeed;
			}
			if (_inputs.Crouch)
			{
				currentVelocity -= Vector3.up * currentSpeed;
			}
		}

		// private void ToggleNoClip()
		// {
		// 	NoClip = !NoClip;
		// 	if (NoClip)
		// 	{
		// 		// Force uncrouch if needed.
		// 		if (IsCrouching)
		// 		{
		// 			_shouldBeCrouching = false;
		// 			_crouchToggled = false;
		// 			Uncrouch();
		// 		}
		// 		Control.PlayerCrouchAction.Disable();
		// 		Control.PlayerToggleCrouchAction.Disable();

		// 		GetComponent<CameraBob>().SuppressHeadbob = true;
		// 		Motor.SetCapsuleCollisionsActivation(false);
		// 		Motor.SetGroundSolvingActivation(false);
		// 		Motor.SetMovementCollisionsSolvingActivation(false);
		// 	}
		// 	else
		// 	{
		// 		Control.PlayerCrouchAction.Enable();
		// 		Control.PlayerToggleCrouchAction.Enable();

		// 		//GetComponent<CameraBob>().SuppressHeadbob = false;
		// 		Motor.SetCapsuleCollisionsActivation(true);
		// 		Motor.SetGroundSolvingActivation(true);
		// 		Motor.SetMovementCollisionsSolvingActivation(true);
		// 	}
		// }

		public void DisableController()
		{
			_inputDisabled = true;
			_moveInputVector = new Vector3(0, 0, 0);
		}

		public void EnableController()
		{
			_inputDisabled = false;
		}

	}

	public struct PlayerInputHelper
	{
		public float Horizontal;
		public float Vertical;
		public bool Run;
		public bool Crouch;
		public bool CrouchToggle;
		public bool Jump;
	}

}