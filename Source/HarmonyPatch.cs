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

    //instruction to draw pawn body when on a bed that's unmade
    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("RenderPawnAt")]
    public static class PawnOnBedRenderer_Patch
    {
        public static void Postfix(PawnRenderer __instance, Vector3 drawLoc)
        {
            //pawn = AccessTools.Property(type: typeof(Pawn), name: "pawn");
            //Pawn pawn = AccessTools.GetDeclaredFields(typeof(Pawn)).
            //Pawn pawn = AccessTools.FirstInner(typeof(Pawn),;
            //Pawn pawn = (Pawn)AccessTools.FieldRefAccess<PawnRenderer, Pawn>("pawn");
            Pawn pawn = AccessTools.DeclaredField(typeof(Pawn), "pawn");
            Building_SoftWarmBed softWarmBed = pawn.CurrentBed();
                //pawn.CurrentBed();
            if (softWarmBed != null)
            {
                CompMakeableBed BedComp = softWarmBed.TryGetComp<CompMakeableBed>();
                if (BedComp != null)
                {
                    if (!BedComp.Loaded)
                    {
                        __instance.RenderPawnAt(drawLoc, __instance.CurRotDrawMode, false);

                    }
                }
            }

        }

    }

}



