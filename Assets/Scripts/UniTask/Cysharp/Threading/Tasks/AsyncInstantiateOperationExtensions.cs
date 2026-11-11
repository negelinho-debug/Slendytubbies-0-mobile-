using System;
using System.Threading;
using System.Threading.Tasks.Sources;
using Cysharp.Threading.Tasks.Internal;
using UnityEngine;

namespace Cysharp.Threading.Tasks;

public static class AsyncInstantiateOperationExtensions
{
	private sealed class AsyncInstantiateOperationConfiguredSource : IUniTaskSource<UnityEngine.Object[]>, IUniTaskSource, IValueTaskSource, IValueTaskSource<UnityEngine.Object[]>, IPlayerLoopItem, ITaskPoolNode<AsyncInstantiateOperationConfiguredSource>
	{
		private static TaskPool<AsyncInstantiateOperationConfiguredSource> pool;

		private AsyncInstantiateOperationConfiguredSource nextNode;

		private AsyncInstantiateOperation asyncOperation;

		private IProgress<float> progress;

		private CancellationToken cancellationToken;

		private CancellationTokenRegistration cancellationTokenRegistration;

		private bool cancelImmediately;

		private bool completed;

		private UniTaskCompletionSourceCore<UnityEngine.Object[]> core;

		private Action<AsyncOperation> continuationAction;

		public ref AsyncInstantiateOperationConfiguredSource NextNode => ref nextNode;

		static AsyncInstantiateOperationConfiguredSource()
		{
			TaskPool.RegisterSizeGetter(typeof(AsyncInstantiateOperationConfiguredSource), () => pool.Size);
		}

		private AsyncInstantiateOperationConfiguredSource()
		{
			continuationAction = Continuation;
		}

		public static IUniTaskSource<UnityEngine.Object[]> Create(AsyncInstantiateOperation asyncOperation, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, bool cancelImmediately, out short token)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return AutoResetUniTaskCompletionSource<UnityEngine.Object[]>.CreateFromCanceled(cancellationToken, out token);
			}
			if (!pool.TryPop(out var result))
			{
				result = new AsyncInstantiateOperationConfiguredSource();
			}
			result.asyncOperation = asyncOperation;
			result.progress = progress;
			result.cancellationToken = cancellationToken;
			result.cancelImmediately = cancelImmediately;
			result.completed = false;
			asyncOperation.completed += result.continuationAction;
			if (cancelImmediately && cancellationToken.CanBeCanceled)
			{
				result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(delegate(object state)
				{
					AsyncInstantiateOperationConfiguredSource asyncInstantiateOperationConfiguredSource = (AsyncInstantiateOperationConfiguredSource)state;
					asyncInstantiateOperationConfiguredSource.core.TrySetCanceled(asyncInstantiateOperationConfiguredSource.cancellationToken);
				}, result);
			}
			PlayerLoopHelper.AddAction(timing, result);
			token = result.core.Version;
			return result;
		}

		public UnityEngine.Object[] GetResult(short token)
		{
			try
			{
				return core.GetResult(token);
			}
			finally
			{
				if (!cancelImmediately || !cancellationToken.IsCancellationRequested)
				{
					TryReturn();
				}
			}
		}

		void IUniTaskSource.GetResult(short token)
		{
			GetResult(token);
		}

		public UniTaskStatus GetStatus(short token)
		{
			return core.GetStatus(token);
		}

		public UniTaskStatus UnsafeGetStatus()
		{
			return core.UnsafeGetStatus();
		}

		public void OnCompleted(Action<object> continuation, object state, short token)
		{
			core.OnCompleted(continuation, state, token);
		}

		public bool MoveNext()
		{
			if (completed || asyncOperation == null)
			{
				return false;
			}
			if (cancellationToken.IsCancellationRequested)
			{
				core.TrySetCanceled(cancellationToken);
				return false;
			}
			if (progress != null)
			{
				progress.Report(asyncOperation.progress);
			}
			if (asyncOperation.isDone)
			{
				core.TrySetResult(asyncOperation.Result);
				return false;
			}
			return true;
		}

		private bool TryReturn()
		{
			core.Reset();
			asyncOperation.completed -= continuationAction;
			asyncOperation = null;
			progress = null;
			cancellationToken = default(CancellationToken);
			cancellationTokenRegistration.Dispose();
			cancelImmediately = false;
			return pool.TryPush(this);
		}

		private void Continuation(AsyncOperation _)
		{
			if (!completed)
			{
				completed = true;
				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
				}
				else
				{
					core.TrySetResult(asyncOperation.Result);
				}
			}
		}
	}

	private sealed class AsyncInstantiateOperationConfiguredSource<T> : IUniTaskSource<T[]>, IUniTaskSource, IValueTaskSource, IValueTaskSource<T[]>, IPlayerLoopItem, ITaskPoolNode<AsyncInstantiateOperationConfiguredSource<T>> where T : UnityEngine.Object
	{
		private static TaskPool<AsyncInstantiateOperationConfiguredSource<T>> pool;

		private AsyncInstantiateOperationConfiguredSource<T> nextNode;

		private AsyncInstantiateOperation<T> asyncOperation;

		private IProgress<float> progress;

		private CancellationToken cancellationToken;

		private CancellationTokenRegistration cancellationTokenRegistration;

		private bool cancelImmediately;

		private bool completed;

		private UniTaskCompletionSourceCore<T[]> core;

		private Action<AsyncOperation> continuationAction;

		public ref AsyncInstantiateOperationConfiguredSource<T> NextNode => ref nextNode;

		static AsyncInstantiateOperationConfiguredSource()
		{
			TaskPool.RegisterSizeGetter(typeof(AsyncInstantiateOperationConfiguredSource<T>), () => pool.Size);
		}

		private AsyncInstantiateOperationConfiguredSource()
		{
			continuationAction = Continuation;
		}

		public static IUniTaskSource<T[]> Create(AsyncInstantiateOperation<T> asyncOperation, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, bool cancelImmediately, out short token)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return AutoResetUniTaskCompletionSource<T[]>.CreateFromCanceled(cancellationToken, out token);
			}
			if (!pool.TryPop(out var result))
			{
				result = new AsyncInstantiateOperationConfiguredSource<T>();
			}
			result.asyncOperation = asyncOperation;
			result.progress = progress;
			result.cancellationToken = cancellationToken;
			result.cancelImmediately = cancelImmediately;
			result.completed = false;
			asyncOperation.completed += result.continuationAction;
			if (cancelImmediately && cancellationToken.CanBeCanceled)
			{
				result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(delegate(object state)
				{
					AsyncInstantiateOperationConfiguredSource<T> asyncInstantiateOperationConfiguredSource = (AsyncInstantiateOperationConfiguredSource<T>)state;
					asyncInstantiateOperationConfiguredSource.core.TrySetCanceled(asyncInstantiateOperationConfiguredSource.cancellationToken);
				}, result);
			}
			PlayerLoopHelper.AddAction(timing, result);
			token = result.core.Version;
			return result;
		}

		public T[] GetResult(short token)
		{
			try
			{
				return core.GetResult(token);
			}
			finally
			{
				if (!cancelImmediately || !cancellationToken.IsCancellationRequested)
				{
					TryReturn();
				}
			}
		}

		void IUniTaskSource.GetResult(short token)
		{
			GetResult(token);
		}

		public UniTaskStatus GetStatus(short token)
		{
			return core.GetStatus(token);
		}

		public UniTaskStatus UnsafeGetStatus()
		{
			return core.UnsafeGetStatus();
		}

		public void OnCompleted(Action<object> continuation, object state, short token)
		{
			core.OnCompleted(continuation, state, token);
		}

		public bool MoveNext()
		{
			if (completed || asyncOperation == null)
			{
				return false;
			}
			if (cancellationToken.IsCancellationRequested)
			{
				core.TrySetCanceled(cancellationToken);
				return false;
			}
			if (progress != null)
			{
				progress.Report(asyncOperation.progress);
			}
			if (asyncOperation.isDone)
			{
				core.TrySetResult(asyncOperation.Result);
				return false;
			}
			return true;
		}

		private bool TryReturn()
		{
			core.Reset();
			asyncOperation.completed -= continuationAction;
			asyncOperation = null;
			progress = null;
			cancellationToken = default(CancellationToken);
			cancellationTokenRegistration.Dispose();
			cancelImmediately = false;
			return pool.TryPush(this);
		}

		private void Continuation(AsyncOperation _)
		{
			if (!completed)
			{
				completed = true;
				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
				}
				else
				{
					core.TrySetResult(asyncOperation.Result);
				}
			}
		}
	}

	public static UniTask<UnityEngine.Object[]> WithCancellation<T>(this AsyncInstantiateOperation asyncOperation, CancellationToken cancellationToken)
	{
		return asyncOperation.ToUniTask(null, PlayerLoopTiming.Update, cancellationToken);
	}

	public static UniTask<UnityEngine.Object[]> WithCancellation<T>(this AsyncInstantiateOperation asyncOperation, CancellationToken cancellationToken, bool cancelImmediately)
	{
		return asyncOperation.ToUniTask(null, PlayerLoopTiming.Update, cancellationToken, cancelImmediately);
	}

	public static UniTask<UnityEngine.Object[]> ToUniTask(this AsyncInstantiateOperation asyncOperation, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
	{
		Error.ThrowArgumentNullException(asyncOperation, "asyncOperation");
		if (cancellationToken.IsCancellationRequested)
		{
			return UniTask.FromCanceled<UnityEngine.Object[]>(cancellationToken);
		}
		if (asyncOperation.isDone)
		{
			return UniTask.FromResult(asyncOperation.Result);
		}
		short token;
		return new UniTask<UnityEngine.Object[]>(AsyncInstantiateOperationConfiguredSource.Create(asyncOperation, timing, progress, cancellationToken, cancelImmediately, out token), token);
	}

	public static UniTask<T[]> WithCancellation<T>(this AsyncInstantiateOperation<T> asyncOperation, CancellationToken cancellationToken) where T : UnityEngine.Object
	{
		return asyncOperation.ToUniTask(null, PlayerLoopTiming.Update, cancellationToken);
	}

	public static UniTask<T[]> WithCancellation<T>(this AsyncInstantiateOperation<T> asyncOperation, CancellationToken cancellationToken, bool cancelImmediately) where T : UnityEngine.Object
	{
		return asyncOperation.ToUniTask(null, PlayerLoopTiming.Update, cancellationToken, cancelImmediately);
	}

	public static UniTask<T[]> ToUniTask<T>(this AsyncInstantiateOperation<T> asyncOperation, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false) where T : UnityEngine.Object
	{
		Error.ThrowArgumentNullException(asyncOperation, "asyncOperation");
		if (cancellationToken.IsCancellationRequested)
		{
			return UniTask.FromCanceled<T[]>(cancellationToken);
		}
		if (asyncOperation.isDone)
		{
			return UniTask.FromResult(asyncOperation.Result);
		}
		short token;
		return new UniTask<T[]>(AsyncInstantiateOperationConfiguredSource<T>.Create(asyncOperation, timing, progress, cancellationToken, cancelImmediately, out token), token);
	}
}
