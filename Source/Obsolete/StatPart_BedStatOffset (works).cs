using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
//using SoftWarmBeds;

namespace RimWorld
{
    public class StatPart_BedStatOffset : StatPart
    {
        private StatDef stat = (StatDef)null;
        private bool subtract = false;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
            Pawn thing = req.Thing as Pawn;
            if (thing != null && thing.CurrentBed() != null)
            {
                float statValue = thing.CurrentBed().GetStatValue(this.stat, true);
                if (this.subtract)
                {
                    val -= statValue;
                }
                else
                {
                    val += statValue;
                }
            }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing)
            {
                Pawn thing = req.Thing as Pawn;
                if (thing != null)
                    if (this.subtract)
                    {
                        return "StatsReport_InBed".Translate() + ": -" +
                               this.BedOffset(thing).ToStringTemperature();
                    }
                    else
                    {
                        return "StatsReport_InBed".Translate() + ": +" +
                               this.BedOffset(thing).ToStringTemperature();
                    }
            }
            return (string)null;
        }

        private float BedOffset(Pawn pawn)
        {
            if (pawn.InBed())
                return pawn.CurrentBed().GetStatValue(this.stat, true);
            else 
                return 0f;
        }
    }
}