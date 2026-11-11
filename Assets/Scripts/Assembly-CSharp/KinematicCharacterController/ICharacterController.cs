using UnityEngine;

namespace KinematicCharacterController;

public interface ICharacterController
{
	void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
	{
	}

	void BeforeCharacterUpdate(float deltaTime)
	{
	}

	void PostGroundingUpdate(float deltaTime)
	{
	}

	void AfterCharacterUpdate(float deltaTime)
	{
	}

	bool IsColliderValidForCollisions(Collider coll)
	{
		return true;
	}

	void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
	{
	}

	void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
	{
	}

	void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, ref HitStabilityReport hitStabilityReport)
	{
	}

	void OnDiscreteCollisionDetected(Collider hitCollider)
	{
	}
}
