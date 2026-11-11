using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class ShaderPrewarm : MonoBehaviour
{
	public bool _doVariantRecording;

	public bool _doPrewarmInEditor;

	public GraphicsStateCollection graphicsStateCollection;

	private void Awake()
	{
		if (!Application.isEditor || _doPrewarmInEditor)
		{
			graphicsStateCollection.WarmUp().Complete();
		}
	}

	private void Start()
	{
		if (_doVariantRecording)
		{
			graphicsStateCollection.BeginTrace();
		}
	}

	private void OnDestroy()
	{
		if (_doVariantRecording)
		{
			graphicsStateCollection.EndTrace();
		}
	}
}
