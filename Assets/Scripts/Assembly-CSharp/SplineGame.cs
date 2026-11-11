using UnityEngine;
using UnityEngine.Splines;

public class SplineGame : MonoBehaviour
{
	public Camera _eventCamera;

	public SplineContainer _container;

	public SplineExtrude _extrude;

	public GameObject _handle;

	[Header("Drag Settings")]
	public float _maxSpeed = 2f;

	public float _maxDot = 0.3f;

	public float _splineLength;

	[Header("Handle Scale")]
	public float _handleDefaultSize = 0.1f;

	public float _handleDraggingSize = 0.07f;

	public float _scaleSmoothTime = 0.12f;

	protected float currentT;

	protected bool isDragging;

	protected float targetT;

	protected float scaleVel;

	protected float currentScale;

	protected float dot;

	protected void Start()
	{
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
		if (_splineLength <= 0f)
		{
			_splineLength = _container.CalculateLength();
		}
		currentScale = _handleDefaultSize;
		_handle.transform.position = _container.EvaluatePosition(currentT);
	}

	protected void Update()
	{
		float target = (isDragging ? _handleDraggingSize : _handleDefaultSize);
		currentScale = Mathf.SmoothDamp(currentScale, target, ref scaleVel, _scaleSmoothTime);
		_handle.transform.localScale = Vector3.one * currentScale;
		if (InputManager.Instance.PressingLMB)
		{
			if (!isDragging)
			{
				isDragging = true;
			}
			HandleDragUsingDelta();
		}
		else
		{
			isDragging = false;
		}
	}

	protected void HandleDragUsingDelta()
	{
		Vector2 lookDelta = InputManager.Instance.GetLookDelta();
		if (!(lookDelta.sqrMagnitude < 0.0001f))
		{
			Vector3 normalized = (_eventCamera.transform.right * lookDelta.x + _eventCamera.transform.up * lookDelta.y).normalized;
			Vector3 rhs = _container.EvaluateTangent(currentT);
			if (rhs.sqrMagnitude < 0.0001f)
			{
				float num = 0.001f;
				rhs = _container.EvaluatePosition(Mathf.Clamp01(currentT + num)) - _container.EvaluatePosition(currentT);
			}
			rhs.Normalize();
			dot = Vector3.Dot(normalized, rhs);
			if (dot > _maxDot)
			{
				float num2 = lookDelta.magnitude / 1000f * (_maxSpeed / _splineLength);
				targetT = Mathf.Clamp01(currentT + num2);
				currentT = Mathf.MoveTowards(currentT, targetT, _maxSpeed / _splineLength * Time.deltaTime);
				_handle.transform.position = _container.EvaluatePosition(currentT);
				_extrude.Range = new Vector2(currentT, 100f);
				_extrude.Rebuild();
			}
		}
	}
}
