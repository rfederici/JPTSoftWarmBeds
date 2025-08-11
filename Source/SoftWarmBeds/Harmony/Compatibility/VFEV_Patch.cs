using HarmonyLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

//Prevents VFE - Vikings Beds from curing Hypotermia if they're not made
[StaticConstructorOnStartup]
public static class VFEV_Patch
{
    static VFEV_Patch()
    {
        if (!LoadedModManager.RunningModsListForReading.Any(x =>
                x.PackageIdPlayerFacing.StartsWith("OskarPotocki.VFE.Vikings")))
        {
            return;
        }

        var harmonyInstance = new Harmony("JPT_SoftWarmBeds.VFEV");

        Log.Message("[SoftWarmBeds] Vanilla Factions Expanded - Vikings detected! Adapting...");

        harmonyInstance.Patch(AccessTools.Method("VFEV.CompCureHypothermia:CompTickRare"),
            new HarmonyMethod(typeof(VFEV_Patch), nameof(Prefix)));
    }

    public static bool Prefix(object __instance)
    {
        if (__instance is not ThingComp compInstance)
        {
            return true;
        }

        if (compInstance.parent is not Building_Bed bed)
        {
            return true;
        }

        var bedComp = bed.TryGetComp<CompMakeableBed>();
        if (bedComp != null)
        {
            return !bedComp.Loaded;
        }

        return true;
    }
}