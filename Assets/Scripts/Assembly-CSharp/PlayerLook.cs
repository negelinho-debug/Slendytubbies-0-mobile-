using KinematicCharacterController;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
	private float headbobAmount;

	private Vector3 originalCameraHolderLocalPosition;

	public float m_HeadbobDamping = 1f;

	public float m_HeadbobSpeed = 1f;

	public float m_HeadbobRange = 0.1f;

	public MouseLook _mouseLook;

	public KinematicCharacterMotor _motor;

	public Transform _cameraHolder;

	public float _sensitivity = 0.2f;

	private int unlockMouseLastFrame;

	private Vector3 remainingOffset;

	private Vector3 perSecondOffset;

	private Quaternion lockRotation;

	private float lockDuration;

	private float lockIntensity;

	public bool isUnlocked { get; private set; }

	private void Start()
	{
		_mouseLook.Init(base.transform, _cameraHolder);
		originalCameraHolderLocalPosition = _cameraHolder.localPosition;
	}

	private void Update()
	{
		Vector2 lookDelta = InputManager.Instance.GetLookDelta();
		float sensitivity = _sensitivity;
		_mouseLook.Update(base.transform, _cameraHolder, lookDelta * sensitivity);
		if (lockDuration > 0f)
		{
			lockDuration -= Time.deltaTime;
			Quaternion combinedRotation = _mouseLook.GetCombinedRotation();
			combinedRotation = Quaternion.Slerp(combinedRotation, lockRotation, Time.deltaTime * lockIntensity);
			_mouseLook.SetRotation(base.transform, _cameraHolder, combinedRotation);
		}
		Vector3 localPosition = originalCameraHolderLocalPosition;
		float b = 0f;
		if (_motor.BaseVelocity.sqrMagnitude > 0f)
		{
			b = 1f;
		}
		headbobAmount = Mathf.Lerp(headbobAmount, b, Time.deltaTime * m_HeadbobDamping);
		localPosition.y += m_HeadbobRange * headbobAmount * Mathf.Sin(Time.time * m_HeadbobSpeed);
		_cameraHolder.localPosition = localPosition;
	}

	public void LockView(Quaternion rotation, float duration, float intensity)
	{
		lockRotation = rotation;
		lockDuration = duration;
		lockIntensity = intensity;
	}

	public Quaternion GetRotation()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		return Quaternion.Euler(_cameraHolder.eulerAngles.x, eulerAngles.y, 0f);
	}

	public void SetRotation(Quaternion rotation)
	{
		_mouseLook.SetRotation(base.transform, _cameraHolder, rotation);
	}
}
