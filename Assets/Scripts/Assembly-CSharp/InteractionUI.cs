using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
	private static InteractionUI instance;

	private InteractionType requested;

	public Image _image;

	public float _fadeTime;

	[Space]
	public Sprite _normalInteractionIcon;

	public Sprite _holdingInteractionIcon;

	private void Awake()
	{
		Color color = _image.color;
		color.a = 0f;
		_image.color = color;
		if ((bool)instance && instance != this)
		{
			base.enabled = false;
		}
		else
		{
			instance = this;
		}
	}

	private void LateUpdate()
	{
		Color color = _image.color;
		float num = 0f;
		if (requested != 0)
		{
			if (requested == InteractionType.Holding)
			{
				_image.sprite = _holdingInteractionIcon;
			}
			else
			{
				_image.sprite = _normalInteractionIcon;
			}
			num = 1f;
			requested = (InteractionType)0;
		}
		if (color.a > num)
		{
			color.a -= Time.deltaTime * (1f / _fadeTime);
		}
		else if (color.a < num)
		{
			color.a += Time.deltaTime * (1f / _fadeTime);
		}
		color.a = Mathf.Clamp01(color.a);
		_image.color = color;
	}

	public static void Show(InteractionType type)
	{
		if ((bool)instance)
		{
			instance.requested = type;
		}
	}
}
