//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    public class CompProperties_MakeableBed : CompProperties
    {
        public CompProperties_MakeableBed()
        {
            this.compClass = typeof(CompMakeableBed);
        }
    
    public ThingDef blanketDef;

    public ThingDef beddingDef;
   
    }
}