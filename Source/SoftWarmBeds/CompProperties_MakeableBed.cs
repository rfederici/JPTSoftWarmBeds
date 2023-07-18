using RimWorld;
using Verse;

namespace SoftWarmBeds;

public class CompProperties_MakeableBed : CompProperties_Flickable
{
    public ThingDef beddingDef;

    public ThingDef blanketDef;

    public CompProperties_MakeableBed()
    {
        compClass = typeof(CompMakeableBed);
        commandLabelKey = "CommandUnmakeBed";
        commandDescKey = "CommandUnmakeBedDesc";
    }
}