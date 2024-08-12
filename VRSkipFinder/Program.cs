using System;
using System.Runtime.CompilerServices;

namespace Program;

internal static class Program
{
	private static void Main()
	{
		const ulong START_SEED = 0x3914985B9DB17948;//0xB2ACC6E278EEBE1B;
        Console.WriteLine("Down Seeds");
		TestSeedDown(START_SEED);
		Console.WriteLine("Left Seeds");
		TestSeedLeft(START_SEED);
		Console.WriteLine("Right Seeds");
		TestSeedRight(START_SEED);
		Console.WriteLine("Up Seeds");
		TestSeedUp(START_SEED);
		Console.WriteLine("Up Right Seeds");
		TestSeedUpRight(START_SEED);
		Console.WriteLine("Up Left Seeds");
		TestSeedUpLeft(START_SEED);
		Console.WriteLine("Down Right Seeds");
		TestSeedDownRight(START_SEED);
		Console.WriteLine("Down Left Seeds");
		TestSeedDownLeft(START_SEED);
		Console.WriteLine("Center Seeds");
		TestSeedCenter(START_SEED);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong AdvancePidRng(ref ulong pidRng)
	{
		pidRng *= 0x5d588b656c078965;
		pidRng += 0x269ec3;
		return pidRng >> 32;
	}

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
}
