//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    [DefOf]
    public class SoftWarmBeds_JobDefOf
    {
        public static JobDef MakeBed;
    }

    [DefOf]
    public static class BedInsulationCold
    {
        public static StatDef Bed_Insulation_Cold;

        static BedInsulationCold()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BedInsulationCold));
        }
    }

    [DefOf]
    public static class BedInsulationHeat
    {
        public static StatDef Bed_Insulation_Heat;

        static BedInsulationHeat()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BedInsulationHeat));
        }
    }

    [DefOf]
    public static class Softness
    {
        public static StatDef Textile_Softness;

        static Softness()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Softness));
        }
    }
}
