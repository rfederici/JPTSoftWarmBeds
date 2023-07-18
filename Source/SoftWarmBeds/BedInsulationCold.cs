using RimWorld;

namespace SoftWarmBeds;

[DefOf]
public static class BedInsulationCold
{
    public static StatDef Bed_Insulation_Cold;

    static BedInsulationCold()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(BedInsulationCold));
    }
}