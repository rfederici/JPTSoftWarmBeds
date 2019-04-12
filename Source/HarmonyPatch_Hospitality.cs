using Harmony;
using RimWorld;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace SoftWarmBeds
{
    //[StaticConstructorOnStartup]
    //internal static class HarmonyInit
    //{
    //    static HarmonyInit()
    //    {
    //        HarmonyInstance.DEBUG = true;
    //    }
    //}

    //replaces Hospitality Gizmo
    [HarmonyPatch(typeof(Building_Bed))]
    [HarmonyPatch("GetGizmos")]
    [HarmonyAfter(new string[] { "HugsLib.Hospitality" })]
    public static class Hospitality_Building_Bed_Patch_Patch_GetGizmos
    {
        public static void Postfix(Building_SoftWarmBed __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = Process(__instance, __result);
        }

        private static IEnumerable<Gizmo> Process(Building_SoftWarmBed __instance, IEnumerable<Gizmo> __result)
        {
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
    }

    //Cancels Hospitality Gizmo postfix:
    [HarmonyPatch(typeof(Hospitality.Harmony.Building_Bed_Patch.GetGizmos))]
    [HarmonyPatch("Postfix")]
    class Postfix_Prefix
    {
        public static bool Prefix(Building_SoftWarmBed __instance)
        {
            //Log.Message("canceling Hospitality postfix");
            return false;
        }
    }

    //SOMETHING HAS CHANGED ON HARMONY, SO THIS DOESN'T WORK
    //[HarmonyPatch(typeof(Building_Bed))]
    //[HarmonyPatch("ForPrisoners", 2) ]
    //[HarmonyAfter(new string[] { "HugsLib.Hospitality" })]
    //public static class Hospitality_Building_Bed_Patch_Patch_ForPrisioners
    //{
    //    public static void Postfix(Building_SoftWarmBed __instance)
    //    {
    //        if (__instance.ForPrisoners)
    //        {
    //            if (__instance is Building_SoftWarmGuestBed)
    //            {
    //                Building_SoftWarmGuestBed.Swap(__instance);
    //            }
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(Hospitality.Hospitality_SpecialInjector))]
    [HarmonyPatch("Inject")]
    public static class Hospitality_SpecialInjector_Inject_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Log.Message("Transpiling Inject");
            Type bedInfo = AccessTools.TypeByName("Building_Bed");

            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            foreach (CodeInstruction instruction in codeInstructions)
            {
                //Log.Message("procurando linha " + OpCodes.Ldtoken + " " + bedInfo);
                if (instruction.opcode == OpCodes.Ldtoken && instruction.operand == bedInfo)
                {
                    //Log.Message("instruction found, patching " + bedInfo + " into " + AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmBed"));
                    yield return new CodeInstruction(opcode: OpCodes.Ldtoken, operand: AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmBed"));

                }
                else
                {
                    yield return instruction;
                }

            }
        }
    }

    [HarmonyPatch(typeof(Hospitality.Hospitality_SpecialInjector))]
    [HarmonyPatch("CreateGuestBedDefs")]
    public static class Hospitality_SpecialInjector_CreateGuestBedDefs_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Log.Message("Transpiling CreateGuestBedDefs");
            Type bedInfo = AccessTools.TypeByName("Hospitality.Building_GuestBed");

            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            foreach (CodeInstruction instruction in codeInstructions)
            {
                if (instruction.opcode == OpCodes.Ldtoken && instruction.operand == bedInfo)
                {
                    //Log.Message("instruction found, patching " + bedInfo + " into " + AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmGuestBed"));
                    yield return new CodeInstruction(opcode: OpCodes.Ldtoken, operand: AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmGuestBed"));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}



