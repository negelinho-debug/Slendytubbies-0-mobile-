using System;
using System.IO;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
	public static VideoManager Instance;

	public VideoVolumeInfo[] _volumeInfos;

	public VideoPlayer _videoComponent;

	public RawImage _imageComponent;

	[Header("Videos")]
	public VideoClip _confetti;

	public VideoClip _dipsyDeath;

	public VideoClip _laaLaaDeath;

	public VideoClip _laaLaaEyes;

	public VideoClip _poDeath;

	public VideoClip _tinkyTransition;

	public VideoClip _custardMachine;

	public VideoClip _whiteDeath;

	public VideoClip _dilatedEyes;

	public VideoClip _watchDogScreamer;

	public VideoClip _clawyEnding;

	public VideoClip _gameEnding;

	public VideoClip _intro;

	public Videos _testClip;

	public static Action OnFinished;

	private bool isClawyEnding;

	private bool isStopped;

	private float hideDelay;

	private Bus masterBus;

	public bool isPreparing { get; private set; }

	public bool isPlaying { get; private set; }

	[ContextMenu("Play Test Video")]
	private void PlayTestVideo()
	{
		PlayVideo(_testClip);
	}

	private float GetVolume(Videos video)
	{
		for (int i = 0; i < _volumeInfos.Length; i++)
		{
			ref VideoVolumeInfo reference = ref _volumeInfos[i];
			if (reference.video == video)
			{
				return reference.volume;
			}
		}
		return 1f;
	}

	private void SetVolume(Videos video)
	{
		float volume = GetVolume(video);
		for (ushort num = 0; num < _videoComponent.audioTrackCount; num++)
		{
			_videoComponent.SetDirectAudioVolume(num, volume);
		}
	}

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		_imageComponent.color = Color.clear;
		_videoComponent.playOnAwake = false;
		_videoComponent.waitForFirstFrame = true;
		masterBus = RuntimeManager.GetBus("bus:/");
		_videoComponent.prepareCompleted += OnPrepareCompleted;
		_videoComponent.loopPointReached += OnVideoFinished;
	}

	private void OnDestroy()
	{
		_videoComponent.prepareCompleted -= OnPrepareCompleted;
		_videoComponent.loopPointReached -= OnVideoFinished;
	}

	public void PlayVideo(int index)
	{
		PlayVideo((Videos)index);
	}

	public void PlayVideo(Videos video)
	{
		Stop();
		isPreparing = true;
		isPlaying = true;
		isStopped = false;
		VideoPlayer videoComponent = _videoComponent;
		videoComponent.clip = video switch
		{
			Videos.Confetti => _confetti, 
			Videos.DipsyDeath => _dipsyDeath, 
			Videos.LaaLaaDeath => _laaLaaDeath, 
			Videos.LaaLaaEyes => _laaLaaEyes, 
			Videos.PoDeath => _poDeath, 
			Videos.TinkyTransition => _tinkyTransition, 
			Videos.CustardMachine => _custardMachine, 
			Videos.WhiteDeath => _whiteDeath, 
			Videos.DilatedEyes => _dilatedEyes, 
			Videos.WatchDogScreamer => _watchDogScreamer, 
			Videos.ClawyEnding => _clawyEnding, 
			Videos.GameEnding => _gameEnding, 
			Videos.Intro => _intro, 
			_ => _videoComponent.clip, 
		};
		isClawyEnding = video == Videos.ClawyEnding;
		SetVolume(video);
		if ((bool)_imageComponent && video != Videos.GameEnding && _imageComponent.texture is RenderTexture renderTexture)
		{
			Color backgroundColor = Color.black;
			if (video == Videos.Confetti)
			{
				backgroundColor = Color.clear;
			}
			CommandBuffer commandBuffer = new CommandBuffer();
			commandBuffer.SetRenderTarget(renderTexture);
			commandBuffer.ClearRenderTarget(RTClearFlags.Color, backgroundColor);
			Graphics.ExecuteCommandBuffer(commandBuffer);
		}
		_videoComponent.Prepare();
	}

	private void OnPrepareCompleted(VideoPlayer vp)
	{
		if (isPreparing)
		{
			isPreparing = false;
			_imageComponent.color = Color.white;
			_videoComponent.aspectRatio = VideoAspectRatio.Stretch;
			_videoComponent.Play();
			if (_videoComponent.clip != _confetti && _videoComponent.clip != _custardMachine && _videoComponent.clip != _laaLaaEyes)
			{
				RuntimeManager.MuteAllEvents(muted: true);
			}
		}
	}

	private void OnVideoFinished(VideoPlayer vp)
	{
		if (_videoComponent.clip == _gameEnding)
		{
			File.WriteAllText("./_.txt", "site17\n \nLMB - Attack \nRMB - ???");
			Application.Quit();
			return;
		}
		OnFinished?.Invoke();
		if (!isClawyEnding)
		{
			Stop();
		}
		isStopped = true;
		hideDelay = Time.time + 0.01f;
		Time.timeScale = 1f;
		RuntimeManager.MuteAllEvents(muted: false);
	}

	private void Update()
	{
		if (isStopped && !isClawyEnding && Time.time > hideDelay)
		{
			isStopped = false;
			_imageComponent.color = Color.clear;
		}
		if (_videoComponent.isPlaying)
		{
			InputManager.Instance.WantHideMouse = true;
		}
	}

	public void Stop()
	{
		isPreparing = false;
		isPlaying = false;
		_videoComponent.Stop();
	}
}
