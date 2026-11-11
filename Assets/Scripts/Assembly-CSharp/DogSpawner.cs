using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DogSpawner : MonoBehaviour
{
	public static DogSpawner Instance;

	public GameObject _prefab;

	public int _maxAmount = 30;

	public Vector2 _distanceLimits;

	public Vector2 _spawnLimits;

	public Vector2 _angleLimits;

	public float _spawnDistance = 10f;

	private VideoPlayer video;

	private float loopDuration;

	private Queue<GameObject> pool = new Queue<GameObject>();

	private List<GameObject> dogs = new List<GameObject>();

	private int currentAmount;

	private float nextSpawn;

	public bool IsActive { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Instance = this;
		}
		video = GetComponent<VideoPlayer>();
		loopDuration = (float)video.length;
	}

	private void Update()
	{
		if (IsActive && currentAmount < _maxAmount && Time.time > nextSpawn)
		{
			GameObject obj = GetObject();
			int index = Random.Range(0, EyeFormation.Instance._eyes.Count);
			Vector3 vector = Quaternion.AngleAxis(Random.Range(_angleLimits.x, _angleLimits.y), Vector3.up) * EyeFormation.Instance._eyes[index].transform.forward;
			obj.transform.position = EyeFormation.Instance._eyes[index].transform.position - vector * _spawnDistance;
			obj.transform.rotation = Quaternion.LookRotation(Player.Instance.transform.position - base.transform.position);
			obj.GetComponent<AngryDog>().Respawn();
			float value = Vector3.Distance(EyeFormation.Instance._eyes[index].transform.position, Player.Instance.transform.position);
			float num = Mathf.Lerp(_spawnLimits.x, _spawnLimits.y, Mathf.InverseLerp(_distanceLimits.x, _distanceLimits.y, value));
			float num2 = Mathf.Ceil((Time.time + num) / loopDuration);
			nextSpawn = num2 * loopDuration;
		}
	}

	public void SetActive(bool active)
	{
		IsActive = active;
	}

	public GameObject GetObject()
	{
		currentAmount++;
		if (pool.Count > 0)
		{
			GameObject obj = pool.Dequeue();
			obj.SetActive(value: true);
			return obj;
		}
		GameObject gameObject = Object.Instantiate(_prefab);
		dogs.Add(gameObject);
		return gameObject;
	}

	public void ReturnObject(GameObject obj)
	{
		currentAmount--;
		obj.SetActive(value: false);
		pool.Enqueue(obj);
	}

	public void RemoveAll()
	{
		if (dogs == null)
		{
			return;
		}
		for (int i = 0; i < dogs.Count; i++)
		{
			if ((bool)dogs[i] && dogs[i].TryGetComponent<AngryDog>(out var component))
			{
				component.Damage();
			}
		}
	}

	public void CirclePlayer()
	{
		float num = 135f;
		for (int i = 0; i < _maxAmount; i++)
		{
			float t = ((_maxAmount == 1) ? 0.5f : ((float)i / (float)(_maxAmount - 1)));
			Vector3 vector = Quaternion.AngleAxis(Mathf.Lerp(0f - num, num, t), Vector3.up) * Player.Instance.transform.forward;
			Vector3 position = Player.Instance.transform.position + vector.normalized * Random.Range(8f, 20f);
			GameObject obj = GetObject();
			obj.GetComponent<AngryDog>().Respawn();
			obj.transform.position = position;
		}
	}
}
