using System;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class Datamosh : MonoBehaviour
{
	private enum State
	{
		NotStarted,
		InProgress,
		End
	}

	private float effectTime;

	private EventInstance datamoshSnapshot;

	private CommandBuffer cmd;

	private bool start;

	private RenderTexture moshTexture;

	private RenderTexture moshIFrame;

	private Shader moshShader;

	private Material moshMaterial;

	private float effectTransparency;

	private bool fadeOut;

	[NonSerialized]
	public bool fadeIn;

	[NonSerialized]
	public bool fastFade;

	public float noUpdateTime;

	public float _fadeOutTime;

	public float _fadeOutTimeFast;

	[Range(0f, 1f)]
	public float _speedTolerance;

	public bool FadeOut
	{
		get
		{
			return fadeOut;
		}
		set
		{
			fadeOut = value;
		}
	}

	private void Awake()
	{
		cmd = new CommandBuffer
		{
			name = "Datamosh"
		};
		moshShader = Shader.Find("Hidden/Datamosh");
		moshMaterial = new Material(moshShader)
		{
			name = ".DataMoshMaterial",
			hideFlags = HideFlags.DontSave
		};
	}

	private void Update()
	{
		if (effectTransparency == 1f && !fadeIn)
		{
			base.enabled = false;
			return;
		}
		float num = (fastFade ? _fadeOutTimeFast : _fadeOutTime);
		if (fadeOut)
		{
			if (datamoshSnapshot.isValid())
			{
				datamoshSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
				datamoshSnapshot.release();
				datamoshSnapshot = default(EventInstance);
			}
			effectTransparency += Time.deltaTime * (1f / num);
		}
		else
		{
			effectTransparency -= Time.deltaTime * (1f / num);
			if (effectTransparency <= 0f)
			{
				fadeIn = false;
			}
		}
		effectTransparency = Mathf.Clamp01(effectTransparency);
	}

	private void OnDestroy()
	{
		if (datamoshSnapshot.isValid())
		{
			datamoshSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
			datamoshSnapshot.release();
		}
		UnityEngine.Object.Destroy(moshMaterial);
	}

	private void OnEnable()
	{
		Debug.Log("Enable datamosh");
		effectTransparency = 0f;
		fadeOut = false;
		fastFade = false;
		fadeIn = false;
	}

	private void OnDisable()
	{
		if ((bool)moshTexture)
		{
			UnityEngine.Object.Destroy(moshTexture);
		}
		if (datamoshSnapshot.isValid())
		{
			datamoshSnapshot.stop(STOP_MODE.IMMEDIATE);
			datamoshSnapshot = default(EventInstance);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!moshTexture || moshTexture.width != source.width || moshTexture.height != source.height)
		{
			if ((bool)moshIFrame)
			{
				UnityEngine.Object.Destroy(moshIFrame);
			}
			if ((bool)moshTexture)
			{
				UnityEngine.Object.Destroy(moshTexture);
			}
			moshTexture = new RenderTexture(source.width, source.height, source.graphicsFormat, GraphicsFormat.None);
			moshIFrame = new RenderTexture(source.width, source.height, source.graphicsFormat, GraphicsFormat.None);
			Graphics.Blit(source, moshTexture);
		}
		cmd.Clear();
		if (Time.time > noUpdateTime)
		{
			int num = Shader.PropertyToID("_DatamoshBackground");
			cmd.Blit(source, moshIFrame);
			cmd.GetTemporaryRT(num, moshTexture.descriptor);
			cmd.Blit(moshTexture, num);
			cmd.SetGlobalTexture("_FrameTexture", source);
			cmd.SetGlobalTexture("_I_FrameTexture", moshIFrame);
			cmd.SetGlobalVector("_Fade", new Vector4(Mathf.Lerp(0.45f, 1.45f, effectTransparency), effectTime, effectTransparency, _speedTolerance));
			cmd.Blit(num, moshTexture, moshMaterial, 0);
			cmd.ReleaseTemporaryRT(num);
		}
		cmd.Blit(moshTexture, destination);
		Graphics.ExecuteCommandBuffer(cmd);
	}
}
