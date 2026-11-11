using System;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController;

[RequireComponent(typeof(CapsuleCollider))]
public class KinematicCharacterMotor : MonoBehaviour
{
	[Header("Components")]
	[ReadOnly]
	public CapsuleCollider Capsule;

	[Header("Capsule Settings")]
	[SerializeField]
	[Tooltip("Radius of the Character Capsule")]
	private float CapsuleRadius = 0.5f;

	[SerializeField]
	[Tooltip("Height of the Character Capsule")]
	private float CapsuleHeight = 2f;

	[SerializeField]
	[Tooltip("Height of the Character Capsule")]
	private float CapsuleYOffset = 1f;

	[SerializeField]
	[Tooltip("Physics material of the Character Capsule (Does not affect character movement. Only affects things colliding with it)")]
	private PhysicsMaterial CapsulePhysicsMaterial;

	[Header("Grounding settings")]
	[Tooltip("Increases the range of ground detection, to allow snapping to ground at very high speeds")]
	public float GroundDetectionExtraDistance;

	[Range(0f, 89f)]
	[Tooltip("Maximum slope angle on which the character can be stable")]
	public float MaxStableSlopeAngle = 60f;

	[Tooltip("Which layers can the character be considered stable on")]
	public LayerMask StableGroundLayers = -1;

	[Tooltip("Notifies the Character Controller when discrete collisions are detected")]
	public bool DiscreteCollisionEvents;

	[Header("Step settings")]
	[Tooltip("Handles properly detecting grounding status on steps, but has a performance cost.")]
	public StepHandlingMethod StepHandling = StepHandlingMethod.Standard;

	[Tooltip("Maximum height of a step which the character can climb")]
	public float MaxStepHeight = 0.5f;

	[Tooltip("Can the character step up obstacles even if it is not currently stable?")]
	public bool AllowSteppingWithoutStableGrounding;

	[Tooltip("Minimum length of a step that the character can step on (used in Extra stepping method). Use this to let the character step on steps that are smaller that its radius")]
	public float MinRequiredStepDepth = 0.1f;

	[Header("Ledge settings")]
	[Tooltip("Handles properly detecting ledge information and grounding status, but has a performance cost.")]
	public bool LedgeAndDenivelationHandling = true;

	[Tooltip("The distance from the capsule central axis at which the character can stand on a ledge and still be stable")]
	public float MaxStableDistanceFromLedge = 0.5f;

	[Tooltip("Prevents snapping to ground on ledges beyond a certain velocity")]
	public float MaxVelocityForLedgeSnap;

	[Tooltip("The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground")]
	[Range(1f, 180f)]
	public float MaxStableDenivelationAngle = 180f;

	[Header("Rigidbody interaction settings")]
	[Tooltip("Handles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies")]
	public bool InteractiveRigidbodyHandling = true;

	[Tooltip("How the character interacts with non-kinematic rigidbodies. \"Kinematic\" mode means the character pushes the rigidbodies with infinite force (as a kinematic body would). \"SimulatedDynamic\" pushes the rigidbodies with a simulated mass value.")]
	public RigidbodyInteractionType RigidbodyInteractionType;

	[Tooltip("Mass used for pushing bodies")]
	public float SimulatedCharacterMass = 1f;

	[Tooltip("Determines if the character preserves moving platform velocities when de-grounding from them")]
	public bool PreserveAttachedRigidbodyMomentum = true;

	[Header("Other settings")]
	[Tooltip("How many times can we sweep for movement per update")]
	public int MaxMovementIterations = 5;

	[Tooltip("How many times can we check for decollision per update")]
	public int MaxDecollisionIterations = 1;

	[Tooltip("Checks for overlaps before casting movement, making sure all collisions are detected even when already intersecting geometry (has a performance cost, but provides safety against tunneling through colliders)")]
	public bool CheckMovementInitialOverlaps = true;

	[Tooltip("Sets the velocity to zero if exceed max movement iterations")]
	public bool KillVelocityWhenExceedMaxMovementIterations = true;

	[Tooltip("Sets the remaining movement to zero if exceed max movement iterations")]
	public bool KillRemainingMovementWhenExceedMaxMovementIterations = true;

	[NonSerialized]
	public CharacterGroundingReport GroundingStatus;

	[NonSerialized]
	public CharacterTransientGroundingReport LastGroundingStatus;

	[NonSerialized]
	public LayerMask CollidableLayers = -1;

	private Transform _transform;

	private Vector3 _transientPosition;

	private Vector3 _initialSimulationPosition;

	private Rigidbody _attachedRigidbody;

	private Vector3 _characterTransformToCapsuleCenter;

	private Vector3 _characterTransformToCapsuleBottom;

	private Vector3 _characterTransformToCapsuleTop;

	private Vector3 _characterTransformToCapsuleBottomHemi;

	private Vector3 _characterTransformToCapsuleTopHemi;

	private Vector3 _attachedRigidbodyVelocity;

	private int _overlapsCount;

	private OverlapResult[] _overlaps = new OverlapResult[16];

	[NonSerialized]
	public ICharacterController CharacterController;

	[NonSerialized]
	public bool LastMovementIterationFoundAnyGround;

	[NonSerialized]
	public int IndexInCharacterSystem;

	[NonSerialized]
	public Vector3 InitialTickPosition;

	[NonSerialized]
	public Rigidbody AttachedRigidbodyOverride;

	[NonSerialized]
	public Vector3 BaseVelocity;

	private RaycastHit[] _internalCharacterHits = new RaycastHit[16];

	private Collider[] _internalProbedColliders = new Collider[16];

	private List<Rigidbody> _rigidbodiesPushedThisMove = new List<Rigidbody>(16);

	private RigidbodyProjectionHit[] _internalRigidbodyProjectionHits = new RigidbodyProjectionHit[16];

	private Rigidbody _lastAttachedRigidbody;

	private bool _solveMovementCollisions = true;

	private bool _solveGrounding = true;

	private int _rigidbodyProjectionHitCount;

	private bool _isMovingFromAttachedRigidbody;

	private bool _mustUnground;

	private float _mustUngroundTimeCounter;

	public const int MaxHitsBudget = 16;

	public const int MaxCollisionBudget = 16;

	public const int MaxGroundingSweepIterations = 2;

	public const int MaxSteppingSweepIterations = 3;

	public const int MaxRigidbodyOverlapsCount = 16;

	public const float CollisionOffset = 0.01f;

	public const float GroundProbeReboundDistance = 0.02f;

	public const float MinimumGroundProbingDistance = 0.005f;

	public const float GroundProbingBackstepDistance = 0.1f;

	public const float SweepProbingBackstepDistance = 0.002f;

	public const float SecondaryProbesVertical = 0.02f;

	public const float SecondaryProbesHorizontal = 0.001f;

	public const float MinVelocityMagnitude = 0.01f;

	public const float SteppingForwardDistance = 0.03f;

	public const float MinDistanceForLedge = 0.05f;

	public const float CorrelationForVerticalObstruction = 0.01f;

	public const float ExtraSteppingForwardDistance = 0.01f;

	public const float ExtraStepHeightPadding = 0.01f;

	private bool awoken;

	public Transform Transform => _transform;

	public Vector3 TransientPosition => _transientPosition;

	public Vector3 CharacterUp => base.transform.up;

	public Vector3 CharacterForward => base.transform.forward;

	public Vector3 CharacterRight => base.transform.right;

	public Vector3 InitialSimulationPosition => _initialSimulationPosition;

	public Rigidbody AttachedRigidbody => _attachedRigidbody;

	public Vector3 CharacterTransformToCapsuleCenter => _characterTransformToCapsuleCenter;

	public Vector3 CharacterTransformToCapsuleBottom => _characterTransformToCapsuleBottom;

	public Vector3 CharacterTransformToCapsuleTop => _characterTransformToCapsuleTop;

	public Vector3 CharacterTransformToCapsuleBottomHemi => _characterTransformToCapsuleBottomHemi;

	public Vector3 CharacterTransformToCapsuleTopHemi => _characterTransformToCapsuleTopHemi;

	public Vector3 AttachedRigidbodyVelocity => _attachedRigidbodyVelocity;

	public int OverlapsCount => _overlapsCount;

	public OverlapResult[] Overlaps => _overlaps;

	public Vector3 Velocity => BaseVelocity + _attachedRigidbodyVelocity;

	private void OnEnable()
	{
		KinematicCharacterSystem.EnsureCreation();
		KinematicCharacterSystem.RegisterCharacterMotor(this);
	}

	private void OnDisable()
	{
		KinematicCharacterSystem.UnregisterCharacterMotor(this);
	}

	private void Reset()
	{
		ValidateData();
	}

	private void OnValidate()
	{
		ValidateData();
	}

	[ContextMenu("Remove Component")]
	private void HandleRemoveComponent()
	{
		CapsuleCollider component = base.gameObject.GetComponent<CapsuleCollider>();
		UnityEngine.Object.DestroyImmediate(this);
		UnityEngine.Object.DestroyImmediate(component);
	}

	public void ValidateData()
	{
		Capsule = GetComponent<CapsuleCollider>();
		CapsuleRadius = Mathf.Clamp(CapsuleRadius, 0f, CapsuleHeight * 0.5f);
		Capsule.direction = 1;
		Capsule.sharedMaterial = CapsulePhysicsMaterial;
		SetCapsuleDimensions(CapsuleRadius, CapsuleHeight, CapsuleYOffset);
		MaxStepHeight = Mathf.Clamp(MaxStepHeight, 0f, float.PositiveInfinity);
		MinRequiredStepDepth = Mathf.Clamp(MinRequiredStepDepth, 0f, CapsuleRadius);
		MaxStableDistanceFromLedge = Mathf.Clamp(MaxStableDistanceFromLedge, 0f, CapsuleRadius);
		base.transform.localScale = Vector3.one;
	}

	public void SetMovementCollisionsSolvingActivation(bool movementCollisionsSolvingActive)
	{
		_solveMovementCollisions = movementCollisionsSolvingActive;
	}

	public void SetGroundSolvingActivation(bool stabilitySolvingActive)
	{
		_solveGrounding = stabilitySolvingActive;
	}

	public void SetPosition(Vector3 position, bool bypassInterpolation = true)
	{
		_transform.position = position;
		_initialSimulationPosition = position;
		_transientPosition = position;
		if (bypassInterpolation)
		{
			InitialTickPosition = position;
		}
	}

	public KinematicCharacterMotorState GetState()
	{
		KinematicCharacterMotorState result = default(KinematicCharacterMotorState);
		result.Position = _transientPosition;
		result.BaseVelocity = BaseVelocity;
		result.AttachedRigidbodyVelocity = _attachedRigidbodyVelocity;
		result.MustUnground = _mustUnground;
		result.MustUngroundTime = _mustUngroundTimeCounter;
		result.LastMovementIterationFoundAnyGround = LastMovementIterationFoundAnyGround;
		result.GroundingStatus.CopyFrom(GroundingStatus);
		result.AttachedRigidbody = _attachedRigidbody;
		return result;
	}

	public void ApplyState(KinematicCharacterMotorState state, bool bypassInterpolation = true)
	{
		SetPosition(state.Position, bypassInterpolation);
		BaseVelocity = state.BaseVelocity;
		_attachedRigidbodyVelocity = state.AttachedRigidbodyVelocity;
		_mustUnground = state.MustUnground;
		_mustUngroundTimeCounter = state.MustUngroundTime;
		LastMovementIterationFoundAnyGround = state.LastMovementIterationFoundAnyGround;
		GroundingStatus.CopyFrom(state.GroundingStatus);
		_attachedRigidbody = state.AttachedRigidbody;
	}

	public void SetCapsuleDimensions(float radius, float height, float yOffset)
	{
		height = Mathf.Max(height, radius * 2f + 0.01f);
		CapsuleRadius = radius;
		CapsuleHeight = height;
		CapsuleYOffset = yOffset;
		Capsule.radius = CapsuleRadius;
		Capsule.height = Mathf.Clamp(CapsuleHeight, CapsuleRadius * 2f, CapsuleHeight);
		Capsule.center = new Vector3(0f, CapsuleYOffset, 0f);
		_characterTransformToCapsuleCenter = Capsule.center;
		_characterTransformToCapsuleBottom = Capsule.center + Vector3.down * (Capsule.height * 0.5f);
		_characterTransformToCapsuleTop = Capsule.center + Vector3.up * (Capsule.height * 0.5f);
		_characterTransformToCapsuleBottomHemi = Capsule.center + Vector3.down * (Capsule.height * 0.5f) + Vector3.up * Capsule.radius;
		_characterTransformToCapsuleTopHemi = Capsule.center + Vector3.up * (Capsule.height * 0.5f) + Vector3.down * Capsule.radius;
	}

	public void SetCapsuleDimensions(float radius, float height)
	{
		height = Mathf.Max(height, radius * 2f + 0.01f);
		float y = height / 2f;
		CapsuleRadius = radius;
		CapsuleHeight = height;
		Capsule.radius = CapsuleRadius;
		Capsule.height = Mathf.Clamp(CapsuleHeight, CapsuleRadius * 2f, CapsuleHeight);
		Capsule.center = new Vector3(0f, y, 0f);
		_characterTransformToCapsuleCenter = Capsule.center;
		_characterTransformToCapsuleBottom = Capsule.center + Vector3.down * (Capsule.height * 0.5f);
		_characterTransformToCapsuleTop = Capsule.center + Vector3.up * (Capsule.height * 0.5f);
		_characterTransformToCapsuleBottomHemi = Capsule.center + Vector3.down * (Capsule.height * 0.5f) + Vector3.up * Capsule.radius;
		_characterTransformToCapsuleTopHemi = Capsule.center + Vector3.up * (Capsule.height * 0.5f) + Vector3.down * Capsule.radius;
	}

	public void SetCapsuleHeight(float heightInMeters)
	{
		SetCapsuleDimensions(CapsuleRadius, heightInMeters);
	}

	public void Awake()
	{
		if (awoken)
		{
			return;
		}
		awoken = true;
		_transform = base.transform;
		ValidateData();
		_transientPosition = _transform.position;
		CollidableLayers = 0;
		for (int i = 0; i < 32; i++)
		{
			if (!Physics.GetIgnoreLayerCollision(base.gameObject.layer, i))
			{
				CollidableLayers = (int)CollidableLayers | (1 << i);
			}
		}
		SetCapsuleDimensions(CapsuleRadius, CapsuleHeight, CapsuleYOffset);
	}

	public void UpdatePhase1(float deltaTime)
	{
		_rigidbodiesPushedThisMove.Clear();
		CharacterController.BeforeCharacterUpdate(deltaTime);
		_transientPosition = _transform.position;
		_initialSimulationPosition = _transientPosition;
		_rigidbodyProjectionHitCount = 0;
		_overlapsCount = 0;
		LastGroundingStatus.CopyFrom(GroundingStatus);
		GroundingStatus = default(CharacterGroundingReport);
		GroundingStatus.GroundNormal = Vector3.up;
		if (_solveMovementCollisions)
		{
			Vector3 direction = Vector3.up;
			float distance = 0f;
			int i = 0;
			bool flag = false;
			for (; i < MaxDecollisionIterations; i++)
			{
				if (flag)
				{
					break;
				}
				int num = CharacterCollisionsOverlap(_transientPosition, _internalProbedColliders);
				if (num > 0)
				{
					for (int j = 0; j < num; j++)
					{
						if (!(GetInteractiveRigidbody(_internalProbedColliders[j]) == null))
						{
							continue;
						}
						Transform component = _internalProbedColliders[j].GetComponent<Transform>();
						if (Physics.ComputePenetration(Capsule, _transientPosition, Quaternion.identity, _internalProbedColliders[j], component.position, component.rotation, out direction, out distance))
						{
							HitStabilityReport hitStabilityReport = new HitStabilityReport
							{
								IsStable = IsStableOnNormal(direction)
							};
							direction = GetObstructionNormal(direction, hitStabilityReport.IsStable);
							Vector3 vector = direction * (distance + 0.01f);
							_transientPosition += vector;
							if (_overlapsCount < _overlaps.Length)
							{
								_overlaps[_overlapsCount] = new OverlapResult(direction, _internalProbedColliders[j]);
								_overlapsCount++;
							}
							break;
						}
					}
				}
				else
				{
					flag = true;
				}
			}
		}
		if (_solveGrounding)
		{
			if (MustUnground())
			{
				_transientPosition += Vector3.up * 0.0075f;
			}
			else
			{
				float probingDistance = 0.005f;
				if (!LastGroundingStatus.SnappingPrevented && (LastGroundingStatus.IsStableOnGround || LastMovementIterationFoundAnyGround))
				{
					probingDistance = ((StepHandling == StepHandlingMethod.None) ? CapsuleRadius : Mathf.Max(CapsuleRadius, MaxStepHeight));
					probingDistance += GroundDetectionExtraDistance;
				}
				ProbeGround(ref _transientPosition, probingDistance, ref GroundingStatus);
				if (!LastGroundingStatus.IsStableOnGround && GroundingStatus.IsStableOnGround)
				{
					BaseVelocity = Vector3.ProjectOnPlane(BaseVelocity, CharacterUp);
					BaseVelocity = GetDirectionTangentToSurface(BaseVelocity, GroundingStatus.GroundNormal) * BaseVelocity.magnitude;
				}
			}
		}
		LastMovementIterationFoundAnyGround = false;
		if (_mustUngroundTimeCounter > 0f)
		{
			_mustUngroundTimeCounter -= deltaTime;
		}
		_mustUnground = false;
		if (_solveGrounding)
		{
			CharacterController.PostGroundingUpdate(deltaTime);
		}
		if (!InteractiveRigidbodyHandling)
		{
			return;
		}
		_lastAttachedRigidbody = _attachedRigidbody;
		if ((bool)AttachedRigidbodyOverride)
		{
			_attachedRigidbody = AttachedRigidbodyOverride;
		}
		else if (GroundingStatus.IsStableOnGround && (bool)GroundingStatus.GroundCollider.attachedRigidbody)
		{
			Rigidbody interactiveRigidbody = GetInteractiveRigidbody(GroundingStatus.GroundCollider);
			if ((bool)interactiveRigidbody)
			{
				_attachedRigidbody = interactiveRigidbody;
			}
		}
		else
		{
			_attachedRigidbody = null;
		}
		Vector3 linearVelocity = Vector3.zero;
		Vector3 angularVelocity = Vector3.zero;
		if ((bool)_attachedRigidbody)
		{
			GetVelocityFromRigidbodyMovement(_attachedRigidbody, _transientPosition, deltaTime, out linearVelocity, out angularVelocity);
		}
		if (PreserveAttachedRigidbodyMomentum && _lastAttachedRigidbody != null && _attachedRigidbody != _lastAttachedRigidbody)
		{
			BaseVelocity += _attachedRigidbodyVelocity;
			BaseVelocity -= linearVelocity;
		}
		_attachedRigidbodyVelocity = default(Vector3);
		if ((bool)_attachedRigidbody)
		{
			_attachedRigidbodyVelocity = linearVelocity;
		}
		if ((bool)GroundingStatus.GroundCollider && (bool)GroundingStatus.GroundCollider.attachedRigidbody && GroundingStatus.GroundCollider.attachedRigidbody == _attachedRigidbody && _attachedRigidbody != null && _lastAttachedRigidbody == null)
		{
			BaseVelocity -= Vector3.ProjectOnPlane(_attachedRigidbodyVelocity, Vector3.up);
		}
		if (_attachedRigidbodyVelocity.sqrMagnitude > 0f)
		{
			_isMovingFromAttachedRigidbody = true;
			if (_solveMovementCollisions)
			{
				InternalCharacterMove(ref _attachedRigidbodyVelocity, deltaTime);
			}
			else
			{
				_transientPosition += _attachedRigidbodyVelocity * deltaTime;
			}
			_isMovingFromAttachedRigidbody = false;
		}
	}

	public void UpdatePhase2(float deltaTime)
	{
		if (_solveMovementCollisions && InteractiveRigidbodyHandling)
		{
			if (InteractiveRigidbodyHandling && (bool)_attachedRigidbody)
			{
				float radius = Capsule.radius;
				if (CharacterGroundSweep(_transientPosition + Vector3.up * radius, -Vector3.up, radius, out var closestHit) && closestHit.collider.attachedRigidbody == _attachedRigidbody && IsStableOnNormal(closestHit.normal))
				{
					float num = radius - closestHit.distance;
					_transientPosition = _transientPosition + Vector3.up * num + Vector3.up * 0.01f;
				}
			}
			if (InteractiveRigidbodyHandling)
			{
				Vector3 direction = Vector3.up;
				float distance = 0f;
				int i = 0;
				bool flag = false;
				for (; i < MaxDecollisionIterations; i++)
				{
					if (flag)
					{
						break;
					}
					int num2 = CharacterCollisionsOverlap(_transientPosition, _internalProbedColliders);
					if (num2 > 0)
					{
						for (int j = 0; j < num2; j++)
						{
							Transform component = _internalProbedColliders[j].GetComponent<Transform>();
							if (!Physics.ComputePenetration(Capsule, _transientPosition, Quaternion.identity, _internalProbedColliders[j], component.position, component.rotation, out direction, out distance))
							{
								continue;
							}
							HitStabilityReport hitStabilityReport = new HitStabilityReport
							{
								IsStable = IsStableOnNormal(direction)
							};
							direction = GetObstructionNormal(direction, hitStabilityReport.IsStable);
							Vector3 vector = direction * (distance + 0.01f);
							_transientPosition += vector;
							if (InteractiveRigidbodyHandling)
							{
								Rigidbody interactiveRigidbody = GetInteractiveRigidbody(_internalProbedColliders[j]);
								if (interactiveRigidbody != null)
								{
									HitStabilityReport hitStabilityReport2 = new HitStabilityReport
									{
										IsStable = IsStableOnNormal(direction)
									};
									if (hitStabilityReport2.IsStable)
									{
										LastMovementIterationFoundAnyGround = hitStabilityReport2.IsStable;
									}
									if (interactiveRigidbody != _attachedRigidbody)
									{
										_ = _transientPosition + _characterTransformToCapsuleCenter;
										Vector3 transientPosition = _transientPosition;
										StoreRigidbodyHit(interactiveRigidbody, Velocity, transientPosition, direction, hitStabilityReport2);
									}
								}
							}
							if (_overlapsCount < _overlaps.Length)
							{
								_overlaps[_overlapsCount] = new OverlapResult(direction, _internalProbedColliders[j]);
								_overlapsCount++;
							}
							break;
						}
					}
					else
					{
						flag = true;
					}
				}
			}
		}
		CharacterController.UpdateVelocity(ref BaseVelocity, deltaTime);
		if (BaseVelocity.magnitude < 0.01f)
		{
			BaseVelocity = Vector3.zero;
		}
		if (BaseVelocity.sqrMagnitude > 0f)
		{
			if (_solveMovementCollisions)
			{
				InternalCharacterMove(ref BaseVelocity, deltaTime);
			}
			else
			{
				_transientPosition += BaseVelocity * deltaTime;
			}
		}
		if (InteractiveRigidbodyHandling)
		{
			ProcessVelocityForRigidbodyHits(ref BaseVelocity, deltaTime);
		}
		if (DiscreteCollisionEvents)
		{
			int num3 = CharacterCollisionsOverlap(_transientPosition, _internalProbedColliders, 0.02f);
			for (int k = 0; k < num3; k++)
			{
				CharacterController.OnDiscreteCollisionDetected(_internalProbedColliders[k]);
			}
		}
		CharacterController.AfterCharacterUpdate(deltaTime);
	}

	private bool IsStableOnNormal(Vector3 normal)
	{
		return Vector3.Angle(Vector3.up, normal) <= MaxStableSlopeAngle;
	}

	private bool IsStableWithSpecialCases(ref HitStabilityReport stabilityReport, Vector3 velocity)
	{
		if (LedgeAndDenivelationHandling)
		{
			if (stabilityReport.LedgeDetected)
			{
				if (stabilityReport.IsMovingTowardsEmptySideOfLedge && Vector3.Project(velocity, stabilityReport.LedgeFacingDirection).magnitude >= MaxVelocityForLedgeSnap)
				{
					return false;
				}
				if (stabilityReport.IsOnEmptySideOfLedge && stabilityReport.DistanceFromLedge > MaxStableDistanceFromLedge)
				{
					return false;
				}
			}
			if (LastGroundingStatus.FoundAnyGround && stabilityReport.InnerNormal.sqrMagnitude != 0f && stabilityReport.OuterNormal.sqrMagnitude != 0f)
			{
				if (Vector3.Angle(stabilityReport.InnerNormal, stabilityReport.OuterNormal) > MaxStableDenivelationAngle)
				{
					return false;
				}
				if (Vector3.Angle(LastGroundingStatus.InnerGroundNormal, stabilityReport.OuterNormal) > MaxStableDenivelationAngle)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ProbeGround(ref Vector3 probingPosition, float probingDistance, ref CharacterGroundingReport groundingReport)
	{
		if (probingDistance < 0.005f)
		{
			probingDistance = 0.005f;
		}
		int num = 0;
		RaycastHit closestHit = default(RaycastHit);
		bool flag = false;
		Vector3 vector = probingPosition;
		Vector3 vector2 = Vector3.down;
		float num2 = probingDistance;
		while (num2 > 0f && num <= 2 && !flag)
		{
			if (CharacterGroundSweep(vector, vector2, num2, out closestHit))
			{
				Vector3 atCharacterPosition = vector + vector2 * closestHit.distance;
				HitStabilityReport stabilityReport = default(HitStabilityReport);
				EvaluateHitStability(closestHit.collider, closestHit.normal, closestHit.point, atCharacterPosition, BaseVelocity, ref stabilityReport);
				groundingReport.HasAnyGround = true;
				groundingReport.GroundNormal = closestHit.normal;
				groundingReport.InnerGroundNormal = stabilityReport.InnerNormal;
				groundingReport.OuterGroundNormal = stabilityReport.OuterNormal;
				groundingReport.GroundCollider = closestHit.collider;
				groundingReport.GroundPoint = closestHit.point;
				groundingReport.SnappingPrevented = false;
				if (stabilityReport.IsStable)
				{
					groundingReport.SnappingPrevented = !IsStableWithSpecialCases(ref stabilityReport, BaseVelocity);
					groundingReport.IsStableOnGround = true;
					if (!groundingReport.SnappingPrevented)
					{
						probingPosition = vector + vector2 * (closestHit.distance - 0.01f);
					}
					CharacterController.OnGroundHit(closestHit.collider, closestHit.normal, closestHit.point, ref stabilityReport);
					flag = true;
				}
				else
				{
					Vector3 vector3 = vector2 * closestHit.distance + Vector3.up * Mathf.Max(0.01f, closestHit.distance);
					vector += vector3;
					num2 = Mathf.Min(0.02f, Mathf.Max(num2 - vector3.magnitude, 0f));
					vector2 = Vector3.ProjectOnPlane(vector2, closestHit.normal).normalized;
				}
			}
			else
			{
				flag = true;
			}
			num++;
		}
	}

	public void ForceUnground(float time = 0.1f)
	{
		_mustUnground = true;
		_mustUngroundTimeCounter = time;
	}

	public bool MustUnground()
	{
		if (!_mustUnground)
		{
			return _mustUngroundTimeCounter > 0f;
		}
		return true;
	}

	public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
	{
		Vector3 rhs = Vector3.Cross(direction, Vector3.up);
		return Vector3.Cross(surfaceNormal, rhs).normalized;
	}

	private bool InternalCharacterMove(ref Vector3 transientVelocity, float deltaTime)
	{
		if (deltaTime <= 0f)
		{
			return false;
		}
		bool result = true;
		Vector3 remainingMovementDirection = transientVelocity.normalized;
		float remainingMovementMagnitude = transientVelocity.magnitude * deltaTime;
		Vector3 originalDirection = remainingMovementDirection;
		int num = 0;
		bool flag = true;
		Vector3 vector = _transientPosition;
		bool previousHitIsStable = false;
		Vector3 previousVelocity = default(Vector3);
		Vector3 previousObstructionNormal = default(Vector3);
		MovementSweepState sweepState = MovementSweepState.Initial;
		for (int i = 0; i < _overlapsCount; i++)
		{
			Vector3 normal = _overlaps[i].Normal;
			if (Vector3.Dot(remainingMovementDirection, normal) < 0f)
			{
				bool flag2 = IsStableOnNormal(normal) && !MustUnground();
				Vector3 vector2 = transientVelocity;
				Vector3 obstructionNormal = GetObstructionNormal(normal, flag2);
				InternalHandleVelocityProjection(flag2, normal, obstructionNormal, originalDirection, ref sweepState, previousHitIsStable, previousVelocity, previousObstructionNormal, ref transientVelocity, ref remainingMovementMagnitude, ref remainingMovementDirection);
				previousHitIsStable = flag2;
				previousVelocity = vector2;
				previousObstructionNormal = obstructionNormal;
			}
		}
		while (remainingMovementMagnitude > 0f && num <= MaxMovementIterations && flag)
		{
			bool flag3 = false;
			Vector3 hitPoint = default(Vector3);
			Vector3 vector3 = default(Vector3);
			float num2 = 0f;
			Collider collider = null;
			if (CheckMovementInitialOverlaps)
			{
				int num3 = CharacterCollisionsOverlap(vector, _internalProbedColliders);
				if (num3 > 0)
				{
					num2 = 0f;
					float num4 = 2f;
					for (int j = 0; j < num3; j++)
					{
						Collider collider2 = _internalProbedColliders[j];
						if (Physics.ComputePenetration(Capsule, vector, Quaternion.identity, collider2, collider2.transform.position, collider2.transform.rotation, out var direction, out var distance))
						{
							float num5 = Vector3.Dot(remainingMovementDirection, direction);
							if (num5 < 0f && num5 < num4)
							{
								num4 = num5;
								vector3 = direction;
								collider = collider2;
								hitPoint = vector + CharacterTransformToCapsuleCenter + direction * distance;
								flag3 = true;
							}
						}
					}
				}
			}
			if (!flag3 && CharacterCollisionsSweep(vector, remainingMovementDirection, remainingMovementMagnitude + 0.01f, out var closestHit, _internalCharacterHits) > 0)
			{
				vector3 = closestHit.normal;
				num2 = closestHit.distance;
				collider = closestHit.collider;
				hitPoint = closestHit.point;
				flag3 = true;
			}
			if (flag3)
			{
				Vector3 vector4 = remainingMovementDirection * Mathf.Max(0f, num2 - 0.01f);
				vector += vector4;
				remainingMovementMagnitude -= vector4.magnitude;
				HitStabilityReport stabilityReport = default(HitStabilityReport);
				EvaluateHitStability(collider, vector3, hitPoint, vector, transientVelocity, ref stabilityReport);
				bool flag4 = false;
				if (_solveGrounding && StepHandling != StepHandlingMethod.None && stabilityReport.ValidStepDetected && Mathf.Abs(Vector3.Dot(vector3, Vector3.up)) <= 0.01f)
				{
					Vector3 normalized = Vector3.ProjectOnPlane(-vector3, Vector3.up).normalized;
					Vector3 vector5 = vector + normalized * 0.03f + Vector3.up * MaxStepHeight;
					RaycastHit closestHit2;
					int num6 = CharacterCollisionsSweep(vector5, -Vector3.up, MaxStepHeight, out closestHit2, _internalCharacterHits, 0f, acceptOnlyStableGroundLayer: true);
					for (int k = 0; k < num6; k++)
					{
						if (_internalCharacterHits[k].collider == stabilityReport.SteppedCollider)
						{
							vector = vector5 + -Vector3.up * (_internalCharacterHits[k].distance - 0.01f);
							flag4 = true;
							transientVelocity = Vector3.ProjectOnPlane(transientVelocity, CharacterUp);
							remainingMovementDirection = transientVelocity.normalized;
							break;
						}
					}
				}
				if (!flag4)
				{
					Vector3 obstructionNormal2 = GetObstructionNormal(vector3, stabilityReport.IsStable);
					CharacterController.OnMovementHit(collider, vector3, hitPoint, ref stabilityReport);
					if (InteractiveRigidbodyHandling && (bool)collider.attachedRigidbody)
					{
						StoreRigidbodyHit(collider.attachedRigidbody, transientVelocity, hitPoint, obstructionNormal2, stabilityReport);
					}
					bool flag5 = stabilityReport.IsStable && !MustUnground();
					Vector3 vector6 = transientVelocity;
					InternalHandleVelocityProjection(flag5, vector3, obstructionNormal2, originalDirection, ref sweepState, previousHitIsStable, previousVelocity, previousObstructionNormal, ref transientVelocity, ref remainingMovementMagnitude, ref remainingMovementDirection);
					previousHitIsStable = flag5;
					previousVelocity = vector6;
					previousObstructionNormal = obstructionNormal2;
				}
			}
			else
			{
				flag = false;
			}
			num++;
			if (num > MaxMovementIterations)
			{
				if (KillRemainingMovementWhenExceedMaxMovementIterations)
				{
					remainingMovementMagnitude = 0f;
				}
				if (KillVelocityWhenExceedMaxMovementIterations)
				{
					transientVelocity = Vector3.zero;
				}
				result = false;
			}
		}
		vector += remainingMovementDirection * remainingMovementMagnitude;
		_transientPosition = vector;
		return result;
	}

	private Vector3 GetObstructionNormal(Vector3 hitNormal, bool stableOnHit)
	{
		Vector3 vector = hitNormal;
		if (GroundingStatus.IsStableOnGround && !MustUnground() && !stableOnHit)
		{
			vector = Vector3.Cross(Vector3.Cross(GroundingStatus.GroundNormal, vector).normalized, Vector3.up).normalized;
		}
		if (vector.sqrMagnitude == 0f)
		{
			vector = hitNormal;
		}
		return vector;
	}

	private void StoreRigidbodyHit(Rigidbody hitRigidbody, Vector3 hitVelocity, Vector3 hitPoint, Vector3 obstructionNormal, HitStabilityReport hitStabilityReport)
	{
		if (_rigidbodyProjectionHitCount < _internalRigidbodyProjectionHits.Length && !hitRigidbody.GetComponent<KinematicCharacterMotor>())
		{
			RigidbodyProjectionHit rigidbodyProjectionHit = new RigidbodyProjectionHit
			{
				Rigidbody = hitRigidbody,
				HitPoint = hitPoint,
				EffectiveHitNormal = obstructionNormal,
				HitVelocity = hitVelocity,
				StableOnHit = hitStabilityReport.IsStable
			};
			_internalRigidbodyProjectionHits[_rigidbodyProjectionHitCount] = rigidbodyProjectionHit;
			_rigidbodyProjectionHitCount++;
		}
	}

	public void SetTransientPosition(Vector3 newPos)
	{
		_transientPosition = newPos;
	}

	private void InternalHandleVelocityProjection(bool stableOnHit, Vector3 hitNormal, Vector3 obstructionNormal, Vector3 originalDirection, ref MovementSweepState sweepState, bool previousHitIsStable, Vector3 previousVelocity, Vector3 previousObstructionNormal, ref Vector3 transientVelocity, ref float remainingMovementMagnitude, ref Vector3 remainingMovementDirection)
	{
		if (transientVelocity.sqrMagnitude <= 0f)
		{
			return;
		}
		Vector3 vector = transientVelocity;
		if (stableOnHit)
		{
			LastMovementIterationFoundAnyGround = true;
			HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
		}
		else if (sweepState == MovementSweepState.Initial)
		{
			HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
			sweepState = MovementSweepState.AfterFirstHit;
		}
		else if (sweepState == MovementSweepState.AfterFirstHit)
		{
			EvaluateCrease(transientVelocity, previousVelocity, obstructionNormal, previousObstructionNormal, stableOnHit, previousHitIsStable, GroundingStatus.IsStableOnGround && !MustUnground(), out var isValidCrease, out var creaseDirection);
			if (isValidCrease)
			{
				if (GroundingStatus.IsStableOnGround && !MustUnground())
				{
					transientVelocity = Vector3.zero;
					sweepState = MovementSweepState.FoundBlockingCorner;
				}
				else
				{
					transientVelocity = Vector3.Project(transientVelocity, creaseDirection);
					sweepState = MovementSweepState.FoundBlockingCrease;
				}
			}
			else
			{
				HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
			}
		}
		else if (sweepState == MovementSweepState.FoundBlockingCrease)
		{
			transientVelocity = Vector3.zero;
			sweepState = MovementSweepState.FoundBlockingCorner;
		}
		float num = transientVelocity.magnitude / vector.magnitude;
		remainingMovementMagnitude *= num;
		remainingMovementDirection = transientVelocity.normalized;
	}

	private void EvaluateCrease(Vector3 currentCharacterVelocity, Vector3 previousCharacterVelocity, Vector3 currentHitNormal, Vector3 previousHitNormal, bool currentHitIsStable, bool previousHitIsStable, bool characterIsStable, out bool isValidCrease, out Vector3 creaseDirection)
	{
		isValidCrease = false;
		creaseDirection = default(Vector3);
		if (characterIsStable && currentHitIsStable && previousHitIsStable)
		{
			return;
		}
		Vector3 vector = Vector3.Cross(currentHitNormal, previousHitNormal).normalized;
		float num = Vector3.Dot(currentHitNormal, previousHitNormal);
		bool flag = false;
		if (num < 0.999f)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(currentHitNormal, vector).normalized;
			Vector3 normalized2 = Vector3.ProjectOnPlane(previousHitNormal, vector).normalized;
			float num2 = Vector3.Dot(normalized, normalized2);
			Vector3 normalized3 = Vector3.ProjectOnPlane(previousCharacterVelocity, vector).normalized;
			if (num2 <= Vector3.Dot(-normalized3, normalized) + 0.001f && num2 <= Vector3.Dot(-normalized3, normalized2) + 0.001f)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (Vector3.Dot(vector, currentCharacterVelocity) < 0f)
			{
				vector = -vector;
			}
			isValidCrease = true;
			creaseDirection = vector;
		}
	}

	public virtual void HandleVelocityProjection(ref Vector3 velocity, Vector3 obstructionNormal, bool stableOnHit)
	{
		if (GroundingStatus.IsStableOnGround && !MustUnground())
		{
			if (stableOnHit)
			{
				velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
				return;
			}
			Vector3 normalized = Vector3.Cross(Vector3.Cross(obstructionNormal, GroundingStatus.GroundNormal).normalized, obstructionNormal).normalized;
			velocity = GetDirectionTangentToSurface(velocity, normalized) * velocity.magnitude;
			velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
		}
		else if (stableOnHit)
		{
			velocity = Vector3.ProjectOnPlane(velocity, CharacterUp);
			velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
		}
		else
		{
			velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
		}
	}

	public virtual void HandleSimulatedRigidbodyInteraction(ref Vector3 processedVelocity, RigidbodyProjectionHit hit, float deltaTime)
	{
	}

	private void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity, float deltaTime)
	{
		for (int i = 0; i < _rigidbodyProjectionHitCount; i++)
		{
			RigidbodyProjectionHit hit = _internalRigidbodyProjectionHits[i];
			if (!hit.Rigidbody || _rigidbodiesPushedThisMove.Contains(hit.Rigidbody) || !(_internalRigidbodyProjectionHits[i].Rigidbody != _attachedRigidbody))
			{
				continue;
			}
			_rigidbodiesPushedThisMove.Add(hit.Rigidbody);
			float simulatedCharacterMass = SimulatedCharacterMass;
			Vector3 hitVelocity = hit.HitVelocity;
			KinematicCharacterMotor component = hit.Rigidbody.GetComponent<KinematicCharacterMotor>();
			bool flag = component != null;
			bool flag2 = !hit.Rigidbody.isKinematic;
			_ = hit.Rigidbody.mass;
			float num = hit.Rigidbody.mass;
			Vector3 bodyVelocity = hit.Rigidbody.linearVelocity;
			if (flag)
			{
				_ = component.SimulatedCharacterMass;
				num = component.SimulatedCharacterMass;
				bodyVelocity = component.BaseVelocity;
			}
			else if (!flag2)
			{
				PhysicsMover component2 = hit.Rigidbody.GetComponent<PhysicsMover>();
				if ((bool)component2)
				{
					bodyVelocity = component2.Velocity;
				}
			}
			float num2 = 1f;
			num2 = ((!(simulatedCharacterMass + num > 0f)) ? 0.5f : (simulatedCharacterMass / (simulatedCharacterMass + num)));
			if (!flag2)
			{
				num2 = 0f;
			}
			else if (RigidbodyInteractionType == RigidbodyInteractionType.Kinematic && !flag)
			{
				num2 = 1f;
			}
			ComputeCollisionResolutionForHitBody(hit.EffectiveHitNormal, hitVelocity, bodyVelocity, num2, out var velocityChangeOnCharacter, out var velocityChangeOnBody);
			processedVelocity += velocityChangeOnCharacter;
			if (flag)
			{
				component.BaseVelocity += velocityChangeOnCharacter;
			}
			else if (flag2)
			{
				hit.Rigidbody.AddForceAtPosition(velocityChangeOnBody, hit.HitPoint, ForceMode.VelocityChange);
			}
			if (RigidbodyInteractionType == RigidbodyInteractionType.SimulatedDynamic)
			{
				HandleSimulatedRigidbodyInteraction(ref processedVelocity, hit, deltaTime);
			}
		}
	}

	public void ComputeCollisionResolutionForHitBody(Vector3 hitNormal, Vector3 characterVelocity, Vector3 bodyVelocity, float characterToBodyMassRatio, out Vector3 velocityChangeOnCharacter, out Vector3 velocityChangeOnBody)
	{
		velocityChangeOnCharacter = default(Vector3);
		velocityChangeOnBody = default(Vector3);
		float num = 1f - characterToBodyMassRatio;
		float num2 = Vector3.Dot(characterVelocity, hitNormal);
		float num3 = Vector3.Dot(bodyVelocity, hitNormal);
		if (num2 < 0f)
		{
			Vector3 vector = hitNormal * num2;
			velocityChangeOnCharacter += vector;
		}
		if (num3 > num2)
		{
			Vector3 vector2 = hitNormal * (num3 - num2);
			velocityChangeOnCharacter += vector2 * num;
			velocityChangeOnBody += -vector2 * characterToBodyMassRatio;
		}
	}

	private bool CheckIfColliderValidForCollisions(Collider coll)
	{
		if (coll == Capsule)
		{
			return false;
		}
		if (!InternalIsColliderValidForCollisions(coll))
		{
			return false;
		}
		return true;
	}

	private bool InternalIsColliderValidForCollisions(Collider coll)
	{
		Rigidbody attachedRigidbody = coll.attachedRigidbody;
		if ((bool)attachedRigidbody)
		{
			bool isKinematic = attachedRigidbody.isKinematic;
			if (_isMovingFromAttachedRigidbody && (!isKinematic || attachedRigidbody == _attachedRigidbody))
			{
				return false;
			}
			if (RigidbodyInteractionType == RigidbodyInteractionType.Kinematic && !isKinematic)
			{
				if ((bool)coll.attachedRigidbody)
				{
					coll.attachedRigidbody.WakeUp();
				}
				return false;
			}
		}
		if (!CharacterController.IsColliderValidForCollisions(coll))
		{
			return false;
		}
		return true;
	}

	public void EvaluateHitStability(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Vector3 withCharacterVelocity, ref HitStabilityReport stabilityReport)
	{
		if (!_solveGrounding)
		{
			stabilityReport.IsStable = false;
			return;
		}
		Vector3 up = Vector3.up;
		Vector3 normalized = Vector3.ProjectOnPlane(hitNormal, up).normalized;
		stabilityReport.IsStable = IsStableOnNormal(hitNormal);
		stabilityReport.FoundInnerNormal = false;
		stabilityReport.FoundOuterNormal = false;
		stabilityReport.InnerNormal = hitNormal;
		stabilityReport.OuterNormal = hitNormal;
		if (LedgeAndDenivelationHandling)
		{
			float num = 0.05f;
			if (StepHandling != StepHandlingMethod.None)
			{
				num = MaxStepHeight;
			}
			bool flag = false;
			bool flag2 = false;
			if (CharacterCollisionsRaycast(hitPoint + up * 0.02f + normalized * 0.001f, -up, num + 0.02f, out var closestHit, _internalCharacterHits) > 0)
			{
				Vector3 normal = (stabilityReport.InnerNormal = closestHit.normal);
				stabilityReport.FoundInnerNormal = true;
				flag = IsStableOnNormal(normal);
			}
			if (CharacterCollisionsRaycast(hitPoint + up * 0.02f + -normalized * 0.001f, -up, num + 0.02f, out var closestHit2, _internalCharacterHits) > 0)
			{
				Vector3 normal2 = (stabilityReport.OuterNormal = closestHit2.normal);
				stabilityReport.FoundOuterNormal = true;
				flag2 = IsStableOnNormal(normal2);
			}
			stabilityReport.LedgeDetected = flag != flag2;
			if (stabilityReport.LedgeDetected)
			{
				stabilityReport.IsOnEmptySideOfLedge = flag2 && !flag;
				stabilityReport.LedgeGroundNormal = (flag2 ? stabilityReport.OuterNormal : stabilityReport.InnerNormal);
				stabilityReport.LedgeRightDirection = Vector3.Cross(hitNormal, stabilityReport.LedgeGroundNormal).normalized;
				stabilityReport.LedgeFacingDirection = Vector3.ProjectOnPlane(Vector3.Cross(stabilityReport.LedgeGroundNormal, stabilityReport.LedgeRightDirection), CharacterUp).normalized;
				stabilityReport.DistanceFromLedge = Vector3.ProjectOnPlane(hitPoint - (atCharacterPosition + _characterTransformToCapsuleBottom), up).magnitude;
				stabilityReport.IsMovingTowardsEmptySideOfLedge = Vector3.Dot(withCharacterVelocity.normalized, stabilityReport.LedgeFacingDirection) > 0f;
			}
			if (stabilityReport.IsStable)
			{
				stabilityReport.IsStable = IsStableWithSpecialCases(ref stabilityReport, withCharacterVelocity);
			}
		}
		if (StepHandling != StepHandlingMethod.None && !stabilityReport.IsStable)
		{
			Rigidbody attachedRigidbody = hitCollider.attachedRigidbody;
			if (!attachedRigidbody || attachedRigidbody.isKinematic)
			{
				DetectSteps(atCharacterPosition, hitPoint, normalized, ref stabilityReport);
				if (stabilityReport.ValidStepDetected)
				{
					stabilityReport.IsStable = true;
				}
			}
		}
		CharacterController.ProcessHitStabilityReport(hitCollider, hitNormal, hitPoint, atCharacterPosition, ref stabilityReport);
	}

	private void DetectSteps(Vector3 characterPosition, Vector3 hitPoint, Vector3 innerHitDirection, ref HitStabilityReport stabilityReport)
	{
		int num = 0;
		Vector3 up = Vector3.up;
		Vector3 vector = Vector3.Project(hitPoint - characterPosition, up);
		Vector3 normalized = Vector3.ProjectOnPlane(hitPoint - characterPosition, up).normalized;
		Vector3 vector2 = hitPoint - vector + up * MaxStepHeight + normalized * 0.01f * 3f;
		num = CharacterCollisionsSweep(vector2, -up, MaxStepHeight + 0.01f, out var closestHit, _internalCharacterHits, 0f, acceptOnlyStableGroundLayer: true);
		if (CheckStepValidity(num, characterPosition, innerHitDirection, vector2, out var hitCollider))
		{
			stabilityReport.ValidStepDetected = true;
			stabilityReport.SteppedCollider = hitCollider;
		}
		if (StepHandling == StepHandlingMethod.Extra && !stabilityReport.ValidStepDetected)
		{
			vector2 = characterPosition + up * MaxStepHeight + -innerHitDirection * MinRequiredStepDepth;
			num = CharacterCollisionsSweep(vector2, -up, MaxStepHeight - 0.01f, out closestHit, _internalCharacterHits, 0f, acceptOnlyStableGroundLayer: true);
			if (CheckStepValidity(num, characterPosition, innerHitDirection, vector2, out hitCollider))
			{
				stabilityReport.ValidStepDetected = true;
				stabilityReport.SteppedCollider = hitCollider;
			}
		}
	}

	private bool CheckStepValidity(int nbStepHits, Vector3 characterPosition, Vector3 innerHitDirection, Vector3 stepCheckStartPos, out Collider hitCollider)
	{
		hitCollider = null;
		Vector3 up = Vector3.up;
		bool flag = false;
		while (nbStepHits > 0 && !flag)
		{
			RaycastHit raycastHit = default(RaycastHit);
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < nbStepHits; i++)
			{
				float distance = _internalCharacterHits[i].distance;
				if (distance > num)
				{
					num = distance;
					raycastHit = _internalCharacterHits[i];
					num2 = i;
				}
			}
			Vector3 vector = stepCheckStartPos + -up * (raycastHit.distance - 0.01f);
			if (CharacterCollisionsOverlap(vector, _internalProbedColliders) <= 0 && CharacterCollisionsRaycast(raycastHit.point + up * 0.02f + -innerHitDirection * 0.001f, -up, MaxStepHeight + 0.02f, out var closestHit, _internalCharacterHits, acceptOnlyStableGroundLayer: true) > 0 && IsStableOnNormal(closestHit.normal) && CharacterCollisionsSweep(characterPosition, up, MaxStepHeight - raycastHit.distance, out var _, _internalCharacterHits) <= 0)
			{
				bool flag2 = false;
				RaycastHit closestHit3;
				if (AllowSteppingWithoutStableGrounding)
				{
					flag2 = true;
				}
				else if (CharacterCollisionsRaycast(characterPosition + Vector3.Project(vector - characterPosition, up), -up, MaxStepHeight, out closestHit3, _internalCharacterHits, acceptOnlyStableGroundLayer: true) > 0 && IsStableOnNormal(closestHit3.normal))
				{
					flag2 = true;
				}
				if (!flag2 && CharacterCollisionsRaycast(raycastHit.point + innerHitDirection * 0.001f, -up, MaxStepHeight, out closestHit3, _internalCharacterHits, acceptOnlyStableGroundLayer: true) > 0 && IsStableOnNormal(closestHit3.normal))
				{
					flag2 = true;
				}
				if (flag2)
				{
					hitCollider = raycastHit.collider;
					flag = true;
					return true;
				}
			}
			if (!flag)
			{
				nbStepHits--;
				if (num2 < nbStepHits)
				{
					_internalCharacterHits[num2] = _internalCharacterHits[nbStepHits];
				}
			}
		}
		return false;
	}

	public void GetVelocityFromRigidbodyMovement(Rigidbody interactiveRigidbody, Vector3 atPoint, float deltaTime, out Vector3 linearVelocity, out Vector3 angularVelocity)
	{
		if (deltaTime > 0f)
		{
			linearVelocity = interactiveRigidbody.linearVelocity;
			angularVelocity = interactiveRigidbody.angularVelocity;
			if (interactiveRigidbody.isKinematic)
			{
				PhysicsMover component = interactiveRigidbody.GetComponent<PhysicsMover>();
				if ((bool)component)
				{
					linearVelocity = component.Velocity;
					angularVelocity = component.AngularVelocity;
				}
			}
			if (angularVelocity != Vector3.zero)
			{
				Vector3 vector = interactiveRigidbody.transform.TransformPoint(interactiveRigidbody.centerOfMass);
				Vector3 vector2 = atPoint - vector;
				Quaternion quaternion = Quaternion.Euler(57.29578f * angularVelocity * deltaTime);
				Vector3 vector3 = vector + quaternion * vector2;
				linearVelocity += (vector3 - atPoint) / deltaTime;
			}
		}
		else
		{
			linearVelocity = default(Vector3);
			angularVelocity = default(Vector3);
		}
	}

	private Rigidbody GetInteractiveRigidbody(Collider onCollider)
	{
		Rigidbody attachedRigidbody = onCollider.attachedRigidbody;
		if ((bool)attachedRigidbody)
		{
			if ((bool)attachedRigidbody.gameObject.GetComponent<PhysicsMover>())
			{
				return attachedRigidbody;
			}
			if (!attachedRigidbody.isKinematic)
			{
				return attachedRigidbody;
			}
		}
		return null;
	}

	public Vector3 GetVelocityForMovePosition(Vector3 fromPosition, Vector3 toPosition, float deltaTime)
	{
		return GetVelocityFromMovement(toPosition - fromPosition, deltaTime);
	}

	public Vector3 GetVelocityFromMovement(Vector3 movement, float deltaTime)
	{
		if (deltaTime <= 0f)
		{
			return Vector3.zero;
		}
		return movement / deltaTime;
	}

	private void RestrictVectorToPlane(ref Vector3 vector, Vector3 toPlane)
	{
		if (vector.x > 0f != toPlane.x > 0f)
		{
			vector.x = 0f;
		}
		if (vector.y > 0f != toPlane.y > 0f)
		{
			vector.y = 0f;
		}
		if (vector.z > 0f != toPlane.z > 0f)
		{
			vector.z = 0f;
		}
	}

	public int CharacterCollisionsOverlap(Vector3 position, Collider[] overlappedColliders, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
	{
		int layerMask = CollidableLayers;
		if (acceptOnlyStableGroundLayer)
		{
			layerMask = (int)CollidableLayers & (int)StableGroundLayers;
		}
		Vector3 point = position + _characterTransformToCapsuleBottomHemi;
		Vector3 point2 = position + _characterTransformToCapsuleTopHemi;
		if (inflate != 0f)
		{
			point += Vector3.down * inflate;
			point2 += Vector3.up * inflate;
		}
		int num = 0;
		for (int num2 = (num = Physics.OverlapCapsuleNonAlloc(point, point2, Capsule.radius + inflate, overlappedColliders, layerMask, QueryTriggerInteraction.Ignore)) - 1; num2 >= 0; num2--)
		{
			if (!CheckIfColliderValidForCollisions(overlappedColliders[num2]))
			{
				num--;
				if (num2 < num)
				{
					overlappedColliders[num2] = overlappedColliders[num];
				}
			}
		}
		return num;
	}

	public int CharacterOverlap(Vector3 position, Collider[] overlappedColliders, LayerMask layers, QueryTriggerInteraction triggerInteraction, float inflate = 0f)
	{
		Vector3 point = position + _characterTransformToCapsuleBottomHemi;
		Vector3 point2 = position + _characterTransformToCapsuleTopHemi;
		if (inflate != 0f)
		{
			point += Vector3.down * inflate;
			point2 += Vector3.up * inflate;
		}
		int num = 0;
		for (int num2 = (num = Physics.OverlapCapsuleNonAlloc(point, point2, Capsule.radius + inflate, overlappedColliders, layers, triggerInteraction)) - 1; num2 >= 0; num2--)
		{
			if (overlappedColliders[num2] == Capsule)
			{
				num--;
				if (num2 < num)
				{
					overlappedColliders[num2] = overlappedColliders[num];
				}
			}
		}
		return num;
	}

	public int CharacterCollisionsSweep(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
	{
		int layerMask = CollidableLayers;
		if (acceptOnlyStableGroundLayer)
		{
			layerMask = (int)CollidableLayers & (int)StableGroundLayers;
		}
		Vector3 point = position + _characterTransformToCapsuleBottomHemi - direction * 0.002f;
		Vector3 point2 = position + _characterTransformToCapsuleTopHemi - direction * 0.002f;
		if (inflate != 0f)
		{
			point += Vector3.down * inflate;
			point2 += Vector3.up * inflate;
		}
		int num = 0;
		int num2 = Physics.CapsuleCastNonAlloc(point, point2, Capsule.radius + inflate, direction, hits, distance + 0.002f, layerMask, QueryTriggerInteraction.Ignore);
		closestHit = default(RaycastHit);
		float num3 = float.PositiveInfinity;
		num = num2;
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			hits[num4].distance -= 0.002f;
			RaycastHit raycastHit = hits[num4];
			float distance2 = raycastHit.distance;
			if (distance2 <= 0f || !CheckIfColliderValidForCollisions(raycastHit.collider))
			{
				num--;
				if (num4 < num)
				{
					hits[num4] = hits[num];
				}
			}
			else if (distance2 < num3)
			{
				closestHit = raycastHit;
				num3 = distance2;
			}
		}
		return num;
	}

	public int CharacterSweep(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, LayerMask layers, QueryTriggerInteraction triggerInteraction, float inflate = 0f)
	{
		closestHit = default(RaycastHit);
		Vector3 point = position + _characterTransformToCapsuleBottomHemi;
		Vector3 point2 = position + _characterTransformToCapsuleTopHemi;
		if (inflate != 0f)
		{
			point += Vector3.down * inflate;
			point2 += Vector3.up * inflate;
		}
		int num = Physics.CapsuleCastNonAlloc(point, point2, Capsule.radius + inflate, direction, hits, distance, layers, triggerInteraction);
		float num2 = float.PositiveInfinity;
		int num3 = num;
		for (int num4 = num - 1; num4 >= 0; num4--)
		{
			RaycastHit raycastHit = hits[num4];
			if (raycastHit.distance <= 0f || raycastHit.collider == Capsule)
			{
				num3--;
				if (num4 < num3)
				{
					hits[num4] = hits[num3];
				}
			}
			else
			{
				float distance2 = raycastHit.distance;
				if (distance2 < num2)
				{
					closestHit = raycastHit;
					num2 = distance2;
				}
			}
		}
		return num3;
	}

	private bool CharacterGroundSweep(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit)
	{
		closestHit = default(RaycastHit);
		int num = Physics.CapsuleCastNonAlloc(position + _characterTransformToCapsuleBottomHemi - direction * 0.1f, position + _characterTransformToCapsuleTopHemi - direction * 0.1f, Capsule.radius, direction, _internalCharacterHits, distance + 0.1f, (int)CollidableLayers & (int)StableGroundLayers, QueryTriggerInteraction.Ignore);
		bool result = false;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = _internalCharacterHits[i];
			float distance2 = raycastHit.distance;
			if (distance2 > 0f && CheckIfColliderValidForCollisions(raycastHit.collider) && distance2 < num2)
			{
				closestHit = raycastHit;
				closestHit.distance -= 0.1f;
				num2 = distance2;
				result = true;
			}
		}
		return result;
	}

	public int CharacterCollisionsRaycast(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, bool acceptOnlyStableGroundLayer = false)
	{
		int layerMask = CollidableLayers;
		if (acceptOnlyStableGroundLayer)
		{
			layerMask = (int)CollidableLayers & (int)StableGroundLayers;
		}
		int num = 0;
		int num2 = Physics.RaycastNonAlloc(position, direction, hits, distance, layerMask, QueryTriggerInteraction.Ignore);
		closestHit = default(RaycastHit);
		float num3 = float.PositiveInfinity;
		num = num2;
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			RaycastHit raycastHit = hits[num4];
			float distance2 = raycastHit.distance;
			if (distance2 <= 0f || !CheckIfColliderValidForCollisions(raycastHit.collider))
			{
				num--;
				if (num4 < num)
				{
					hits[num4] = hits[num];
				}
			}
			else if (distance2 < num3)
			{
				closestHit = raycastHit;
				num3 = distance2;
			}
		}
		return num;
	}
}
