using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    public class CompProperties_MakeableBed : CompProperties
    {
        public CompProperties_MakeableBed()
        {
            compClass = typeof(CompMakeableBed);
        }
    
    public ThingDef blanketDef;

    public ThingDef beddingDef;
   
    }
}