using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

	private const int MAX_TURNFRAMES = 30;

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
		LidClosed = 1 << 15,
	}

	private record ThreadParam(int StartInput, int EndInput);

	private static void ThreadProc(object? param)
	{
		var threadParam = (ThreadParam)param!;
		for (var i = threadParam.StartInput; i < threadParam.EndInput; i++)
		{
			for (var j = 0; j < 2; j++)
			{
				var saveType = (SaveType)j;
				SearchKeyInput(i, false, false, saveType);
				SearchKeyInput(i, false, true, saveType);
				SearchKeyInput(i, true, false, saveType);
				SearchKeyInput(i, true, true, saveType);
			}
		}
	}

	private static void Main()
	{
		for (var i = 0; i < _bcdValues.Length; i++)
		{
			_bcdValues[i] = (byte)(((i / 10) << 4) + i % 10);
		}

		var maxParallelism = Environment.ProcessorCount * 3 / 2;
		var threads = new Thread[maxParallelism];
		var inputsPerThread = 4096 / maxParallelism;
		for (var i = 0; i < maxParallelism; i++)
		{
			threads[i] = new Thread(ThreadProc) { IsBackground = true };
			var threadParam = new ThreadParam(i * inputsPerThread, (i + 1) * inputsPerThread);
			threads[i].Start(threadParam);
		}

		// last couple of inputs covered here
		{
			var threadParam = new ThreadParam(maxParallelism * inputsPerThread, 4096);
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

	private static void CheckEncounter(ref ulong pidRng)
	{
		AdvancePidRng(ref pidRng);

		var encounterCheck = (AdvancePidRng(ref pidRng) * 0xFFFF) >> 32;
		encounterCheck /= 0x290;
		if (encounterCheck < 4)
		{
			// extra rolls for encounter
			// we have repel, so we don't particularly care this happens
			// (but the extra rolls happen regardless for whatever reason)
			AdvancePidRng(ref pidRng);
			AdvancePidRng(ref pidRng);
		}
	}

	private static ulong? CheckDustCloud(ulong pidRng, int numTurnFrames, int stepsToNextTrainer,
		byte numRightwardsSlots, ReadOnlySpan<byte> wantedRightwardsSlots,
		byte numLeftwardsSlots, ReadOnlySpan<byte> wantedLeftwardsSlots,
		byte numUpwardsSlots, ReadOnlySpan<byte> wantedUpwardsSlots,
		byte numDownwardsSlots, ReadOnlySpan<byte> wantedDownwardsSlots)
	{
		for (var i = 0; i < numTurnFrames; i++)
		{
			CheckEncounter(ref pidRng);
		}

		var dustCloudRng = (AdvancePidRng(ref pidRng) * 1000) >> 32;
		if (dustCloudRng >= 100)
		{
			return null;
		}

		// 0 means scan rightwards, 1 means scan leftwards, 2 means scan upwards, 3 means scan downwards
		var direction = (AdvancePidRng(ref pidRng) * 4) >> 32;
		switch (direction)
		{
			case 0:
			{
				var dustCloudLoc = (AdvancePidRng(ref pidRng) * numRightwardsSlots) >> 32;
				if (!wantedRightwardsSlots.Contains((byte)dustCloudLoc))
				{
					return null;
				}

				break;
			}

			case 1:
			{
				var dustCloudLoc = (AdvancePidRng(ref pidRng) * numLeftwardsSlots) >> 32;
				if (!wantedLeftwardsSlots.Contains((byte)dustCloudLoc))
				{
					return null;
				}

				break;
			}

			case 2:
			{
				var dustCloudLoc = (AdvancePidRng(ref pidRng) * numUpwardsSlots) >> 32;
				if (!wantedUpwardsSlots.Contains((byte)dustCloudLoc))
				{
					return null;
				}

				break;
			}

			case 3:
			{
				var dustCloudLoc = (AdvancePidRng(ref pidRng) * numDownwardsSlots) >> 32;
				if (!wantedDownwardsSlots.Contains((byte)dustCloudLoc))
				{
					return null;
				}

				break;
			}
		}

		for (var i = 0; i < stepsToNextTrainer; i++)
		{
			CheckEncounter(ref pidRng);
		}

		return pidRng;
	}

	private static void CheckSeedPathSkip5(ulong pidRng, string path1234, List<string> results)
	{
		// no trainer rng rolls, as we cancel the dust cloud with the crystal (:

		// repel would likely wear off at this point... which means 1 less encounter check than expected
		for (var i = 0; i < 18; i++)
		{
			CheckEncounter(ref pidRng);
		}

		for (var i = 0; i < MAX_TURNFRAMES; i++)
		{
			// optimal paths, spawning cloud in optimal positions and suboptimal positions
			// left 1, down 2 of trainer to skip
			var nextPidRngA = CheckDustCloud(pidRng, i, 0,
				numRightwardsSlots: 0x28, wantedRightwardsSlots: [6, 12, 18, 24],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x13, wantedUpwardsSlots: [7, 14],
				numDownwardsSlots: 0x2A, wantedDownwardsSlots: [2, 9, 19]);

			if (nextPidRngA.HasValue)
			{
				results.Add($"{path1234} A{i} | End Seed {nextPidRngA.Value:X016}");
			}

			// suboptimal path, spawning cloud in optimal positions and suboptimal positions
			// left 1 of trainer to skip
			var nextPidRngB = CheckDustCloud(pidRng, i, 0,
				numRightwardsSlots: 0x23, wantedRightwardsSlots: [6, 12, 18, 24],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0, wantedUpwardsSlots: [],
				numDownwardsSlots: 0x2F, wantedDownwardsSlots: [7, 14, 21, 31]);

			if (nextPidRngB.HasValue)
			{
				results.Add($"{path1234} B{i} | End Seed {nextPidRngB.Value:X016}");
			}

			// left 1, down 4 of trainer to skip, not particularly optimal
			var nextPidRngC = CheckDustCloud(pidRng, i, 0,
				numRightwardsSlots: 0x32, wantedRightwardsSlots: [6, 12, 18, 24],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x24, wantedUpwardsSlots: [7, 14, 21, 31],
				numDownwardsSlots: 0x28, wantedDownwardsSlots: [5]);

			if (nextPidRngC.HasValue)
			{
				results.Add($"{path1234} C{i} | End Seed {nextPidRngC.Value:X016}");
			}

			// left 2, down 1 of trainer to skip, not particularly optimal
			var nextPidRngD = CheckDustCloud(pidRng, i, 0,
				numRightwardsSlots: 0x27, wantedRightwardsSlots: [7, 13, 19, 25],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x0B, wantedUpwardsSlots: [7],
				numDownwardsSlots: 0x29, wantedDownwardsSlots: [1, 7, 13, 22]);

			if (nextPidRngD.HasValue)
			{
				results.Add($"{path1234} D{i} | End Seed {nextPidRngD.Value:X016}");
			}

			// left 2, down 3 of trainer to skip, not particularly optimal
			var nextPidRngE = CheckDustCloud(pidRng, i, 0,
				numRightwardsSlots: 0x2B, wantedRightwardsSlots: [7, 13, 19, 25],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x17, wantedUpwardsSlots: [7, 13, 19],
				numDownwardsSlots: 0x24, wantedDownwardsSlots: [2, 11]);

			if (nextPidRngE.HasValue)
			{
				results.Add($"{path1234} E{i} | End Seed {nextPidRngE.Value:X016}");
			}

			// left 2, down 5 of trainer to skip, not particularly optimal
			var nextPidRngF = CheckDustCloud(pidRng, i, 0,
				numRightwardsSlots: 0x35, wantedRightwardsSlots: [7, 13, 19, 25],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x2A, wantedUpwardsSlots: [7, 13, 19, 28],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngF.HasValue)
			{
				results.Add($"{path1234} F{i} | End Seed {nextPidRngF.Value:X016}");
			}
		}
	}

	private static void CheckSeedPathSkip4(ulong pidRng, string path123, List<string> results)
	{
		// no trainer rng rolls, as we cancel the dust cloud with the crystal (:

		for (var i = 0; i < 19; i++)
		{
			CheckEncounter(ref pidRng);
		}

		for (var i = 0; i < MAX_TURNFRAMES; i++)
		{
			// optimal path, as close as possible
			var nextPidRngA = CheckDustCloud(pidRng, i, 4,
				numRightwardsSlots: 0x1C, wantedRightwardsSlots: [12],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x16, wantedUpwardsSlots: [14],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngA.HasValue)
			{
				CheckSeedPathSkip5(nextPidRngA.Value, $"{path123} A{i}", results);
			}

			// suboptimal path, 2 steps away from optimal position
			var nextPidRngB = CheckDustCloud(pidRng, i, 6,
				numRightwardsSlots: 0x16, wantedRightwardsSlots: [6],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x14, wantedUpwardsSlots: [8],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngB.HasValue)
			{
				CheckSeedPathSkip5(nextPidRngB.Value, $"{path123} B{i}", results);
			}

			// suboptimal path, 4 steps away from optimal position, left side
			var nextPidRngC = CheckDustCloud(pidRng, i, 8,
				numRightwardsSlots: 0x0E, wantedRightwardsSlots: [2],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x0E, wantedUpwardsSlots: [2],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngC.HasValue)
			{
				CheckSeedPathSkip5(nextPidRngC.Value, $"{path123} C{i}", results);
			}

			// suboptimal path, 4 steps away from optimal position, right side
			var nextPidRngD = CheckDustCloud(pidRng, i, 8,
				numRightwardsSlots: 0x11, wantedRightwardsSlots: [0],
				numLeftwardsSlots: 0x09, wantedLeftwardsSlots: [2],
				numUpwardsSlots: 0x18, wantedUpwardsSlots: [2],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngD.HasValue)
			{
				CheckSeedPathSkip5(nextPidRngD.Value, $"{path123} D{i}", results);
			}
		}
	}

	private static void CheckSeedPathSkip3(ulong pidRng, string path12, List<string> results)
	{
		// dunno what this is for (some 1/25 roll on starting battle? per enemy pokemon?)
		for (var i = 0; i < 3; i++)
		{
			AdvancePidRng(ref pidRng);
		}

		for (var i = 0; i < 19; i++)
		{
			CheckEncounter(ref pidRng);
		}

		for (var i = 0; i < MAX_TURNFRAMES; i++)
		{
			// optimal path, spawning cloud in optimal position
			var nextPidRngA = CheckDustCloud(pidRng, i, /*17*/ 10,
				numRightwardsSlots: 0, wantedRightwardsSlots: [],
				numLeftwardsSlots: 0x21, wantedLeftwardsSlots: [28],
				numUpwardsSlots: 0, wantedUpwardsSlots: [],
				numDownwardsSlots: 0x18, wantedDownwardsSlots: [12]);

			if (nextPidRngA.HasValue)
			{
				CheckSeedPathSkip4(nextPidRngA.Value, $"{path12} A{i}", results);
			}

			// optimal path, spawning cloud in suboptimal position (needs 2 extra steps to get around)
			var nextPidRngB = CheckDustCloud(pidRng, i, /*19*/ 12,
				numRightwardsSlots: 0, wantedRightwardsSlots: [],
				numLeftwardsSlots: 0x21, wantedLeftwardsSlots: [22],
				numUpwardsSlots: 0x1C, wantedUpwardsSlots: [24],
				numDownwardsSlots: 0x18, wantedDownwardsSlots: [4]);

			if (nextPidRngB.HasValue)
			{
				CheckSeedPathSkip4(nextPidRngB.Value, $"{path12} B{i}", results);
			}

			// path ending 1 right of plasma trainer
			// this either has 2 extra steps or 4 extra steps
			var nextPidRngC = CheckDustCloud(pidRng, i, /*19*/ 12,
				numRightwardsSlots: 0, wantedRightwardsSlots: [],
				numLeftwardsSlots: 0x1F, wantedLeftwardsSlots: [20],
				numUpwardsSlots: 0x21, wantedUpwardsSlots: [24],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngC.HasValue)
			{
				CheckSeedPathSkip4(nextPidRngC.Value, $"{path12} C{i}", results);
			}

			// path ending 1 right of plasma trainer
			// this either has 2 extra steps or 4 extra steps
			var nextPidRngD = CheckDustCloud(pidRng, i, /*21*/ 14,
				numRightwardsSlots: 0, wantedRightwardsSlots: [],
				numLeftwardsSlots: 0x1F, wantedLeftwardsSlots: [14],
				numUpwardsSlots: 0x21, wantedUpwardsSlots: [16],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngD.HasValue)
			{
				CheckSeedPathSkip4(nextPidRngD.Value, $"{path12} D{i}", results);
			}

			// path ending ending at the topright corner
			// this 4 extra steps always
			var nextPidRngE = CheckDustCloud(pidRng, i, /*21*/ 14,
				numRightwardsSlots: 0, wantedRightwardsSlots: [],
				numLeftwardsSlots: 0x22, wantedLeftwardsSlots: [21, 27],
				numUpwardsSlots: 0, wantedUpwardsSlots: [],
				numDownwardsSlots: 0x1C, wantedDownwardsSlots: [10, 17]);

			if (nextPidRngE.HasValue)
			{
				CheckSeedPathSkip4(nextPidRngE.Value, $"{path12} E{i}", results);
			}
		}
	}

	private static void CheckSeedPathSkip2(ulong pidRng, string path1, List<string> results)
	{
		// dunno what this is for (some 1/25 roll on starting battle?)
		AdvancePidRng(ref pidRng);

		for (var i = 0; i < 19; i++)
		{
			CheckEncounter(ref pidRng);
		}

		for (var i = 0; i < MAX_TURNFRAMES; i++)
		{
			// optimal path, spawning cloud in optimal position
			var nextPidRngA = CheckDustCloud(pidRng, i, 13,
				numRightwardsSlots: 0x12, wantedRightwardsSlots: [7, 8],
				numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
				numUpwardsSlots: 0x10, wantedUpwardsSlots: [9, 10],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngA.HasValue)
			{
				CheckSeedPathSkip3(nextPidRngA.Value, $"{path1} A{i}", results);
			}

			// optimal path, spawning cloud in suboptimal position (costing 2 extra steps)
			var nextPidRngB = CheckDustCloud(pidRng, i, 15,
				numRightwardsSlots: 0x12, wantedRightwardsSlots: [6],
				numLeftwardsSlots: 0x10, wantedLeftwardsSlots: [3],
				numUpwardsSlots: 0x10, wantedUpwardsSlots: [8],
				numDownwardsSlots: 0, wantedDownwardsSlots: []);

			if (nextPidRngB.HasValue)
			{
				CheckSeedPathSkip3(nextPidRngB.Value, $"{path1} B{i}", results);
			}
		}
	}

	private static List<string> CheckSeedPathSkip1(ulong pidRng, SaveType saveType)
	{
		var results = new List<string>();

		// initial advances, these happen before getting to the overworld
		for (var i = 0; i < 5; i++)
		{
			InitAdvanceRng(ref pidRng);
		}

		for (var i = 0; i < 19; i++)
		{
			CheckEncounter(ref pidRng);
		}

		for (var i = 0; i < MAX_TURNFRAMES; i++)
		{
			// ReSharper disable once ConvertIfStatementToSwitchStatement
			if (saveType == SaveType.EarlySaves)
			{
				// earliest save possible on first trainer
				var nextPidRngA = CheckDustCloud(pidRng, i, 10,
					numRightwardsSlots: 0x13, wantedRightwardsSlots: [0, 1, 2],
					numLeftwardsSlots: 0x0C, wantedLeftwardsSlots: [1],
					numUpwardsSlots: 0x16, wantedUpwardsSlots: [1, 2, 3],
					numDownwardsSlots: 0, wantedDownwardsSlots: []);

				if (nextPidRngA.HasValue)
				{
					CheckSeedPathSkip2(nextPidRngA.Value, $"A{i}", results);
				}

				// save 1 step later for first trainer
				var nextPidRngB = CheckDustCloud(pidRng, i, 9,
					numRightwardsSlots: 0x14, wantedRightwardsSlots: [4, 5, 6],
					numLeftwardsSlots: 0x0D, wantedLeftwardsSlots: [3],
					numUpwardsSlots: 0x1B, wantedUpwardsSlots: [6, 7, 8],
					numDownwardsSlots: 0, wantedDownwardsSlots: []);

				if (nextPidRngB.HasValue)
				{
					CheckSeedPathSkip2(nextPidRngB.Value, $"B{i}", results);
				}
			}

			if (saveType == SaveType.LateSaves)
			{
				// save 2 steps later for first trainer
				var nextPidRngC = CheckDustCloud(pidRng, i, 8,
					numRightwardsSlots: 0x14, wantedRightwardsSlots: [4, 5, 6],
					numLeftwardsSlots: 0x0C, wantedLeftwardsSlots: [3],
					numUpwardsSlots: 0x1B, wantedUpwardsSlots: [6, 7, 8],
					numDownwardsSlots: 0, wantedDownwardsSlots: []);

				if (nextPidRngC.HasValue)
				{
					CheckSeedPathSkip2(nextPidRngC.Value, $"C{i}", results);
				}

				// save 3 steps later for first trainer, going Up at first junction
				var nextPidRngD = CheckDustCloud(pidRng, i, 7,
					numRightwardsSlots: 0x14, wantedRightwardsSlots: [4, 5, 6],
					numLeftwardsSlots: 0x0B, wantedLeftwardsSlots: [3],
					numUpwardsSlots: 0x1A, wantedUpwardsSlots: [6, 7, 8],
					numDownwardsSlots: 0, wantedDownwardsSlots: []);

				if (nextPidRngD.HasValue)
				{
					CheckSeedPathSkip2(nextPidRngD.Value, $"D{i}", results);
				}

				// save 3 steps later for first trainer, going Left at first junction
				var nextPidRngE = CheckDustCloud(pidRng, i, 7,
					numRightwardsSlots: 0x15, wantedRightwardsSlots: [6, 7, 8],
					numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
					numUpwardsSlots: 0x1B, wantedUpwardsSlots: [6, 7, 8],
					numDownwardsSlots: 0, wantedDownwardsSlots: []);

				if (nextPidRngE.HasValue)
				{
					CheckSeedPathSkip2(nextPidRngE.Value, $"E{i}", results);
				}

				// save 4 steps later for first trainer, going Up x2 at first junction
				var nextPidRngF = CheckDustCloud(pidRng, i, 6,
					numRightwardsSlots: 0x14, wantedRightwardsSlots: [4, 5, 6],
					numLeftwardsSlots: 0x0B, wantedLeftwardsSlots: [3],
					numUpwardsSlots: 0x11, wantedUpwardsSlots: [6, 7, 8],
					numDownwardsSlots: 0, wantedDownwardsSlots: []);

				if (nextPidRngF.HasValue)
				{
					CheckSeedPathSkip2(nextPidRngF.Value, $"F{i}", results);
				}

				// save 4 steps later for first trainer, going Left+Up at first junction
				var nextPidRngG = CheckDustCloud(pidRng, i, 6,
					numRightwardsSlots: 0x15, wantedRightwardsSlots: [6, 7, 8],
					numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
					numUpwardsSlots: 0x19, wantedUpwardsSlots: [6, 7, 8],
					numDownwardsSlots: 0, wantedDownwardsSlots: []);

				if (nextPidRngG.HasValue)
				{
					CheckSeedPathSkip2(nextPidRngG.Value, $"G{i}", results);
				}

				// save 5 steps later for first trainer, furthest possible save
				var nextPidRngH = CheckDustCloud(pidRng, i, 5,
					numRightwardsSlots: 0x15, wantedRightwardsSlots: [6, 7, 8],
					numLeftwardsSlots: 0, wantedLeftwardsSlots: [],
					numUpwardsSlots: 0x10, wantedUpwardsSlots: [6, 7, 8],
					numDownwardsSlots: 0, wantedDownwardsSlots: []);

				if (nextPidRngH.HasValue)
				{
					CheckSeedPathSkip2(nextPidRngH.Value, $"H{i}", results);
				}
			}
		}


		return results;
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

	private enum SaveType
	{
		EarlySaves = 0,
		LateSaves = 1,
	}

	private static void SearchKeyInput(int keyInput, bool closeLid, bool earlyTimer0, SaveType saveType)
	{
		Span<byte> sha1Hash = stackalloc byte[160];
		Span<byte> initData = stackalloc byte[16 * 4];
		var timer0 = (byte)(earlyTimer0 ? 0xD5 : 0xD6);
		// black nazos
		_blackNazos.AsSpan().CopyTo(initData);
		// timer0/vcount (vcount is always 0x5B, timer0 alternates between 0xBD5 and 0xBD6)
		initData[5 * 4 + 0] = timer0;
		initData[5 * 4 + 1] = 0x0B;
		initData[5 * 4 + 2] = 0x5B;
		initData[5 * 4 + 3] = 0x00;
		// lower mac (always 49:16) ^ timer0 overflows (1)
		initData[6 * 4 + 0] = 0x01;
		initData[6 * 4 + 1] = 0x00;
		initData[6 * 4 + 2] = 0x49;
		initData[6 * 4 + 3] = 0x16;
		// upper mac ^ vframe ^ gxstat (always 00:09:BF:0E, 6, and 0x86)
		initData[7 * 4 + 0] = 0x06;
		initData[7 * 4 + 1] = 0x09;
		initData[7 * 4 + 2] = 0xBF;
		initData[7 * 4 + 3] = 0x88;
		// date, in YY MM DD WD format
		initData[8 * 4 + 0] = _bcdValues[64];
		initData[8 * 4 + 1] = _bcdValues[4];
		initData[8 * 4 + 2] = _bcdValues[30];
		initData[8 * 4 + 3] = ComputeWeekday(64, 4, 30);
		// [9] is time, in HH MM SS 00 format
		initData[9 * 4 + 0] = (byte)(_bcdValues[23] | 0x40);
		initData[9 * 4 + 1] = _bcdValues[28];
		initData[9 * 4 + 2] = saveType switch
		{
			SaveType.EarlySaves => _bcdValues[11],
			SaveType.LateSaves => _bcdValues[12],
			_ => throw new InvalidOperationException()
		};
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
		// keypad inputs
		initData[12 * 4 + 0] = (byte)(~keyInput & 0xFF);
		initData[12 * 4 + 1] = (byte)((~keyInput >> 8) & (closeLid ? 0xAF : 0x2F));
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

		if (closeLid)
		{
			keyInput |= (int)KeyInput.LidClosed;
		}

		static int ParseDelayFromResult(string result)
		{
			// meh, slow
			// but probably not something to worry about
			var path = result.Split('|')[0];
			var strs = path.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			var delay = strs.Sum(str => int.Parse(str[1..]));
			for (var i = 0; i < 5; i++)
			{
				var pathId = strs[i][0];
				switch (i)
				{
					case 0:
						// regardless of the path, there are no extra steps
						break;
					case 1:
						switch (pathId)
						{
							case 'B':
								delay += 2;
								break;
						}

						break;
					case 2:
						switch (pathId)
						{
							case 'B':
							case 'C':
								delay += 2;
								break;
							case 'D':
							case 'E':
								delay += 4;
								break;
						}

						break;
					case 3:
						switch (pathId)
						{
							case 'B':
								delay += 2;
								break;
							case 'C':
							case 'D':
								delay += 4;
								break;
						}

						break;
					case 4:
						// guesses...
						switch (pathId)
						{
							case 'B':
								delay += 2;
								break;
							case 'C':
							case 'D':
								delay += 4;
								break;
							case 'E':
							case 'F':
								delay += 6;
								break;
						}

						break;
				}
			}

			return delay;
		}

		static int FindStepsUntilEncounter(string result)
		{
			var seed = ulong.Parse(result.Split('|')[1][10..], NumberStyles.HexNumber);
			// keep in mind repel will likely wear off in the middle of plasma skip...
			var i = 0;
			for (i = 0; i < 3; i++)
			{
				AdvancePidRng(ref seed);
				var encounterCheck = (AdvancePidRng(ref seed) * 0xFFFF) >> 32;
				encounterCheck /= 0x290;
				if (encounterCheck < 4)
				{
					AdvancePidRng(ref seed);
					AdvancePidRng(ref seed);
				}
			}

			while (true)
			{
				if (i == 11)
				{
					AdvancePidRng(ref seed);
					AdvancePidRng(ref seed);
					AdvancePidRng(ref seed);
				}

				AdvancePidRng(ref seed);
				var encounterCheck = (AdvancePidRng(ref seed) * 0xFFFF) >> 32;
				encounterCheck /= 0x290;
				if (encounterCheck < 4)
				{
					break;
				}

				i++;
			}

			return i;
		}

		{
			PkmnSHA1.HashBlock(initData, sha1Hash);
			var seed = BinaryPrimitives.ReadUInt64LittleEndian(sha1Hash);

			// seed is advanced once for MTRNG (this is what we'll end up seeing on startup)
			AdvancePidRng(ref seed);

			var results = CheckSeedPathSkip1(seed, saveType);
			foreach (var result in results)
			{
				var delay = ParseDelayFromResult(result);
				if (delay > 40)
				{
					continue;
				}
				var stepsUntilEncounter = FindStepsUntilEncounter(result);
				Console.WriteLine($"Delay {delay} | Encounter {stepsUntilEncounter} | Path {result} | Timer0 {timer0:X02} | Input {(KeyInput)keyInput} | Seed: {seed:X016}");
			}
		}
	}
}
