using UnityEngine;

public class ChangeFace : MonoBehaviour
{
	public float[] _faceOffsets;

	public float _blinkOffset;

	public float _blinkDuration;

	public float _timeBetweenBlinks;

	public Material _faceMaterial;

	private int currentOffset;

	private int nextOffset;

	private float nextBlink;

	private float blinkTime;

	private void Start()
	{
		nextBlink = float.PositiveInfinity;
	}

	public void SwapFace(int index)
	{
		nextOffset = index;
		nextBlink = Time.time;
	}

	private void Update()
	{
		if (Time.time >= nextBlink)
		{
			blinkTime = _blinkDuration;
			nextBlink = float.PositiveInfinity;
			_faceMaterial.mainTextureOffset = new Vector2(_blinkOffset, 0f);
		}
		if (blinkTime > 0f)
		{
			blinkTime -= Time.deltaTime;
			return;
		}
		currentOffset = nextOffset;
		_faceMaterial.mainTextureOffset = new Vector2(_faceOffsets[currentOffset], 0f);
	}
}
