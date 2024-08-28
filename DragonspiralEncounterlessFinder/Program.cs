using System;
using System.Runtime.CompilerServices;

namespace Program;

internal static class Program
{
	private static void Main()
	{
		var pidRng = 0x9320AE646462A2FEUL;
		for (var i = 0; i < 500; i++)
		{
			var curPidRng = pidRng;
			AdvancePidRng(ref pidRng);

			for (var j = 0; j < 20; j++)
			{
				var phenomenonOffset = j;
				var oldPidRng = curPidRng;
				var success = true;
				for (var k = 0; k < 116; k++)
				{
					if (k % 20 == phenomenonOffset)
					{
						if (CheckPhenomenon(ref curPidRng))
						{
							phenomenonOffset++;
							phenomenonOffset %= 20;
						}
					}

					// these steps don't generate encounters
					if (k is 94 or 102 or 105)
					{
						continue;
					}

					if (CheckEncounter(ref curPidRng))
					{
						success = false;
						break;
					}
				}

				curPidRng = oldPidRng;

				if (success)
				{
					Console.WriteLine($"Found working seed {curPidRng:X016} / Advances: {i} / Phenomenon Offset: {j + 1}");
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong AdvancePidRng(ref ulong pidRng)
	{
		pidRng *= 0x5d588b656c078965;
		pidRng += 0x269ec3;
		return pidRng >> 32;
	}

	private static bool CheckEncounter(ref ulong pidRng)
	{
		AdvancePidRng(ref pidRng);
		var encounterCheck = (AdvancePidRng(ref pidRng) * 0xFFFF) >> 32;
		encounterCheck /= 0x290;
		return encounterCheck < 4;
	}

	private static bool CheckPhenomenon(ref ulong pidRng)
	{
		var phenomenonCheck = (AdvancePidRng(ref pidRng) * 1000) >> 32;
		if (phenomenonCheck < 100)
		{
			AdvancePidRng(ref pidRng);
		}

		return phenomenonCheck < 100;
	}
}
