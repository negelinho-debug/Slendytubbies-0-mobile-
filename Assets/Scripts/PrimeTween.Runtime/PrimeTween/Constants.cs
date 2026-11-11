using JetBrains.Annotations;

namespace PrimeTween;

internal static class Constants
{
	internal const string onCompleteCallbackIgnored = "Tween's OnComplete callback was ignored.";

	internal const string durationInvalidError = "Tween's duration is invalid.";

	internal const string cantManipulateNested = "It's not allowed to manipulate 'nested' animations, please use the parent Sequence instead.\nWhen an animation is added to another sequence, it becomes 'nested', and manipulating it directly is no longer allowed.\nUse Stop()/Complete()/isPaused/timeScale/elapsedTime/etc. of the parent animation instead.\n";

	internal const string isDeadMessage = "Animation is not alive. Please check the 'isAlive' property before calling this API.\n";

	internal const string unscaledTimeTooltip = "The animation will use real time, ignoring 'Time.timeScale'.";

	internal const string easeTooltip = "Easing curve of an animation.\n\nDefault is Ease.OutQuad. The Default ease can be modified via 'PrimeTweenConfig.defaultEase' setting.\n\nSet to Ease.Custom to control the easing with custom AnimationCurve.";

	internal const string cyclesTooltip = "The number of repetitions. Setting cycles to '-1' will repeat the animation indefinitely.";

	internal const string cycleModeTooltip = "Controls how the animation behaves with multiple cycles.";

	internal const string defaultCtorError = "Animation is not created. Please check the 'isAlive' property before calling this API.\n- Use 'Sequence.Create()' to start a Sequence.\n- Use static 'Tween.' methods to start a Tween.\n";

	internal const string startDelayTooltip = "Delays the start of a tween.";

	internal const string endDelayTooltip = "Delays the completion of a tween.\n\nFor example, can be used to add the delay between cycles.\n\nOr can be used to postpone the execution of the onComplete callback.";

	internal const string updateTypeTooltip = "Controls Unity's event function, which updates the animation.\n\nThe default is MonoBehaviour.Update().";

	internal const string infiniteTweenInSequenceError = "It's not allowed to have infinite tweens (cycles == -1) in a sequence. If you want the sequence to repeat forever, SetRemainingCycles(-1) on the parent sequence instead.";

	internal const string customTweensDontSupportStartFromCurrentWarning = "Custom tweens don't support the 'startFromCurrent' because they don't know the current value of animated property.\nThis means that the animated value will be changed abruptly if a new tween is started mid-way.\nPlease pass the current value to the 'T.WithDirection(bool toEndValue, T currentValue)' method or use the constructor that accepts the 'startValue'.\n";

	internal const string startFromCurrentTooltip = "If true, the current value of an animated property will be used instead of the 'startValue'.\n\nThis field typically should not be manipulated directly. Instead, it's set by TweenSettings(T endValue, TweenSettings settings) constructor or by WithDirection() method.";

	internal const string startValueTooltip = "Start value of an animation.\n\nFor example, if you're animating a window, the 'startValue' can represent the closed (off-screen) position of the window.";

	internal const string endValueTooltip = "End value of an animation.\n\nFor example, if you're animating a window, the 'endValue' can represent the opened position of the window.";

	internal const string setTweensCapacityMethod = "'PrimeTweenConfig.SetTweensCapacity(int capacity)'";

	internal const string maxAliveTweens = "Max alive tweens";

	internal const string animationAlreadyStarted = "Animation has already been started, it's not allowed to manipulate it anymore.";

	internal const string recursiveCallError = "Please don't call this API recursively from Tween.Custom() or tween.OnUpdate().";

	internal const string nestTwiceError = "An animation can be added to a sequence only once and can only belong to one sequence.";

	internal const string addDeadTweenToSequenceError = "It's not allowed to add 'dead' tweens to a sequence.";

	internal const string customAnimationCurveInvalidError = "Ease is Ease.Custom, but AnimationCurve is not configured correctly. Using Ease.Default instead.";

	internal const string cycleModeRestartTooltip = "Restarts the animation from the beginning.";

	internal const string cycleModeYoyoTooltip = "Animates forth and back, like a yoyo. Easing is the same on the backward cycle.";

	internal const string cycleModeIncrementalTooltip = "At the end of a cycle increments the `endValue` by the difference between `startValue` and `endValue`.\n\nFor example, if a tween moves position.x from 0 to 1, then after the first cycle, the tween will move the position.x from 1 to 2, and so on.";

	internal const string cycleModeRewindTooltip = "Rewinds the animation as if time was reversed. Easing is reversed on the backward cycle.";

	internal const string materialPropBlockTooltip = "Animates a MaterialPropertyBlock property and applies it to the target by calling Renderer.SetPropertyBlock().\nUse this to animate sharedMaterial properties without creating new material instances.\n\nNOTE: PrimeTween doesn't clear the MaterialPropertyBlock after animation completion to preserve the animated property at its endValue.\nAs a result, directly modifying the Material property will have no effect since the MaterialPropertyBlock takes precedence.\nTo modify the sharedMaterial property after animation completion, call 'renderer.SetPropertyBlock(null)' to clear the override.";

	[NotNull]
	internal static string buildWarningCanBeDisabledMessage(string settingName)
	{
		return "To disable this warning, set 'PrimeTweenConfig." + settingName + " = false;'.";
	}
}
