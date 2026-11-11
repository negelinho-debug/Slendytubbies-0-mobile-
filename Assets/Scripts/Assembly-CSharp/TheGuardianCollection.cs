using UnityEngine.InputSystem;

public class TheGuardianCollection : IInteractable
{
	public BreakingNewsSeanAddedCustardCollectingToTheGuardianCollection _videoGame;

	public bool _isGuaranteed;

	public override void Use()
	{
		if (!Mouse.current.rightButton.isPressed)
		{
			_videoGame.CollectTheGuardians();
			base.gameObject.SetActive(value: false);
		}
	}
}
