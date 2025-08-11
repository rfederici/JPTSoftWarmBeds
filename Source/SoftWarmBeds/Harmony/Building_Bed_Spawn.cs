using HarmonyLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

// Handle bed respawning to restore bed makings after grav ship transfers
[HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.SpawnSetup))]
public class Building_Bed_Spawn
{
    public static void Postfix(object __instance)
    {
        if (__instance is not Building_Bed bed) return;

        if (Odyssey_Patch.OdysseyLoaded)
        {
            var bedComp = bed.TryGetComp<CompMakeableBed>();
            if (bedComp != null && Odyssey_Patch.TryGetPreservedBedMaking(bed, out var data))
            {
                bedComp.LoadBedding(data.BeddingDef, data.StuffDef);
                Odyssey_Patch.RemovePreservedBedMaking(bed);
            }
        }
    }
} 