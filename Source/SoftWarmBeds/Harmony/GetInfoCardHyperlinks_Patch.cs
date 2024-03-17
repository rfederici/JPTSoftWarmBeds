using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

//Adjusts the info report on comfortable temperatures - InfoCard
[HarmonyPatch(typeof(StatPart_GearStatOffset), "GetInfoCardHyperlinks")]
public class GetInfoCardHyperlinks_Patch
{
    public static IEnumerable<Dialog_InfoCard.Hyperlink> Postfix(IEnumerable<Dialog_InfoCard.Hyperlink> original,
        StatRequest req)
    {
        if (!req.HasThing || req.Thing == null)
        {
            yield break;
        }

        if (req.Thing is Pawn pawn && pawn.InBed())
        {
            yield return new Dialog_InfoCard.Hyperlink(pawn.CurrentBed());
        }
    }
}