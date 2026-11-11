using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Credits : MonoBehaviour
{
	public GameObject _render;

	public TMP_Text _creditsText;

	public ExcelElement _creditsElement;

	public float _creditsAppearDelay;

	public float _creditsDevsStayDuration;

	public float _creditsTranslatorsStayDuration;

	private void Start()
	{
		_render.SetActive(value: true);
		CreditsShow().Forget();
	}

	private async UniTask CreditsShow()
	{
		_creditsText.text = "";
		await UniTask.WaitForSeconds(_creditsAppearDelay);
		string text = _creditsElement.ReadLocalized();
		text = text.Replace("{0}", "FtRemus \n Renarder \n Vwriter \n Alex Hollow");
		_creditsText.text = text;
		await UniTask.WaitForSeconds(_creditsDevsStayDuration);
		string text2 = _creditsElement.ReadLocalized(1);
		text2 = text2.Replace("{0}", "Omega63 - Spanish \n PeterPony - Portuguese (Brazil) \n K4ceace - Polish \nSandwich - Italian \n Kolibrikit - Romanian");
		_creditsText.text = text2;
		await UniTask.WaitForSeconds(_creditsTranslatorsStayDuration);
		_creditsText.text = "";
	}
}
