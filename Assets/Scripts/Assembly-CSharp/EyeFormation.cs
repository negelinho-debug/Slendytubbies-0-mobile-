using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using PrimeTween;
using UnityEngine;

public class EyeFormation : MonoBehaviour
{
	public static EyeFormation Instance;

	public List<Eye> _eyes = new List<Eye>();

	public GameObject _endingObject;

	[Space(10f)]
	public float _orbitArc = 30f;

	public float _orbitDistance = 8f;

	public float _orbitHeight = 1.5f;

	[Space(10f)]
	public float _evadeArc = 20f;

	public float _evadeDistance = 30f;

	public float _evadeHeight = 2f;

	[Space(10f)]
	public StudioEventEmitter _musicEmitter;

	private Vector3 savedForward;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			Instance = this;
		}
	}

	private void OnEnable()
	{
		Eye.LookedAt = (Action)Delegate.Combine(Eye.LookedAt, new Action(OnLookAt));
		Eye.OnDamage = (Action<GameObject>)Delegate.Combine(Eye.OnDamage, (Action<GameObject>)delegate
		{
			savedForward = Player.Instance.transform.forward;
		});
		Eye.OnDeath = (Action)Delegate.Combine(Eye.OnDeath, new Action(OnDeath));
	}

	private void OnDisable()
	{
		Eye.LookedAt = (Action)Delegate.Remove(Eye.LookedAt, new Action(OnLookAt));
		Eye.OnDamage = (Action<GameObject>)Delegate.Remove(Eye.OnDamage, (Action<GameObject>)delegate
		{
			savedForward = Player.Instance.transform.forward;
		});
		Eye.OnDeath = (Action)Delegate.Remove(Eye.OnDeath, new Action(OnDeath));
	}

	private void OnLookAt()
	{
		savedForward = Player.Instance.transform.forward;
		UpdateFormation(_orbitArc, _orbitHeight, _orbitDistance);
		_musicEmitter.SetParameter("Progression", 1f);
	}

	private void OnDeath()
	{
		UpdateFormation(_evadeArc, _evadeHeight, _evadeDistance);
		_musicEmitter.SetParameter("Progression", 2f);
	}

	public void UpdateFormation(float arc, float height, float distance)
	{
		if (_eyes.Count > 0)
		{
			if (_eyes.Count == 1)
			{
				StartCoroutine(Ending());
			}
			float num = arc * 0.5f;
			for (int i = 0; i < _eyes.Count; i++)
			{
				float t = ((_eyes.Count == 1) ? 0.5f : ((float)i / (float)(_eyes.Count - 1)));
				Vector3 vector = Quaternion.AngleAxis(Mathf.Lerp(0f - num, num, t), Vector3.up) * savedForward;
				Vector3 desiredPosition = Player.Instance.transform.position + Vector3.up * height + vector.normalized * distance;
				_eyes[i].SetDesiredPosition(desiredPosition);
			}
		}
	}

	private IEnumerator Ending()
	{
		yield return Tween.Delay(3f).ToYieldInstruction();
		_eyes[0].gameObject.SetActive(value: false);
		DogSpawner.Instance.RemoveAll();
		_musicEmitter.SetParameter("Progression", 3f);
		Player.Instance.GetComponent<PlayerHealth>().ReachedEnd();
		Vector3 vector = Player.Instance.transform.position + Vector3.up * 5f;
		Quaternion lookRot1 = Quaternion.LookRotation(vector - Player.Instance.Camera.transform.position);
		Player.Instance.Look.LockView(lookRot1, 1f, 9999f);
		yield return Tween.Delay(1f).ToYieldInstruction();
		DogSpawner.Instance.CirclePlayer();
		Vector3 position = new Vector3(Player.Instance.transform.position.x, _endingObject.transform.position.y, Player.Instance.transform.position.z);
		_endingObject.transform.position = position;
		_endingObject.SetActive(value: true);
		_musicEmitter.SetParameter("Progression", 4f);
		Player.Instance.Look.LockView(lookRot1, 0.5f, 5f);
		yield return Tween.Delay(0.5f).ToYieldInstruction();
		Quaternion rotation = Quaternion.LookRotation(Player.Instance.transform.position + Player.Instance.transform.forward + Vector3.up * 1.5f - Player.Instance.Camera.transform.position);
		Player.Instance.Look.LockView(rotation, 1f, 2f);
	}
}
