using RimWorld;

namespace SoftWarmBeds;

[DefOf]
public static class BedStatDefOf
{
    public static StatDef Bed_Insulation_Heat;
    public static StatDef Bed_Insulation_Cold;
    public static StatDef Textile_Softness;

    static BedStatDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(BedStatDefOf));
    }
}