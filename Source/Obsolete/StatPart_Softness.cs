using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    public class StatPart_Softness : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (req.HasThing)
            {
                ThingDef stuff = req.Thing.Stuff;
                string text = (stuff == null) ? "None".Translate() : stuff.LabelCap;
                string text2 = (stuff == null) ? "0" : this.Softness(req).ToStringPercent("F0");

                stringBuilder.AppendLine(string.Concat(new string[]
                {
                    "StatsReport_Material".Translate(),
                    " (",
                    text,
                    "): ",
                    text2
                }));
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float value)
        {
            if (req.HasThing)
            {
                float num = value * this.Softness(req) - value;
                value += num;
            }
        }

        public float Softness(StatRequest req)
        {
            return 1 - (armorGrade(this.GetBedStuff(req)) / ((1 + (furFactor(this.GetBedStuff(req)) * 2) + (valueFactor(this.GetBedStuff(req)) / 2)) / 2));
        }

        private float armorGrade(ThingDef x)
        {
            float blunt;
            float sharp;
            //if (x.HasThing)
            //{
            //    blunt = x.Thing.GetStatValue(this.stuffPowerArmorBlunt, true);
            //    sharp = x.Thing.GetStatValue(this.stuffPowerArmorSharp, true);
            //}
            //else
            //{
                blunt = x.GetStatValueAbstract(this.stuffPowerArmorBlunt, null);
                sharp = x.GetStatValueAbstract(this.stuffPowerArmorSharp, null);
            //}
            return (blunt + sharp) / 2;
        }

        private float furFactor(ThingDef x)
        {
            float heat;
            float cold;
            //if (x.HasThing)
            //{
            //    heat = x.Thing.GetStatValue(this.stuffPowerStatHeat, true);
            //    cold = x.Thing.GetStatValue(this.stuffPowerStatCold, true);
            //}
            //else
            //{
                heat = x.GetStatValueAbstract(this.stuffPowerStatHeat, null);
                cold = x.GetStatValueAbstract(this.stuffPowerStatCold, null);
            //}
            float delta = Math.Abs(heat - cold);
            float reach = ((heat + cold) / 2) - 12;
            return Math.Max(delta, reach) / 10;

        }

        private float valueFactor(ThingDef x)
        {
            float val;
            //if (x.HasThing)
            //{
            //    val = x.Thing.GetStatValue(this.stuffMarketValue, true);
            //}
            //else
            //{
                val = x.GetStatValueAbstract(this.stuffMarketValue, null);
            //}
            return (float)Math.Pow(val, (1 / 3));
        }

        private ThingDef GetBedStuff(StatRequest req)
        {
            CompMakeableBed BedComp = req.Thing.TryGetComp<CompMakeableBed>();
            return BedComp.blanketStuff;
        }

        public StatDef stuffPowerStatHeat;
        public StatDef stuffPowerStatCold;
        public StatDef stuffPowerArmorSharp;
        public StatDef stuffPowerArmorBlunt;
        public StatDef stuffMarketValue;

    }
}