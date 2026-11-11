using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UberEvents : MonoBehaviour
{
	public enum InvokeMethod
	{
		ButtonPress,
		ButtonInTrigger,
		OnTrigger,
		OnTimer,
		OnStart,
		OnVideoFinish
	}

	public enum Keys
	{
		Space,
		Esc,
		LMB,
		R,
		E
	}

	public InvokeMethod m_Method;

	public Keys m_InvokeKey;

	public bool m_OneTimeUse;

	public float m_InvokeDelay;

	public UnityEvent m_InvokeEvents;

	private bool wasUsed;

	private float invokeTime;

	private void Start()
	{
		if (m_Method == InvokeMethod.OnStart)
		{
			m_InvokeEvents?.Invoke();
			wasUsed = true;
		}
		invokeTime = Time.time + m_InvokeDelay;
	}

	private void OnEnable()
	{
		if (m_Method == InvokeMethod.OnVideoFinish)
		{
			VideoManager.OnFinished = (Action)Delegate.Combine(VideoManager.OnFinished, new Action(InvokeOnVideo));
		}
	}

	private void OnDisable()
	{
		VideoManager.OnFinished = (Action)Delegate.Remove(VideoManager.OnFinished, new Action(InvokeOnVideo));
	}

	private void InvokeOnVideo()
	{
		m_InvokeEvents?.Invoke();
		wasUsed = true;
	}

	private void Update()
	{
		if (m_Method == InvokeMethod.ButtonPress)
		{
			InvokeOnButtonPress();
		}
		else if (m_Method == InvokeMethod.OnTimer && Time.time >= invokeTime)
		{
			m_InvokeEvents?.Invoke();
			wasUsed = true;
		}
		if (m_OneTimeUse && wasUsed)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (m_Method == InvokeMethod.OnTrigger && other == Player.Instance.Capsule)
		{
			m_InvokeEvents?.Invoke();
			wasUsed = true;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (m_Method == InvokeMethod.ButtonInTrigger && other == Player.Instance.Capsule)
		{
			InvokeOnButtonPress();
		}
	}

	private void InvokeOnButtonPress()
	{
		switch (m_InvokeKey)
		{
		case Keys.Space:
			if (InputManager.Instance.SpaceTriggered())
			{
				m_InvokeEvents?.Invoke();
				wasUsed = true;
			}
			break;
		case Keys.Esc:
			if (InputManager.Instance.EscTriggered())
			{
				m_InvokeEvents?.Invoke();
				wasUsed = true;
			}
			break;
		case Keys.LMB:
			if (InputManager.Instance.LMBTriggered())
			{
				m_InvokeEvents?.Invoke();
				wasUsed = true;
			}
			break;
		case Keys.R:
			if (InputManager.Instance.TriggeredR())
			{
				m_InvokeEvents?.Invoke();
				wasUsed = true;
			}
			break;
		case Keys.E:
			if (InputManager.Instance.TriggeredE())
			{
				m_InvokeEvents?.Invoke();
				wasUsed = true;
			}
			break;
		}
	}

	public void AddScene(string levelName)
	{
		SceneManager.LoadScene(levelName, LoadSceneMode.Additive);
	}

	public void ChangeScene(string levelName)
	{
		SceneManager.LoadScene(levelName);
	}
}
