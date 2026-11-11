using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController;

[DefaultExecutionOrder(-100)]
public class KinematicCharacterSystem : MonoBehaviour
{
	private static KinematicCharacterSystem _instance;

	public static List<KinematicCharacterMotor> CharacterMotors = new List<KinematicCharacterMotor>();

	public static List<PhysicsMover> PhysicsMovers = new List<PhysicsMover>();

	private static float _lastCustomInterpolationStartTime = -1f;

	private static float _lastCustomInterpolationDeltaTime = -1f;

	public static KCCSettings Settings;

	public static void EnsureCreation()
	{
		if (_instance == null)
		{
			GameObject obj = new GameObject("KinematicCharacterSystem");
			_instance = obj.AddComponent<KinematicCharacterSystem>();
			obj.hideFlags = HideFlags.NotEditable;
			_instance.hideFlags = HideFlags.NotEditable;
			Settings = ScriptableObject.CreateInstance<KCCSettings>();
			Object.DontDestroyOnLoad(obj);
		}
	}

	public static KinematicCharacterSystem GetInstance()
	{
		return _instance;
	}

	public static void SetCharacterMotorsCapacity(int capacity)
	{
		if (capacity < CharacterMotors.Count)
		{
			capacity = CharacterMotors.Count;
		}
		CharacterMotors.Capacity = capacity;
	}

	public static void RegisterCharacterMotor(KinematicCharacterMotor motor)
	{
		CharacterMotors.Add(motor);
	}

	public static void UnregisterCharacterMotor(KinematicCharacterMotor motor)
	{
		CharacterMotors.Remove(motor);
	}

	public static void SetPhysicsMoversCapacity(int capacity)
	{
		if (capacity < PhysicsMovers.Count)
		{
			capacity = PhysicsMovers.Count;
		}
		PhysicsMovers.Capacity = capacity;
	}

	public static void RegisterPhysicsMover(PhysicsMover mover)
	{
		PhysicsMovers.Add(mover);
		mover.Rigidbody.interpolation = RigidbodyInterpolation.None;
	}

	public static void UnregisterPhysicsMover(PhysicsMover mover)
	{
		PhysicsMovers.Remove(mover);
	}

	private void OnDisable()
	{
		Object.Destroy(base.gameObject);
	}

	private void Awake()
	{
		_instance = this;
	}

	private void FixedUpdate()
	{
		if (Settings.AutoSimulation)
		{
			float deltaTime = Time.deltaTime;
			if (Settings.Interpolate)
			{
				PreSimulationInterpolationUpdate(deltaTime);
			}
			Simulate(deltaTime, CharacterMotors, PhysicsMovers);
			if (Settings.Interpolate)
			{
				PostSimulationInterpolationUpdate(deltaTime);
			}
		}
	}

	private void LateUpdate()
	{
		if (Settings.Interpolate)
		{
			CustomInterpolationUpdate();
		}
	}

	public static void PreSimulationInterpolationUpdate(float deltaTime)
	{
		for (int i = 0; i < CharacterMotors.Count; i++)
		{
			KinematicCharacterMotor kinematicCharacterMotor = CharacterMotors[i];
			kinematicCharacterMotor.InitialTickPosition = kinematicCharacterMotor.TransientPosition;
			kinematicCharacterMotor.Transform.position = kinematicCharacterMotor.TransientPosition;
		}
		for (int j = 0; j < PhysicsMovers.Count; j++)
		{
			PhysicsMover physicsMover = PhysicsMovers[j];
			physicsMover.InitialTickPosition = physicsMover.TransientPosition;
			physicsMover.InitialTickRotation = physicsMover.TransientRotation;
			physicsMover.Transform.position = physicsMover.TransientPosition;
			physicsMover.Rigidbody.position = physicsMover.TransientPosition;
			physicsMover.Rigidbody.rotation = physicsMover.TransientRotation;
		}
	}

	public static void Simulate(float deltaTime, List<KinematicCharacterMotor> motors, List<PhysicsMover> movers)
	{
		int count = motors.Count;
		int count2 = movers.Count;
		for (int i = 0; i < count2; i++)
		{
			movers[i].VelocityUpdate(deltaTime);
		}
		for (int j = 0; j < count; j++)
		{
			motors[j].UpdatePhase1(deltaTime);
		}
		for (int k = 0; k < count2; k++)
		{
			PhysicsMover physicsMover = movers[k];
			physicsMover.Transform.SetPositionAndRotation(physicsMover.TransientPosition, physicsMover.TransientRotation);
			physicsMover.Rigidbody.position = physicsMover.TransientPosition;
			physicsMover.Rigidbody.rotation = physicsMover.TransientRotation;
		}
		for (int l = 0; l < count; l++)
		{
			KinematicCharacterMotor kinematicCharacterMotor = motors[l];
			kinematicCharacterMotor.UpdatePhase2(deltaTime);
			kinematicCharacterMotor.Transform.position = kinematicCharacterMotor.TransientPosition;
		}
	}

	public static void PostSimulationInterpolationUpdate(float deltaTime)
	{
		_lastCustomInterpolationStartTime = Time.time;
		_lastCustomInterpolationDeltaTime = deltaTime;
		for (int i = 0; i < CharacterMotors.Count; i++)
		{
			KinematicCharacterMotor kinematicCharacterMotor = CharacterMotors[i];
			kinematicCharacterMotor.Transform.position = kinematicCharacterMotor.InitialTickPosition;
		}
		for (int j = 0; j < PhysicsMovers.Count; j++)
		{
			PhysicsMover physicsMover = PhysicsMovers[j];
			if (physicsMover.MoveWithPhysics)
			{
				physicsMover.Rigidbody.position = physicsMover.InitialTickPosition;
				physicsMover.Rigidbody.rotation = physicsMover.InitialTickRotation;
				physicsMover.Rigidbody.MovePosition(physicsMover.TransientPosition);
				physicsMover.Rigidbody.MoveRotation(physicsMover.TransientRotation);
			}
			else
			{
				physicsMover.Rigidbody.position = physicsMover.TransientPosition;
				physicsMover.Rigidbody.rotation = physicsMover.TransientRotation;
			}
		}
	}

	private static void CustomInterpolationUpdate()
	{
		float t = Mathf.Clamp01((Time.time - _lastCustomInterpolationStartTime) / _lastCustomInterpolationDeltaTime);
		for (int i = 0; i < CharacterMotors.Count; i++)
		{
			KinematicCharacterMotor kinematicCharacterMotor = CharacterMotors[i];
			kinematicCharacterMotor.Transform.position = Vector3.Lerp(kinematicCharacterMotor.InitialTickPosition, kinematicCharacterMotor.TransientPosition, t);
		}
		for (int j = 0; j < PhysicsMovers.Count; j++)
		{
			PhysicsMover physicsMover = PhysicsMovers[j];
			physicsMover.Transform.SetPositionAndRotation(Vector3.Lerp(physicsMover.InitialTickPosition, physicsMover.TransientPosition, t), Quaternion.Slerp(physicsMover.InitialTickRotation, physicsMover.TransientRotation, t));
			Vector3 position = physicsMover.Transform.position;
			Quaternion rotation = physicsMover.Transform.rotation;
			physicsMover.PositionDeltaFromInterpolation = position - physicsMover.LatestInterpolationPosition;
			physicsMover.RotationDeltaFromInterpolation = Quaternion.Inverse(physicsMover.LatestInterpolationRotation) * rotation;
			physicsMover.LatestInterpolationPosition = position;
			physicsMover.LatestInterpolationRotation = rotation;
		}
	}
}
