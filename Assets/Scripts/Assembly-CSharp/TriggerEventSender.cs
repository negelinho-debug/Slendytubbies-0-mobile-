using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interaction/Trigger Event Sender")]
public class TriggerEventSender : MonoBehaviour
{
	private List<TriggerEventSender> childrenTriggers;

	private HashSet<Collider> inTrigger = new HashSet<Collider>();

	public bool IncludeChildTriggers;

	public bool AnyTag = true;

	public string TagToActivate = "Player";

	public UnityEvent<Collider> m_OnTriggerEnter;

	public UnityEvent<Collider> m_OnTriggerExit;

	public event Action<Collider> onTriggerEnter;

	public event Action<Collider> onTriggerExit;

	private void OnDestroy()
	{
		this.onTriggerEnter = null;
		this.onTriggerExit = null;
	}

	private void Start()
	{
		if (!IncludeChildTriggers)
		{
			return;
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		childrenTriggers = new List<TriggerEventSender>();
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.isTrigger)
			{
				if (!collider.TryGetComponent<TriggerEventSender>(out var component))
				{
					component = collider.gameObject.AddComponent<TriggerEventSender>();
				}
				component.onTriggerEnter += InvokeOnTriggerEnter;
				component.onTriggerExit += InvokeOnTriggerExit;
				childrenTriggers.Add(component);
			}
		}
	}

	private void OnDisable()
	{
		foreach (Collider item in inTrigger)
		{
			if ((bool)item)
			{
				if (item.TryGetComponent<ITriggerInteractionReceiver>(out var component))
				{
					component.OnExitedTrigger(this);
				}
				this.onTriggerExit?.Invoke(item);
			}
		}
		inTrigger.Clear();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (AnyTag || other.CompareTag(TagToActivate))
		{
			InvokeOnTriggerEnter(other);
			inTrigger.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (AnyTag || other.CompareTag(TagToActivate))
		{
			InvokeOnTriggerExit(other);
			inTrigger.Remove(other);
		}
	}

	public virtual bool IsInTrigger(Collider col)
	{
		return inTrigger.Contains(col);
	}

	protected void InvokeOnTriggerEnter(Collider col)
	{
		if (base.isActiveAndEnabled && inTrigger.Add(col))
		{
			if (col.TryGetComponent<ITriggerInteractionReceiver>(out var component))
			{
				component.OnEnteredTrigger(this);
			}
			this.onTriggerEnter?.Invoke(col);
			m_OnTriggerEnter.Invoke(col);
		}
	}

	protected void InvokeOnTriggerExit(Collider col)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (IncludeChildTriggers)
		{
			for (int i = 0; i < childrenTriggers.Count; i++)
			{
				if (childrenTriggers[i].IsInTrigger(col))
				{
					return;
				}
			}
		}
		if (inTrigger.Remove(col))
		{
			if (col.TryGetComponent<ITriggerInteractionReceiver>(out var component))
			{
				component.OnExitedTrigger(this);
			}
			this.onTriggerExit?.Invoke(col);
			m_OnTriggerExit.Invoke(col);
		}
	}

	public Collider[] GetAllTriggers()
	{
		return (from col in GetComponentsInChildren<Collider>()
			where (bool)col && col.isTrigger
			select col).ToArray();
	}
}
