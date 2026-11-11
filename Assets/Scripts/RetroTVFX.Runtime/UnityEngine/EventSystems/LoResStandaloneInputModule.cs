using System;

namespace UnityEngine.EventSystems;

[AddComponentMenu("Event/Standalone Input Module")]
public class LoResStandaloneInputModule : PointerInputModule
{
	[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
	public enum InputMode
	{
		Mouse,
		Buttons
	}

	public int DesiredResX = 320;

	public int DesiredResY = 240;

	public float FisheyeX = 0.05f;

	public float FisheyeY = 0.05f;

	public bool StretchToDisplay;

	public float DisplayAspect = 1.33f;

	private float _nextAction;

	private Vector2 _lastMousePosition;

	private Vector2 _mousePosition;

	[SerializeField]
	private string m_HorizontalAxis = "Horizontal";

	[SerializeField]
	private string m_VerticalAxis = "Vertical";

	[SerializeField]
	private string m_SubmitButton = "Submit";

	[SerializeField]
	private string m_CancelButton = "Cancel";

	[SerializeField]
	private float m_InputActionsPerSecond = 10f;

	[SerializeField]
	private bool m_AllowActivationOnMobileDevice;

	private readonly MouseState m_MouseState = new MouseState();

	[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
	public InputMode inputMode => InputMode.Mouse;

	public bool allowActivationOnMobileDevice
	{
		get
		{
			return m_AllowActivationOnMobileDevice;
		}
		set
		{
			m_AllowActivationOnMobileDevice = value;
		}
	}

	public float inputActionsPerSecond
	{
		get
		{
			return m_InputActionsPerSecond;
		}
		set
		{
			m_InputActionsPerSecond = value;
		}
	}

	public string horizontalAxis
	{
		get
		{
			return m_HorizontalAxis;
		}
		set
		{
			m_HorizontalAxis = value;
		}
	}

	public string verticalAxis
	{
		get
		{
			return m_VerticalAxis;
		}
		set
		{
			m_VerticalAxis = value;
		}
	}

	public string submitButton
	{
		get
		{
			return m_SubmitButton;
		}
		set
		{
			m_SubmitButton = value;
		}
	}

	public string cancelButton
	{
		get
		{
			return m_CancelButton;
		}
		set
		{
			m_CancelButton = value;
		}
	}

	protected LoResStandaloneInputModule()
	{
	}

	public override void UpdateModule()
	{
		_lastMousePosition = _mousePosition;
		_mousePosition = Input.mousePosition;
	}

	public override bool IsModuleSupported()
	{
		if (!m_AllowActivationOnMobileDevice)
		{
			return Input.mousePresent;
		}
		return true;
	}

	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule())
		{
			return false;
		}
		return Input.GetButtonDown(m_SubmitButton) | Input.GetButtonDown(m_CancelButton) | !Mathf.Approximately(Input.GetAxisRaw(m_HorizontalAxis), 0f) | !Mathf.Approximately(Input.GetAxisRaw(m_VerticalAxis), 0f) | ((_mousePosition - _lastMousePosition).sqrMagnitude > 0f) | Input.GetMouseButtonDown(0);
	}

	public override void ActivateModule()
	{
		base.ActivateModule();
		_mousePosition = Input.mousePosition;
		_lastMousePosition = Input.mousePosition;
		GameObject gameObject = base.eventSystem.currentSelectedGameObject;
		if (gameObject == null)
		{
			gameObject = base.eventSystem.firstSelectedGameObject;
		}
		base.eventSystem.SetSelectedGameObject(null, GetBaseEventData());
		base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
	}

	public override void DeactivateModule()
	{
		base.DeactivateModule();
		ClearSelection();
	}

	public override void Process()
	{
		bool flag = SendUpdateEventToSelectedObject();
		if (base.eventSystem.sendNavigationEvents)
		{
			if (!flag)
			{
				flag |= SendMoveEventToSelectedObject();
			}
			if (!flag)
			{
				SendSubmitEventToSelectedObject();
			}
		}
		ProcessMouseEvent();
	}

	private bool SendSubmitEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		if (Input.GetButtonDown(m_SubmitButton))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
		}
		if (Input.GetButtonDown(m_CancelButton))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
		}
		return baseEventData.used;
	}

	private bool AllowMoveEventProcessing(float time)
	{
		return Input.GetButtonDown(m_HorizontalAxis) | Input.GetButtonDown(m_VerticalAxis) | (time > _nextAction);
	}

	private Vector2 GetRawMoveVector()
	{
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxisRaw(m_HorizontalAxis);
		zero.y = Input.GetAxisRaw(m_VerticalAxis);
		if (Input.GetButtonDown(m_HorizontalAxis))
		{
			if (zero.x < 0f)
			{
				zero.x = -1f;
			}
			if (zero.x > 0f)
			{
				zero.x = 1f;
			}
		}
		if (Input.GetButtonDown(m_VerticalAxis))
		{
			if (zero.y < 0f)
			{
				zero.y = -1f;
			}
			if (zero.y > 0f)
			{
				zero.y = 1f;
			}
		}
		return zero;
	}

	private bool SendMoveEventToSelectedObject()
	{
		float unscaledTime = Time.unscaledTime;
		if (!AllowMoveEventProcessing(unscaledTime))
		{
			return false;
		}
		Vector2 rawMoveVector = GetRawMoveVector();
		AxisEventData axisEventData = GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
		if (!Mathf.Approximately(axisEventData.moveVector.x, 0f) || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
		}
		_nextAction = unscaledTime + 1f / m_InputActionsPerSecond;
		return axisEventData.used;
	}

	protected override MouseState GetMousePointerEventData()
	{
		PointerEventData data;
		bool pointerData = GetPointerData(-1, out data, create: true);
		data.Reset();
		Vector2 vector = Input.mousePosition;
		float num = Screen.width;
		if (!StretchToDisplay)
		{
			num = DisplayAspect * (float)Screen.height;
			vector.x -= ((float)Screen.width - num) * 0.5f;
		}
		vector.x /= num;
		vector.y /= Screen.height;
		vector.y = 1f - vector.y;
		vector.x = Mathf.Clamp01(vector.x);
		vector.y = Mathf.Clamp01(vector.y);
		Vector2 vector2 = vector;
		vector2 -= new Vector2(0.5f, 0.5f);
		vector2 *= 2f;
		float num2 = (float)Screen.width / (float)Screen.height;
		if (!StretchToDisplay)
		{
			num2 = 1f / DisplayAspect;
		}
		Vector2 zero = Vector2.zero;
		zero.x = vector2.y * vector2.y * (0f - FisheyeX) * vector2.x * 0.1f;
		zero.y = vector2.x * vector2.x * (0f - FisheyeY) * vector2.y * 0.1f * num2;
		vector -= zero;
		vector.y = 1f - vector.y;
		vector.x *= DesiredResX;
		vector.y *= DesiredResY;
		vector.y -= 1f;
		if (pointerData)
		{
			data.position = vector;
		}
		Vector2 vector3 = vector;
		data.delta = vector3 - data.position;
		data.position = vector3;
		data.scrollDelta = Input.mouseScrollDelta;
		data.button = PointerEventData.InputButton.Left;
		base.eventSystem.RaycastAll(data, m_RaycastResultCache);
		RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
		data.pointerCurrentRaycast = pointerCurrentRaycast;
		m_RaycastResultCache.Clear();
		GetPointerData(-2, out var data2, create: true);
		CopyFromTo(data, data2);
		data2.button = PointerEventData.InputButton.Right;
		GetPointerData(-3, out var data3, create: true);
		CopyFromTo(data, data3);
		data3.button = PointerEventData.InputButton.Middle;
		m_MouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), data);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), data2);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), data3);
		return m_MouseState;
	}

	private void ProcessMouseEvent()
	{
		MouseState mousePointerEventData = GetMousePointerEventData();
		bool pressed = mousePointerEventData.AnyPressesThisFrame();
		bool released = mousePointerEventData.AnyReleasesThisFrame();
		MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
		if (UseMouse(pressed, released, eventData.buttonData))
		{
			ProcessMousePress(eventData);
			ProcessMove(eventData.buttonData);
			ProcessDrag(eventData.buttonData);
			ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
			if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
			{
				ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), eventData.buttonData, ExecuteEvents.scrollHandler);
			}
		}
	}

	private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
	{
		if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
		{
			return true;
		}
		return false;
	}

	private bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	private void ProcessMousePress(MouseButtonEventData data)
	{
		PointerEventData buttonData = data.buttonData;
		GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
		if (data.PressedThisFrame())
		{
			buttonData.eligibleForClick = true;
			buttonData.delta = Vector2.zero;
			buttonData.dragging = false;
			buttonData.useDragThreshold = true;
			buttonData.pressPosition = buttonData.position;
			buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
			DeselectIfSelectionChanged(gameObject, buttonData);
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == buttonData.lastPress)
			{
				if (unscaledTime - buttonData.clickTime < 0.3f)
				{
					int clickCount = buttonData.clickCount + 1;
					buttonData.clickCount = clickCount;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.clickTime = unscaledTime;
			}
			else
			{
				buttonData.clickCount = 1;
			}
			buttonData.pointerPress = gameObject2;
			buttonData.rawPointerPress = gameObject;
			buttonData.clickTime = unscaledTime;
			buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (data.ReleasedThisFrame())
		{
			ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
			{
				ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
			}
			else if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.dropHandler);
			}
			buttonData.eligibleForClick = false;
			buttonData.pointerPress = null;
			buttonData.rawPointerPress = null;
			if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
			}
			buttonData.dragging = false;
			buttonData.pointerDrag = null;
			if (gameObject != buttonData.pointerEnter)
			{
				HandlePointerExitAndEnter(buttonData, null);
				HandlePointerExitAndEnter(buttonData, gameObject);
			}
		}
	}
}
