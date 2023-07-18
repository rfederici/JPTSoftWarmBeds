using RimWorld;

namespace SoftWarmBeds;

[DefOf]
public static class BedInsulationHeat
{
    public static StatDef Bed_Insulation_Heat;

    static BedInsulationHeat()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(BedInsulationHeat));
    }
}