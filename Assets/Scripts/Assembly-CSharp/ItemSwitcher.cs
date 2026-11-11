using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSwitcher : MonoBehaviour
{
	private enum Item
	{
		Invalid = -1,
		Axe,
		Remote
	}

	private InventoryState state;

	private Item currentItem;

	private Item nextItem;

	private Axe axe;

	private Remote remote;

	public Animator _animator;

	[Header("Axe")]
	public GameObject _axeObject;

	[Header("Remote")]
	public GameObject _remoteObject;

	public bool controlsLocked { get; set; }

	private void Start()
	{
		axe = GetComponent<Axe>();
		remote = GetComponent<Remote>();
	}

	private void Update()
	{
		if (controlsLocked || (Mouse.current.rightButton.isPressed && !axe.isAttacking))
		{
			SelectItem(Item.Remote);
		}
		else
		{
			SelectItem(Item.Axe);
		}
		UpdateItemTransitionAnimation();
	}

	private void SelectItem(Item slot)
	{
		ref Item reference = ref currentItem;
		ref Item reference2 = ref nextItem;
		if (slot == reference2)
		{
			return;
		}
		switch (state)
		{
		case InventoryState.Normal:
			if (slot != reference)
			{
				state = InventoryState.TakeIn_Begin;
				reference2 = slot;
			}
			break;
		case InventoryState.TakeIn_Begin:
		case InventoryState.TakeIn_Wait:
			if (reference2 != slot && reference != slot)
			{
				reference2 = slot;
				break;
			}
			reference2 = slot;
			state = InventoryState.TakeOut_Begin;
			break;
		case InventoryState.TakeOut_Begin:
		case InventoryState.TakeOut_Wait:
			if (reference != slot)
			{
				reference2 = slot;
				state = InventoryState.TakeIn_Begin;
			}
			break;
		}
	}

	private int TakeIn(Item i)
	{
		return Animator.StringToHash((i == Item.Axe) ? "TakeIn_Axe" : "TakeIn_Remote");
	}

	private int TakeOut(Item i)
	{
		return Animator.StringToHash((i == Item.Axe) ? "TakeOut_Axe" : "TakeOut_Remote");
	}

	private GameObject GetItemGameObject(Item i)
	{
		if (i != Item.Axe)
		{
			return _remoteObject;
		}
		return _axeObject;
	}

	private ISillyItem GetItemScript(Item i)
	{
		if (i != Item.Axe)
		{
			return remote;
		}
		return axe;
	}

	private void UpdateItemTransitionAnimation()
	{
		AnimatorStateInfo currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo nextAnimatorStateInfo = _animator.GetNextAnimatorStateInfo(0);
		ref Item reference = ref currentItem;
		ref Item reference2 = ref nextItem;
		switch (state)
		{
		case InventoryState.TakeIn_Begin:
			GetItemScript(reference).Unequip();
			_animator.CrossFadeInFixedTime(TakeIn(reference), 0.1f, 0);
			_animator.Update(0f);
			state = InventoryState.TakeIn_Wait;
			break;
		case InventoryState.TakeIn_Wait:
			if (currentAnimatorStateInfo.shortNameHash == TakeIn(reference) && currentAnimatorStateInfo.normalizedTime > 0.95f)
			{
				state = InventoryState.TakeOut_Begin;
			}
			break;
		case InventoryState.TakeOut_Begin:
			GetItemGameObject(reference).SetActive(value: false);
			GetItemGameObject(reference2).SetActive(value: true);
			_animator.CrossFadeInFixedTime(TakeOut(reference2), 0.1f, 0);
			_animator.Update(0f);
			reference = reference2;
			reference2 = Item.Invalid;
			state = InventoryState.TakeOut_Wait;
			break;
		case InventoryState.TakeOut_Wait:
			if (nextAnimatorStateInfo.shortNameHash != TakeOut(reference) && currentAnimatorStateInfo.shortNameHash != TakeIn(reference))
			{
				GetItemScript(reference).Equip();
				state = InventoryState.Normal;
			}
			break;
		case InventoryState.Normal:
			break;
		}
	}
}
