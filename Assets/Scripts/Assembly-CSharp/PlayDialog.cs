using UnityEngine;

public class PlayDialog : MonoBehaviour
{
	public void StartDialog(ExcelElement dialogElement)
	{
		Dialogue.Play(dialogElement);
	}
}
