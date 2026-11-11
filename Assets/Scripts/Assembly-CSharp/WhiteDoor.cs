using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WhiteDoor : MonoBehaviour
{
	public UnityEvent _onEnter;

	public Transform m_TeleportPoint;

	public Image m_WhiteScreen;

	[Space(10f)]
	[Tooltip("Minimal distance to start the effect")]
	public float m_MinDistance = 1f;

	[Tooltip("Distance at which effect has full force")]
	public float m_EndDistance = 0.3f;

	private Collider doorCollider;

	private Color screenColor;

	private float distanceMultiplier;

	private float dotProductMultiplier;

	private float whiteScreenMultiplier;

	private void Start()
	{
		doorCollider = GetComponent<Collider>();
	}

	private void Update()
	{
		float num = Vector3.Distance(doorCollider.ClosestPoint(Player.Instance.transform.position), Player.Instance.transform.position);
		distanceMultiplier = Mathf.InverseLerp(0f, 1f, 1f - (num - m_EndDistance) / (m_MinDistance - m_EndDistance));
		float num2 = Vector3.Dot(base.transform.forward, Player.Instance.transform.forward);
		dotProductMultiplier = ((num2 < 0f) ? Mathf.Abs(num2) : 0f);
		whiteScreenMultiplier = distanceMultiplier * dotProductMultiplier / (distanceMultiplier + dotProductMultiplier - distanceMultiplier * dotProductMultiplier);
		screenColor = m_WhiteScreen.color;
		screenColor.a = whiteScreenMultiplier;
		m_WhiteScreen.color = screenColor;
		if ((double)whiteScreenMultiplier >= 0.99)
		{
			Player.Instance.Motor.SetPosition(m_TeleportPoint.position);
			Player.Instance.transform.rotation = m_TeleportPoint.rotation;
			StartCoroutine(FadeOut(1f));
			_onEnter.Invoke();
			base.enabled = false;
		}
	}

	private IEnumerator FadeOut(float duration)
	{
		float time = 0f;
		screenColor = m_WhiteScreen.color;
		while (time < duration)
		{
			screenColor.a = Mathf.Lerp(1f, 0f, time / duration);
			m_WhiteScreen.color = screenColor;
			time += Time.deltaTime;
			yield return null;
		}
		screenColor.a = 0f;
		m_WhiteScreen.color = screenColor;
	}
}
