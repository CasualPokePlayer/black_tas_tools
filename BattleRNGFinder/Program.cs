using System;
using System.Buffers.Binary;
using System.Threading;

namespace Program;

internal static partial class Program
{
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
			SearchKeyInput(i, false);
			SearchKeyInput(i, true);
		}
	}

	private static void Main()
	{
#if false
		SearchKeyInput(0, false);
#else
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
#endif
	}

	//private const string INIT_MESSAGE_NO_TOUCH = "2036FE0200000000E09B2602749D2602749D26025725990015754916A665BC8843043004625957000000000000060000A02500008000000000000000000001A0";
	private const string INIT_MESSAGE_TOUCHING = "2036FE0200000000E09B2602749D2602749D26023C569C00F1954916AA6BBB8864050104004817000000000011011000FF2F00008000000000000000000001A0";

	private static bool CheckBattleRng(ulong battleRng)
	{
		//return CheckBianca1(battleRng);
		//return CheckBianca1NoSpeedTie(battleRng);
		//return CheckCheren1(battleRng);
		//return CheckN1Growl(battleRng);
		//return CheckN1Scratch(battleRng);
		//return CheckN1TailWhip(battleRng);
		//return CheckJimmy(battleRng);
		//return CheckBianca2(battleRng);
		//return CheckCheren2(battleRng);
		//return CheckMaxwell(battleRng);
		//return CheckTia(battleRng);
		//return CheckCilan(battleRng);
		//return CheckPlasmaGrunt1(battleRng);
		//return CheckPlasmaGrunt2(battleRng);
		//return CheckKumiAmy(battleRng);
		//return CheckCheren3(battleRng);
		//return CheckCheren3TakeDown(battleRng);
		//return CheckWoobatCatch(battleRng);
		//return CheckPlasmaGrunt3(battleRng);
		//return CheckPlasmaGrunt4(battleRng);
		//return CheckN2(battleRng);
		//return CheckCarter(battleRng);
		//return CheckSatomi(battleRng);
		//return CheckLydia(battleRng);
		//return CheckLenoraNoXSpeedConfuseSelfHit(battleRng);
		//return CheckLenoraNoXSpecConfuseSelfHit(battleRng);
		//return CheckLenoraNoXSpec(battleRng);
		//return CheckPlasmaGrunt5(battleRng);
		//return CheckPlasmaGrunt6(battleRng);
		//return CheckAudra(battleRng);
		//return CheckPlasmaGrunt7(battleRng);
		//return CheckPlasmaGrunt8(battleRng);
		//return CheckPlasmaGrunt9(battleRng);
		//return CheckJack(battleRng);
		//return CheckBurgh(battleRng);
		//return CheckBianca3(battleRng);
		//return CheckCheren4(battleRng);
		//return CheckPlasmaGrunt10(battleRng);
		//return CheckN3GustFirst(battleRng);
		//return CheckMagnoliaXSpec(battleRng);
		//return CheckMagnoliaNoXSpec(battleRng);
		//return CheckCody(battleRng);
		//return CheckRolan(battleRng);
		//return CheckColette(battleRng);
		//return CheckColette2Flinch(battleRng);
		//return CheckElesa2Confusion(battleRng);
		//return CheckCheren5(battleRng);
		//return CheckSarahBilly(battleRng);
		//return CheckPlasmaGrunt11(battleRng);
		//return CheckPlasmaGrunt12(battleRng);
		//return CheckPlasmaGrunt13(battleRng);
		//return CheckPlasmaGrunt14(battleRng);
		//return CheckFelix(battleRng);
		//return CheckDon(battleRng);
		//return CheckKatie(battleRng);
		//return CheckClay(battleRng);
		//return CheckBianca4(battleRng);
		//return CheckPlasmaGrunt15(battleRng);
		//return CheckN4(battleRng);
		//return CheckDoreen(battleRng);
		//return CheckCliff(battleRng);
		//return CheckTed(battleRng);
		//return CheckChase(battleRng);
		//return CheckArnold(battleRng);
		//return CheckSkyla(battleRng);
		//return CheckCheren6(battleRng);
		//return CheckTerrell(battleRng);
		//return CheckGrant(battleRng);
		//return CheckMiriam(battleRng);
		//return CheckMikiko(battleRng);
		//return CheckChandra(battleRng);
		//return CheckBrycenSwagger(battleRng);
		//return CheckBrycenXSpec(battleRng);
		//return CheckBrycenFlinch(battleRng);
		//return CheckBrycenSwaggerNoMiss(battleRng);
		//return CheckPlasmaGrunt16(battleRng);
		//return CheckPlasmaGrunt17(battleRng);
		//return CheckPlasmaGrunt18(battleRng);
		//return CheckPlasmaGrunt19(battleRng);
		//return CheckBianca5(battleRng);
		//return CheckBianca5Fly(battleRng);
		//return CheckWebster(battleRng);
		//return CheckOlwen(battleRng);
		//return CheckClara(battleRng);
		//return CheckHugo(battleRng);
		//return CheckTom(battleRng);
		//return CheckDara(battleRng);
		//return CheckKim(battleRng);
		//return CheckDrayden(battleRng);
		//return CheckCheren7(battleRng);
		//return CheckShanta(battleRng);
		//return CheckMarshall(battleRng);
		//return CheckGrimsley(battleRng);
		//return CheckCaitlin(battleRng);
		//return CheckShauntal(battleRng);
		//return CheckReshiramCatch(battleRng);
		//return CheckN5(battleRng);
		return CheckGhetsis(battleRng);
	}

	private static void SearchKeyInput(int keyInput, bool closeLid)
	{
		Span<byte> sha1Hash = stackalloc byte[20];
		var initData = Convert.FromHexString(INIT_MESSAGE_TOUCHING);
		if (initData.Length != 16 * 4)
		{
			throw new InvalidOperationException();
		}

		// [11] is touch inputs, 00060000 == not touching
		initData[11 * 4 + 0] = 0x00;
		initData[11 * 4 + 1] = 0x06;
		initData[11 * 4 + 2] = 0x00;
		initData[11 * 4 + 3] = 0x00;
		// [12] is keypad inputs
		initData[12 * 4 + 0] = (byte)(~keyInput & 0xFF);
		initData[12 * 4 + 1] = (byte)((~keyInput >> 8) & (closeLid ? 0xAF : 0x2F));
		initData[12 * 4 + 2] = 0x00;
		initData[12 * 4 + 3] = 0x00;
#if false
		// check once without touching
		{
			PkmnSHA1.HashBlock(initData, sha1Hash);
			var seed = BinaryPrimitives.ReadUInt64LittleEndian(sha1Hash);

			if (CheckBattleRng(seed))
			{
				Console.WriteLine($"Key Input {(KeyInput)keyInput} | No Touch | Seed {seed:X016}");
			}
		}

		initData = Convert.FromHexString(INIT_MESSAGE_TOUCHING);
		if (initData.Length != 16 * 4)
		{
			throw new InvalidOperationException();
		}

		initData[12 * 4 + 0] = (byte)(~keyInput & 0xFF);
		initData[12 * 4 + 1] = (byte)((~keyInput >> 8) & 0x2F);
		initData[12 * 4 + 2] = 0x00;
		initData[12 * 4 + 3] = 0x00;
#endif

		initData[11 * 4 + 1] = 0x01;

		if (closeLid)
		{
			keyInput |= (int)KeyInput.LidClosed;
		}

		for (var touchX = 0; touchX < 256; touchX++)
		{
			initData[11 * 4 + 2] = (byte)((touchX & 0xF) << 4);
			initData[11 * 4 + 3] = (byte)(touchX >> 4);

			for (var touchY = 0; touchY < 192; touchY++)
			{
				initData[11 * 4 + 0] = (byte)touchY;
				PkmnSHA1.HashBlock(initData, sha1Hash);
				var seed = BinaryPrimitives.ReadUInt64LittleEndian(sha1Hash);

				if (CheckBattleRng(seed))
				{
					Console.WriteLine($"Key Input {(KeyInput)keyInput} | Touch {touchX}/{touchY} | Seed {seed:X016}");
				}
			}
		}
	}
}
