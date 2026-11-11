using UnityEngine;

[CreateAssetMenu(fileName = "Surfaces Object", menuName = "Report/Surfaces Object")]
public class FootstepSurfaces : ScriptableObject
{
	[SerializeField]
	public Surface[] m_Surfaces;

	[SerializeField]
	public TerrainSurface[] m_TerrainSurfaces;

	public PhysicsMaterial m_TerrainMaterial;
}
