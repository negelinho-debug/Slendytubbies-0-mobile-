using UnityEngine.Events;

public class InteractTransition : IInteractable
{
	public InteractFlags _flags;

	public float _transitionDuration = 1f;

	public float _holdDuration = 1f;

	public UnityEvent _onInteract;

	private void OnEnable()
	{
		Active = true;
	}

	private void OnDisable()
	{
		Active = false;
	}

	private void OnInteract()
	{
		if (_flags.Contains(InteractFlags.FadeOut))
		{
			ScreenFade.Instance.StartFade(1f, 0f, _transitionDuration);
		}
		_onInteract.Invoke();
		base.gameObject.SetActive(!_flags.Contains(InteractFlags.DisableOnUse));
	}

	public override void Use()
	{
		Active = !_flags.Contains(InteractFlags.OneTimeOnly);
		if (_flags.Contains(InteractFlags.FadeIn))
		{
			ScreenFade.Instance.StartFade(0f, 1f, _transitionDuration);
		}
		Player.Instance.FreezePlayer((_flags & InteractFlags.FreezePlayer) != 0);
		if (_flags.Contains(InteractFlags.HoldFade))
		{
			StartCoroutine(ScreenFade.Instance.HoldBlackScreen(_holdDuration, OnInteract, _transitionDuration));
		}
		else
		{
			OnInteract();
		}
	}
}
