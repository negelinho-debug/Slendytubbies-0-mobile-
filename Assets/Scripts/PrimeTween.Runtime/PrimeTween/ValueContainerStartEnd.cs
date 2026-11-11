using System;
using UnityEngine;

namespace PrimeTween;

[Serializable]
internal struct ValueContainerStartEnd
{
	[SerializeField]
	internal TweenAnimation.TweenType tweenType;

	[SerializeField]
	[Tooltip("If true, the current value of an animated property will be used instead of the 'startValue'.\n\nThis field typically should not be manipulated directly. Instead, it's set by TweenSettings(T endValue, TweenSettings settings) constructor or by WithDirection() method.")]
	internal bool startFromCurrent;

	[SerializeField]
	[Tooltip("Start value of an animation.\n\nFor example, if you're animating a window, the 'startValue' can represent the closed (off-screen) position of the window.")]
	internal TweenAnimation.ValueWrapper startValue;

	[SerializeField]
	[Tooltip("End value of an animation.\n\nFor example, if you're animating a window, the 'endValue' can represent the opened position of the window.")]
	internal TweenAnimation.ValueWrapper endValue;
}
