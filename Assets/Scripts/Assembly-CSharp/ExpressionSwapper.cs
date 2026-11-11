using UnityEngine;

public class ExpressionSwapper : MonoBehaviour
{
	public Transform _expressionBone;

	public Renderer _renderer;

	[Space(10f)]
	public float _scaleFactor = 0.45f;

	public int _materialIndex;

	private Vector3 startOffset;

	private void Start()
	{
		startOffset = _expressionBone.localPosition;
	}

	private void Update()
	{
		float x = (_expressionBone.localPosition - startOffset).magnitude / _scaleFactor;
		_renderer.materials[_materialIndex].mainTextureOffset = new Vector2(x, 0f);
	}
}
