using HarmonyLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

//Preventing people without beds from using the default BedRestEffectiveness value (80%). Switching to the minimun value instead. 
[HarmonyPatch(typeof(Need_Rest), "TickResting")]
public class Need_Rest_Patch
{
    public static bool Prefix(float restEffectiveness, Pawn ___pawn)
    {
        if (!___pawn.RaceProps.Humanlike || ___pawn.CurrentBed() != null || ___pawn.Faction is not { IsPlayer: true } ||
            restEffectiveness != StatDefOf.BedRestEffectiveness.valueIfMissing)
        {
            return true;
        }

        ___pawn.needs.rest.TickResting(StatDefOf.BedRestEffectiveness.minValue);
        return false;
    }
}