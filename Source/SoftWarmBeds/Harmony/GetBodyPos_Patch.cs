using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SoftWarmBeds;

//Instruction to draw pawn body when on a bed that's unmade (RW 1.3 only)
[HarmonyPatch(typeof(PawnRenderer), "GetBodyPos", new[] { typeof(Vector3), typeof(bool) },
    new[] { ArgumentType.Normal, ArgumentType.Out })]
public static class GetBodyPos_Patch
{
    public static void Postfix(Pawn ___pawn, ref bool showBody)
    {
        if (showBody)
        {
            return;
        }

        var softWarmBed = ___pawn.CurrentBed();
        var bedComp = softWarmBed.TryGetComp<CompMakeableBed>();
        showBody = ___pawn.RaceProps.Humanlike && bedComp is { Loaded: false };
    }
}