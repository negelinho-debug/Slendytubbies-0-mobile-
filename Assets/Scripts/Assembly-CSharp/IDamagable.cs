using UnityEngine;

public interface IDamagable
{
	void Damage()
	{
		Debug.LogError("Implement Damage(Vector3 point) please");
	}

	void Damage(Vector3 point)
	{
		Damage();
	}
}
