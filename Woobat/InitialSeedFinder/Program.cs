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

	// note: first entry is day 0 (which doesn't exist)
	private static readonly bool[] _aprilHailDays =
	[
		false,
		true, true, true, true, false, false, false, false, true, true,
		false, false, true, true, false, false, false, false, true, true,
		false, false, false, true, false, false, false, false, false, true
	];

	private static readonly bool[] _augustHailDays =
	[
		false,
		true, true, false, false, false, false, false, true, true, true,
		true, false, false, false, false, false, false, true, true, false,
		false, false, true, true, false, false, false, false, true, true, false
	];

	private static readonly bool[] _decemberHailDays =
	[
		false,
		true, true, true, false, false, false, false, true, true, false,
		false, true, true, true, false, false, false, true, false, false,
		false, true, true, true, false, false, false, false, true, true, false
	];

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

		var maxParallelism = Environment.ProcessorCount * 2 / 3;
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

		var atkReducingNature = nature is 5 or 10 or 15 or 20;
		var defReducingNature = nature is 1 or 11 or 16 or 21;
		var spAtkReducingNature = nature is 3 or 8 or 13 or 23;
		var spAtkPlusNature = nature is 15 or 16 or 19 or 17;
		var spdReducingNature = nature is 2 or 7 or 17 or 22;

		if (spAtkReducingNature)
		{
			// sp atk reducing nature won't do, we need sp atk for twins fight
			return null;
		}

		// mtrng is advanced 10 times before being used for starter genning
		mtrng.Advance(10);

		var hpIv = mtrng.Next() >> 27;
		if (hpIv >= 30)
		{
			// 30+ hp ivs prevent optimal tepig fight
			return null;
		}

		var atkIv = mtrng.Next() >> 27;
		if (atkReducingNature && atkIv < 30)
		{
			// if we have a atk reducing nature, we need at least 30 atk ivs
			return null;
		}

		var defIv = mtrng.Next() >> 27;
		if (!defReducingNature && defIv >= 10)
		{
			// tepig fight needs either def reducing nature or def iv < 10
			return null;
		}

		// damage from one scratch (max roll, non-crit)
		var ndamage = defIv switch
		{
			< 10 when defReducingNature => 6,
			>= 10 when defReducingNature => 5,
			< 30 => 5,
			>= 30 => 4,
		};

		// damage from one scratch (max roll, crit)
		var twinsDamage = defIv switch
		{
			< 10 when defReducingNature => 18,
			>= 10 when defReducingNature => 16,
			< 10 => 16,
			>= 10 => 14,
		};

		// osha's hp in twins fight
		var twinsHp = hpIv switch
		{
			< 3 => 26,
			< 15 => 27,
			< 28 => 28,
			_ => 29
		};

		var torrentRange = twinsHp / 3;
		twinsHp -= ndamage + twinsDamage;
		if (twinsHp > torrentRange)
		{
			// we need to get into torrent to KO a purrloin with osha
			return null;
		}

		var spAtkIv = mtrng.Next() >> 27;
		if (!spAtkPlusNature && spAtkIv < 12)
		{
			// we need a sp atk plus nature or at least 12 sp atk for twins fight
			return null;
		}

		var spDefIv = mtrng.Next() >> 27;
		var spdIv = mtrng.Next() >> 27;
		if (!spdReducingNature && spdIv >= 10)
		{
			// we don't want to outspeed tepig, spd reducing can't outspeed, but other natures can with >= 10 speed ivs
			// (plus speed is same as neutral here oddly enough)
			return null;
		}

#if false
		var blinkCounter = 0;
		for (var i = 0; i < 139; i++)
		{
			var blinkRng = ((ulong)mtrng.genrand_int32() * 100) >> 32;
			if (blinkCounter == 0 && blinkRng == 0)
			{
				blinkCounter = 5;
			}
			else if (blinkCounter != 0)
			{
				blinkCounter--;
			}
		}
#endif

		// bianca 1: ~1513 MTRNG advances minimum?
		uint battleMtRngAdvances = 1513;
		battleMtRngAdvances += 6; // pkrus and some other misc rng calls
		mtrng.Advance(battleMtRngAdvances);

		// pid rng advances once on starting battle
		AdvancePidRng(ref pidRng);

		// cheren 1: ~1538 MTRNG advances minimum?
		battleMtRngAdvances = 1538;
		battleMtRngAdvances += 6; // pkrus and some other misc rng calls
		mtrng.Advance(battleMtRngAdvances);

		// pid rng advances once on starting battle
		AdvancePidRng(ref pidRng);

		// nickname mtrng advances
		mtrng.Advance(7);

		// pre-tutorial mtrng advances
		mtrng.Advance(14);

		// tutorial mtrng advances
		battleMtRngAdvances = 4429;
		mtrng.Advance(battleMtRngAdvances);

		// 128 steps passed
		mtrng.Advance(1);

		// N 1
		battleMtRngAdvances = 2544;
		mtrng.Advance(battleMtRngAdvances);

		// we need ample time to manip IVRNG specific stuff, so give us 15-20 frames worth to work with (might be less or more, due to variance in overall MTRNG; 15 is enough for nearly any case)
		const int minPkrusDelay = 15 * 2;
		const int maxPkrusDelay = 20 * 2;
		var pkrusDelay = -1;
		mtrng.Advance(minPkrusDelay);
		for (var i = minPkrusDelay; i < maxPkrusDelay; i++)
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

		// picks party member (only 1, so there can only be one call)
		mtrng.Advance(1);
		uint strain;
		do
		{
			strain = mtrng.Next() >> 24;
		}
		while ((strain & 7) == 0);

		var starterStats = $"HP IV: {hpIv} | Atk IV: {atkIv} | Def IV: {defIv} | SpAtk IV: {spAtkIv} | SpDef IV: {spDefIv} | Spd IV: {spdIv} | PID: {pid:X08} | Nature: {_natureStrs[nature]} | Pkrus Delay: {pkrusDelay}";

		// other misc mtrng calls
		mtrng.Advance(5);

		hpIv = mtrng.Next() >> 27;
		atkIv = mtrng.Next() >> 27;
		if (atkIv < 9)
		{
			return null;
		}

		defIv = mtrng.Next() >> 27;
		spAtkIv = mtrng.Next() >> 27;
		spDefIv = mtrng.Next() >> 27;
		spdIv = mtrng.Next() >> 27;
		if (spdIv < 10)
		{
			return null;
		}

		// a guess for how long it will take... we'll need to be generous here to have a shot
		for (var i = 0; i < 1700; i++)
		{
			AdvancePidRng(ref pidRng);
		}

		// YIKES, 200 FRAMES OF DELAY???
		// well, xtransceiver burns pidrng calls, so not that bad
		// hopefully we can align RNG correctly...
		var lillipupDelay = -1;
		for (var i = 0; i < 200; i++)
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

			nature = (AdvancePidRng(ref lillipupPidRng) * 25) >> 32;
			// needs to be 23+ ivs for neutral attack, or 10+ ivs for +attack nature
			atkReducingNature = nature is 5 or 10 or 15 or 20;
			if (atkReducingNature)
			{
				// nature can't be reducing attack
				continue;
			}

			var atkPlusNature = nature is 1 or 2 or 3 or 4;
			if (!atkPlusNature && atkIv < 23)
			{
				// atk iv isn't high enough for this nature
				continue;
			}

			if (nature is 2 or 7 or 17 or 22 && spdIv < 30)
			{
				// -spd nature needs at least 30+ speed ivs
				continue;
			}

			lillipupDelay = i;
			break;
		}

		if (lillipupDelay == -1)
		{
			return null;
		}

		var lillipupStats = $"HP IV: {hpIv} | Atk IV: {atkIv} | Def IV: {defIv} | SpAtk IV: {spAtkIv} | SpDef IV: {spDefIv} | Spd IV: {spdIv} | PID: {pid:X08} | Nature: {_natureStrs[nature]} | Delay: {lillipupDelay}";

		for (var i = 0; i < 450; i++)
		{
			AdvancePidRng(ref pidRng);
		}

		var woobatPidDelay = -1;
		for (var i = 0; i < 20; i++)
		{
			// don't change our "global" pid rng
			var woobatPidRng = pidRng;
			AdvancePidRng(ref pidRng); // except this once to advance the loop

			// don't know what this is for (some 50/50 roll???)
			AdvancePidRng(ref woobatPidRng);

			var encounterCheck = (AdvancePidRng(ref woobatPidRng) * 0xFFFF) >> 32;
			encounterCheck /= 0x290;
			if (encounterCheck >= 4)
			{
				// no encounter, try next one
				continue;
			}

			// check the slot we got, we need slot 9 or 11
			var encounterSlot = (AdvancePidRng(ref woobatPidRng) * 0xFFFF) >> 32;
			encounterSlot /= 0x290;
			if (encounterSlot is not (94 or 95 or 96 or 97 or 99))
			{
				// not encounter slot 9 or 11, not what we want
				continue;
			}

			// don't know what this is for
			AdvancePidRng(ref woobatPidRng);

			pid = (uint)AdvancePidRng(ref woobatPidRng);
			pid ^= 0x10000;
			var abilitySlot = (pid >> 16) & 1;
			if (abilitySlot == 1)
			{
				// we need unaware (can't use quick claw with klutz)
				continue;
			}

			nature = (AdvancePidRng(ref woobatPidRng) * 25) >> 32;
			// we need mild or rash nature
			if (nature is not (16 or 19))
			{
				continue;
			}

			woobatPidDelay = i;
			break;
		}

		if (woobatPidDelay == -1)
		{
			return null;
		}

		mtrng.Advance(32850 - 36 * 4);

		// doubles advance MTRNG quite quickly (4x per frame), so we could afford quite a bit of delay
		var woobatIvDelay = -1;
		const int NUM_IVS = 5;
		Span<uint> mtResults = stackalloc uint[100 + NUM_IVS];
		for (var i = 0; i < mtResults.Length; i++)
		{
			mtResults[i] = mtrng.Next();
		}

		for (var i = 0; i < mtResults.Length - NUM_IVS; i++)
		{
			hpIv = mtResults[i + 0] >> 27;
#if false
			if (hpIv is not (5 or >= 8 and <= 14 or >= 23))
			{
				continue;
			}
#endif
			atkIv = mtResults[i + 1] >> 27;
			if (atkIv < 23)
			{
				continue;
			}

			defIv = mtResults[i + 2] >> 27;
			spAtkIv = mtResults[i + 3] >> 27;
			if (spAtkIv != 31)
			{
				continue;
			}

			spDefIv = mtResults[i + 4] >> 27;
#if false
			if (spDefIv < 22)
			{
				continue;
			}

			// if we have less than 30 hp ivs, we need at least 29 spdef ivs
			if (hpIv < 30 && spDefIv < 29)
			{
				continue;
			}
#endif
			spdIv = mtResults[i + 5] >> 27;
			if (spdIv < 28)
			{
				continue;
			}

			woobatIvDelay = i;
			break;
		}

		if (woobatIvDelay == -1)
		{
			return null;
		}

		var woobatStats = $"HP IV: {hpIv} | Atk IV: {atkIv} | Def IV: {defIv} | SpAtk IV: {spAtkIv} | SpDef IV: {spDefIv} | Spd IV: {spdIv} | PID: {pid:X08} | Nature: {_natureStrs[nature]} | PID Delay: {woobatPidDelay} | IV Delay: {woobatIvDelay}";

#if false // doesn't actually work
		// NPC pid rng advances
		static uint CreateNPCTimer(ref ulong pidRng)
		{
			// returns 0x10, 0x20, 0x30, or 0x40
			var result = (AdvancePidRng(ref pidRng) * 4) >> 32;
			return (uint)((result + 1) * 0x10);
		}

		// note that mom does not advance rng at all
		// also note that when the timer is created, the timer is also advanced on the same frame...

		// npc1 timer is offscreen initially, it takes 0x52 ticks before they will be on screen and able to move
		// until then, when the timer runs out they will "immediately" start the timer again (or rather after 2 ticks)
		// ACTUALLY NO, IT'S NOT OFFSCREEN, IT'S AN INVISIBLE BORDER PRVENTING NPCS FROM GOING PLACES

		var npc1Timer = CreateNPCTimer(ref pidRng) - 1;
		var npc2Timer = CreateNPCTimer(ref pidRng) - 1;
		var npc3Timer = CreateNPCTimer(ref pidRng) - 1;
		uint npc1Cooldown = 0, npc2Cooldown = 0, npc3Cooldown = 0;

		for (var i = 0; i < 336; i++)
		{
			if (npc1Cooldown == 0)
			{
				if (--npc1Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc1Cooldown = i < 0x52 ? 2u : 9u;
				}
			}
			else if (--npc1Cooldown == 0)
			{
				npc1Timer = CreateNPCTimer(ref pidRng) - 1;
			}

			if (npc2Cooldown == 0)
			{
				if (--npc2Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc2Cooldown = 9;
				}
			}
			else if (--npc2Cooldown == 0)
			{
				npc2Timer = CreateNPCTimer(ref pidRng) - 1;
			}

			if (npc3Cooldown == 0)
			{
				if (--npc3Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc3Cooldown = 9;
				}
			}
			else if (--npc3Cooldown == 0)
			{
				npc3Timer = CreateNPCTimer(ref pidRng) - 1;
			}
		}

		// exiting bianca's house
		// npc3 is offscreen this time
		npc1Timer = CreateNPCTimer(ref pidRng) - 1;
		npc2Timer = CreateNPCTimer(ref pidRng) - 1;
		npc3Timer = CreateNPCTimer(ref pidRng) - 1;
		npc1Cooldown = 0;
		npc2Cooldown = 0;
		npc3Cooldown = 0;

		for (var i = 0; i < 232; i++)
		{
			if (npc1Cooldown == 0)
			{
				if (--npc1Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc1Cooldown = 9;
				}
			}
			else if (--npc1Cooldown == 0)
			{
				npc1Timer = CreateNPCTimer(ref pidRng) - 1;
			}

			if (npc2Cooldown == 0)
			{
				if (--npc2Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc2Cooldown = 9;
				}
			}
			else if (--npc2Cooldown == 0)
			{
				npc2Timer = CreateNPCTimer(ref pidRng) - 1;
			}

			if (npc3Cooldown == 0)
			{
				if (--npc3Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc3Cooldown = i < 0x38 ? 2u : 9u;
				}
			}
			else if (--npc3Cooldown == 0)
			{
				npc3Timer = CreateNPCTimer(ref pidRng) - 1;
			}
		}

		// two rng calls doing nickname (for some reason)
		AdvancePidRng(ref pidRng);
		AdvancePidRng(ref pidRng);

		// exiting prof's lab
		// npc3 is offscreen mostly, npc1 is always offscreen, npc2 is never offscreen
		npc1Timer = CreateNPCTimer(ref pidRng) - 1;
		npc2Timer = CreateNPCTimer(ref pidRng) - 1;
		npc3Timer = CreateNPCTimer(ref pidRng) - 1;
		npc1Cooldown = 0;
		npc2Cooldown = 0;
		npc3Cooldown = 0;

		for (var i = 0; i < 173; i++)
		{
			if (npc1Cooldown == 0)
			{
				if (--npc1Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc1Cooldown = 2;
				}
			}
			else if (--npc1Cooldown == 0)
			{
				npc1Timer = CreateNPCTimer(ref pidRng) - 1;
			}

			if (npc2Cooldown == 0)
			{
				if (--npc2Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc2Cooldown = 9;
				}
			}
			else if (--npc2Cooldown == 0)
			{
				npc2Timer = CreateNPCTimer(ref pidRng) - 1;
			}

			if (npc3Cooldown == 0)
			{
				if (--npc3Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc3Cooldown = i < 0x3B ? 9u : 2u;
				}
			}
			else if (--npc3Cooldown == 0)
			{
				npc3Timer = CreateNPCTimer(ref pidRng) - 1;
			}
		}

		// route 1, before catching tutorial
		// only npc2 has a timer at all, and is always visible
		npc2Timer = CreateNPCTimer(ref pidRng) - 1;
		npc2Cooldown = 0;

		for (var i = 0; i < 169; i++)
		{
			if (npc2Cooldown == 0)
			{
				if (--npc2Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc2Cooldown = 9;
				}
			}
			else if (--npc2Cooldown == 0)
			{
				npc2Timer = CreateNPCTimer(ref pidRng) - 1;
			}
		}

		// entering into catching tutorial has pid rng advanced 4 times
		for (var i = 0; i < 4; i++)
		{
			AdvancePidRng(ref pidRng);
		}

		// continuing npc2 timer from before tutorial
		var grassTileTicks = new[] { 27, 35, 43, 51, 59, 67, 75, 323, 331, 339, 347, 355 };
		for (var i = 0; i < 394; i++)
		{
			if (grassTileTicks.Any(tick => i == tick))
			{
				AdvancePidRng(ref pidRng); // don't know what this is for (selecting encounter slot?)
				var encounterCheck = (AdvancePidRng(ref pidRng) * 0xFFFF) >> 32;
				encounterCheck /= 0x290;
				if (encounterCheck < 8)
				{
					// got an unwanted encounter
					return null;
				}
			}

			// goes offscreen before reaching second grass patch (TODO: find exact point)
			if (npc2Cooldown == 0)
			{
				if (--npc2Timer == 0)
				{
					AdvancePidRng(ref pidRng);
					npc2Cooldown = i < 238 ? 9u : 2u;
				}
			}
			else if (--npc2Cooldown == 0)
			{
				npc2Timer = CreateNPCTimer(ref pidRng) - 1;
			}
		}
#endif

		return $"Starter: {starterStats} | Lillipup: {lillipupStats} | Woobat: {woobatStats}";
	}

	private static bool IsBlacklistedDay(int month, int day)
	{
		switch (day)
		{
			// next month, don't allow
			case 32:
			// April only has 30 days
			case 31 when month == 4:
				return true;
			default:
				return month switch
				{
					4 => _aprilHailDays[day],
					8 => _augustHailDays[day],
					12 => _decemberHailDays[day],
					_ => throw new InvalidOperationException()
				};
		}
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
#if false
	private static void SearchYear(int year)
	{
		using var mtrng = new MT19937();
		Span<byte> sha1Hash = stackalloc byte[160];
		Span<byte> initData = stackalloc byte[16 * 4];
		// black nazos
		_blackNazos.AsSpan().CopyTo(initData);
		// timer0/vcount (always 0xBD3 and 0x5A)
		initData[5 * 4 + 0] = 0xD3;
		initData[5 * 4 + 1] = 0x0B;
		initData[5 * 4 + 2] = 0x5A;
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
			for (var day = 1; day <= 31; day++)
			{
				if (IsBlacklistedDay(month, day))
				{
					continue;
				}

				initData[8 * 4 + 0] = _bcdValues[year];
				initData[8 * 4 + 1] = _bcdValues[month];
				initData[8 * 4 + 2] = _bcdValues[day];
				initData[8 * 4 + 3] = ComputeWeekday(year, month, day);

				var hourLimit = IsBlacklistedDay(month, day + 1) ? 21 : 24;
				for (var hour = 0; hour < hourLimit; hour++)
				{
					initData[9 * 4 + 0] = _bcdValues[hour];
					if (hour >= 12)
					{
						initData[9 * 4 + 0] |= 0x40;
					}

					for (var minute = 0; minute < 60; minute++)
					{
						initData[9 * 4 + 1] = _bcdValues[minute];

						for (var second = 0; second < 60; second++)
						{
							initData[9 * 4 + 2] = _bcdValues[second];

							for (var input = 0; input < 4096; input++)
							{
								initData[12 * 4 + 0] = (byte)(~input & 0xFF);
								initData[12 * 4 + 1] = (byte)((~input >> 8) & 0x2F);

								PkmnSHA1.HashBlock(initData, sha1Hash);
								var seed = BinaryPrimitives.ReadUInt64LittleEndian(sha1Hash);

								// seed is advanced once before being used for MTRNG
								var mtseed = AdvancePidRng(ref seed);
								mtrng.Init((uint)mtseed);
								var result = CheckSeed(seed, mtrng);
								if (result != null)
								{
									Console.WriteLine($"Year {2000 + year} | Month {month} | Day {day} | Hour {hour} | Minute {minute} | Second {second} | Input {(KeyInput)input} | Seed: {seed:X016} | {result}");
								}
							}
						}
					}
				}
			}
		}
	}
#endif
#if false
	private static void SearchYear(int year)
	{
		using var mtrng = new MT19937();
		Span<byte> sha1Hash = stackalloc byte[160];
		Span<byte> initData = stackalloc byte[16 * 4];
		// black nazos
		_blackNazos.AsSpan().CopyTo(initData);
		// timer0/vcount (always 0xBD3 and 0x5A)
		initData[5 * 4 + 0] = 0xD3;
		initData[5 * 4 + 1] = 0x0B;
		initData[5 * 4 + 2] = 0x5A;
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

			for (var hour = 0; hour < 22; hour++)
			{
				initData[9 * 4 + 0] = _bcdValues[hour];
				if (hour >= 12)
				{
					initData[9 * 4 + 0] |= 0x40;
				}

				for (var minute = 0; minute < 60; minute++)
				{
					initData[9 * 4 + 1] = _bcdValues[minute];

					for (var second = 0; second < 60; second++)
					{
						initData[9 * 4 + 2] = _bcdValues[second];

						for (var input = 0; input < 4096; input++)
						{
							initData[12 * 4 + 0] = (byte)(~input & 0xFF);
							initData[12 * 4 + 1] = (byte)((~input >> 8) & 0x2F);

							PkmnSHA1.HashBlock(initData, sha1Hash);
							var seed = BinaryPrimitives.ReadUInt64LittleEndian(sha1Hash);

							// seed is advanced once before being used for MTRNG
							var mtseed = AdvancePidRng(ref seed);
							mtrng.Init((uint)mtseed);
							var result = CheckSeed(seed, mtrng);
							if (result != null)
							{
								Console.WriteLine($"Year {2000 + year} | Month {month} | Day {day} | Hour {hour} | Minute {minute} | Second {second} | Input {(KeyInput)input} | Seed: {seed:X016} | {result}");
							}
						}
					}
				}
			}
		}
	}
#endif
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
			for (var minute = 58; minute < 60; minute++)
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
			for (var minute = 0; minute < 15; minute++)
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
