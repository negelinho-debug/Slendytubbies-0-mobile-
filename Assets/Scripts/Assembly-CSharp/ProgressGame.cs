using UnityEngine;

public class ProgressGame : MonoBehaviour
{
	public void Progress(int index)
	{
		GameCompletionFlags gameCompletionFlags = index switch
		{
			0 => GameCompletionFlags.Intro, 
			1 => GameCompletionFlags.Dipsy, 
			2 => GameCompletionFlags.LaaLaa, 
			3 => GameCompletionFlags.Po, 
			4 => GameCompletionFlags.Tinky, 
			5 => GameCompletionFlags.White, 
			6 => GameCompletionFlags.Ron, 
			_ => GameCompletionFlags.None, 
		};
		if (!GameState.saveState.Flags.Contains(gameCompletionFlags))
		{
			GameState.saveState.Flags.Add(gameCompletionFlags);
			GameState.SaveGameToDisk();
		}
	}
}
