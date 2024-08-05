using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Program;

public sealed unsafe class MT19937 : IDisposable
{
	// Period parameters
	private const int N = 624;
	private const int M = 397;

	// + 4 due to needing padding for SIMD shuffling
	private readonly uint* _mt = (uint*)NativeMemory.AlignedAlloc(N * sizeof(uint) + 4, 128 / 8); // mt lookup table
	private uint _mti; // mt index

	public void Dispose()
	{
		NativeMemory.AlignedFree(_mt);
	}

	// seeds mt table
	public void Init(uint s)
	{
		_mt[0] = s;
		for (_mti = 1; _mti < N; _mti++)
		{
			_mt[_mti] = 0x6C078965U * (_mt[_mti - 1] ^ (_mt[_mti - 1] >> 30)) + _mti;
		}
	}

	// taken from Pokefinder
	private void Shuffle()
	{
		var upperMask = Vector128.Create<uint>(0x80000000);
		var lowerMask = Vector128.Create<uint>(0x7FFFFFFF);
		var matrix = Vector128.Create<uint>(0x9908B0DF);
		var one = Vector128<uint>.One;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		Vector128<uint> Recursion(Vector128<uint> m0, Vector128<uint> m1, Vector128<uint> m2)
		{
			var y = (m0 & upperMask) | (m1 & lowerMask);
			var y1 = y >>> 1;
			var mag01 = Sse2.CompareEqual(y & one, one) & matrix;
			return y1 ^ mag01 ^ m2;
		}

		const uint firstHalfAlignedEnd = (N - M) & ~3u;
		for (var i = 0; i < firstHalfAlignedEnd; i += 4)
		{
			var m0 = Vector128.LoadAligned(_mt + i);
			var m1 = Vector128.Load(_mt + i + 1);
			var m2 = Vector128.Load(_mt + i + M);
			Recursion(m0, m1, m2).StoreAligned(_mt + i);
		}

		var last = Vector128.Load(_mt + firstHalfAlignedEnd + M).WithElement(3, _mt[0]);

		{
			var m0 = Vector128.LoadAligned(_mt + firstHalfAlignedEnd);
			var m1 = Vector128.Load(_mt + firstHalfAlignedEnd + 1);
			Recursion(m0, m1, last).StoreAligned(_mt + firstHalfAlignedEnd);
		}

		const uint secondHalfAlignedStart = N - M + (-(N - M) & 3);
		const uint secondHalfAlignedEnd = (firstHalfAlignedEnd + M) & ~3u;
		for (var i = secondHalfAlignedStart; i < secondHalfAlignedEnd; i += 4)
		{
			var m0 = Vector128.LoadAligned(_mt + i);
			var m1 = Vector128.Load(_mt + i + 1);
			var m2 = Vector128.Load(_mt + i - (N - M));
			Recursion(m0, m1, m2).StoreAligned(_mt + i);
		}

		{
			var m0 = Vector128.LoadAligned(_mt + secondHalfAlignedEnd);
			var m2 = Vector128.Load(_mt + secondHalfAlignedEnd - (N - M));
			Recursion(m0, last, m2).StoreAligned(_mt + secondHalfAlignedEnd);
		}
	}

	public void Advance(uint numAdvances)
	{
		var nextMti = _mti + numAdvances;
		while (nextMti >= N)
		{
			Shuffle();
			nextMti -= N;
		}

		_mti = nextMti;
	}

	public uint Next()
	{
		if (_mti >= N)
		{
			Shuffle();
			_mti = 0;
		}

		var y = _mt[_mti++];

		// Tempering
		y ^= y >> 11;
		y ^= (y << 7) & 0x9D2C5680U;
		y ^= (y << 15) & 0xEFC60000U;
		y ^= y >> 18;

		return y;
	}
}