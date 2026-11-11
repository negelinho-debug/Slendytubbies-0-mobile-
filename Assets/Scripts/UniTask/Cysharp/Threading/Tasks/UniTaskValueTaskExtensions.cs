using System.Threading.Tasks;

namespace Cysharp.Threading.Tasks;

public static class UniTaskValueTaskExtensions
{
	public static ValueTask AsValueTask(this in UniTask task)
	{
		return task;
	}

	public static ValueTask<T> AsValueTask<T>(this in UniTask<T> task)
	{
		return task;
	}

	public static async UniTask<T> AsUniTask<T>(this ValueTask<T> task)
	{
		return await task;
	}

	public static async UniTask AsUniTask(this ValueTask task)
	{
		await task;
	}
}
