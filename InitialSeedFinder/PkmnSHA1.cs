using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// base impl taken from acryptohashnet, with modifications + SIMD improvements for Pokemon

namespace Program;

/// <summary>
/// Defined by FIPS 180-4: Secure Hash Standard (SHS)
/// </summary>
public static class PkmnSHA1
{
	// round 1
	private const uint ConstantR1 = 0x5a827999; // [2 ^ 30 * sqrt(2)]
	// round 2
	private const uint ConstantR2 = 0x6ed9eba1; // [2 ^ 30 * sqrt(3)]
	// round 3
	private const uint ConstantR3 = 0x8f1bbcdc; // [2 ^ 30 * sqrt(5)]
	// round 4
	private const uint ConstantR4 = 0xca62c1d6; // [2 ^ 30 * sqrt(10)]

	private const uint InitA = 0x67452301;
	private const uint InitB = 0xefcdab89;
	private const uint InitC = 0x98badcfe;
	private const uint InitD = 0x10325476;
	private const uint InitE = 0xc3d2e1f0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Ch(uint x, uint y, uint z) => (x & y) ^ (~x & z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Maj(uint x, uint y, uint z) => (x & y) ^ (x & z) ^ (y & z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Parity(uint x, uint y, uint z) => x ^ y ^ z;

	// non-SIMD reference impl
	public static void HashBlock(ReadOnlySpan<byte> block, Span<byte> dst)
	{
		Span<uint> buffer = stackalloc uint[80];

		// Fill buffer for transformation
		for (var i = 0; i < 16; i++)
		{
			buffer[i] = BinaryPrimitives.ReadUInt32BigEndian(block[(i * 4)..]);
		}

		// Expand buffer
		for (var ii = 16; ii < buffer.Length; ii++)
		{
			var x = buffer[ii - 3] ^ buffer[ii - 8] ^ buffer[ii - 14] ^ buffer[ii - 16];
			// added in sha-1
			buffer[ii] = BitOperations.RotateLeft(x, 1);
		}

		var a = InitA;
		var b = InitB;
		var c = InitC;
		var d = InitD;
		var e = InitE;

		var index = 0;
		// round 1
		for (; index < 20 && index < buffer.Length - 4; index += 5)
		{
			e += buffer[index + 0] + ConstantR1 + Ch(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[index + 1] + ConstantR1 + Ch(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[index + 2] + ConstantR1 + Ch(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[index + 3] + ConstantR1 + Ch(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[index + 4] + ConstantR1 + Ch(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);
		}

		// round 2
		for (; index < 40 && index < buffer.Length - 4; index += 5)
		{
			e += buffer[index + 0] + ConstantR2 + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[index + 1] + ConstantR2 + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[index + 2] + ConstantR2 + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[index + 3] + ConstantR2 + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[index + 4] + ConstantR2 + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);
		}

		// round 3
		for (; index < 60 && index < buffer.Length - 4; index += 5)
		{
			e += buffer[index + 0] + ConstantR3 + Maj(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[index + 1] + ConstantR3 + Maj(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[index + 2] + ConstantR3 + Maj(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[index + 3] + ConstantR3 + Maj(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[index + 4] + ConstantR3 + Maj(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);
		}

		// round 4
		for (; index < 80 && index < buffer.Length - 4; index += 5)
		{
			e += buffer[index + 0] + ConstantR4 + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[index + 1] + ConstantR4 + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[index + 2] + ConstantR4 + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[index + 3] + ConstantR4 + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[index + 4] + ConstantR4 + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);
		}

		a += InitA;
		b += InitB;
		c += InitC;
		d += InitD;
		e += InitE;

		BinaryPrimitives.WriteUInt32BigEndian(dst, a);
		BinaryPrimitives.WriteUInt32BigEndian(dst[4..], b);
		BinaryPrimitives.WriteUInt32BigEndian(dst[8..], c);
		BinaryPrimitives.WriteUInt32BigEndian(dst[12..], d);
		BinaryPrimitives.WriteUInt32BigEndian(dst[16..], e);
	}
}
