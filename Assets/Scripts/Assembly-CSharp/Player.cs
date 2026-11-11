using KinematicCharacterController;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class Player : MonoBehaviour
{
	public static Player Instance { get; private set; }

	public new Transform transform { get; private set; }

	public new GameObject gameObject { get; private set; }

	public CinemachineBrain Brain { get; private set; }

	public Camera Camera { get; private set; }

	public CapsuleCollider Capsule { get; private set; }

	public PlayerLook Look { get; private set; }

	public KinematicCharacterMotor Motor { get; private set; }

	public PlayerController Controller { get; private set; }

	private void OnEnable()
	{
		InputManager.Instance.SetLayout(InputLayoutLayer.Gameplay, PlayerInputLayout.Gameplay);
	}

	private void Awake()
	{
		if ((bool)Instance && Instance != this)
		{
			gameObject.SetActive(value: false);
		}
		else
		{
			Instance = this;
		}
		transform = base.transform;
		gameObject = base.gameObject;
		Camera = Camera.main;
		Brain = Camera?.GetComponent<CinemachineBrain>();
		Capsule = GetComponent<CapsuleCollider>();
		Look = GetComponent<PlayerLook>();
		Motor = GetComponent<KinematicCharacterMotor>();
		Controller = GetComponent<PlayerController>();
	}

	public void FreezePlayer(bool isFrozen)
	{
		Controller.IsFreezed = isFrozen;
		Look.enabled = !isFrozen;
	}

	public void Teleport(Transform point)
	{
		Instance.Motor.SetPosition(point.position);
		Instance.Look.SetRotation(point.rotation);
	}
}
