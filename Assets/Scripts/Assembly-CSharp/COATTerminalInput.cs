using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class COATTerminalInput : Selectable, IUpdateSelectedHandler, IEventSystemHandler
{
	private Event uiEvent = new Event();

	public event Action<Event, BaseEventData> onTerminalInput;

	public void OnUpdateSelected(BaseEventData eventData)
	{
		while (Event.PopEvent(uiEvent))
		{
			this.onTerminalInput?.Invoke(uiEvent, eventData);
		}
	}
}
