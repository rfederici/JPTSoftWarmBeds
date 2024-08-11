using HarmonyLib;
using Verse;

namespace SoftWarmBeds;

[HarmonyPatch(typeof(PawnRenderNodeWorker_Body), nameof(PawnRenderNodeWorker_Body.CanDrawNow))]
public static class PawnRenderNodeWorker_Body_CanDrawNow
{
    public static void Postfix(PawnDrawParms parms, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        var bedComp = parms.bed?.TryGetComp<CompMakeableBed>();
        __result = parms.pawn.RaceProps.Humanlike && bedComp is { Loaded: false };
    }
}