using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Video;

public class WatchDog : MonoBehaviour
{
	public VideoClip[] _clips;

	public float _lookingAtAngle = 25f;

	public float _teleportDistance = 2f;

	public float _teleportAngleDelta = 5f;

	public float _teleportAngle = 43f;

	private VideoPlayer player;

	private Animator animator;

	private bool tookAxe;

	private bool playedIntro;

	private bool isPlaying;

	private int clipIndex;

	private void Start()
	{
		player = GetComponent<VideoPlayer>();
		animator = GetComponent<Animator>();
		player.clip = _clips[0];
		player.Play();
		player.Pause();
	}

	private void Update()
	{
		float num = Vector3.Dot((Player.Instance.transform.position - base.transform.position).normalized, Player.Instance.transform.forward);
		if (num < Mathf.Cos(_lookingAtAngle) && !isPlaying && !playedIntro)
		{
			animator.SetTrigger("Progress");
			isPlaying = true;
		}
		animator.SetBool("IsHanding", num < Mathf.Cos(_lookingAtAngle));
		if (isPlaying && playedIntro)
		{
			if (player.frame >= (long)(player.frameCount - 1) && clipIndex < _clips.Length - 1)
			{
				clipIndex++;
				player.clip = _clips[clipIndex];
				player.Play();
			}
			else if (player.frame >= (long)(player.frameCount - 1) && clipIndex >= _clips.Length - 1)
			{
				player.isLooping = true;
				isPlaying = false;
			}
		}
		if (isPlaying && !animator.IsInTransition(0) && !animator.GetCurrentAnimatorStateInfo(0).IsName("PreAppear") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Appear"))
		{
			player.clip = _clips[clipIndex];
			player.Play();
			playedIntro = true;
		}
		if (num >= Mathf.Cos(_teleportAngle) && InputManager.Instance.GetLookDelta().magnitude >= 0.01f)
		{
			if (tookAxe)
			{
				base.gameObject.SetActive(value: false);
			}
			base.transform.position = GetOutOfViewPosition();
			base.transform.rotation = Quaternion.LookRotation(Player.Instance.transform.position - base.transform.position);
			if (playedIntro)
			{
				animator.Play("HandAxe");
			}
		}
	}

	private Vector3 GetOutOfViewPosition()
	{
		Vector2 lookDelta = InputManager.Instance.GetLookDelta();
		float num = Camera.VerticalToHorizontalFieldOfView(Player.Instance.Camera.fieldOfView, Player.Instance.Camera.aspect);
		Vector3 vector = ((lookDelta.magnitude > 0.01f) ? (Quaternion.AngleAxis((num + _teleportAngleDelta) * Mathf.Sign(lookDelta.x), Vector3.up) * Player.Instance.transform.forward) : Player.Instance.transform.forward);
		Vector3 vector2 = Player.Instance.transform.position + vector * _teleportDistance;
		if (NavMesh.SamplePosition(vector2, out var hit, 10f, -1))
		{
			vector2 = hit.position;
		}
		return vector2;
	}

	public void TakeAxe()
	{
		tookAxe = true;
	}
}
