using System;
using HostGame;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, ICharacterController
{
	[Serializable]
	public struct CapsuleCheckCommand
	{
		public Vector3 start;

		public Vector3 end;

		public float radius;

		public LayerMask mask;

		public bool Check(Vector3 point)
		{
			Vector3 vector = start + point;
			Vector3 vector2 = end + point;
			return Physics.CheckCapsule(vector, vector2, radius, mask);
		}

		public void DrawGizmo(Vector3 point)
		{
			Gizmos.DrawWireSphere(point + start, radius);
			Gizmos.DrawWireSphere(point + end, radius);
		}
	}

	private InputData input;

	[SerializeField]
	private KinematicCharacterMotor motor;

	private bool wantedCrouchLastFrame;

	private float originalCapsuleHeight;

	private float jumpInput;

	private float speed;

	private Vector3 externalForceDirection;

	private float externalForceSpeed;

	private float stamina;

	private float timeUntilRegen;

	[Header("Speed Settings")]
	[SerializeField]
	private float m_GravityMultiplier = 1f;

	[Tooltip("Meters gained per second")]
	[SerializeField]
	private float m_Acceleration = 7.5f;

	public float walkSpeedMultiplier = 1f;

	[SerializeField]
	public float m_WalkSpeed = 2.5f;

	[Header("Run")]
	[SerializeField]
	private float m_RunSpeed = 7.5f;

	[SerializeField]
	private float m_RunningAcceleration = 10f;

	[Tooltip("Amount of speed added when changing walk to run (only if player speed is less or equal to WalkSpeed)")]
	[SerializeField]
	private float m_RunningBoost = 1.5f;

	[Tooltip("Used when going from run to walk")]
	[SerializeField]
	private float m_Deacceleration = 10f;

	[Space]
	[SerializeField]
	private float m_RunStaminaCost = 10f;

	[Header("Jump")]
	[Range(0f, 1f)]
	[SerializeField]
	private float m_OnLostGroundSpeedLost;

	[Min(0f)]
	[SerializeField]
	private float m_JumpHeight;

	[Min(0f)]
	[SerializeField]
	private float m_InAirSpeed = 0.1f;

	[Min(0f)]
	[SerializeField]
	private float m_InSlideSpeed = 0.5f;

	[Min(0f)]
	[SerializeField]
	private float m_MaxJumpInputDelay = 0.1f;

	[Header("Crouch")]
	[SerializeField]
	private float m_CrouchSpeed = 1.2f;

	[Min(0f)]
	[Tooltip("Set to 0 to disable crouching")]
	[SerializeField]
	private float m_CrouchHeight = 0.9f;

	[SerializeField]
	private EaseCurveType m_CrouchEase = EaseCurveType.EaseInOutSin;

	[SerializeField]
	private AnimatedBool m_CrouchAnimationTime = new AnimatedBool(value: false, 2.857143f);

	[SerializeField]
	private CapsuleCheckCommand m_CeilingCheck;

	[Header("Push")]
	[SerializeField]
	private float m_MaxSpeedWhenPushed = 5f;

	[SerializeField]
	private float m_MaxSpeedFromExternalForce = 50f;

	[SerializeField]
	private float m_CharacterFriction = 0.8f;

	public float WalkSpeed => m_WalkSpeed;

	public float CrouchSpeed => m_CrouchSpeed;

	public float RunSpeed => m_RunSpeed;

	public AnimatedBool CrouchAnimationTime => m_CrouchAnimationTime;

	public bool IsRunning { get; private set; }

	public InputData MotorInput => input;

	public bool IsFreezed { get; set; }

	public float Stamina => stamina;

	public bool Noclip { get; set; }

	public void RegenStamina()
	{
		stamina = 1f;
	}

	private void Awake()
	{
		motor.CharacterController = this;
		originalCapsuleHeight = motor.Capsule.height;
	}

	private void OnEnable()
	{
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
	}

	private void OnDestroy()
	{
	}

	public bool IsColliderValidForCollisions(Collider coll)
	{
		return !Noclip;
	}

	private void Update()
	{
		InputManager instance = InputManager.Instance;
		bool flag = false;
		bool num = instance.SpaceTriggered();
		float deltaTime = Time.deltaTime;
		if (m_CrouchHeight > 0f)
		{
			if (wantedCrouchLastFrame && !flag)
			{
				flag |= m_CeilingCheck.Check(base.transform.position);
			}
			wantedCrouchLastFrame = flag;
			m_CrouchAnimationTime.SetTarget(flag);
			float value = m_CrouchAnimationTime.Value;
			float num2 = m_CrouchAnimationTime.Update(deltaTime);
			if (value != num2)
			{
				num2 = m_CrouchEase.Evaluate(num2);
				motor.SetCapsuleHeight(Mathf.Lerp(originalCapsuleHeight, m_CrouchHeight, num2));
			}
		}
		jumpInput -= deltaTime;
		if (num && !IsFreezed)
		{
			jumpInput = m_MaxJumpInputDelay;
		}
		_ = motor.BaseVelocity.magnitude;
		_ = CrouchAnimationTime.Value;
		if (base.transform.position.y < -200f)
		{
			LevelSwitch.instance.SwitchLevel(SceneManager.GetActiveScene().name, LevelSwitch.SwitchType.NoiseNoIntro);
		}
	}

	void ICharacterController.UpdateVelocity(ref Vector3 velocity, float deltaTime)
	{
		Vector3 vec = InputManager.Instance.GetMovement().ToV3();
		if (IsFreezed)
		{
			vec = default(Vector3);
			speed = 0f;
		}
		bool flag = vec.sqrMagnitude > 1E-05f;
		if (flag)
		{
			vec = base.transform.TransformDirection(vec.normalized);
		}
		bool flag2 = false;
		if (!motor.GroundingStatus.IsStableOnGround && motor.LastGroundingStatus.IsStableOnGround)
		{
			Vector3 v = Vector3.ProjectOnPlane(velocity, Vector3.up);
			v *= 1f - m_OnLostGroundSpeedLost;
			velocity = v.WithY(velocity.y);
		}
		IsRunning = false;
		if (!motor.GroundingStatus.HasAnyGround || !motor.GroundingStatus.IsStableOnGround)
		{
			velocity += Physics.gravity * (deltaTime * m_GravityMultiplier);
			if (flag && ((m_InAirSpeed > 0f) | (m_InSlideSpeed > 0f)))
			{
				Vector3 vector = Vector3.ProjectOnPlane(velocity, Vector3.up);
				vec *= (motor.GroundingStatus.HasAnyGround ? m_InAirSpeed : m_InSlideSpeed);
				vec += vector;
				vec = Vector3.ClampMagnitude(vec, Mathf.Max(m_InAirSpeed, vector.magnitude));
				velocity = vec.WithY(velocity.y);
			}
		}
		else if (!flag)
		{
			speed = 0f;
			velocity = default(Vector3);
			if (externalForceSpeed > 0f)
			{
				velocity += externalForceDirection * Mathf.Min(externalForceSpeed, m_MaxSpeedWhenPushed);
				velocity = velocity.Reorient(in motor.GroundingStatus.GroundNormal);
			}
		}
		else
		{
			float a;
			float num;
			if (flag2)
			{
				a = m_RunSpeed;
				num = m_RunningAcceleration;
			}
			else if (speed > m_WalkSpeed)
			{
				a = Mathf.Lerp(m_RunSpeed, m_CrouchSpeed, m_CrouchAnimationTime.Value);
				num = 0f - m_Deacceleration;
			}
			else
			{
				a = Mathf.Lerp(m_WalkSpeed, m_CrouchSpeed, m_CrouchAnimationTime.Value);
				num = m_Acceleration;
			}
			speed = Mathf.Min(a, speed + deltaTime * num) * walkSpeedMultiplier;
			vec *= speed;
			if (externalForceSpeed > 0f)
			{
				vec = CombineWithExternalForce(vec);
				speed = Mathf.Min(m_MaxSpeedWhenPushed, speed);
			}
			vec = vec.Reorient(in motor.GroundingStatus.GroundNormal);
			velocity = vec;
		}
		float num2 = externalForceSpeed * externalForceSpeed / (19.62f * m_CharacterFriction);
		float num3 = externalForceSpeed * externalForceSpeed / (2f * num2);
		externalForceSpeed -= deltaTime * num3;
		externalForceSpeed = Mathf.Max(externalForceSpeed, 0f);
		if (m_JumpHeight > 0f && jumpInput > 0f && motor.GroundingStatus.HasAnyGround)
		{
			jumpInput = 0f;
			Vector3 vector2 = (motor.GroundingStatus.IsStableOnGround ? Vector3.up : motor.GroundingStatus.GroundNormal);
			float num4 = Mathf.Sqrt(Mathf.Abs(2f * m_JumpHeight * Physics.gravity.y));
			motor.ForceUnground();
			velocity += vector2 * num4;
		}
		walkSpeedMultiplier = 1f;
	}

	public void Push(Vector3 force)
	{
		force = CombineWithExternalForce(force);
		externalForceSpeed = force.magnitude;
		externalForceDirection = force.normalized;
	}

	public bool UseStamina(float amount)
	{
		bool result = stamina > 0f;
		stamina -= amount;
		stamina = Mathf.Max(0f, stamina);
		timeUntilRegen = 1f;
		return result;
	}

	private Vector3 CombineWithExternalForce(Vector3 direction)
	{
		float num = Mathf.Min(externalForceSpeed, m_MaxSpeedFromExternalForce);
		float num2 = Mathf.Max(direction.magnitude, num);
		Vector3 vector = externalForceDirection * num;
		return (vector + direction).normalized * num2;
	}
}
