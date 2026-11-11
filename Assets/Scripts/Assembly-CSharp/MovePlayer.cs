using UnityEngine;

public class MovePlayer : MonoBehaviour
{
	public Transform _movePoint;

	public float _moveDuration;

	public bool _onStart;

	private bool isMoving;

	private Vector3 startPosition;

	private Quaternion startRotation;

	private float moveTime;

	private void Start()
	{
		if (_movePoint == null)
		{
			_movePoint = base.transform;
		}
		if (_onStart)
		{
			StartMoving();
		}
	}

	public void StartMoving()
	{
		if (_moveDuration == 0f)
		{
			Player.Instance.Teleport(_movePoint);
		}
		startPosition = Player.Instance.transform.position;
		startRotation = Player.Instance.transform.rotation;
		moveTime = 0f;
		isMoving = true;
	}

	private void Update()
	{
		if (isMoving)
		{
			if (moveTime < _moveDuration)
			{
				moveTime += Time.deltaTime;
				Vector3 position = Vector3.Lerp(startPosition, _movePoint.position, moveTime / _moveDuration);
				Quaternion rotation = Quaternion.Lerp(startRotation, _movePoint.rotation, moveTime / _moveDuration);
				Player.Instance.Motor.SetPosition(position);
				Player.Instance.transform.rotation = rotation;
			}
			else
			{
				isMoving = false;
			}
		}
	}
}
