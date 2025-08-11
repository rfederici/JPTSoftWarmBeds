using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

// Odyssey compatibility for grav ship bed transfers
[StaticConstructorOnStartup]
public static class Odyssey_Patch
{
    private static readonly Dictionary<Building_Bed, BedMakingData> PreservedBedMakings = new();
    private static bool isInGravShipTransfer = false;
    
    public static bool OdysseyLoaded { get; private set; }
    
    public static bool IsInGravShipTransfer => isInGravShipTransfer;
    
    public static void AddPreservedBedMaking(Building_Bed bed, BedMakingData data)
    {
        PreservedBedMakings[bed] = data;
    }
    
    public static bool TryGetPreservedBedMaking(Building_Bed bed, out BedMakingData data)
    {
        return PreservedBedMakings.TryGetValue(bed, out data);
    }
    
    public static void RemovePreservedBedMaking(Building_Bed bed)
    {
        PreservedBedMakings.Remove(bed);
    }
    
    static Odyssey_Patch()
    {
        // Check if Odyssey is loaded
        OdysseyLoaded = LoadedModManager.RunningModsListForReading.Any(x =>
            x.PackageIdPlayerFacing.StartsWith("Ludeon.RimWorld.Odyssey"));
        
        if (!OdysseyLoaded) return;

        var harmonyInstance = new Harmony("JPT_SoftWarmBeds.Odyssey");
        
        Log.Message("[SoftWarmBeds] Odyssey detected! Adapting for grav ship transfers...");
        
        // Patch all necessary methods for grav ship compatibility
        PatchGravShipMethods(harmonyInstance);
    }
    
    private static void PatchGravShipMethods(Harmony harmonyInstance)
    {
        // Patch the main launch method - Building_ShipComputerCore.ForceLaunch
        var shipComputerType = AccessTools.TypeByName("RimWorld.Building_ShipComputerCore");
        if (shipComputerType != null)
        {
            var forceLaunchMethod = AccessTools.Method(shipComputerType, "ForceLaunch");
            if (forceLaunchMethod != null)
            {
                harmonyInstance.Patch(forceLaunchMethod, 
                    new HarmonyMethod(typeof(Odyssey_Patch), nameof(StartGravShipTransfer)));

            }
        }
        
        // Patch the landing method - WorldComponent_GravshipController.InitiateLanding
        var gravshipControllerType = AccessTools.TypeByName("Verse.WorldComponent_GravshipController");
        if (gravshipControllerType != null)
        {
            var landingMethod = AccessTools.Method(gravshipControllerType, "InitiateLanding");
            if (landingMethod != null)
            {
                harmonyInstance.Patch(landingMethod, 
                    new HarmonyMethod(typeof(Odyssey_Patch), nameof(EndGravShipTransfer)));
            }
        }
        
        // Patch ritual launch methods
        var ritualTypes = new[]
        {
            "RimWorld.RitualBehaviorWorker_GravshipLaunch",
            "RimWorld.RitualObligationTargetWorker_GravshipLaunch",
            "RimWorld.RitualOutcomeEffectWorker_GravshipLaunch"
        };
        
        foreach (var typeName in ritualTypes)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type != null)
            {
                var ritualMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in ritualMethods)
                {
                    if (method.Name.Contains("TryStart") || method.Name.Contains("Start") || 
                        method.Name.Contains("Execute") || method.Name.Contains("Apply"))
                    {
                        try
                        {
                            // Only patch concrete methods, not abstract ones
                            if (!method.IsAbstract && method.MethodImplementationFlags == MethodImplAttributes.IL)
                            {
                                harmonyInstance.Patch(method, 
                                    new HarmonyMethod(typeof(Odyssey_Patch), nameof(StartGravShipTransfer)));
                                Log.Message($"[SoftWarmBeds] ✓ Patched {typeName}.{method.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning($"[SoftWarmBeds] Failed to patch {typeName}.{method.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
    
    public static void StartGravShipTransfer()
    {
        isInGravShipTransfer = true;
        Log.Message("[SoftWarmBeds] Starting grav ship transfer - preserving bed makings...");
        
        var maps = Find.Maps;
        foreach (var map in maps)
        {
            var beds = map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>();
            foreach (var bed in beds)
            {
                var bedComp = bed.TryGetComp<CompMakeableBed>();
                if (bedComp?.Loaded == true)
                {
                    PreservedBedMakings[bed] = new BedMakingData
                    {
                        BeddingDef = bedComp.LoadedBeddingDef,
                        StuffDef = bedComp.BlanketStuffDef
                    };
                }
            }
        }
        Log.Message("[SoftWarmBeds] Grav ship transfer started - bed makings preserved");
    }
    
    public static void EndGravShipTransfer()
    {
        var maps = Find.Maps;
        foreach (var map in maps)
        {
            var beds = map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>();
            foreach (var bed in beds)
            {
                var bedComp = bed.TryGetComp<CompMakeableBed>();
                if (bedComp != null && PreservedBedMakings.TryGetValue(bed, out var data))
                {
                    bedComp.LoadBedding(data.BeddingDef, data.StuffDef);
                    PreservedBedMakings.Remove(bed);
                }
            }
        }
        
        PreservedBedMakings.Clear();
        isInGravShipTransfer = false;
        Log.Message("[SoftWarmBeds] Grav ship transfer completed - bed makings restored");
    }
    
    public class BedMakingData
    {
        public ThingDef BeddingDef { get; set; }
        public ThingDef StuffDef { get; set; }
    }
} 