using System;
using System.Runtime.CompilerServices;

namespace Program;

internal static class Program
{
	private static void TestSeedDown(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction == 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					0 => 0x1A,
					1 => 0x19,
					3 => 0x18,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					0 => 20,
					1 => 12,
					3 => 8,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void TestSeedLeft(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction is 1 or 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					0 => 0x22,
					3 => 0x1F,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					0 => 26,
					3 => 12,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void TestSeedRight(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction is 0 or 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					1 => 0x1F,
					3 => 0x18,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					1 => 17,
					3 => 10,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void TestSeedUp(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction == 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					0 => 0x25,
					1 => 0x1B,
					3 => 0x1E,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					0 => 31,
					1 => 14,
					3 => 14,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void TestSeedUpRight(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction is 0 or 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					1 => 0x21,
					3 => 0x1B,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					1 => 19,
					3 => 13,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void TestSeedUpLeft(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction is 1 or 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					0 => 0x27,
					3 => 0x21,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					0 => 31,
					3 => 15,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void TestSeedDownRight(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction is 0 or 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					1 => 0x1D,
					3 => 0x15,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					1 => 15,
					3 => 7,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void TestSeedDownLeft(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction is 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					0 => 0x1D,
					1 => 0x16,
					3 => 0x1C,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					0 => 21,
					1 => 9,
					3 => 9,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void TestSeedCenter(ulong seed)
	{
		for (var i = 0; i < 1000; i++)
		{
			var spawnCloud = (AdvancePidRng(ref seed) * 1000) >> 32;
			if (spawnCloud < 100)
			{
				var spawnCloudSeed = seed;
				var direction = (AdvancePidRng(ref spawnCloudSeed) * 4) >> 32;
				if (direction is 0 or 2)
				{
					continue;
				}

				var slotRng = AdvancePidRng(ref spawnCloudSeed);
				byte numSlots = direction switch
				{
					1 => 0x1A,
					3 => 0x1B,
					_ => throw new InvalidOperationException()
				};
				var slot = (slotRng * numSlots) >> 32;
				ulong wantedSlot = direction switch
				{
					1 => 13,
					3 => 11,
					_ => throw new InvalidOperationException()
				};

				if (slot != wantedSlot)
				{
					continue;
				}

				Console.WriteLine($"F {i} / D {direction} / S {slot}");
			}
		}
	}

	private static void Main()
	{
		//TestSeedDown(0x4ECBDC1BF97E7BDF);
		TestSeedDown(0x4F850570690A6EC4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong AdvancePidRng(ref ulong pidRng)
	{
		pidRng *= 0x5d588b656c078965;
		pidRng += 0x269ec3;
		return pidRng >> 32;
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
}
