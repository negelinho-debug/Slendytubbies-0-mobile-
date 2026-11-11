using UnityEngine;

public class HeadLookAt : MonoBehaviour
{
	[Header("References")]
	public Transform head;

	public Transform aimTarget;

	public Transform lookAtTarget;

	public Transform rootRef;

	[Header("Settings")]
	[Range(0f, 1f)]
	public float weight = 1f;

	public float followSpeed = 8f;

	public float rotationSmooth = 8f;

	[Header("Limits (Degrees)")]
	public float maxHorizontalAngle = 60f;

	public float maxVerticalAngle = 40f;

	private Vector3 smoothedDirection;

	private void Start()
	{
		if (rootRef == null)
		{
			rootRef = head.parent;
		}
		smoothedDirection = rootRef.forward;
	}

	private void LateUpdate()
	{
		if (!(head == null) && !(aimTarget == null))
		{
			Vector3 b;
			if (lookAtTarget == null || weight <= 0f)
			{
				b = rootRef.forward;
			}
			else
			{
				Vector3 normalized = (lookAtTarget.position - head.position).normalized;
				Vector3 vector = rootRef.InverseTransformDirection(normalized);
				float value = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
				float value2 = (0f - Mathf.Asin(vector.y)) * 57.29578f;
				float angle = Mathf.Clamp(value, 0f - maxHorizontalAngle, maxHorizontalAngle);
				value2 = Mathf.Clamp(value2, 0f - maxVerticalAngle, maxVerticalAngle);
				b = Quaternion.AngleAxis(angle, rootRef.up) * Quaternion.AngleAxis(value2, rootRef.right) * rootRef.forward;
			}
			smoothedDirection = Vector3.Slerp(smoothedDirection, b, Time.deltaTime * rotationSmooth);
			Vector3 b2 = head.position + smoothedDirection * 2f;
			aimTarget.position = Vector3.Lerp(aimTarget.position, b2, Time.deltaTime * followSpeed * weight);
		}
	}

	public void SetTarget(Transform target)
	{
		lookAtTarget = target;
	}
}
