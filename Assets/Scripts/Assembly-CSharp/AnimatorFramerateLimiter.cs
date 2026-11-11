using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorFramerateLimiter : MonoBehaviour
{
	public Animator _animator;

	public float _targetFramerate;

	private void Awake()
	{
	}

	private void LateUpdate()
	{
	}

	private void Reset()
	{
		_animator = GetComponent<Animator>();
	}
}
