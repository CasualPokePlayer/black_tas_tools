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

	public const uint InitA = 0x67452301;
	public const uint InitB = 0xefcdab89;
	public const uint InitC = 0x98badcfe;
	public const uint InitD = 0x10325476;
	public const uint InitE = 0xc3d2e1f0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Ch(uint x, uint y, uint z) => (x & y) ^ (~x & z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Maj(uint x, uint y, uint z) => (x & y) ^ (x & z) ^ (y & z);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Parity(uint x, uint y, uint z) => x ^ y ^ z;
#if false
	public static unsafe void HashBlock(ReadOnlySpan<byte> block, Span<byte> dst)
	{
		// want 256-bit alignment here
		const int BUFFER_ALIGNMENT = 256 / 8;
		const ulong BUFFER_ALIGNMENT_MASK = BUFFER_ALIGNMENT - 1;
		var unalignedBuffer = stackalloc uint[80 + BUFFER_ALIGNMENT];
		var buffer = (uint*)((((nuint)unalignedBuffer - 1) | BUFFER_ALIGNMENT_MASK) + 1);

		// Fill buffer for transformation
		fixed (byte* blockPtr = block)
		{
			var blockVec = Vector256.Load(blockPtr);
			var shuffleVec = Vector256.Create(
				// byte cast is needed for the correct overload to be used
				(byte)3, 2, 1, 0,
				7, 6, 5, 4,
				11, 10, 9, 8,
				15, 14, 13, 12,
				3, 2, 1, 0,
				7, 6, 5, 4,
				11, 10, 9, 8,
				15, 14, 13, 12);
			var suffled = Avx2.Shuffle(blockVec, shuffleVec);
			suffled.StoreAligned((byte*)buffer);

			blockVec = Vector256.Load(blockPtr + 32);
			suffled = Avx2.Shuffle(blockVec, shuffleVec);
			suffled.StoreAligned((byte*)buffer + 32);
		}

		// clear rest of the buffer... we need this cleared to properly expand the buffer
		new Span<uint>(buffer + 16, 64).Clear();

		// Expand buffer (unrolled SIMD)
		{
			var x1 = Vector128.Load(&buffer[16 - 3]);
			var x2 = Vector128.LoadAligned(&buffer[16 - 8]);
			var x3 = Vector128.Load(&buffer[16 - 14]);
			var x4 = Vector128.LoadAligned(&buffer[16 - 16]);
			var x = x1 ^ x2 ^ x3 ^ x4;
			// _mm_rol_epi32 is AVX512 :(
			// rotate left by 1, using normal shifts
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[16]);
			// must xor this element now (since buffer[16] is now filled / not 0)
			buffer[16 + 3] ^= BitOperations.RotateLeft(buffer[16], 1);

			x1 = Vector128.Load(&buffer[20 - 3]);
			x2 = Vector128.LoadAligned(&buffer[20 - 8]);
			x3 = Vector128.Load(&buffer[20 - 14]);
			x4 = Vector128.LoadAligned(&buffer[20 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[20]);
			buffer[20 + 3] ^= BitOperations.RotateLeft(buffer[20], 1);

			x1 = Vector128.Load(&buffer[24 - 3]);
			x2 = Vector128.LoadAligned(&buffer[24 - 8]);
			x3 = Vector128.Load(&buffer[24 - 14]);
			x4 = Vector128.LoadAligned(&buffer[24 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[24]);
			buffer[24 + 3] ^= BitOperations.RotateLeft(buffer[24], 1);

			x1 = Vector128.Load(&buffer[28 - 3]);
			x2 = Vector128.LoadAligned(&buffer[28 - 8]);
			x3 = Vector128.Load(&buffer[28 - 14]);
			x4 = Vector128.LoadAligned(&buffer[28 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[28]);
			buffer[28 + 3] ^= BitOperations.RotateLeft(buffer[28], 1);

			x1 = Vector128.Load(&buffer[32 - 3]);
			x2 = Vector128.LoadAligned(&buffer[32 - 8]);
			x3 = Vector128.Load(&buffer[32 - 14]);
			x4 = Vector128.LoadAligned(&buffer[32 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[32]);
			buffer[32 + 3] ^= BitOperations.RotateLeft(buffer[32], 1);

			x1 = Vector128.Load(&buffer[36 - 3]);
			x2 = Vector128.LoadAligned(&buffer[36 - 8]);
			x3 = Vector128.Load(&buffer[36 - 14]);
			x4 = Vector128.LoadAligned(&buffer[36 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[36]);
			buffer[36 + 3] ^= BitOperations.RotateLeft(buffer[36], 1);

			x1 = Vector128.Load(&buffer[40 - 3]);
			x2 = Vector128.LoadAligned(&buffer[40 - 8]);
			x3 = Vector128.Load(&buffer[40 - 14]);
			x4 = Vector128.LoadAligned(&buffer[40 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[40]);
			buffer[40 + 3] ^= BitOperations.RotateLeft(buffer[40], 1);

			x1 = Vector128.Load(&buffer[44 - 3]);
			x2 = Vector128.LoadAligned(&buffer[44 - 8]);
			x3 = Vector128.Load(&buffer[44 - 14]);
			x4 = Vector128.LoadAligned(&buffer[44 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[44]);
			buffer[44 + 3] ^= BitOperations.RotateLeft(buffer[44], 1);

			x1 = Vector128.Load(&buffer[48 - 3]);
			x2 = Vector128.LoadAligned(&buffer[48 - 8]);
			x3 = Vector128.Load(&buffer[48 - 14]);
			x4 = Vector128.LoadAligned(&buffer[48 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[48]);
			buffer[48 + 3] ^= BitOperations.RotateLeft(buffer[48], 1);

			x1 = Vector128.Load(&buffer[52 - 3]);
			x2 = Vector128.LoadAligned(&buffer[52 - 8]);
			x3 = Vector128.Load(&buffer[52 - 14]);
			x4 = Vector128.LoadAligned(&buffer[52 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[52]);
			buffer[52 + 3] ^= BitOperations.RotateLeft(buffer[52], 1);

			x1 = Vector128.Load(&buffer[56 - 3]);
			x2 = Vector128.LoadAligned(&buffer[56 - 8]);
			x3 = Vector128.Load(&buffer[56 - 14]);
			x4 = Vector128.LoadAligned(&buffer[56 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[56]);
			buffer[56 + 3] ^= BitOperations.RotateLeft(buffer[56], 1);

			x1 = Vector128.Load(&buffer[60 - 3]);
			x2 = Vector128.LoadAligned(&buffer[60 - 8]);
			x3 = Vector128.Load(&buffer[60 - 14]);
			x4 = Vector128.LoadAligned(&buffer[60 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[60]);
			buffer[60 + 3] ^= BitOperations.RotateLeft(buffer[60], 1);

			x1 = Vector128.Load(&buffer[64 - 3]);
			x2 = Vector128.LoadAligned(&buffer[64 - 8]);
			x3 = Vector128.Load(&buffer[64 - 14]);
			x4 = Vector128.LoadAligned(&buffer[64 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[64]);
			buffer[64 + 3] ^= BitOperations.RotateLeft(buffer[64], 1);

			x1 = Vector128.Load(&buffer[68 - 3]);
			x2 = Vector128.LoadAligned(&buffer[68 - 8]);
			x3 = Vector128.Load(&buffer[68 - 14]);
			x4 = Vector128.LoadAligned(&buffer[68 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[68]);
			buffer[68 + 3] ^= BitOperations.RotateLeft(buffer[68], 1);

			x1 = Vector128.Load(&buffer[72 - 3]);
			x2 = Vector128.LoadAligned(&buffer[72 - 8]);
			x3 = Vector128.Load(&buffer[72 - 14]);
			x4 = Vector128.LoadAligned(&buffer[72 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[72]);
			buffer[72 + 3] ^= BitOperations.RotateLeft(buffer[72], 1);

			x1 = Vector128.Load(&buffer[76 - 3]);
			x2 = Vector128.LoadAligned(&buffer[76 - 8]);
			x3 = Vector128.Load(&buffer[76 - 14]);
			x4 = Vector128.LoadAligned(&buffer[76 - 16]);
			x = x1 ^ x2 ^ x3 ^ x4;
			x = (x << 1) | (x >>> 31);
			x.StoreAligned(&buffer[76]);
			buffer[76 + 3] ^= BitOperations.RotateLeft(buffer[76], 1);
		}

		var a = InitA;
		var b = InitB;
		var c = InitC;
		var d = InitD;
		var e = InitE;

		// pre-calc round 1 constant add
		{
			var x1 = Vector256.LoadAligned(&buffer[0]) + Vector256.Create(ConstantR1);
			x1.StoreAligned(&buffer[0]);
			var x2 = Vector256.LoadAligned(&buffer[8]) + Vector256.Create(ConstantR1);
			x2.StoreAligned(&buffer[8]);
			var x3 = Vector256.LoadAligned(&buffer[16]) + Vector256.Create(Vector128.Create(ConstantR1), Vector128.Create(ConstantR2));
			x3.StoreAligned(&buffer[16]);
		}

		// round 1, unrolled
		{
			e += buffer[0 + 0] + Ch(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[0 + 1] + Ch(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[0 + 2] + Ch(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[0 + 3] + Ch(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[0 + 4] + Ch(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[5 + 0] + Ch(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[5 + 1] + Ch(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[5 + 2] + Ch(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[5 + 3] + Ch(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[5 + 4] + Ch(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[10 + 0] + Ch(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[10 + 1] + Ch(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[10 + 2] + Ch(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[10 + 3] + Ch(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[10 + 4] + Ch(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[15 + 0] + Ch(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[15 + 1] + Ch(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[15 + 2] + Ch(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[15 + 3] + Ch(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[15 + 4] + Ch(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);
		}

		// pre-calc round 2 constant add
		{
			var x1 = Vector256.LoadAligned(&buffer[24]) + Vector256.Create(ConstantR2);
			x1.StoreAligned(&buffer[24]);
			var x2 = Vector256.LoadAligned(&buffer[32]) + Vector256.Create(ConstantR2);
			x2.StoreAligned(&buffer[32]);
		}

		// round 2, unrolled
		{
			e += buffer[20 + 0] + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[20 + 1] + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[20 + 2] + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[20 + 3] + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[20 + 4] + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[25 + 0] + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[25 + 1] + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[25 + 2] + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[25 + 3] + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[25 + 4] + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[30 + 0] + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[30 + 1] + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[30 + 2] + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[30 + 3] + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[30 + 4] + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[35 + 0] + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[35 + 1] + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[35 + 2] + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[35 + 3] + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[35 + 4] + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);
		}

		// pre-calc round 3 constant add
		{
			var x1 = Vector256.LoadAligned(&buffer[40]) + Vector256.Create(ConstantR3);
			x1.StoreAligned(&buffer[40]);
			var x2 = Vector256.LoadAligned(&buffer[48]) + Vector256.Create(ConstantR3);
			x2.StoreAligned(&buffer[48]);
			var x3 = Vector256.LoadAligned(&buffer[56]) + Vector256.Create(Vector128.Create(ConstantR3), Vector128.Create(ConstantR4));
			x3.StoreAligned(&buffer[56]);
		}

		// round 3, unrolled
		{
			e += buffer[40 + 0] + Maj(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[40 + 1] + Maj(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[40 + 2] + Maj(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[40 + 3] + Maj(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[40 + 4] + Maj(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[45 + 0] + Maj(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[45 + 1] + Maj(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[45 + 2] + Maj(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[45 + 3] + Maj(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[45 + 4] + Maj(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[50 + 0] + Maj(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[50 + 1] + Maj(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[50 + 2] + Maj(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[50 + 3] + Maj(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[50 + 4] + Maj(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[55 + 0] + Maj(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[55 + 1] + Maj(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[55 + 2] + Maj(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[55 + 3] + Maj(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[55 + 4] + Maj(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);
		}

		// pre-calc round 4 constant add
		{
			var x1 = Vector256.LoadAligned(&buffer[64]) + Vector256.Create(ConstantR4);
			x1.StoreAligned(&buffer[64]);
			var x2 = Vector256.LoadAligned(&buffer[72]) + Vector256.Create(ConstantR4);
			x2.StoreAligned(&buffer[72]);
		}

		// round 4, unrolled
		{
			e += buffer[60 + 0] + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[60 + 1] + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[60 + 2] + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[60 + 3] + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[60 + 4] + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[65 + 0] + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[65 + 1] + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[65 + 2] + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[65 + 3] + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[65 + 4] + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[70 + 0] + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[70 + 1] + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[70 + 2] + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[70 + 3] + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[70 + 4] + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);

			e += buffer[75 + 0] + Parity(b, c, d) + BitOperations.RotateLeft(a, 5);
			b = BitOperations.RotateLeft(b, 30);

			d += buffer[75 + 1] + Parity(a, b, c) + BitOperations.RotateLeft(e, 5);
			a = BitOperations.RotateLeft(a, 30);

			c += buffer[75 + 2] + Parity(e, a, b) + BitOperations.RotateLeft(d, 5);
			e = BitOperations.RotateLeft(e, 30);

			b += buffer[75 + 3] + Parity(d, e, a) + BitOperations.RotateLeft(c, 5);
			d = BitOperations.RotateLeft(d, 30);

			a += buffer[75 + 4] + Parity(c, d, e) + BitOperations.RotateLeft(b, 5);
			c = BitOperations.RotateLeft(c, 30);
		}

		var hashVecAD = Vector128.Create(a, b, c, d);
		hashVecAD += Vector128.Create(InitA, InitB, InitC, InitD);
		e += InitE;

		fixed (byte* dstPtr = dst)
		{
			var shuffleVec = Vector128.Create(
				// byte cast is needed for the correct overload to be used
				(byte)3, 2, 1, 0,
				7, 6, 5, 4,
				11, 10, 9, 8,
				15, 14, 13, 12);
			var shuffled = Ssse3.Shuffle(hashVecAD.AsByte(), shuffleVec);
			shuffled.Store(dstPtr);
			Unsafe.WriteUnaligned(&dstPtr[16], BinaryPrimitives.ReverseEndianness(e)); 
		}
	}
#else
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
#endif
}
