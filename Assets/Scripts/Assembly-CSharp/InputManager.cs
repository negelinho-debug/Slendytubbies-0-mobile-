using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	public PlayerInputLayout[] layouts;

	private PlayerInputLayout previousLayout;

	private PlayerInputLayout activeLayout = PlayerInputLayout.None;

	public InputSystem_Actions controls;

	public bool WantHideMouse;

	private bool LayoutMouseVisibility;

	public bool PressingLShift { get; private set; }

	public bool PressingSpace { get; private set; }

	public bool PressingCtrl { get; private set; }

	public bool PressingLMB { get; private set; }

	public bool PressingRMB { get; private set; }

	public bool PressingR { get; private set; }

	public static InputManager Instance { get; private set; }

	public void SetLayout(InputLayoutLayer layer, PlayerInputLayout layout)
	{
		layouts[(int)layer] = layout;
	}

	private void SetLayoutInternal(PlayerInputLayout layout)
	{
		if (activeLayout != layout)
		{
			switch (layout)
			{
			case PlayerInputLayout.Gameplay:
				LayoutMouseVisibility = false;
				Cursor.lockState = CursorLockMode.Locked;
				controls.Player.Enable();
				controls.UI.Enable();
				break;
			case PlayerInputLayout.GameplayWithMouse:
				LayoutMouseVisibility = true;
				Cursor.lockState = CursorLockMode.None;
				controls.Player.Enable();
				controls.UI.Enable();
				break;
			case PlayerInputLayout.UI:
				LayoutMouseVisibility = true;
				Cursor.lockState = CursorLockMode.None;
				controls.Player.Disable();
				controls.UI.Enable();
				break;
			case PlayerInputLayout.None:
				LayoutMouseVisibility = true;
				Cursor.lockState = CursorLockMode.None;
				controls.Player.Disable();
				controls.UI.Disable();
				break;
			case PlayerInputLayout.HiddenUnlockedMouse:
				LayoutMouseVisibility = false;
				Cursor.lockState = CursorLockMode.None;
				controls.Player.Enable();
				controls.UI.Enable();
				break;
			case PlayerInputLayout.KeyboardOnly:
				LayoutMouseVisibility = false;
				Cursor.lockState = CursorLockMode.Locked;
				controls.Player.Disable();
				controls.UI.Enable();
				break;
			}
			if (activeLayout >= PlayerInputLayout.Gameplay)
			{
				previousLayout = activeLayout;
			}
			activeLayout = layout;
		}
	}

	private void Awake()
	{
		if ((bool)Instance && Instance != this)
		{
			Debug.LogError("Input manager already exists!!", this);
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Instance = this;
		}
		layouts = new PlayerInputLayout[3];
		Array.Fill(layouts, PlayerInputLayout.Invalid);
		activeLayout = PlayerInputLayout.Invalid;
		controls = new InputSystem_Actions();
		controls.Player.Sprint.performed += delegate
		{
			PressingLShift = true;
		};
		controls.Player.Sprint.canceled += delegate
		{
			PressingLShift = false;
		};
		controls.Player.Jump.performed += delegate
		{
			PressingSpace = true;
		};
		controls.Player.Jump.canceled += delegate
		{
			PressingSpace = false;
		};
		controls.Player.Crouch.performed += delegate
		{
			PressingCtrl = true;
		};
		controls.Player.Crouch.canceled += delegate
		{
			PressingCtrl = false;
		};
		controls.Player.Reload.performed += delegate
		{
			PressingR = true;
		};
		controls.Player.Reload.canceled += delegate
		{
			PressingR = false;
		};
		controls.Player.Attack.performed += delegate
		{
			PressingLMB = true;
		};
		controls.Player.Attack.canceled += delegate
		{
			PressingLMB = false;
		};
		controls.Player.Aim.performed += delegate
		{
			PressingRMB = true;
		};
		controls.Player.Aim.canceled += delegate
		{
			PressingRMB = false;
		};
		SetLayoutInternal(PlayerInputLayout.Gameplay);
	}

	public Vector2 GetLookDelta()
	{
		return controls.Player.Look.ReadValue<Vector2>();
	}

	public Vector2 GetMovement()
	{
		return controls.Player.Move.ReadValue<Vector2>();
	}

	public bool SpaceTriggered()
	{
		return controls.Player.Jump.triggered;
	}

	public bool EscTriggered()
	{
		return controls.UI.Pause.triggered;
	}

	public bool LMBTriggered()
	{
		return controls.Player.Attack.triggered;
	}

	public bool RMBTriggered()
	{
		return controls.Player.Aim.triggered;
	}

	public bool TriggeredE()
	{
		return controls.Player.Interact.triggered;
	}

	public bool TriggeredR()
	{
		return controls.Player.Reload.triggered;
	}

	public bool EnterTriggered()
	{
		return controls.UI.Submit.triggered;
	}

	public bool BackspaceTriggered()
	{
		return controls.UI.Backspace.triggered;
	}

	public bool AnyTriggered()
	{
		return controls.UI.Any.triggered;
	}

	private void OnEnable()
	{
		controls.Enable();
	}

	private void OnDisable()
	{
		controls?.Disable();
	}

	private void Update()
	{
		PlayerInputLayout layoutInternal = PlayerInputLayout.UI;
		for (int num = layouts.Length - 1; num >= 0; num--)
		{
			if (layouts[num] != PlayerInputLayout.Invalid)
			{
				layoutInternal = layouts[num];
				break;
			}
		}
		SetLayoutInternal(layoutInternal);
		if (WantHideMouse)
		{
			Cursor.visible = false;
			WantHideMouse = false;
		}
		else
		{
			Cursor.visible = LayoutMouseVisibility;
		}
	}
}
