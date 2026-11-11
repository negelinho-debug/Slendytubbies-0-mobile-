using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

public class FootstepSystem : MonoBehaviour
{
	public Sounds m_DefaultSoundID;

	public FootstepSurfaces m_SurfacesObject;

	public EventReference m_FootstepSounds;

	public float m_DistanceBetweenSteps = 2.2f;

	[Range(0f, 1f)]
	public float m_VolumeAdditive;

	public LayerMask m_LayerMask;

	private EventInstance currentInstance;

	private TerrainChecker terrainChecker;

	private NavMeshAgent navMeshAgent;

	private string currentLayer;

	private float stepProgress;

	private int oldSurface;

	private bool isAi;

	private bool inWater;

	private void Start()
	{
		isAi = (navMeshAgent = GetComponent<NavMeshAgent>());
		terrainChecker = new TerrainChecker();
	}

	private void OnDestroy()
	{
		currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		currentInstance.release();
	}

	private void Update()
	{
		if (isAi)
		{
			AiFootsteps();
		}
		else
		{
			PlayerFootsteps();
		}
	}

	private int CheckSurface()
	{
		if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 3f, m_LayerMask))
		{
			if (hitInfo.collider.sharedMaterial != m_SurfacesObject.m_TerrainMaterial)
			{
				for (int i = 0; i < m_SurfacesObject.m_Surfaces.Length; i++)
				{
					if (hitInfo.collider.sharedMaterial == m_SurfacesObject.m_Surfaces[i].m_SurfaceMaterial)
					{
						oldSurface = (int)m_SurfacesObject.m_Surfaces[i].m_Sound;
						return (int)m_SurfacesObject.m_Surfaces[i].m_Sound;
					}
				}
				return (int)m_DefaultSoundID;
			}
			Terrain component = hitInfo.transform.GetComponent<Terrain>();
			if (terrainChecker != null && currentLayer != terrainChecker.GetLayerName(base.transform.position, component))
			{
				currentLayer = terrainChecker.GetLayerName(base.transform.position, component);
			}
			for (int j = 0; j < m_SurfacesObject.m_TerrainSurfaces.Length; j++)
			{
				if (currentLayer == m_SurfacesObject.m_TerrainSurfaces[j].m_LayerName)
				{
					oldSurface = (int)m_SurfacesObject.m_TerrainSurfaces[j].m_Sound;
					return (int)m_SurfacesObject.m_TerrainSurfaces[j].m_Sound;
				}
			}
			return (int)m_DefaultSoundID;
		}
		return oldSurface;
	}

	private void PlaySound(int soundID, bool is2D)
	{
		EventDescription eventDescription = RuntimeManager.GetEventDescription(m_FootstepSounds);
		eventDescription.unloadSampleData();
		eventDescription.loadSampleData();
		EventInstance eventInstance = (currentInstance = RuntimeManager.CreateInstance(m_FootstepSounds));
		eventInstance.set3DAttributes(base.transform.To3DAttributes());
		eventInstance.setParameterByName("Is2D", is2D ? 1 : 0);
		eventInstance.setParameterByName("Surface", soundID);
		eventInstance.setParameterByName("Volume", m_VolumeAdditive);
		eventInstance.start();
	}

	public void PlaySoundImmediate(bool is2D = false)
	{
		PlaySound(CheckSurface(), is2D);
	}

	private void PlayerFootsteps()
	{
		float magnitude = Player.Instance.Motor.Velocity.magnitude;
		if (magnitude == 0f)
		{
			stepProgress = 0f;
		}
		if (!Player.Instance.Motor.GroundingStatus.HasAnyGround)
		{
			return;
		}
		stepProgress += magnitude * Time.deltaTime;
		if (stepProgress >= m_DistanceBetweenSteps)
		{
			stepProgress = 0f;
			if (inWater)
			{
				PlaySound(1, is2D: true);
			}
			else if (CheckSurface() >= 0)
			{
				PlaySound(CheckSurface(), is2D: true);
			}
		}
	}

	private void AiFootsteps()
	{
		float magnitude = navMeshAgent.velocity.magnitude;
		if (magnitude == 0f)
		{
			stepProgress = 0f;
		}
		stepProgress += magnitude * Time.deltaTime;
		if (stepProgress >= m_DistanceBetweenSteps)
		{
			stepProgress = 0f;
			if (inWater)
			{
				PlaySound(1, is2D: false);
			}
			else if (CheckSurface() >= 0)
			{
				PlaySound(CheckSurface(), is2D: false);
			}
		}
	}

	public void SetInWater(bool active)
	{
		inWater = active;
	}
}
