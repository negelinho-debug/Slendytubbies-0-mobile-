using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace HostGame;

[BurstCompile]
public static class CurveExtensions
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInOut2_000005EE_0024PostfixBurstDelegate(float x);

	internal static class SmoothInOut2_000005EE_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInOut2_000005EE_0024PostfixBurstDelegate>(SmoothInOut2).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInOut2_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInOut3_000005EF_0024PostfixBurstDelegate(float x);

	internal static class SmoothInOut3_000005EF_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInOut3_000005EF_0024PostfixBurstDelegate>(SmoothInOut3).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInOut3_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInOut4_000005F0_0024PostfixBurstDelegate(float x);

	internal static class SmoothInOut4_000005F0_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInOut4_000005F0_0024PostfixBurstDelegate>(SmoothInOut4).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInOut4_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInSin_000005F1_0024PostfixBurstDelegate(float x);

	internal static class SmoothInSin_000005F1_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInSin_000005F1_0024PostfixBurstDelegate>(SmoothInSin).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInSin_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothOutSin_000005F2_0024PostfixBurstDelegate(float x);

	internal static class SmoothOutSin_000005F2_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothOutSin_000005F2_0024PostfixBurstDelegate>(SmoothOutSin).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothOutSin_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInOutSin_000005F3_0024PostfixBurstDelegate(float x);

	internal static class SmoothInOutSin_000005F3_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInOutSin_000005F3_0024PostfixBurstDelegate>(SmoothInOutSin).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInOutSin_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInExp_000005F4_0024PostfixBurstDelegate(float x);

	internal static class SmoothInExp_000005F4_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInExp_000005F4_0024PostfixBurstDelegate>(SmoothInExp).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInExp_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothOutExp_000005F5_0024PostfixBurstDelegate(float x);

	internal static class SmoothOutExp_000005F5_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothOutExp_000005F5_0024PostfixBurstDelegate>(SmoothOutExp).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothOutExp_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInOutExp_000005F6_0024PostfixBurstDelegate(float x);

	internal static class SmoothInOutExp_000005F6_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInOutExp_000005F6_0024PostfixBurstDelegate>(SmoothInOutExp).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInOutExp_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInCirc_000005F7_0024PostfixBurstDelegate(float x);

	internal static class SmoothInCirc_000005F7_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInCirc_000005F7_0024PostfixBurstDelegate>(SmoothInCirc).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInCirc_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothOutCirc_000005F8_0024PostfixBurstDelegate(float x);

	internal static class SmoothOutCirc_000005F8_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothOutCirc_000005F8_0024PostfixBurstDelegate>(SmoothOutCirc).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothOutCirc_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float SmoothInOutCirc_000005F9_0024PostfixBurstDelegate(float x);

	internal static class SmoothInOutCirc_000005F9_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SmoothInOutCirc_000005F9_0024PostfixBurstDelegate>(SmoothInOutCirc).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float>)functionPointer)(x);
				}
			}
			return SmoothInOutCirc_0024BurstManaged(x);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float Evaluate_000005FA_0024PostfixBurstDelegate(EaseCurveType curve, float x);

	internal static class Evaluate_000005FA_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Evaluate_000005FA_0024PostfixBurstDelegate>(Evaluate).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(EaseCurveType curve, float x)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<EaseCurveType, float, float>)functionPointer)(curve, x);
				}
			}
			return curve.Evaluate_0024BurstManaged(x);
		}
	}

	public const float PI = MathF.PI;

	public static float SmoothIn2(float x)
	{
		return x * x;
	}

	public static float SmoothIn3(float x)
	{
		return x * x * x;
	}

	public static float SmoothIn4(float x)
	{
		return x * x * x * x;
	}

	public static float SmoothOut2(float x)
	{
		return 1f - (1f - x) * (1f - x);
	}

	public static float SmoothOut3(float x)
	{
		return 1f - (1f - x) * (1f - x) * (1f - x);
	}

	public static float SmoothOut4(float x)
	{
		return 1f - (1f - x) * (1f - x) * (1f - x) * (1f - x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInOut2_000005EE_0024PostfixBurstDelegate))]
	public static float SmoothInOut2(float x)
	{
		return SmoothInOut2_000005EE_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInOut3_000005EF_0024PostfixBurstDelegate))]
	public static float SmoothInOut3(float x)
	{
		return SmoothInOut3_000005EF_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInOut4_000005F0_0024PostfixBurstDelegate))]
	public static float SmoothInOut4(float x)
	{
		return SmoothInOut4_000005F0_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInSin_000005F1_0024PostfixBurstDelegate))]
	public static float SmoothInSin(float x)
	{
		return SmoothInSin_000005F1_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothOutSin_000005F2_0024PostfixBurstDelegate))]
	public static float SmoothOutSin(float x)
	{
		return SmoothOutSin_000005F2_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInOutSin_000005F3_0024PostfixBurstDelegate))]
	public static float SmoothInOutSin(float x)
	{
		return SmoothInOutSin_000005F3_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInExp_000005F4_0024PostfixBurstDelegate))]
	public static float SmoothInExp(float x)
	{
		return SmoothInExp_000005F4_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothOutExp_000005F5_0024PostfixBurstDelegate))]
	public static float SmoothOutExp(float x)
	{
		return SmoothOutExp_000005F5_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInOutExp_000005F6_0024PostfixBurstDelegate))]
	public static float SmoothInOutExp(float x)
	{
		return SmoothInOutExp_000005F6_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInCirc_000005F7_0024PostfixBurstDelegate))]
	public static float SmoothInCirc(float x)
	{
		return SmoothInCirc_000005F7_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothOutCirc_000005F8_0024PostfixBurstDelegate))]
	public static float SmoothOutCirc(float x)
	{
		return SmoothOutCirc_000005F8_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002ESmoothInOutCirc_000005F9_0024PostfixBurstDelegate))]
	public static float SmoothInOutCirc(float x)
	{
		return SmoothInOutCirc_000005F9_0024BurstDirectCall.Invoke(x);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(HostGame_002EEvaluate_000005FA_0024PostfixBurstDelegate))]
	public static float Evaluate(this EaseCurveType curve, float x)
	{
		return Evaluate_000005FA_0024BurstDirectCall.Invoke(curve, x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInOut2_0024BurstManaged(float x)
	{
		return x * x * x + (1f - x) * SmoothOut2(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInOut3_0024BurstManaged(float x)
	{
		return x * x * x * x + (1f - x) * SmoothOut3(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInOut4_0024BurstManaged(float x)
	{
		return x * x * x * x * x + (1f - x) * SmoothOut4(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInSin_0024BurstManaged(float x)
	{
		return 1f - math.cos(x * MathF.PI / 2f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothOutSin_0024BurstManaged(float x)
	{
		return math.sin(x * MathF.PI / 2f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInOutSin_0024BurstManaged(float x)
	{
		return (0f - (math.cos(MathF.PI * x) - 1f)) / 2f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInExp_0024BurstManaged(float x)
	{
		return math.pow(2f, 10f * x - 10f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothOutExp_0024BurstManaged(float x)
	{
		return 1f - math.pow(2f, -10f * x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInOutExp_0024BurstManaged(float x)
	{
		if (!(x < 0.5f))
		{
			return (2f - math.pow(2f, -20f * x + 10f)) / 2f;
		}
		return math.pow(2f, 20f * x - 10f) / 2f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInCirc_0024BurstManaged(float x)
	{
		return 1f - math.sqrt(1f - x * x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothOutCirc_0024BurstManaged(float x)
	{
		return math.sqrt(1f - (x - 1f) * (x - 1f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float SmoothInOutCirc_0024BurstManaged(float x)
	{
		if (!(x < 0.5f))
		{
			return (math.sqrt(1f - (-2f * x + 2f) * (-2f * x + 2f)) + 1f) / 2f;
		}
		return (1f - math.sqrt(1f - 4f * x * x)) / 2f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float Evaluate_0024BurstManaged(this EaseCurveType curve, float x)
	{
		return curve switch
		{
			EaseCurveType.Linear => x, 
			EaseCurveType.EaseInSin => SmoothInSin(x), 
			EaseCurveType.EaseOutSin => SmoothOutSin(x), 
			EaseCurveType.EaseInOutSin => SmoothInOutSin(x), 
			EaseCurveType.EaseInQuad => SmoothIn2(x), 
			EaseCurveType.EaseOutQuad => SmoothOut2(x), 
			EaseCurveType.EaseInOutQuad => SmoothInOut2(x), 
			EaseCurveType.EaseInCubic => SmoothIn3(x), 
			EaseCurveType.EaseOutCubic => SmoothOut3(x), 
			EaseCurveType.EaseInOutCubic => SmoothInOut3(x), 
			EaseCurveType.EaseInQuart => SmoothIn4(x), 
			EaseCurveType.EaseOutQuart => SmoothOut4(x), 
			EaseCurveType.EaseInOutQuart => SmoothInOut4(x), 
			EaseCurveType.EaseInExp => SmoothInExp(x), 
			EaseCurveType.EaseOutExp => SmoothOutExp(x), 
			EaseCurveType.EaseInOutExp => SmoothInOutExp(x), 
			EaseCurveType.EaseInCircle => SmoothInCirc(x), 
			EaseCurveType.EaseOutCircle => SmoothOutCirc(x), 
			EaseCurveType.EaseInOutCircle => SmoothInOutCirc(x), 
			_ => x, 
		};
	}
}
