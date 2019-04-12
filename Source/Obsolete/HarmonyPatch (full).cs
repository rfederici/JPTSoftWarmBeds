using Harmony;
using RimWorld;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using Hospitality;

namespace SoftWarmBeds
{
    //[StaticConstructorOnStartup]
    //internal static class HarmonyInit
    //{
    //    static HarmonyInit()
    //    {
    //        HarmonyInstance.DEBUG = true;
    //        HarmonyInstance harmonyInstance = HarmonyInstance.Create("JPT_SoftWarmBeds");
    //        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
    //    }
    //}

    //    //trying prefix trick
    //    [HarmonyPatch(typeof(Hospitality.Hospitality_SpecialInjector))]
    //    [HarmonyPatch("Inject")]
    //    class Inject_Prefix
    //    {
    //        public static bool Prefix(ClassName __instance)
    //        {
    //            this.InjectTab(typeof(RimWorld.ITab_Pawn_Guest), delegate (ThingDef def)
    //            {
    //                RaceProperties race = def.race;
    //                return race != null && race.Humanlike;
    //            });

    //            this.InjectComp(typeof(CompProperties_Guest), delegate (ThingDef def)
    //            {
    //                RaceProperties race = def.race;
    //                return race != null && race.Humanlike;
    //            });
    //            Type bed = typeof(Building_SoftWarmBed);
    //            ThingDef[] bedDefs = (from def in DefDatabase<ThingDef>.AllDefsListForReading
    //                                  where bed.IsAssignableFrom(def.thingClass) && def.building.bed_humanlike
    //                                  select def).ToArray<ThingDef>();
    //            CompProperties_Facility[] facilities = this.GetFacilitiesFor(bedDefs).ToArray<CompProperties_Facility>();
    //            this.CreateGuestBedDefs(bedDefs, facilities); return false;
    //        }
    //    }
    //}

    //trying Transpiler "alien races style"
    [HarmonyPatch(typeof(Hospitality.Hospitality_SpecialInjector))]
    [HarmonyPatch("Inject")]
    public static class Hospitality_SpecialInjector_Inject_Patch
    {
        //private static readonly Type patchType = typeof(Hospitality_SpecialInjector_Inject_Patch);

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Message("Transpiling Inject");
            //MethodInfo bedInfo = AccessTools.Method(type: typeof(Building_Bed), name: nameof(Building_Bed));
            Type bedInfo = AccessTools.TypeByName("Building_Bed");
            
            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            foreach (CodeInstruction instruction in codeInstructions)
            {
                //Log.Message("procurando linha " + OpCodes.Ldtoken + " " + bedInfo);
                if (instruction.opcode == OpCodes.Ldtoken && instruction.operand == bedInfo)
                {
                    //Log.Message("instruction found, patching " + bedInfo + " into " + AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmBed"));
                    //yield return new CodeInstruction(opcode: OpCodes.Ldtoken, operand: AccessTools.Method(type: typeof(SoftWarmBeds.Building_SoftWarmBed), name: nameof(SoftWarmBeds.Building_SoftWarmBed)));//(type: patchType, name: nameof(BedTweak)));
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
            Log.Message("Transpiling CreateGuestBedDefs");
            Type bedInfo = AccessTools.TypeByName("Hospitality.Building_GuestBed");

            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            foreach (CodeInstruction instruction in codeInstructions)
            {
                if (instruction.opcode == OpCodes.Ldtoken && instruction.operand == bedInfo)
                {
                    Log.Message("instruction found, patching " + bedInfo + " into " + AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmGuestBed"));
                    yield return new CodeInstruction(opcode: OpCodes.Ldtoken, operand: AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmGuestBed"));
                }
                else
                {
                    yield return instruction;
                }

            }
        }
    }

    [HarmonyPatch(typeof(Building_Bed))]
    [HarmonyPatch("GetGizmos")]
    //[HarmonyAfter(new string[] { "HugsLib.Hospitality" })]
    public static class Hospitality_Building_Bed_Patch_Patch
    {
         
        [HarmonyPostfix]
        public static void Postfix(Building_Bed __instance, ref IEnumerable<Gizmo> __result)
        {
            Log.Message("Postfixing Building_Bed");
            __result = SoftWarmProcess(__instance, __result);
        }

        private static IEnumerable<Gizmo> SoftWarmProcess(Building_Bed __instance, IEnumerable<Gizmo> __result)
        {
            if (!__instance.ForPrisoners && !__instance.Medical && __instance.def.building.bed_humanlike)
            {
                yield return
                    new Command_Toggle
                    {
                        defaultLabel = "JP esteve aqui",//"CommandBedSetAsGuestLabel".Translate(),
                        defaultDesc = "JP esteve aqui",//"CommandBedSetAsGuestDesc".Translate(),
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
}



