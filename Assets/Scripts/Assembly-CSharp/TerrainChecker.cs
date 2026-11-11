using UnityEngine;

public class TerrainChecker
{
	private float[] GetTextureMix(Vector3 playerPos, Terrain terr)
	{
		Vector3 position = terr.transform.position;
		TerrainData terrainData = terr.terrainData;
		int x = Mathf.RoundToInt((playerPos.x - position.x) / terrainData.size.x * (float)terrainData.alphamapWidth);
		int y = Mathf.RoundToInt((playerPos.z - position.z) / terrainData.size.x * (float)terrainData.alphamapHeight);
		float[,,] alphamaps = terrainData.GetAlphamaps(x, y, 1, 1);
		float[] array = new float[alphamaps.GetUpperBound(2) + 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = alphamaps[0, 0, i];
		}
		return array;
	}

	public string GetLayerName(Vector3 playerPos, Terrain terr)
	{
		float[] textureMix = GetTextureMix(playerPos, terr);
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < textureMix.Length; i++)
		{
			if (textureMix[i] > num)
			{
				num2 = i;
				num = textureMix[i];
			}
		}
		return terr.terrainData.terrainLayers[num2].name;
	}
}
