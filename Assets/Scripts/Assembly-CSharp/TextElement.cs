using System.Collections;
using UnityEngine;

public class TextElement : MonoBehaviour
{
	public float delay;

	public Element _element;

	private IEnumerator Start()
	{
		if (_element.flags.Contains(TextFlags.PlayOnStart))
		{
			yield return new WaitForSeconds(delay);
			PlayText();
		}
	}

	public void PlayText()
	{
		MenuText.Instance.StartText(_element);
	}
}
