using HostGame;
using KinematicCharacterController;
using Unity.Mathematics;
using UnityEngine;

public class PlayerHandsSway : MonoBehaviour
{
	private Vector3 myPosition;

	private Quaternion myRotation;

	private TimerStamp lowerHandsTimer;

	private Vector3 handsHeightOffset;

	private Quaternion loweringRotation;

	private float loweringTargetAngle;

	private float loweringTargetAngleVelocity;

	private float loweringTime;

	private float bobTime;

	private float bobStrength;

	private float bobStrengthVelocity;

	public Transform _camera;

	[SerializeField]
	private Vector3 m_LocalPositionOffset;

	[SerializeField]
	private float m_MaxTimestep = 0.025f;

	[SerializeField]
	private float m_MaxAngularVelocity = 100f;

	[Header("Bob")]
	[SerializeField]
	private bool m_EnableBob = true;

	[SerializeField]
	private float m_BobSmoothTime = 0.1f;

	[SerializeField]
	private float m_BobSpeed = 11f;

	[SerializeField]
	private float m_BobMagnitude = 0.01f;

	[Header("Position Sway")]
	[SerializeField]
	private bool m_EnablePositionSway = true;

	[SerializeField]
	private float m_PositionSwayTime;

	[SerializeField]
	private float m_MaxPositionSwayDistance;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_HandsToZero;

	[CurveSetup(CurveColor.Green, 0f, 1f, 0f, 1f)]
	[SerializeField]
	private AnimationCurve m_OffsetCurve;

	[Header("Sway Settings")]
	[SerializeField]
	private bool m_EnableRotationSway = true;

	[SerializeField]
	private float m_MaxCameraDeviationAngle = 10f;

	[Tooltip("How strongly the hands are pulled along when the camera rotates.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float cameraFollowStrength = 0.75f;

	[SerializeField]
	[Tooltip("How strongly the player's hands are pulled towards the camera's orientation. This is multiplied by the angle between these two.")]
	[Range(0f, 300f)]
	private float springForce = 250f;

	[SerializeField]
	[Tooltip("How quickly the hands lose their inertia.")]
	[Range(1f, 50f)]
	private float swayDrag = 20f;

	[SerializeField]
	private Vector3 m_HandsLowering;

	private Vector3 angularVelocity = Vector3.zero;

	private Quaternion lastCameraRotation;

	private Vector3 swayPositionVelocity;

	private Vector3 swayLocalPos;

	private void Start()
	{
		swayPositionVelocity = default(Vector3);
		loweringRotation = Quaternion.identity;
		base.transform.GetPositionAndRotation(out myPosition, out myRotation);
		lastCameraRotation = _camera.rotation;
	}

	private void LateUpdate()
	{
		Transform camera = _camera;
		Player instance = Player.Instance;
		KinematicCharacterMotor motor = Player.Instance.Motor;
		if (m_EnablePositionSway)
		{
			Vector3 velocity = motor.Velocity;
			Vector3 vector = motor.transform.InverseTransformDirection(velocity);
			float f = vector.x / instance.Controller.WalkSpeed;
			f = m_OffsetCurve.Evaluate(Mathf.Abs(f)) * Mathf.Sign(f);
			float f2 = vector.y / instance.Controller.WalkSpeed;
			f2 = m_OffsetCurve.Evaluate(Mathf.Abs(f2)) * Mathf.Sign(f2);
			float f3 = vector.z / instance.Controller.WalkSpeed;
			f3 = m_OffsetCurve.Evaluate(Mathf.Abs(f3)) * Mathf.Sign(f3);
			Vector3 target = new Vector3(f, f2, f3) * m_MaxPositionSwayDistance;
			Vector3 vector2 = Vector3.SmoothDamp(swayLocalPos, target, ref swayPositionVelocity, m_PositionSwayTime);
			swayPositionVelocity = math.select(swayPositionVelocity, 0f, math.isnan(swayPositionVelocity));
			swayLocalPos = math.select(vector2, swayLocalPos, math.isnan(vector2));
			Vector3 vector3 = new Vector3(swayLocalPos.x, 0f - swayLocalPos.z, swayLocalPos.y);
			Vector3 a = base.transform.localPosition - m_LocalPositionOffset;
			a = Vector3.Lerp(a, Vector3.zero, m_HandsToZero);
			float target2 = 0f;
			if (m_EnableBob && velocity.sqrMagnitude > 0f)
			{
				bobTime += Time.deltaTime * m_BobSpeed;
				target2 = 1f;
			}
			bobStrength = Mathf.SmoothDamp(bobStrength, target2, ref bobStrengthVelocity, m_BobSmoothTime);
			float y = bobStrength * m_BobMagnitude * Mathf.Sin(bobTime);
			base.transform.localPosition = a + vector3 + m_LocalPositionOffset + new Vector3(0f, y, 0f);
			Vector3 position = camera.position;
			Vector3 vector4 = _camera.TransformDirection(swayLocalPos);
			_ = position - vector4;
		}
		if (m_EnableRotationSway)
		{
			base.transform.localRotation *= Quaternion.Inverse(loweringRotation);
			float num = Mathf.Min(m_MaxTimestep, Time.smoothDeltaTime);
			Quaternion rotation = camera.rotation;
			angularVelocity *= Mathf.Max(0f, 1f - swayDrag * num);
			Quaternion quaternion = Quaternion.Inverse(lastCameraRotation) * rotation;
			myRotation = Quaternion.Slerp(myRotation, myRotation * quaternion, cameraFollowStrength);
			float num2 = Quaternion.Angle(myRotation, rotation);
			float maxDegreesDelta = Mathf.Max(0f, num2 - m_MaxCameraDeviationAngle);
			myRotation = Quaternion.RotateTowards(myRotation, myRotation * quaternion, maxDegreesDelta);
			Vector3 eulerAngles = (Quaternion.Inverse(base.transform.rotation) * rotation).eulerAngles;
			eulerAngles = NormalizeEulerAngles(eulerAngles);
			float num3 = Quaternion.Angle(myRotation, rotation);
			float num4 = springForce * num3;
			angularVelocity += base.transform.TransformVector(eulerAngles).normalized * num4 * num;
			angularVelocity = math.clamp(angularVelocity, 0f - m_MaxAngularVelocity, m_MaxAngularVelocity);
			Quaternion rotation2 = (myRotation *= Quaternion.Euler(base.transform.InverseTransformVector(angularVelocity * num)));
			float target3 = 0f;
			if (!lowerHandsTimer.RanOut)
			{
				target3 = m_HandsLowering.y;
			}
			loweringTargetAngle = Mathf.SmoothDamp(loweringTargetAngle, target3, ref loweringTargetAngleVelocity, loweringTime);
			loweringRotation = Quaternion.AngleAxis(loweringTargetAngle, Vector3.right);
			lastCameraRotation = rotation;
			base.transform.rotation = rotation2;
			base.transform.localRotation *= loweringRotation;
		}
	}

	private Vector3 NormalizeEulerAngles(Vector3 angles)
	{
		return new Vector3(NormalizeAngle(angles.x), NormalizeAngle(angles.y), NormalizeAngle(angles.z));
		static float NormalizeAngle(float angle)
		{
			if (angle > 180f)
			{
				return angle - 360f;
			}
			if (angle < -180f)
			{
				return angle + 360f;
			}
			return angle;
		}
	}

	private static float fast_negexp(float x)
	{
		return 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
	}

	public void Lower(float time, float animTime = 0.35f)
	{
		loweringTime = animTime;
		lowerHandsTimer = TimerStamp.FromSeconds(time);
	}
}
