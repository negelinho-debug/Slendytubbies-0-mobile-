using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace PrimeTween;

internal class TweenArray
{
	internal readonly struct Lock : IDisposable
	{
		private readonly TweenArray array;

		internal Lock(TweenArray array)
		{
			this.array = array;
			array.numLocks++;
		}

		public void Dispose()
		{
			array.numLocks--;
		}
	}

	public struct Enumerator : IDisposable
	{
		private readonly TweenArray _array;

		private int _index;

		private readonly Lock _lock;

		public EnumeratorElement Current => new EnumeratorElement(_array, _index);

		internal Enumerator(TweenArray array)
		{
			_array = array;
			_index = -1;
			_lock = new Lock(array);
		}

		public bool MoveNext()
		{
			_index++;
			return _index < _array.Count;
		}

		void IDisposable.Dispose()
		{
			_lock.Dispose();
		}
	}

	public struct EnumeratorElement
	{
		private readonly TweenArray _array;

		internal readonly int index;

		public ref TweenData tween => ref _array[index];

		public ref UnmanagedTweenData data => ref _array.GetDataAt(index);

		internal EnumeratorElement(TweenArray array, int index)
		{
			_array = array;
			this.index = index;
		}
	}

	private TweenData[] _tweens;

	private NativeArray<UnmanagedTweenData> _data;

	private int _capacity;

	private int numLocks;

	internal readonly string _name;

	internal unsafe UnmanagedTweenData* _dataPtr { get; private set; }

	internal int Count { get; private set; }

	internal bool isLocked => numLocks > 0;

	internal ref TweenData this[int index] => ref _tweens[index];

	public int Capacity
	{
		get
		{
			return _capacity;
		}
		set
		{
			if (_capacity == value)
			{
				return;
			}
			TweenData[] tweens = _tweens;
			NativeArray<UnmanagedTweenData> data = _data;
			CreateBuffers(value);
			Array.Copy(tweens, _tweens, Count);
			NativeArray<UnmanagedTweenData>.Copy(data, _data, Count);
			data.Dispose();
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				EnumeratorElement current = enumerator.Current;
				_ = ref current.tween;
				_ = ref current.data;
			}
		}
	}

	public TweenArray(int capacity, string name)
	{
		CreateBuffers(capacity);
		_name = name;
	}

	private unsafe void CreateBuffers(int capacity)
	{
		_tweens = new TweenData[capacity];
		_data = new NativeArray<UnmanagedTweenData>(capacity, Allocator.Persistent);
		_dataPtr = (UnmanagedTweenData*)_data.GetUnsafePtr();
		_capacity = capacity;
	}

	internal void MoveAndClearOld(TweenData tween, int oldIndex, int newIndex)
	{
		_data[newIndex] = _data[oldIndex];
		tween.cold._index = newIndex;
		this[newIndex] = tween;
		this[oldIndex] = default(TweenData);
		_data[oldIndex] = default(UnmanagedTweenData);
	}

	internal void RemoveLast(ColdData cold)
	{
		int index = Count - 1;
		cold._tweenArray = null;
		cold._index = -1;
		this[index] = default(TweenData);
		_data[index] = default(UnmanagedTweenData);
		Count--;
	}

	internal void TrimEndNulls(int numRemoved)
	{
		for (int i = Count - numRemoved; i < Count; i++)
		{
		}
		Count -= numRemoved;
	}

	internal void Clear()
	{
		for (int i = 0; i < Count; i++)
		{
			this[i] = default(TweenData);
			_data[i] = default(UnmanagedTweenData);
		}
		Count = 0;
	}

	public void Add(ColdData tween)
	{
		int count = Count;
		if (count == Capacity)
		{
			Capacity = ((Capacity == 0) ? 4 : (Capacity * 2));
		}
		tween._tweenArray = this;
		tween._index = count;
		tween.data.id = tween.id;
		this[count] = new TweenData
		{
			cold = tween
		};
		Count++;
	}

	public void Dispose()
	{
		_data.Dispose();
		_data = default(NativeArray<UnmanagedTweenData>);
		_capacity = 0;
		Count = 0;
	}

	internal ColdData Single()
	{
		return this[0].cold;
	}

	internal NativeArray<UnmanagedTweenData> GetData()
	{
		return _data;
	}

	internal unsafe ref UnmanagedTweenData GetDataAt(int index)
	{
		return ref UnsafeUtility.AsRef<UnmanagedTweenData>(_dataPtr + index);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
