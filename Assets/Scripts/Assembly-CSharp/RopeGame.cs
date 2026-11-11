using FMOD.Studio;
using FMODUnity;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RopeGame : MonoBehaviour
{
	private enum State
	{
		WaitingForInput,
		Tugging
	}

	private State state;

	public Collider _tugCollider;

	public Vector2 _tugDirection;

	public Transform _holder;

	[Header("Counter Force")]
	public float _counterSpeed = 1f;

	public float _counterAccelTime = 1f;

	public float _dragMultiplier = 0.2f;

	[Header("Dragging")]
	public float _dragSpeed = 1.5f;

	public float _maxDragDistance = 1f;

	public float _distanceToComplete = 10f;

	protected float tugProgress;

	protected float currentFallSpeed;

	protected Vector2 dragStart;

	protected Vector2 lastHitPoint;

	protected bool isDragging;

	protected bool mouseUp;

	[Header("Hands")]
	public Animator _viewAnimator;

	public Renderer _ropeRenderer;

	public float _dragDecelerationTime = 1f;

	[Header("Po")]
	public Transform _poObject;

	public Animator _poAnimator;

	public Vector2 _heightLimits;

	public float _recoilStrength = 0.15f;

	public float _recoilSmoothTime = 0.15f;

	public float _recoilHoldTime = 0.05f;

	[Space(10f)]
	public float _offsetMultiplier = 1f;

	public float _progressIncrement = 0.03f;

	public UnityEvent _onCompletion;

	[Header("UI")]
	public ExcelElement _pullElement;

	public ExcelElement _holdElement;

	[Header("Sounds")]
	public EventReference _pullHands;

	public EventReference _pullRope;

	public EventReference _choking;

	public float _ropeVolumeDecay;

	public AnimationCurve _heightEase;

	private EventInstance chokeInstance;

	private EventInstance handsInstance;

	private EventInstance ropeInstance;

	private float completionProgress;

	private float animationProgress;

	private bool shownStruggleHint;

	private bool firstStruggle;

	private bool isStruggling;

	private float nextStruggle;

	private float struggleTime;

	private float struggleLerpTime;

	private float struggleLerpSpeed;

	private float ropeVolumeTime;

	private float drag;

	private float dragDecel;

	private bool lastDragging;

	private float currentHeight;

	private bool reachedEnd;

	private float recoilOffset;

	private float recoilVelocity;

	private float recoilTarget;

	private float recoilTimer;

	private Vector2 mousePosTuggingMax;

	private bool tugMadeProgress;

	private float tugBeganTime;

	private float handsAnimatorSpeed;

	private float handsAnimatorVelocity;

	private float tHeightLast;

	private float currentRopeTextureOffset;

	private void Start()
	{
		Player.Instance.FreezePlayer(isFrozen: true);
		Player.Instance.Brain.DefaultBlend.Time = 0f;
		RenderSettings.fog = true;
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.GameplayWithMouse);
		dragDecel = _dragDecelerationTime;
		reachedEnd = true;
		chokeInstance = RuntimeManager.CreateInstance(_choking);
		chokeInstance.start();
		handsInstance = RuntimeManager.CreateInstance(_pullHands);
		handsInstance.start();
		ropeInstance = RuntimeManager.CreateInstance(_pullRope);
		ropeInstance.start();
		MenuText.Instance.StartText(_pullElement);
		Vector3 startValue = _holder.position - _holder.forward;
		Vector3 position = _holder.position;
		Tween.Position(_holder, startValue, position, 1f);
	}

	private void OnDisable()
	{
		chokeInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		chokeInstance.release();
		handsInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		handsInstance.release();
		ropeInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		ropeInstance.release();
	}

	private void Update()
	{
		Vector2 value = Mouse.current.position.value;
		Vector2 value2 = Mouse.current.delta.value;
		switch (state)
		{
		case State.WaitingForInput:
			isDragging = false;
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				tugMadeProgress = false;
				mousePosTuggingMax = Mouse.current.position.value;
				tugBeganTime = Time.time;
				state = State.Tugging;
			}
			currentFallSpeed += Time.deltaTime * (_counterSpeed / _counterAccelTime);
			currentFallSpeed = Mathf.Min(currentFallSpeed, _counterSpeed);
			break;
		case State.Tugging:
		{
			isDragging = true;
			float num = Time.time - tugBeganTime;
			currentFallSpeed = 0f;
			bool flag = (!Mouse.current.leftButton.wasPressedThisFrame && Mouse.current.leftButton.wasReleasedThisFrame) || (num > 0.15f && tugMadeProgress && value.y > mousePosTuggingMax.y);
			value2.x /= Screen.width;
			value2.y /= Screen.height;
			float num2 = Vector2.Dot(value2.normalized, _tugDirection.normalized);
			if (num2 >= 0f)
			{
				float num3 = _heightEase.Evaluate(animationProgress);
				tugProgress += num2 * _dragSpeed * num3 * Time.deltaTime;
				tugProgress = Mathf.Clamp01(tugProgress);
				tugMadeProgress = true;
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				isDragging = false;
				state = State.WaitingForInput;
			}
			break;
		}
		}
		if (tugProgress > 0f)
		{
			tugProgress = Mathf.Max(0f, tugProgress - currentFallSpeed * Time.deltaTime);
		}
		if (isDragging && value2.magnitude > 0f)
		{
			dragDecel = 0f;
			reachedEnd = false;
			ropeInstance.setParameterByName("Rope State", 1f);
		}
		else
		{
			ropeInstance.setParameterByName("Rope State", 0f);
			if (!reachedEnd && currentHeight < _heightLimits.x + 0.01f)
			{
				reachedEnd = true;
				recoilTarget = (0f - _recoilStrength) / 2f;
				recoilTimer = _recoilHoldTime / 2f;
			}
		}
		if (dragDecel <= _dragDecelerationTime)
		{
			dragDecel += Time.deltaTime;
			drag = Mathf.Lerp(1f, 0f, dragDecel / _dragDecelerationTime);
		}
		handsInstance.setParameterByName("Volume", drag);
		if (tugProgress > 0f)
		{
			ropeVolumeTime = 0f;
		}
		if (ropeVolumeTime <= _ropeVolumeDecay)
		{
			ropeVolumeTime += Time.deltaTime;
			float value3 = Mathf.Lerp(1f, 0f, ropeVolumeTime / _ropeVolumeDecay);
			ropeInstance.setParameterByName("Volume", value3);
		}
		if (lastDragging != isDragging)
		{
			if (isDragging)
			{
				recoilTarget = 0f - _recoilStrength;
				recoilTimer = _recoilHoldTime;
			}
			lastDragging = isDragging;
		}
		if (recoilTimer > 0f)
		{
			recoilTimer -= Time.deltaTime;
		}
		else
		{
			recoilTarget = 0f;
		}
		recoilOffset = Mathf.SmoothDamp(recoilOffset, recoilTarget, ref recoilVelocity, _recoilSmoothTime);
		float num4 = tugProgress;
		currentHeight = Mathf.Lerp(_heightLimits.x, _heightLimits.y, tugProgress) + recoilOffset;
		_poObject.localPosition = new Vector3(_poObject.localPosition.x, currentHeight, _poObject.localPosition.z);
		handsAnimatorSpeed = Mathf.SmoothDamp(target: (!(num4 - tHeightLast > 0f)) ? 0f : 1f, current: handsAnimatorSpeed, currentVelocity: ref handsAnimatorVelocity, smoothTime: 0.1f);
		tHeightLast = num4;
		currentRopeTextureOffset = num4 * _offsetMultiplier;
		_viewAnimator.speed = handsAnimatorSpeed;
		_ropeRenderer.material.mainTextureOffset = new Vector2(0f, currentRopeTextureOffset);
		animationProgress = Mathf.Max(tugProgress, animationProgress);
		float value4 = Mathf.Clamp01(animationProgress / 0.85f);
		_poAnimator.SetFloat("Progress", value4);
		chokeInstance.setParameterByName("Progress", value4);
		if (tugProgress >= 0.99f)
		{
			completionProgress += Time.deltaTime * 2f;
		}
		if (completionProgress >= 1f)
		{
			_onCompletion.Invoke();
			_viewAnimator.speed = 0f;
			base.enabled = false;
		}
	}
}
