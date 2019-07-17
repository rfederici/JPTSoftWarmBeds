using Harmony;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SoftWarmBeds
{
    [StaticConstructorOnStartup]
    //public static class HarmonyPatches
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            
            try
            {
                ((Action)(() =>
                {

                    if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Hospitality"))
                    {
                        HarmonyInstance harmonyInstance = HarmonyInstance.Create("JPT_SoftWarmBeds.Hospitality");

                        Log.Message("[SoftWarmBeds] Hospitality detected! Adapting...");

                        harmonyInstance.Patch(original: AccessTools.Method(type: typeof(Building_Bed), name: "GetGizmos"),
                            prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Building_Bed_GetGizmos_Postfix)), transpiler: null);

                        //harmonyInstance.Patch(original: AccessTools.Method(type: typeof(Building_Bed), name: "ForPrisoners"),
                        //    prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Building_Bed_ForPrisoners_Postfix)), transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName(name: "Hospitality.Harmony.Building_Bed_Patch+GetGizmos"), name: "Postfix"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(Building_Bed_GetGizmos_Postfix_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.Harmony.Building_Bed_Patch+ForPrisoners"), name: "Postfix"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(Building_Bed_ForPrisoners_Postfix_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.Hospitality_SpecialInjector"), name: "CreateGuestBedDefs"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(SpecialInjector_CreateGuestBedDefs_Prefix)), postfix: null, transpiler: null);

                    }

                }))();
            }
            catch (TypeLoadException ex) { }

        }

        //replaces Hospitality Gizmo
        public static void Building_Bed_GetGizmos_Postfix(Building_Bed __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = Process(__instance, __result);
        }

        private static IEnumerable<Gizmo> Process(Building_Bed __instance, IEnumerable<Gizmo> __result)
        {
            //Log.Message("Patching Building_Bed_GetGizmos_Prefix");
            if (!__instance.ForPrisoners && !__instance.Medical && __instance.def.building.bed_humanlike)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandBedSetAsGuestLabel".Translate(),
                    defaultDesc = "CommandBedSetAsGuestDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/AsGuest"),
                    isActive = () => false,
                    toggleAction = () => Building_SoftWarmGuestBed.Swap(__instance),
                    hotKey = KeyBindingDefOf.Misc4
                };
            }
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }
        }

        //Cancels Hospitality Gizmo postfix:
        public static bool Building_Bed_GetGizmos_Postfix_Prefix(Building_Bed __instance)
        {
            return false;
        }
          
        public static void Building_Bed_ForPrisoners_Postfix(Building_Bed __instance)
        {
            if (!__instance.ForPrisoners) return;

            if (__instance is Building_SoftWarmGuestBed)
            {
                Building_SoftWarmGuestBed.Swap(__instance);
            }
        }

        public static bool Building_Bed_ForPrisoners_Postfix_Prefix(Building_Bed __instance)
        {
            return false;
        }

        public static bool SpecialInjector_CreateGuestBedDefs_Prefix(ThingDef[] bedDefs, CompProperties_Facility[] facilities, object __instance)
        {
            StringBuilder stringBuilder = new StringBuilder("Created (soft & warm) guest beds for the following beds: ");
            FieldInfo[] fields = typeof(ThingDef).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (ThingDef thingDef in bedDefs)
            {
                ThingDef thingDef2 = new ThingDef();
                foreach (FieldInfo fieldInfo in fields)
                {
                    fieldInfo.SetValue(thingDef2, fieldInfo.GetValue(thingDef));
                }
                MethodInfo copyComps = AccessTools.Method("Hospitality_SpecialInjector:CopyComps", new[] { typeof(ThingDef), typeof(ThingDef) });
                copyComps.Invoke(__instance, new object[] { thingDef2, thingDef });
                ThingDef thingDef3 = thingDef2;
                thingDef3.defName += "Guest";
                thingDef2.label = "GuestBedFormat".Translate(thingDef2.label);
                thingDef2.thingClass = typeof(Building_SoftWarmGuestBed);
                thingDef2.shortHash = 0;
                thingDef2.minifiedDef = null;
                thingDef2.tradeability = Tradeability.None;
                thingDef2.scatterableOnMapGen = false;
                typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[]
                {
            thingDef2,
            typeof(ThingDef)
                });
                DefDatabase<ThingDef>.Add(thingDef2);
                stringBuilder.Append(thingDef.defName + ", ");
                foreach (CompProperties_Facility compProperties_Facility in facilities)
                {
                    compProperties_Facility.linkableBuildings.Add(thingDef2);
                }
            }
            Log.Message(stringBuilder.ToString().TrimEnd(new char[] { ' ', ',' }), false);
            return false;
        }

    }

}



