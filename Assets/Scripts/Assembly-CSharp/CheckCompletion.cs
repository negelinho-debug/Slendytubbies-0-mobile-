using UnityEngine;
using UnityEngine.Events;

public class CheckCompletion : MonoBehaviour
{
	public GameCompletionFlags _requiredCompletion;

	public UnityEvent _onCompletion;

	private void Start()
	{
		Check(_requiredCompletion);
	}

	public void Check(GameCompletionFlags completion)
	{
		if (GameState.saveState.CompletionState.Contains(completion.ToString()) || (completion == GameCompletionFlags.Everything && GameState.saveState.CompletionState.Count >= 7))
		{
			_onCompletion.Invoke();
		}
	}
}
