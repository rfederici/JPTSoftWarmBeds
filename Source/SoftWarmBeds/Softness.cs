using RimWorld;

namespace SoftWarmBeds;

[DefOf]
public static class Softness
{
    public static StatDef Textile_Softness;

    static Softness()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(Softness));
    }
}