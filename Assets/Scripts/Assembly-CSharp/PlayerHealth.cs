using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamagable
{
	public float _damage = 20f;

	public float _maxHealth = 100f;

	public float _regenDelay = 0.3f;

	public float _regenSpeed = 3f;

	public PostProcessVolume _healthVolume;

	public GameObject _credits;

	private bool isDead;

	private bool reachedEnding;

	private float currentHealth;

	private float regenTimer;

	private void Start()
	{
		currentHealth = _maxHealth;
		_healthVolume.weight = 0f;
	}

	private void OnEnable()
	{
		VideoManager.OnFinished = (Action)Delegate.Combine(VideoManager.OnFinished, new Action(OnDeath));
	}

	private void OnDisable()
	{
		VideoManager.OnFinished = (Action)Delegate.Remove(VideoManager.OnFinished, new Action(OnDeath));
	}

	private void Update()
	{
		if (regenTimer > 0f)
		{
			regenTimer -= Time.deltaTime;
		}
		if (currentHealth < _maxHealth && regenTimer <= 0f)
		{
			currentHealth += _regenSpeed * Time.deltaTime;
			if (currentHealth > _maxHealth)
			{
				currentHealth = _maxHealth;
			}
		}
		_healthVolume.weight = 1f - currentHealth / _maxHealth;
	}

	public void ReachedEnd()
	{
		reachedEnding = true;
	}

	public void Damage()
	{
		Damage(_damage);
	}

	public void Damage(float dmg)
	{
		currentHealth -= dmg;
		regenTimer = _regenDelay;
		if (currentHealth <= 0f)
		{
			DieInstantly();
		}
	}

	public void DieInstantly()
	{
		if (!isDead)
		{
			isDead = true;
			GameState.saveState.Flags.Add(GameCompletionFlags.Prologue);
			GameState.SaveGameToDisk();
			VideoManager.Instance.PlayVideo(reachedEnding ? Videos.Intro : Videos.WatchDogScreamer);
			DogSpawner.Instance?.RemoveAll();
			DogSpawner.Instance?.SetActive(active: false);
			if (_credits != null)
			{
				_credits.SetActive(value: true);
			}
			EyeFormation.Instance?._musicEmitter.Stop();
		}
	}

	private void OnDeath()
	{
		Bus bus = RuntimeManager.GetBus("bus:/Sound");
		if (bus.isValid())
		{
			bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		else
		{
			Debug.LogError("Could not get sound bus");
		}
		SceneManager.LoadScene(reachedEnding ? "Menu" : SceneManager.GetActiveScene().name);
	}
}
