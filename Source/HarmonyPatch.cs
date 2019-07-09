using Harmony;
using RimWorld;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace SoftWarmBeds
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            //HarmonyInstance.DEBUG = true;
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("JPT_SoftWarmBeds");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //instruction to draw pawn body when on a bed that's unmade
    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("RenderPawnAt")]
    [HarmonyPatch(new Type[] { typeof(Vector3) })]
    public static class PawnOnBedRenderer_Patch
    {
        public static void Postfix(PawnRenderer __instance, Pawn ___pawn, Vector3 drawLoc)
        {
            //Log.Message("Postfixing RenderPawnAt");
            if (___pawn.RaceProps.Humanlike && ___pawn.CurrentBed() is Building_SoftWarmBed)
            //if (softWarmBed != null && ___pawn.RaceProps.Humanlike)
            {
                //Log.Message(___pawn + " in line to get body drawn");
                Building_SoftWarmBed softWarmBed = (Building_SoftWarmBed)___pawn.CurrentBed();
                CompMakeableBed BedComp = softWarmBed.TryGetComp<CompMakeableBed>();
                if (BedComp != null)
                {
                    if (!BedComp.Loaded)
                    {
                        // Thanks to @Zamu & @Mehni!
                        var rotDrawMode = (RotDrawMode)AccessTools.Property(typeof(PawnRenderer), "CurRotDrawMode").GetGetMethod(true).Invoke(__instance, new object[0]);
                        MethodInfo layingFacing = AccessTools.Method(type: typeof(PawnRenderer), name: "LayingFacing");
                        Rot4 rot = (Rot4)layingFacing.Invoke(__instance, new object[0]);
                        //Log.Message("layingFancing=" + rot);
                        float angle;
                        Vector3 rootLoc;
                        Rot4 rotation = softWarmBed.Rotation;
                        rotation.AsInt += 2;
                        angle = rotation.AsAngle;
                        AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)softWarmBed.def.altitudeLayer, 15);
                        Vector3 vector2 = ___pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
                        Vector3 vector3 = vector2;
                        vector3.y += 0.02734375f;
                        float d = -__instance.BaseHeadOffsetAt(Rot4.South).z;
                        Vector3 a = rotation.FacingCell.ToVector3();
                        rootLoc = vector2 + a * d;
                        rootLoc.y += 0.0078125f;
                        //Log.Message("Trying renderPawnInternal with:" + rootLoc + ", " + angle + ", " + rot + ", " + rot + ", " + rotDrawMode);
                        MethodInfo renderPawnInternal = AccessTools.Method(type: typeof(PawnRenderer), name: "RenderPawnInternal", parameters: new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool)});
                        renderPawnInternal.Invoke(__instance, new object[] { rootLoc, angle, true, rot, rot, rotDrawMode, false, false });
                    }
                }
            }
        }
    }

}



