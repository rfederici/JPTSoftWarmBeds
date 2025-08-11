using HarmonyLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

//Command to update the blanket color when needed
[HarmonyPatch(typeof(Thing), nameof(Thing.Notify_ColorChanged))]
public class Thing_Notify_ColorChanged
{
    public static void Postfix(object __instance)
    {
        if (__instance is not Building_Bed bed)
        {
            return;
        }

        var bedComp = bed.TryGetComp<CompMakeableBed>();
        if (bedComp is { Loaded: true, BlanketDef: not null })
        {
            bedComp.Blanket.Notify_ColorChanged();
        }
    }
}