using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BreakingNewsSeanAddedCustardCollectingToTheGuardianCollection : MonoBehaviour
{
	private int _custardsCollected;

	public int _custardCount;

	public int _custardCountAvailable;

	public CinemachineCamera _playerCam;

	public Axe _playerAxe;

	public PostProcessVolume _playerPostProcessing;

	public ArrowTubi _arrowTubi;

	public YetiTubi _yetiTubi;

	private WhiteNoiseVignette _vignette;

	public EventReference _BackgroundMusic;

	public EventReference _axeHitWeak;

	public EventReference _axeHitStrong;

	public float _axeAttackCooldownWeak;

	public float _axeAttackCooldownStrong;

	public EventInstance musicEvent;

	[Header("On Custard Collected")]
	public EventReference _collectEventUsual;

	public Element _hint;

	public float _playerSpeedMultiplierUsual = 1.05f;

	public float _playerFOVMultiplier = 1.05f;

	public float _textShakeMultiplier = 1.3f;

	[Header("Final Custard Effects")]
	public Color _finalCustardTextColor;

	public EventReference _collectEventFinal;

	public float _playerSpeedMultiplierFinal = 1.2f;

	public float _gameProgress { get; private set; }

	private void Awake()
	{
		_vignette = _playerPostProcessing.profile.GetSetting<WhiteNoiseVignette>();
		List<TheGuardianCollection> list = new List<TheGuardianCollection>();
		TheGuardianCollection[] array = Object.FindObjectsByType<TheGuardianCollection>(FindObjectsSortMode.None);
		TheGuardianCollection[] array2 = array;
		foreach (TheGuardianCollection theGuardianCollection in array2)
		{
			theGuardianCollection._videoGame = this;
			if (!theGuardianCollection._isGuaranteed)
			{
				list.Add(theGuardianCollection);
			}
		}
		int num = array.Length - _custardCountAvailable;
		while (list.Count > 0 && num > 0)
		{
			int index = Random.Range(0, list.Count);
			list[index].gameObject.SetActive(value: false);
			list.RemoveAt(index);
			num--;
		}
		_playerAxe._eyeHit = _axeHitWeak;
		_playerAxe._attackDuration = _axeAttackCooldownWeak;
		_yetiTubi._videoGame = this;
		musicEvent = RuntimeManager.CreateInstance(_BackgroundMusic);
		musicEvent.setParameterByName("CustardsCollectedNum", 0f);
		musicEvent.start();
	}

	private void OnDisable()
	{
		if (musicEvent.isValid())
		{
			musicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			musicEvent.release();
		}
	}

	public void CollectTheGuardians()
	{
		_custardsCollected++;
		int num = _custardCount - _custardsCollected;
		_gameProgress = (float)_custardsCollected / (float)_custardCount;
		musicEvent.setParameterByName("CustardsCollectedNum", _custardsCollected);
		_hint.text = num.ToString();
		MenuText.Instance.StartText(_hint);
		MenuText.Instance._TMPText.color = Color.Lerp(Color.white, _finalCustardTextColor, _gameProgress * _gameProgress);
		_hint.intensity *= _textShakeMultiplier;
		_playerCam.Lens.FieldOfView *= _playerFOVMultiplier;
		_vignette.intensity.Override(0.4f * _gameProgress * _gameProgress);
		RuntimeManager.PlayOneShot(_collectEventUsual);
		if (num == 0)
		{
			_playerAxe._eyeHit = _axeHitStrong;
			_playerAxe._attackDuration = _axeAttackCooldownStrong;
			RuntimeManager.PlayOneShot(_collectEventFinal);
			Player.Instance.Controller.m_WalkSpeed *= _playerSpeedMultiplierFinal;
			_arrowTubi._vulnerable = true;
			Object.FindObjectOfType<PlayerHealth>()._damage = 90f;
			TheGuardianCollection[] array = Object.FindObjectsByType<TheGuardianCollection>(FindObjectsSortMode.None);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
		}
		else
		{
			Player.Instance.Controller.m_WalkSpeed *= _playerSpeedMultiplierUsual;
		}
	}
}
