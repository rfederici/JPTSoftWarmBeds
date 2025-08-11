using HarmonyLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

// Instructions to deal with the used bedding on despawn
[HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.DeSpawn))]
public class Building_Bed_DeSpawn
{
    public static void Prefix(object __instance)
    {
        if (__instance is not Building_Bed bed) return;

        var bedComp = bed.TryGetComp<CompMakeableBed>();
        if (bedComp is { Loaded: true, NotTheBlanket: true })
        {
            if (Odyssey_Patch.IsInGravShipTransfer)
            {
                Odyssey_Patch.AddPreservedBedMaking(bed, new Odyssey_Patch.BedMakingData
                {
                    BeddingDef = bedComp.LoadedBeddingDef,
                    StuffDef = bedComp.BlanketStuffDef
                });
                return;
            }
            
            // Normal despawn - unmake the bed
            bedComp.Unmake();
        }
    }
}