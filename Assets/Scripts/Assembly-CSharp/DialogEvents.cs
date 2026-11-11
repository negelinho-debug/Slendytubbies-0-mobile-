using System;
using UnityEngine;
using UnityEngine.Events;

public class DialogEvents : MonoBehaviour
{
	[Serializable]
	public struct LogEvent
	{
		public int DialogIndex;

		public UnityEvent DialogEvent;
	}

	public LogEvent[] _logEvents;

	private void OnEnable()
	{
		Dialogue.OnLineChanged = (Action<int>)Delegate.Combine(Dialogue.OnLineChanged, new Action<int>(InvokeLogEvent));
	}

	private void OnDisable()
	{
		Dialogue.OnLineChanged = (Action<int>)Delegate.Remove(Dialogue.OnLineChanged, new Action<int>(InvokeLogEvent));
	}

	private void InvokeLogEvent(int index)
	{
		for (int i = 0; i < _logEvents.Length; i++)
		{
			if (index == _logEvents[i].DialogIndex)
			{
				_logEvents[i].DialogEvent.Invoke();
				if (i == _logEvents.Length - 1)
				{
					base.gameObject.SetActive(value: false);
				}
				break;
			}
		}
	}
}
