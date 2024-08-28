using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Program;

internal static class Program
{
	private static readonly byte[] _blackNazos =
	[
		0xB0, 0x60, 0x21, 0x02,
		0xAC, 0x61, 0x21, 0x02,
		0xAC, 0x61, 0x21, 0x02,
		0xF8, 0x61, 0x21, 0x02,
		0xF8, 0x61, 0x21, 0x02
	];

	private static readonly byte[] _bcdValues = new byte[100];

	[Flags]
	private enum KeyInput : ushort
	{
		// ReSharper disable UnusedMember.Local
		A = 1 << 0,
		B = 1 << 1,
		Select = 1 << 2,
		Start = 1 << 3,
		Right = 1 << 4,
		Left = 1 << 5,
		Up = 1 << 6,
		Down = 1 << 7,
		R = 1 << 8,
		L = 1 << 9,
		X = 1 << 10,
		Y = 1 << 11,
		LidClose = 1 << 15,
	}

	private record ThreadParam(int StartYear, int EndYear);

	private static void ThreadProc(object? param)
	{
		var threadParam = (ThreadParam)param!;
		for (var i = threadParam.StartYear; i < threadParam.EndYear; i++)
		{
			SearchYear(i, false, false);
			SearchYear(i, true, false);
			SearchYear(i, false, true);
			SearchYear(i, true, true);
			Console.WriteLine($"Year {i} finished");
		}
	}

	private static void Main()
	{
		for (var i = 0; i < _bcdValues.Length; i++)
		{
			_bcdValues[i] = (byte)(((i / 10) << 4) + i % 10);
		}

		var maxParallelism = Environment.ProcessorCount * 3 / 4;
		var threads = new Thread[maxParallelism];
		var yearsPerThread = 100 / maxParallelism;
		for (var i = 0; i < maxParallelism; i++)
		{
			threads[i] = new Thread(ThreadProc) { IsBackground = true };
			var threadParam = new ThreadParam(i * yearsPerThread, (i + 1) * yearsPerThread);
			threads[i].Start(threadParam);
		}

		// last couple of years covered here
		{
			var threadParam = new ThreadParam(maxParallelism * yearsPerThread, 100);
			ThreadProc(threadParam);
		}

		for (var i = 0; i < maxParallelism; i++)
		{
			threads[i].Join();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong AdvancePidRng(ref ulong pidRng)
	{
		pidRng *= 0x5d588b656c078965;
		pidRng += 0x269ec3;
		return pidRng >> 32;
	}

	private static readonly byte[][] _rngInitTable =
	[
		[50, 100, 100, 100],
		[50, 50, 100, 100],
		[30, 50, 100, 100],
		[25, 30, 50, 100],
		[20, 25, 33, 50]
	];

	private static void InitAdvanceRng(ref ulong pidRng)
	{
		// pidrng is advanced some times at the beginning of the game
		for (var i = 0; i < 5; i++)
		{
			for (var j = 0; j < 4; j++)
			{
				if (_rngInitTable[i][j] == 100)
				{
					break;
				}

				var rngInitRes = AdvancePidRng(ref pidRng);
				rngInitRes *= 101;
				rngInitRes >>= 32;
				if (rngInitRes <= _rngInitTable[i][j])
				{
					break;
				}
			}
		}
	}

	private static readonly string[] _natureStrs = [ "Hardy", "Lonely", "Brave", "Adamant", "Naughty", "Bold", "Docile", "Relaxed", "Impish", "Lax", "Timid", "Hasty", "Serious", "Jolly", "Naive", "Modest", "Mild", "Quiet", "Bashful", "Rash", "Calm", "Gentle", "Sassy", "Careful", "Quirky" ];

	private static string? CheckSeed(ulong pidRng, MT19937 mtrng)
	{
		for (var i = 0; i < 3; i++)
		{
			InitAdvanceRng(ref pidRng);
		}

		// unrelated to InitAdvanceRng, but inbetween the usages
		for (var i = 0; i < 4; i++)
		{
			AdvancePidRng(ref pidRng);
		}

		for (var i = 0; i < 4; i++)
		{
			InitAdvanceRng(ref pidRng);
		}

		// seems consistently 13
		for (var i = 0; i < 13; i++)
		{
			AdvancePidRng(ref pidRng);
		}

		var pid = (uint)AdvancePidRng(ref pidRng);
		pid ^= 0x10000;
		var nature = (AdvancePidRng(ref pidRng) * 25) >> 32;

		var spdReducingNature = nature is 2 or 7 or 17 or 22;
		if (!spdReducingNature)
		{
			// we need a speed reducing nature to speed tie initial rival fights
			return null;
		}

		var defPlusNature = nature is 7;

		// mtrng is advanced 10 times before being used for starter genning
		mtrng.Advance(10);

		var hpIv = mtrng.Next() >> 27;
		if (defPlusNature && hpIv >= 23)
		{
			// def plus and 23+ ivs is too bulky
			return null;
		}

		var atkIv = mtrng.Next() >> 27;
		if (atkIv < 10)
		{
			// we need at least 10 atk ivs
			return null;
		}

		var defIv = mtrng.Next() >> 27;

		// if we're def+, we need < 3 def ivs
		// if we're def neutral, we need < 10 def ivs if we have >= 23 hp ivs, otherwise just < 15 def ivs
		var defIvThreadHold = defPlusNature ? 3 : (hpIv >= 23 ? 10 : 15);

		if (defIv >= defIvThreadHold)
		{
			return null;
		}

		var spAtkIv = mtrng.Next() >> 27;

		var spDefIv = mtrng.Next() >> 27;
		var spdIv = mtrng.Next() >> 27;
		if (spdIv > 13)
		{
			// we want to be slow enough to speedtie rivals
			return null;
		}

		var starterStats = $"HP IV: {hpIv} | Atk IV: {atkIv} | Def IV: {defIv} | SpAtk IV: {spAtkIv} | SpDef IV: {spDefIv} | Spd IV: {spdIv} | PID: {pid:X08} | Nature: {_natureStrs[nature]}";

		for (var i = 0; i < 2; i++)
		{
			// pid rng advances once on starting battle
			AdvancePidRng(ref pidRng);
		}

		// bianca 1: ~1519 MTRNG advances minimum?
		// cheren 1: ~1544 MTRNG advances minimum?
		// 14 pre-tutorial mtrng advances
		// ~4429 tutorial mtrng advances
		// 128 steps passed (1 advancement)
		// N 1: ~2625 MTRNG advances minimum?
		// 134 MTRNG frames too soon?
		mtrng.Advance(10266);

		var lillipupIvDelay = -1;
		const int NUM_IVS = 5;
		Span<uint> mtResults = stackalloc uint[50 + NUM_IVS];
		for (var i = 0; i < mtResults.Length; i++)
		{
			mtResults[i] = mtrng.Next();
		}

		for (var i = 0; i < mtResults.Length - NUM_IVS; i++)
		{
			hpIv = mtResults[i + 0] >> 27;
			if (hpIv != 31)
			{
				continue;
			}

			atkIv = mtResults[i + 1] >> 27;
			if (atkIv != 31)
			{
				continue;
			}

			defIv = mtResults[i + 2] >> 27;
			spAtkIv = mtResults[i + 3] >> 27;
			spDefIv = mtResults[i + 4] >> 27;

			spdIv = mtResults[i + 5] >> 27;
			if (spdIv < 29)
			{
				continue;
			}

			lillipupIvDelay = i;
			break;
		}

		if (lillipupIvDelay == -1)
		{
			return null;
		}

		// a guess for how long it will take... we'll need to be generous here to have a shot
		for (var i = 0; i < 1700; i++)
		{
			AdvancePidRng(ref pidRng);
		}

		// YIKES, 100 FRAMES OF DELAY???
		// well, xtransceiver burns pidrng calls, so not that bad
		// hopefully we can align RNG correctly...
		var lillipupDelay = -1;
		for (var i = 0; i < 100; i++)
		{
			// don't change our "global" pid rng
			var lillipupPidRng = pidRng;
			AdvancePidRng(ref pidRng); // except this once to advance the loop

			// don't know what this is for (some 50/50 roll???)
			AdvancePidRng(ref lillipupPidRng);

			var encounterCheck = (AdvancePidRng(ref lillipupPidRng) * 0xFFFF) >> 32;
			encounterCheck /= 0x290;
			if (encounterCheck >= 8)
			{
				// no encounter, try next one
				continue;
			}

			// check the slot we got, we need slot 9 or 11
			var encounterSlot = (AdvancePidRng(ref lillipupPidRng) * 0xFFFF) >> 32;
			encounterSlot /= 0x290;
			if (encounterSlot is not (94 or 95 or 96 or 97 or 99))
			{
				// not encounter slot 9 or 11, not what we want
				continue;
			}

			// don't know what this is for
			AdvancePidRng(ref lillipupPidRng);

			pid = (uint)AdvancePidRng(ref lillipupPidRng);
			pid ^= 0x10000;
			var abilitySlot = (pid >> 16) & 1;
			if (abilitySlot == 0)
			{
				// we need pickup (don't want intimidate later on)
				continue;
			}

			nature = (AdvancePidRng(ref lillipupPidRng) * 25) >> 32;
			if (nature is not (1 or 3 or 4))
			{
				// need lonely/adamant/naughty
				continue;
			}

			if (nature == 1 && defIv < 10)
			{
				// need a bit of defense if we're doing lonely
				continue;
			}

			lillipupDelay = i;
			break;
		}

		if (lillipupDelay == -1)
		{
			return null;
		}

		mtrng.Advance(4176 - (uint)mtResults.Length);

		var pkrusDelay = -1;
		for (var i = 0; i < 50; i++)
		{
			var pkrusRng = mtrng.Next() >> 16;
			// it's not & 0x3FFF == 0, that results in 0 getting pokerus (it doesn't!)
			if (pkrusRng is 0x4000 or 0x8000 or 0xC000)
			{
				pkrusDelay = i;
				break;
			}
		}

		if (pkrusDelay == -1)
		{
			return null;
		}

		// picks party member
		// want it to pick lillipup
		var pkrusMon = ((ulong)mtrng.Next() * 2) >> 32;
		if (pkrusMon != 0)
		{
			return null;
		}

		var lillipupStats = $"HP IV: {hpIv} | Atk IV: {atkIv} | Def IV: {defIv} | SpAtk IV: {spAtkIv} | SpDef IV: {spDefIv} | Spd IV: {spdIv} | PID: {pid:X08} | Nature: {_natureStrs[nature]} | IV Delay: {lillipupIvDelay} | PID Delay: {lillipupDelay} | Pkrus Delay: {pkrusDelay}";
		return $"Starter: {starterStats} | Lillipup: {lillipupStats}";
	}

	// stolen from pokefinder
	private static byte ComputeWeekday(int year, int month, int day)
	{
		var a = month < 3 ? 1 : 0;
		var y = 2000 + year + 4800 - a;
		var m = month + 12 * a - 3;
		var jd = day + ((153 * m + 2) / 5) - 32045 + 365 * y + (y / 4) - (y / 100) + (y / 400);
		return (byte)((jd + 1) % 7);
	}

	private static void SearchYear(int year, bool lidClosed, bool shortMessage)
	{
		using var mtrng = new MT19937();
		Span<byte> sha1Hash = stackalloc byte[160];
		Span<byte> initData = stackalloc byte[16 * 4];
		// black nazos
		_blackNazos.AsSpan().CopyTo(initData);
		// timer0/vcount
		// non-auto bootup or firmware skip has 0xBD3 and 0x5A
		// short message has 0xBD2 and 0x5A
		// long message has 0xBD3 and 0x5B
		initData[5 * 4 + 0] = (byte)(shortMessage ? 0xD2 : 0xD3);
		initData[5 * 4 + 1] = 0x0B;
		initData[5 * 4 + 2] = (byte)(shortMessage ? 0x5A : 0x5B);
		initData[5 * 4 + 3] = 0x00;
		// lower mac (always 49:16)
		initData[6 * 4 + 0] = 0x00;
		initData[6 * 4 + 1] = 0x00;
		initData[6 * 4 + 2] = 0x49;
		initData[6 * 4 + 3] = 0x16;
		// upper mac ^ vframe ^ gxstat (always 00:09:BF:0E, 5, and 6)
		initData[7 * 4 + 0] = 0x05;
		initData[7 * 4 + 1] = 0x09;
		initData[7 * 4 + 2] = 0xBF;
		initData[7 * 4 + 3] = 0x08;
		// [8] is date, in YY MM DD WD format
		// [9] is time, in HH MM SS 00 format
		initData[9 * 4 + 3] = 0x00;
		// other misc constants
		initData[10 * 4 + 0] = 0x00;
		initData[10 * 4 + 1] = 0x00;
		initData[10 * 4 + 2] = 0x00;
		initData[10 * 4 + 3] = 0x00;
		initData[11 * 4 + 0] = 0x00;
		initData[11 * 4 + 1] = 0x00;
		initData[11 * 4 + 2] = 0x00;
		initData[11 * 4 + 3] = 0x00;
		// [12] is inputs, FF2F0000 == no inputs
		initData[12 * 4 + 2] = 0x00;
		initData[12 * 4 + 3] = 0x00;
		initData[13 * 4 + 0] = 0x80;
		initData[13 * 4 + 1] = 0x00;
		initData[13 * 4 + 2] = 0x00;
		initData[13 * 4 + 3] = 0x00;
		initData[14 * 4 + 0] = 0x00;
		initData[14 * 4 + 1] = 0x00;
		initData[14 * 4 + 2] = 0x00;
		initData[14 * 4 + 3] = 0x00;
		initData[15 * 4 + 0] = 0x00;
		initData[15 * 4 + 1] = 0x00;
		initData[15 * 4 + 2] = 0x01;
		initData[15 * 4 + 3] = 0xA0;

		// only check winter months
		// note we 1-index months and days (but not years, hours, nor minutes), as this matches what we'd convert for BCD
		for (var month = 4; month <= 12; month += 4)
		{
			var day = month == 4 ? 30 : 31;
			initData[8 * 4 + 0] = _bcdValues[year];
			initData[8 * 4 + 1] = _bcdValues[month];
			initData[8 * 4 + 2] = _bcdValues[day];
			initData[8 * 4 + 3] = ComputeWeekday(year, month, day);

			initData[9 * 4 + 0] = _bcdValues[21];
			initData[9 * 4 + 0] |= 0x40;
			for (var minute = 57; minute < 60; minute++)
			{
				initData[9 * 4 + 1] = _bcdValues[minute];

				for (var second = 0; second < 60; second++)
				{
					initData[9 * 4 + 2] = _bcdValues[second];

					for (var input = 0; input < 4096; input++)
					{
						initData[12 * 4 + 0] = (byte)(~input & 0xFF);
						initData[12 * 4 + 1] = (byte)((~input >> 8) & (lidClosed ? 0xAF : 0x2F));

						PkmnSHA1.HashBlock(initData, sha1Hash);
						var seed = BinaryPrimitives.ReadUInt64LittleEndian(sha1Hash);

						// seed is advanced once before being used for MTRNG
						var mtseed = AdvancePidRng(ref seed);
						mtrng.Init((uint)mtseed);
						var result = CheckSeed(seed, mtrng);
						if (result != null)
						{
							Console.WriteLine($"Year {2000 + year} | Month {month} | Day {day} | Hour 21 | Minute {minute} | Second {second} | Input {(KeyInput)input | (lidClosed ? KeyInput.LidClose : 0)} | Message: {(shortMessage ? "Short" : "Long")} | Seed: {seed:X016} | {result}");
						}
					}
				}
			}

			initData[9 * 4 + 0] = _bcdValues[22];
			initData[9 * 4 + 0] |= 0x40;
			for (var minute = 0; minute < 16; minute++)
			{
				initData[9 * 4 + 1] = _bcdValues[minute];

				for (var second = 0; second < 60; second++)
				{
					initData[9 * 4 + 2] = _bcdValues[second];

					for (var input = 0; input < 4096; input++)
					{
						initData[12 * 4 + 0] = (byte)(~input & 0xFF);
						initData[12 * 4 + 1] = (byte)((~input >> 8) & (lidClosed ? 0xAF : 0x2F));

						PkmnSHA1.HashBlock(initData, sha1Hash);
						var seed = BinaryPrimitives.ReadUInt64LittleEndian(sha1Hash);

						// seed is advanced once before being used for MTRNG
						var mtseed = AdvancePidRng(ref seed);
						mtrng.Init((uint)mtseed);
						var result = CheckSeed(seed, mtrng);
						if (result != null)
						{
							Console.WriteLine($"Year {2000 + year} | Month {month} | Day {day} | Hour 22 | Minute {minute} | Second {second} | Input {(KeyInput)input | (lidClosed ? KeyInput.LidClose : 0)} | Message: {(shortMessage ? "Short" : "Long")} | Seed: {seed:X016} | {result}");
						}
					}
				}
			}
		}
	}
}
