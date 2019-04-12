using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    public class StatPart_BedStuff : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing)
			{
                CompMakeableBed BedComp = req.Thing.TryGetComp<CompMakeableBed>();
                ThingDef BedStuff = BedComp.blanketStuff;
                StringBuilder stringBuilder = new StringBuilder();
                if (BedComp.Loaded == true)
                {
                    string text = (BedStuff == null) ? "None".Translate() : BedStuff.LabelCap;
                    string text2 = (BedStuff == null) ? "0" : BedStuff.GetStatValueAbstract(this.additiveStat, null).ToStringByStyle(this.parentStat.ToStringStyleUnfinalized, ToStringNumberSense.Absolute);
                    stringBuilder.AppendLine(string.Concat(new string[]
                    {
                        "StatsReport_Material".Translate(),
                        " (",
                        text,
                        "): ",
                        text2
                    }));
                    if (this.multiplierStat != null)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("StatsReport_StuffEffectMultiplier".Translate() + ": x" + this.GetMultiplier(req).ToStringPercent("F0"));
                    }
                }
                return stringBuilder.ToString().TrimEndNewlines();
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float value)
        {
			if (req.HasThing)
			{
                CompMakeableBed BedComp = req.Thing.TryGetComp<CompMakeableBed>();
                ThingDef BedStuff = BedComp.blanketStuff;
                if (!BedComp.Loaded)
                {
                    return;
                }
                float num = (BedStuff == null) ? 0f : BedStuff.GetStatValueAbstract(this.additiveStat, null);
                if (this.multiplierStat != null)
                {
                    value += this.GetMultiplier(req) * num;
                    return;
                }
                value += num;  
            }
        }

        private float GetMultiplier(StatRequest req)
        {
            if (req.HasThing)
            {
                return req.Thing.GetStatValue(this.multiplierStat, true);
            }
            return req.Def.GetStatValueAbstract(this.multiplierStat, null);
        }

        public StatDef additiveStat;

        public StatDef multiplierStat = null;

    }
}