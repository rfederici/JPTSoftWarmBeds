using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

//Interface to Hospitality for seamless guest bed switching
[StaticConstructorOnStartup]
public static class Hospitality_Patch
{
    static Hospitality_Patch()
    {
        if (!LoadedModManager.RunningModsListForReading.Any(x =>
                x.PackageIdPlayerFacing.StartsWith("Orion.Hospitality")))
        {
            return;
        }

        var harmonyInstance = new Harmony("JPT_SoftWarmBeds.Hospitality");

        Log.Message("[SoftWarmBeds] Hospitality detected! Adapting...");

        harmonyInstance.Patch(
            AccessTools.Method("Hospitality.Building_GuestBed:Swap", [typeof(Building_Bed)]),
            new HarmonyMethod(typeof(Hospitality_Patch), nameof(SwapPatch)));

        harmonyInstance.Patch(AccessTools.Method("Hospitality.Building_GuestBed:GetInspectString"),
            null,
            new HarmonyMethod(typeof(Building_Bed_GetInspectString), nameof(Building_Bed_GetInspectString.Postfix)));
    }

    public static bool SwapPatch(object __instance, Building_Bed bed)
    {
        var bedComp = bed.TryGetComp<CompMakeableBed>();
        if (bedComp == null)
        {
            return true;
        }

        bedComp.NotTheBlanket = false;
        Swap(__instance, bed, bedComp.settings, bedComp);
        return false;
    }

    public static void Swap(object __instance, Building_Bed bed, StorageSettings settings, CompMakeableBed compMakeable)
    {
        //reflection info
        var guestBed = AccessTools.TypeByName("Hospitality.Building_GuestBed");
        var makeBedinfo = AccessTools.Method(guestBed, "MakeBed", [typeof(Building_Bed), typeof(string)]);
        //
        var newName = bed.GetType() == guestBed
            ? bed.def.defName.Split(["Guest"], StringSplitOptions.RemoveEmptyEntries)[0]
            : $"{bed.def.defName}Guest";

        //var compArt = bed.TryGetComp<CompArt>();
        //var art = compArt?.Active != null && compArt.taleRef != null ? new { authorName = compArt.authorNameInt, title = compArt.titleInt, taleRef = new TaleReference { tale = compArt.taleRef.tale, seed = compArt.taleRef.seed } } : null;
        //compArt?.taleRef?.tale?.Notify_NewlyUsed();
        // Thanks again to @Zamu for figuring out it was actually very simple!
        var newBed = (Building_Bed)makeBedinfo.Invoke(__instance, [bed, newName]);
        newBed.SetFactionDirect(bed.Faction);
        var spawnedBed = (Building_Bed)GenSpawn.Spawn(newBed, bed.Position, bed.Map, bed.Rotation);
        spawnedBed.HitPoints = bed.HitPoints;
        spawnedBed.ForPrisoners = bed.ForPrisoners;
        // This should be on Hospitality, Orion! 
        spawnedBed.AllComps.Clear();
        spawnedBed.AllComps.AddRange(bed.AllComps);
        foreach (var comp in spawnedBed.AllComps)
        {
            comp.parent = spawnedBed;
            if ( /*art != null && */comp is CompArt newArt)
            {
                Log.Message($"New art for bed: {newArt.Title}");
                //newArt.authorNameInt = art.authorName;
                //newArt.titleInt = art.title;
                //newArt.taleRef = art.taleRef;
            }
        }

        compMakeable.parent.Notify_ColorChanged();
        spawnedBed.StyleDef = bed.StyleDef;
        Find.Selector.Select(spawnedBed, false);
    }
}