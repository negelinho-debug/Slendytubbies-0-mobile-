using UnityEngine;
using UnityEngine.InputSystem;

public class TugGame : MonoBehaviour
{
	public Collider _tugCollider;

	public Vector2 _tugDirection;

	[Header("Counter Force")]
	public float _counterSpeed = 1f;

	public float _counterAccelTime = 1f;

	public float _dragMultiplier = 0.2f;

	[Header("Dragging")]
	public float _dragSpeed = 1.5f;

	public float _maxDragDistance = 1f;

	public float _distanceToComplete = 10f;

	protected float tugProgress;

	protected float targetCounter;

	protected float currentCounter;

	protected float counterAccelTimer;

	protected Vector2 dragStart;

	protected Vector2 lastHitPoint;

	protected Vector2 delta;

	protected bool isDragging;

	protected bool mouseUp;

	protected void Update()
	{
		if (InputManager.Instance.PressingLMB)
		{
			if (!isDragging && mouseUp)
			{
				isDragging = true;
				mouseUp = false;
				dragStart = Mouse.current.position.value;
			}
			if (isDragging)
			{
				delta = Mouse.current.delta.value;
				float num = Vector2.Dot(delta.normalized, _tugDirection.normalized);
				Vector2.Dot(delta, _tugDirection);
				if (num >= 0f)
				{
					tugProgress += num * Time.deltaTime * _dragSpeed;
					tugProgress = Mathf.Clamp(tugProgress, 0f, 1.01f);
				}
				else
				{
					isDragging = false;
				}
			}
		}
		else
		{
			mouseUp = true;
		}
		if (!InputManager.Instance.PressingLMB && isDragging)
		{
			isDragging = false;
			targetCounter = _counterSpeed;
			counterAccelTimer = 0f;
		}
		if (counterAccelTimer < _counterAccelTime)
		{
			counterAccelTimer += Time.deltaTime;
			currentCounter = Mathf.Lerp(currentCounter, targetCounter, counterAccelTimer / _counterAccelTime);
		}
		if (tugProgress > 0f)
		{
			tugProgress = Mathf.Max(0f, tugProgress - currentCounter * Time.deltaTime / _distanceToComplete);
		}
	}
}
