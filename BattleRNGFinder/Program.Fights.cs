using System;
using System.Runtime.CompilerServices;

namespace Program;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable ConvertIfStatementToReturnStatement

internal static partial class Program
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong AdvanceBattleRng(ref ulong battleRng)
	{
		battleRng *= 0x5d588b656c078965;
		battleRng += 0x269ec3;
		return battleRng >> 32;
	}

	private static bool? CheckSpeedTieRoll(ref ulong battleRng, bool? wantFirst)
	{
		// dunno what this is for, some 50/50 roll at 0x0689CF8D, happens only when a speedtie occurs?
		// doesn't actually influence speedtie calcs
		AdvanceBattleRng(ref battleRng);

		var result1 = AdvanceBattleRng(ref battleRng);
		result1 *= 2;
		result1 >>= 32;
		var result2 = AdvanceBattleRng(ref battleRng);
		result2 *= 2;
		result2 >>= 32;
		var wentFirst = result1 == result2;
		if (wantFirst.HasValue && wentFirst != wantFirst)
		{
			return null;
		}

		return wentFirst;
	}

	private static bool? CheckAccuracyRoll(ref ulong battleRng, byte accuracy, bool? wantHit)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotHit = result < accuracy;
		if (wantHit.HasValue && gotHit != wantHit)
		{
			return null;
		}

		return gotHit;
	}

	private static bool? CheckCriticalHitRoll(ref ulong battleRng, bool? wantCrit)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 16;
		result >>= 32;
		var gotCrit = result == 0;
		if (wantCrit.HasValue && gotCrit != wantCrit)
		{
			return null;
		}

		return gotCrit;
	}

	private static bool? CheckBoostedCriticalHitRoll(ref ulong battleRng, bool? wantCrit)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 8;
		result >>= 32;
		var gotCrit = result == 0;
		if (wantCrit.HasValue && gotCrit != wantCrit)
		{
			return null;
		}

		return gotCrit;
	}

	private static byte? CheckDamageRoll(ref ulong battleRng, byte? minRoll, byte? maxRoll)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 16;
		result >>= 32;
		var damageRoll = (byte)(100 - result);
		if (damageRoll < minRoll || damageRoll > maxRoll)
		{
			return null;
		}

		return damageRoll;
	}

	private static bool? CheckFlinchRoll(ref ulong battleRng, byte flinchChance, bool? wantFlinch)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotFlinch = result < flinchChance;
		if (wantFlinch.HasValue && gotFlinch != wantFlinch)
		{
			return null;
		}

		return gotFlinch;
	}

	private static bool? CheckParalysis(ref ulong battleRng, byte paraChance, bool? wantPara)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotPara = result < paraChance;
		if (wantPara.HasValue && gotPara != wantPara)
		{
			return null;
		}

		return gotPara;
	}

	private static bool? CheckConfusionRoll(ref ulong battleRng, byte confusionChance, bool? wantConfusion)
	{
		// this call determines confusion turn count
		AdvanceBattleRng(ref battleRng);

		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotConfusion = result < confusionChance;
		if (wantConfusion.HasValue && gotConfusion != wantConfusion)
		{
			return null;
		}

		return gotConfusion;
	}

	private static byte? CheckConfusionTurnCountRoll(ref ulong battleRng, byte? minTurnCount, byte? maxTurnCount)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 4;
		result >>= 32;
		var confusionTurnCount = (byte)(result + 2);
		if (confusionTurnCount < minTurnCount || confusionTurnCount > maxTurnCount)
		{
			return null;
		}

		return confusionTurnCount;
	}

	private static bool? CheckConfusionSelfHit(ref ulong battleRng, bool? wantSelfHit)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotSelfHit = result < 50;
		if (wantSelfHit.HasValue && gotSelfHit != wantSelfHit)
		{
			return null;
		}

		return gotSelfHit;
	}

	private static bool? CheckQuickClaw(ref ulong battleRng, bool? wantActivation)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotActivation = result < 20;
		if (wantActivation.HasValue && gotActivation != wantActivation)
		{
			return null;
		}

		return gotActivation;
	}

	private static byte? CheckForewarn(ref ulong battleRng, byte numMoves, byte? wantedMove)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= numMoves;
		result >>= 32;
		var selectedMove = (byte)result;
		if (wantedMove.HasValue && selectedMove != wantedMove)
		{
			return null;
		}

		return selectedMove;
	}

	private static byte? CheckFrisk(ref ulong battleRng, byte numMons, byte? wantedMon)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= numMons;
		result >>= 32;
		var selectedMon = (byte)result;
		if (wantedMon.HasValue && selectedMon != wantedMon)
		{
			return null;
		}

		return selectedMon;
	}

	private static bool? CheckCursedBody(ref ulong battleRng, bool? wantDisable)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotDisable = result < 30;
		if (wantDisable.HasValue && gotDisable != wantDisable)
		{
			return null;
		}

		return gotDisable;
	}

	private static bool? CheckFlameBody(ref ulong battleRng, bool? wantBurn)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotBurn = result < 30;
		if (wantBurn.HasValue && gotBurn != wantBurn)
		{
			return null;
		}

		return gotBurn;
	}

	private static bool? CheckQuickClawSpeedTie(ref ulong battleRng, bool? wantFirst, bool hasOdd5050)
	{
		if (hasOdd5050)
		{
			// sometimes this doesn't happen?
			AdvanceBattleRng(ref battleRng);
		}

		var result1 = AdvanceBattleRng(ref battleRng);
		result1 *= 2;
		result1 >>= 32;

		var result2 = AdvanceBattleRng(ref battleRng);
		result2 *= 100;
		result2 >>= 32;
		var gotActivation = result2 < 20;
		if (gotActivation)
		{
			// we pretty much never want activation if we're in a speedtie
			// if we want first or not, we'll let the speedtie decide
			return null;
		}

		var result3 = AdvanceBattleRng(ref battleRng);
		result3 *= 2;
		result3 >>= 32;
		var wentFirst = result1 == result3;
		if (wantFirst.HasValue && wentFirst != wantFirst)
		{
			return null;
		}

		return wentFirst;
	}

	private static bool? CheckStatic(ref ulong battleRng, bool? wantPara)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotPara = result < 30;
		if (wantPara.HasValue && gotPara != wantPara)
		{
			return null;
		}

		return gotPara;
	}

	private static bool? CheckPoisonPoint(ref ulong battleRng, bool? wantPoison)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotPoison = result < 30;
		if (wantPoison.HasValue && gotPoison != wantPoison)
		{
			return null;
		}

		return gotPoison;
	}

	private static bool? CheckStatDrop(ref ulong battleRng, byte dropChance, bool? wantDrop)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotDrop = result < dropChance;
		if (wantDrop.HasValue && gotDrop != wantDrop)
		{
			return null;
		}

		return gotDrop;
	}

	private static bool? CheckBurn(ref ulong battleRng, byte burnChance, bool? wantBurn)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 100;
		result >>= 32;
		var gotBurn = result < burnChance;
		if (wantBurn.HasValue && gotBurn != wantBurn)
		{
			return null;
		}

		return gotBurn;
	}

	private static bool? CheckPokeballShake(ref ulong battleRng, uint shakeChance, bool? wantShake)
	{
		var result = AdvanceBattleRng(ref battleRng);
		result *= 65536;
		result >>= 32;
		var gotShake = result < shakeChance;
		if (wantShake.HasValue && gotShake != wantShake)
		{
			return null;
		}

		return gotShake;
	}

	private static bool CheckBianca1(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckSpeedTieRoll(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		var t1DamageRoll = CheckDamageRoll(ref battleRng, 90, 100);
		if (!t1DamageRoll.HasValue) return false;
		// leer
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;

		// turn 2
		if (!CheckSpeedTieRoll(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, (byte)(t1DamageRoll == 100 ? 90 : 100), 100).HasValue) return false;

		return true;
	}

	private static bool CheckCheren1(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckSpeedTieRoll(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		// leer
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckSpeedTieRoll(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckN1(ulong battleRng)
	{
		// turn 1
		// scratch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		var t1DamageRoll = CheckDamageRoll(ref battleRng, 92, 100);
		if (!t1DamageRoll.HasValue) return false;

		// turn 2
		// scratch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, (byte)(t1DamageRoll == 100 ? 92 : 100), 100).HasValue) return false;

		return true;
	}

	private static bool CheckLillipupCatch(ulong battleRng)
	{
		if (!CheckPokeballShake(ref battleRng, 46266, true).HasValue) return false;
		if (!CheckPokeballShake(ref battleRng, 46266, true).HasValue) return false;
		if (!CheckPokeballShake(ref battleRng, 46266, true).HasValue) return false;

		return true;
	}

	private static bool CheckJimmy(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		return true;
	}

	private static bool CheckBianca2(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		return true;
	}

	private static bool CheckCheren2(ulong battleRng)
	{
		// turn 1
		// leer and tail whip
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 88, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckMaxwell(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, true).HasValue) return false;

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		return true;
	}

	private static bool CheckTia(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckChili(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		// turn 2
		// work up / leer
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;

		// turn 3
		// work up / tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt1(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt2(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckKumiAmy(ulong battleRng)
	{
		// note: double battle
		// turn 1
		if (!CheckSpeedTieRoll(ref battleRng, null).HasValue) return false; // purrloins speedtie
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		// scratch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 86, 100).HasValue) return false;
		// turn 2
		// there's still a speedtie check if there's only 1 purrloin?
		// except it doesn't have the odd 50/50 roll
		for (var i = 0; i < 2; i++) AdvanceBattleRng(ref battleRng);
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 91, 100).HasValue) return false;

		return true;
	}

	private static bool CheckCheren3(ulong battleRng)
	{
		// turn 1
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt3(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt4(ulong battleRng)
	{
		// note: multi-battle
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;
		// ember
		/*if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckBurn(ref battleRng, 10, false).HasValue) return false;
		// leer
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;*/

		// I have no idea what this is even supposed to be, this is seemingly speedtie checks
		// but that makes no sense, as nothing speedties, and the weird useless 50/50 roll isn't done
		for (var i = 0; i < 6; i++)
		{
			AdvanceBattleRng(ref battleRng);
		}

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		return true;
	}

	private static bool CheckN2(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 89, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;
		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 89, 100).HasValue) return false;
		// turn 3
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckCarter(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 98, 100).HasValue) return false;

		return true;
	}

	private static bool CheckSatomi(ulong battleRng)
	{
		// turn 1
		// satomi always uses x defend turn 1 (so no rng calls)
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckLydia(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;
		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;
		// turn 3
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckLenora(ulong battleRng)
	{
		// turn 1
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		// turn 2
		// hypnosis
		if (!CheckAccuracyRoll(ref battleRng, 60, false).HasValue) return false;
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt5(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt6(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckAudra(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt7(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt8(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt9(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckJack(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 98, 100).HasValue) return false;

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;
		if (!CheckPoisonPoint(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckLouis(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;

		return true;
	}

	private static bool CheckBurgh(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		var t1DamageRoll = CheckDamageRoll(ref battleRng, 95, 100);
		if (!t1DamageRoll.HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, true).HasValue) return false;

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, (byte)(t1DamageRoll == 100 ? 98 : 100), 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// bite
		// note: shell armor
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		var t3DamageRoll = CheckDamageRoll(ref battleRng, null, null);
		if (!CheckFlinchRoll(ref battleRng, 30, true).HasValue) return false;

		// turn 4
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		var t4DamageRoll = CheckDamageRoll(ref battleRng, null, null);
		if (!CheckFlinchRoll(ref battleRng, 30, true).HasValue) return false;

		// turn 5
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		var t5DamageRoll = CheckDamageRoll(ref battleRng, null, null);
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		static byte DamageRollToDamage(byte? damageRoll)
		{
			return damageRoll switch
			{
				85 or 86 or 87 or 88 or 89 => 16,
				90 or 91 or 92 or 93 or 94 => 17,
				95 or 96 or 97 or 98 or 99 => 18,
				100 => 19,
				_ => throw new InvalidOperationException()
			};
		}

		if (DamageRollToDamage(t3DamageRoll) +
		    DamageRollToDamage(t4DamageRoll) +
		    DamageRollToDamage(t5DamageRoll) < 54) return false;

		// turn 6
		// razor leaf
		if (!CheckAccuracyRoll(ref battleRng, 95, false).HasValue) return false;
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 96, 100).HasValue) return false;

		return true;
	}

	private static bool CheckBianca3(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 2
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 99, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckForewarn(ref battleRng, 1, null).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn	4
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckCheren4(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 4
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt10(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 92, 100).HasValue) return false;

		return true;
	}

	private static bool CheckN3(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 91, 100).HasValue) return false;

		// turn 3
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 4
		// tackle
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 90, 100).HasValue) return false;

		return true;
	}

	private static bool CheckMagnolia(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckStatic(ref battleRng, false).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckStatic(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckCody(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 94, 100).HasValue) return false;

		return true;
	}

	private static bool CheckRolan(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 87, 100).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 87, 100).HasValue) return false;

		return true;
	}

	private static bool CheckColette(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckStatic(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckElesa(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckStatic(ref battleRng, false).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckStatic(ref battleRng, false).HasValue) return false;

		// turn 3
		// crunch
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 92, 100).HasValue) return false;

		return true;
	}

	private static bool CheckCheren5(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 4
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckSarahBilly(ulong battleRng)
	{
		// turn 1
		// odd case, herdiers speedtie, but the quick claw is still in the middle of the calls...
		if (!CheckQuickClawSpeedTie(ref battleRng, null, true).HasValue) return false;
		// fake out
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 100, true).HasValue) return false;
		// bite
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		for (var i = 0; i < 6; i++)
		{
			AdvanceBattleRng(ref battleRng);
		}

		// turn 2
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt11(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt12(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt13(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 97, 100).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt14(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 96, 100).HasValue) return false;

		return true;
	}

	private static bool CheckFelix(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;

		return true;
	}

	private static bool CheckDon(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckKatie(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 93, 100).HasValue) return false;

		return true;
	}

	private static bool CheckClay(ulong battleRng)
	{
		// turn 1
		// take down
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 85, false).HasValue) return false;
		// swagger
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckConfusionTurnCountRoll(ref battleRng, 2, 2).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckConfusionSelfHit(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 93, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, false).HasValue) return false;

		// turn 4
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 99, 100).HasValue) return false;

		return true;
	}

	private static bool CheckBianca4(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 2
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 4
		// tackle
		if (!CheckForewarn(ref battleRng, 1, null).HasValue) return false;
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 87, 100).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt15(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckN4(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 91, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, true).HasValue) return false;

		// turn 2
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 89, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		// turn 4
		// take down
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 91, 100).HasValue) return false;

		// turn 5
		// take down
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckDoreen(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 88, 100).HasValue) return false;

		return true;
	}

	private static bool CheckCliff(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 87, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 2
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 87, 100).HasValue) return false;

		return true;
	}

	private static bool CheckTed(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 86, 100).HasValue) return false;

		return true;
	}

	private static bool CheckChase(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 93, 100).HasValue) return false;

		return true;
	}

	private static bool CheckArnold(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckSkyla(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// take down
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 91, 100).HasValue) return false;

		return true;
	}

	private static bool CheckCheren6(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 99, 100).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 96, 100).HasValue) return false;

		// turn 3
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 90, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 4
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckTerrell(ulong battleRng)
	{
		// turn 1
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 89, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, true).HasValue) return false;

		// turn 2
		// bite
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 90, 100).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 98, 100).HasValue) return false;

		return true;
	}

	private static bool CheckGrant(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 93, 100).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 91, 100).HasValue) return false;

		return true;
	}

	private static bool CheckMiriam(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClawSpeedTie(ref battleRng, true, true).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckKendrew(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		return true;
	}

	private static bool CheckMikiko(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 96, 100).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckChandra(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckBrycen(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// take down
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 4
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS1(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS2(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 86, 100).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS3(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS4(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS5(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 94, 100).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS6(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 90, 100).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 97, 100).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS7(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS8(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGruntDS9(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 91, 100).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt16(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt17(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 97, 100).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 97, 100).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt18(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckPlasmaGrunt19(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;

		return true;
	}

	private static bool CheckBianca5XSpec(ulong battleRng)
	{
		// turn 1
		// x spec / work up

		// turn 2
		// air slash
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 95, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// psychic
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 4
		// heart stamp
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlinchRoll(ref battleRng, 30, null).HasValue) return false;

		// turn 5
		// air slash
		if (!CheckForewarn(ref battleRng, 2, 0).HasValue) return false;
		if (!CheckQuickClaw(ref battleRng, false).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 95, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 86, 100).HasValue) return false;

		return true;
	}

	private static bool CheckBianca5(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;;

		// turn 3
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 4
		// crunch
		if (!CheckForewarn(ref battleRng, 1, 0).HasValue) return false;
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckWebster(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckOlwen(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 96, 100).HasValue) return false;

		return true;
	}

	private static bool CheckJose(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckClara(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckHugo(ulong battleRng)
	{
		// turn 1
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckTom(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckDara(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckKim(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckDrayden(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 99, 100).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 92, 100).HasValue) return false;

		return true;
	}

	private static bool CheckCheren7(ulong battleRng)
	{
		// turn 1
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		// turn 2
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// reversal
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 4
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckMarshal(ulong battleRng)
	{
		// turn 1
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;

		// turn 2
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 3
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 98, 100).HasValue) return false;

		// turn 4
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 85, 90).HasValue) return false;
		// stone edge
		if (!CheckAccuracyRoll(ref battleRng, 80, false).HasValue) return false;

		// turn 5
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;

		return true;
	}

	private static bool CheckCaitlin(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		// turn 3
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 4
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckGrimsley(ulong battleRng)
	{
		// turn 1
		// take down
		if (!CheckAccuracyRoll(ref battleRng, 85, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 100, 100).HasValue) return false;

		// turn 3
		// reversal
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 88, 100).HasValue) return false;

		// turn 4
		// tackle
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}

	private static bool CheckShauntal(ulong battleRng)
	{
		// turn 1
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 99, 100).HasValue) return false;

		// turn 3
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckFlameBody(ref battleRng, false).HasValue) return false;

		// turn 4
		// crunch
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 96, 100).HasValue) return false;
		if (!CheckCursedBody(ref battleRng, false).HasValue) return false;

		return true;
	}

	private static bool CheckReshiramCatch(ulong battleRng)
	{
		if (!CheckPokeballShake(ref battleRng, 32275, true).HasValue) return false;
		if (!CheckPokeballShake(ref battleRng, 32275, true).HasValue) return false;
		if (!CheckPokeballShake(ref battleRng, 32275, true).HasValue) return false;

		return true;
	}

	private static bool CheckN5(ulong battleRng)
	{
		// turn 1
		// fusion bolt
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 85, 86).HasValue) return false;
		// dragon breath
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckParalysis(ref battleRng, 30, null).HasValue) return false;

		// turn 2
		// dragon breath
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;
		if (!CheckParalysis(ref battleRng, 30, null).HasValue) return false;

		// turn 3
		// stone edge
		if (!CheckAccuracyRoll(ref battleRng, 80, false).HasValue) return false;
		// dragon breath
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;
		if (!CheckParalysis(ref battleRng, 30, null).HasValue) return false;

		// turn 4
		// fusion flare
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 5
		// fusion flare
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 6
		// focus blast
		if (!CheckAccuracyRoll(ref battleRng, 70, false).HasValue) return false;
		// fusion flare
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 95, 100).HasValue) return false;

		return true;
	}

	private static bool CheckGhetsis(ulong battleRng)
	{
		// turn 1
		// fusion flare
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 2
		// fusion flare
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 99, 100).HasValue) return false;

		// turn 3
		// extrasensory
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		var t3Crit = CheckCriticalHitRoll(ref battleRng, null)!.Value;
		var t3DamageRoll = CheckDamageRoll(ref battleRng, null, null);
		if (!CheckFlinchRoll(ref battleRng, 10, true).HasValue) return false;

		// turn 4
		// dragon breath
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, !t3Crit).HasValue) return false;

		byte t4MinDamageRoll;
		if (t3Crit)
		{
			if (t3DamageRoll < 90)
			{
				return false;
			}

			t4MinDamageRoll = t3DamageRoll switch
			{
				90 => 100,
				91 => 98,
				92 => 96,
				93 => 94,
				94 or 95 => 92,
				96 => 90,
				97 => 88,
				98 => 86,
				99 or 100 => 85,
				_ => throw new InvalidOperationException()
			};
		}
		else
		{
			t4MinDamageRoll = t3DamageRoll switch
			{
				85 or 86 or 87 => 92,
				88 or 89 => 91,
				90 or 91 or 92 => 90,
				93 => 89,
				94 or 95 or 96 => 88,
				97 or 98 => 87,
				99 or 100 => 86,
				_ => throw new InvalidOperationException()
			};
		}

		if (!CheckDamageRoll(ref battleRng, t4MinDamageRoll, 100).HasValue) return false;
		if (!CheckParalysis(ref battleRng, 30, null).HasValue) return false;

		// turn 5
		// fusion flare
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;

		// turn 6
		// dragon pulse
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, false).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, 85, 86).HasValue) return false;
		// dragon breath
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;
		if (!CheckDamageRoll(ref battleRng, null, null).HasValue) return false;
		if (!CheckParalysis(ref battleRng, 30, null).HasValue) return false;

		// turn 7
		// fusion flare
		if (!CheckAccuracyRoll(ref battleRng, 100, true).HasValue) return false;
		if (!CheckCriticalHitRoll(ref battleRng, true).HasValue) return false;

		return true;
	}
}
