using UnityEngine;
using UnityEngine.Experimental.Rendering;

[ExecuteAlways]
public class IdeaThing : MonoBehaviour
{
	public RenderTexture rtColor;

	public RenderTexture rtDepth;

	public Camera _mainCamera;

	public Camera _secondaryCamera;

	public Transform _changeSphere;

	public float _changeSphereRadius;

	private void OnEnable()
	{
		rtColor = new RenderTexture(_mainCamera.pixelWidth, _mainCamera.pixelHeight, 1, DefaultFormat.HDR);
		rtColor.depth = 0;
		rtColor.name = "Portal Render Texture";
		rtColor.Create();
		rtDepth = new RenderTexture(_mainCamera.pixelWidth, _mainCamera.pixelHeight, 1, DefaultFormat.DepthStencil);
		rtDepth.depth = 32;
		rtDepth.name = "Portal Render Texture Depth";
		rtDepth.Create();
		_secondaryCamera.SetTargetBuffers(rtColor.colorBuffer, rtDepth.depthBuffer);
		_mainCamera.depthTextureMode |= DepthTextureMode.Depth;
		_secondaryCamera.depthTextureMode |= DepthTextureMode.Depth;
	}

	private void Update()
	{
		Vector4 value = _changeSphere.position;
		value.w = _changeSphere.localScale.x;
		Shader.SetGlobalVector("_TransitionSphere", value);
	}

	private void OnDisable()
	{
		rtColor.Release();
		rtDepth.Release();
	}
}
