using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using UnityEngine.UI;

public class KickLaaLaa : SplineGame
{
	[Header("Animators")]
	public Animator _laaLaaAnimator;

	public Animator _tinkyAnimator;

	public Animator _poAnimator;

	[Header("Animations")]
	public string[] _laaLaaStageStateNames;

	public string _laaLaaHit1StateName;

	public string _laaLaaHit2StateName;

	public string[] _attackStateNames;

	public float _impulseForce = 0.05f;

	public Vector3[] _impulseVelocities;

	[Header("Animation Parameters")]
	public int _hitsToComplete;

	public float _stageModifier = 0.9f;

	public UnityEvent _onCompletion;

	[Space(10f)]
	public AnimationCurve _twitchWeight;

	public AnimationCurve _impactWeight;

	[Space(10f)]
	public Transform _cameraHolder;

	public Vector3 _shakeStrength;

	[Header("Blood")]
	public Image[] _bloodImages;

	public float _bloodFadeOutDuration;

	[Header("Material Parameters")]
	public ParticleSystem[] _splatters;

	public Renderer[] _renderers;

	public float[] _bodyOffset;

	public float[] _faceOffset;

	public float _sectionModifier = 0.8f;

	[Space(10f)]
	public SplineContainer[] _splines;

	public GameObject[] _arrows;

	public EventReference _crowbarHit;

	public EventReference _axeHit;

	private bool isPlaying;

	private int currentSpline;

	private int currentAnimator;

	private int hits;

	private int laaLaaState;

	private float progress;

	private AnimatorStateInfo stateInfo;

	private string currentStateName;

	private FMODSoundPlayer FMODPlayer = new FMODSoundPlayer();

	private CinemachineImpulseSource impulseSource;

	private int lastBloodImage = -1;

	private new void Start()
	{
		base.Start();
		impulseSource = GetComponent<CinemachineImpulseSource>();
		Player.Instance.Brain.DefaultBlend.Time = 0f;
		Player.Instance.FreezePlayer(isFrozen: true);
		_container = _splines[currentSpline];
		_extrude.Container = _splines[currentSpline];
		_extrude.Rebuild();
		for (int i = 0; i < _arrows.Length; i++)
		{
			_arrows[i].SetActive(i == currentSpline);
		}
		_handle.transform.position = _container.EvaluatePosition(currentT);
		_laaLaaAnimator.SetLayerWeight(1, _twitchWeight.Evaluate(0f));
		_laaLaaAnimator.SetLayerWeight(2, _impactWeight.Evaluate(0f));
		for (int j = 0; j < _bloodImages.Length; j++)
		{
			_bloodImages[j].color = Color.clear;
		}
	}

	private new void Update()
	{
		base.Update();
		if (!(currentT >= 1f))
		{
			return;
		}
		if (!isPlaying)
		{
			isPlaying = true;
			isDragging = false;
			_handle.SetActive(value: false);
			if (currentAnimator == 0)
			{
				_tinkyAnimator.Play(_attackStateNames[currentSpline]);
				FMODPlayer.PlaySound(_axeHit, Camera.main.transform.position);
			}
			else
			{
				_poAnimator.Play(_attackStateNames[currentSpline]);
				FMODPlayer.PlaySound(_crowbarHit);
			}
			currentStateName = _attackStateNames[currentSpline];
			for (int i = 0; i < _arrows.Length; i++)
			{
				_arrows[i].SetActive(value: false);
			}
			List<int> list = Enumerable.Range(0, _bloodImages.Length).ToList();
			list.Remove(lastBloodImage);
			int index = Random.Range(0, list.Count);
			Tween.Color((Graphic)_bloodImages[list[index]], Color.white, Color.clear, _bloodFadeOutDuration, default(PrimeTween.Easing), 1, CycleMode.Restart, 0f, 0f, useUnscaledTime: false);
			lastBloodImage = list[index];
			int num = Random.Range(0, 2);
			_laaLaaAnimator.Play((num == 0) ? _laaLaaHit1StateName : _laaLaaHit2StateName);
			Tween.ShakeLocalRotation(_cameraHolder, _shakeStrength, 0.5f);
			_splatters[currentSpline].Play(withChildren: true);
			SetMaterialOffsets();
			return;
		}
		stateInfo = ((currentAnimator == 0) ? _tinkyAnimator.GetCurrentAnimatorStateInfo(0) : _poAnimator.GetCurrentAnimatorStateInfo(0));
		if (isPlaying && !_tinkyAnimator.IsInTransition(0) && !stateInfo.IsName(currentStateName))
		{
			isPlaying = false;
			currentSpline++;
			if (currentSpline > _splines.Length - 1)
			{
				currentSpline = 0;
			}
			currentAnimator++;
			if (currentAnimator > 1)
			{
				currentAnimator = 0;
			}
			_container = _splines[currentSpline];
			_extrude.Container = _splines[currentSpline];
			_extrude.Range = new Vector2(0f, 100f);
			_extrude.Rebuild();
			for (int j = 0; j < _arrows.Length; j++)
			{
				_arrows[j].SetActive(j == currentSpline);
			}
			targetT = 0f;
			currentT = 0f;
			_handle.transform.position = _container.EvaluatePosition(currentT);
			hits++;
			progress = (float)hits / (float)_hitsToComplete;
			float num2 = 1f / (float)_laaLaaStageStateNames.Length * _stageModifier;
			float num3 = (float)laaLaaState * num2 + num2;
			_laaLaaAnimator.SetLayerWeight(1, _twitchWeight.Evaluate(progress));
			_laaLaaAnimator.SetLayerWeight(2, _impactWeight.Evaluate(progress));
			if (progress >= num3)
			{
				laaLaaState++;
				if (laaLaaState < _laaLaaStageStateNames.Length - 1)
				{
					_laaLaaAnimator.CrossFade(_laaLaaStageStateNames[laaLaaState], 0.3f);
				}
			}
		}
		if (hits >= _hitsToComplete)
		{
			VideoManager.Instance.PlayVideo(Videos.LaaLaaDeath);
			_onCompletion.Invoke();
			base.gameObject.SetActive(value: false);
		}
	}

	private void CompleteGame()
	{
		_onCompletion.Invoke();
		base.gameObject.SetActive(value: false);
		ScreenFade.Instance.StartFade(1f, 0f, 1f);
	}

	private void SetMaterialOffsets()
	{
		Renderer[] renderers = _renderers;
		foreach (Renderer renderer in renderers)
		{
			for (int j = 0; j < _bodyOffset.Length; j++)
			{
				float num = 1f / (float)_bodyOffset.Length * _sectionModifier;
				float num2 = (float)j * num + num;
				if (progress >= num2)
				{
					if (renderer.materials.Length > 1)
					{
						renderer.materials[1].mainTextureOffset = new Vector2(_bodyOffset[j], 0f);
					}
					else
					{
						renderer.materials[0].mainTextureOffset = new Vector2(_bodyOffset[j], 0f);
					}
				}
			}
			if (renderer.materials.Length <= 1)
			{
				continue;
			}
			for (int k = 0; k < _faceOffset.Length; k++)
			{
				float num3 = 1f / (float)_faceOffset.Length * _sectionModifier;
				float num4 = (float)k * num3 + num3;
				if (progress >= num4)
				{
					renderer.materials[0].mainTextureOffset = new Vector2(_faceOffset[k], 0f);
				}
			}
		}
	}
}
