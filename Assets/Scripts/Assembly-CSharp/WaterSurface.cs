using UnityEngine;

public class WaterSurface : MonoBehaviour
{
	public Transform m_WaterPushOrigin;

	public Vector3 m_PushDirection;

	public float m_PushForce = 3f;

	public float m_MaxPushDistance = 10f;

	public float m_MinPushDistance = 5f;

	private float distance;

	private float forceMultiplier;

	private void OnTriggerEnter(Collider other)
	{
		FootstepSystem component = other.GetComponent<FootstepSystem>();
		if (component != null)
		{
			component.SetInWater(active: true);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if ((bool)Player.Instance && other == Player.Instance.Capsule)
		{
			distance = Mathf.Abs((m_WaterPushOrigin.transform.position - Player.Instance.transform.position).y);
			forceMultiplier = Mathf.InverseLerp(0f, 1f, (distance - m_MinPushDistance) / (m_MaxPushDistance - m_MinPushDistance));
			if (forceMultiplier > 0f)
			{
				Player.Instance.Controller.Push(m_PushDirection.normalized * forceMultiplier * m_PushForce);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		FootstepSystem component = other.GetComponent<FootstepSystem>();
		if (component != null)
		{
			component.SetInWater(active: false);
		}
	}
}
