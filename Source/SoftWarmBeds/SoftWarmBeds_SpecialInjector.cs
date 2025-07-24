using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace SoftWarmBeds;

public sealed class SoftWarmBeds_SpecialInjector
{
    private readonly double adjust = 0.25;

    private float calculateSoftness(ThingDef def)
    {
        return 1 - (armorGrade(def) / ((1 + (furFactor(def) * 2) + (valueFactor(def) / 2)) / 2)) - (float)adjust;
    }

    public void Inject()
    {
        var texDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x =>
            x.stuffProps != null && (x.stuffProps.categories.Contains(StuffCategoryDefOf.Leathery) ||
                                     x.stuffProps.categories.Contains(StuffCategoryDefOf.Fabric)));
        injectStatBase(texDefs);
    }

    private static float armorGrade(ThingDef def)
    {
        var blunt = Math.Min(1, def.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Blunt, 0f));
        var sharp = Math.Min(1, def.statBases.GetStatValueFromList(StatDefOf.StuffPower_Armor_Sharp, 0f));
        return (blunt + sharp) / 2;
    }

    private static float furFactor(ThingDef def)
    {
        var heat = def.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Heat, 0f);
        var cold = def.statBases.GetStatValueFromList(StatDefOf.StuffPower_Insulation_Cold, 0f);
        var delta = Math.Abs(heat - cold);
        var reach = ((heat + cold) / 2) - 12;
        return Math.Max(delta, reach) / 10;
    }

    private static float valueFactor(ThingDef def)
    {
        var val = def.statBases.GetStatValueFromList(StatDefOf.MarketValue, 0f);
        return (float)Math.Pow(val, 1.0 / 3.0);
    }

    private void injectStatBase(IEnumerable<ThingDef> list)
    {
        var stringBuilder = new StringBuilder("[SoftWarmBeds] Added softness stat to: ");
        foreach (var thingDef in list)
        {
            var statModifier = new StatModifier
            {
                stat = BedStatDefOf.Textile_Softness,
                value = calculateSoftness(thingDef)
            };
            thingDef.statBases.Add(statModifier);
            stringBuilder.Append($"{thingDef.defName} ({statModifier.value.ToStringPercent()}), ");
        }

        Log.Message(stringBuilder.ToString().TrimEnd(' ', ','));
    }
}