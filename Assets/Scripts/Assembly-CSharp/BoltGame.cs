using System.Collections;
using FMODUnity;
using PrimeTween;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BoltGame : MonoBehaviour
{
	public Animator _handAnimator;

	public SkinnedMeshRenderer _boltRenderer;

	public Collider[] _pointsToHit;

	public Rig[] _rigs;

	public UnityEvent _onCompletion;

	[Space(10f)]
	public float _blackScreenDelay = 0.3f;

	public float _endingDelay = 2f;

	[Space(10f)]
	public EventReference _punchSound;

	public EventReference _swingSound;

	[Header("Laa Laa")]
	public Renderer _laaLaaRenderer;

	public float[] _offsets;

	private bool hasPunched;

	private int currentPoint;

	private void Start()
	{
		Player.Instance.Brain.DefaultBlend.Time = 0f;
		Player.Instance.FreezePlayer(isFrozen: true);
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.GameplayWithMouse);
		for (int i = 0; i < _pointsToHit.Length; i++)
		{
			_pointsToHit[i].gameObject.SetActive(i == currentPoint);
		}
		for (int j = 0; j < _rigs.Length; j++)
		{
			_rigs[j].weight = 0f;
		}
	}

	private void Update()
	{
		if (!InputManager.Instance.LMBTriggered() || hasPunched || !Physics.Raycast(Player.Instance.Camera.ScreenPointToRay(Mouse.current.position.value), out var hitInfo, 100f))
		{
			return;
		}
		for (int i = 0; i < _pointsToHit.Length; i++)
		{
			if (hitInfo.collider == _pointsToHit[i])
			{
				hasPunched = true;
				StartCoroutine(Hit());
			}
		}
	}

	private IEnumerator Hit()
	{
		_handAnimator.Play("Impact");
		_boltRenderer.SetBlendShapeWeight(0, 100f);
		RuntimeManager.PlayOneShot(_swingSound, Player.Instance.Camera.transform.position);
		for (int i = 0; i < _rigs.Length; i++)
		{
			_rigs[i].weight = ((i == currentPoint) ? 1f : 0f);
		}
		yield return Tween.Delay(_blackScreenDelay);
		RuntimeManager.PlayOneShot(_punchSound, Player.Instance.Camera.transform.position);
		VideoManager.Instance.PlayVideo(Videos.LaaLaaEyes);
		while (VideoManager.Instance.isPreparing)
		{
			yield return null;
		}
		currentPoint++;
		_laaLaaRenderer.material.SetTextureOffset("_MainTex", new Vector2(_offsets[currentPoint], 0f));
		for (int j = 0; j < _pointsToHit.Length; j++)
		{
			_pointsToHit[j].gameObject.SetActive(j == currentPoint);
		}
		_boltRenderer.SetBlendShapeWeight(0, 0f);
		for (int k = 0; k < _rigs.Length; k++)
		{
			_rigs[k].weight = 0f;
		}
		if (currentPoint >= _pointsToHit.Length)
		{
			yield return Tween.Delay(_endingDelay);
			_onCompletion.Invoke();
		}
		else
		{
			hasPunched = false;
		}
	}
}
