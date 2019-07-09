using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    //Based on Gadjung's "RealBeds"
    public class StatPart_BedStatOffset : StatPart
    {

        private StatDef stat = (StatDef)null;
        private bool subtract = false;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                Thing insulation = pawn.CurrentBed();
                if (pawn != null && insulation != null)
                {
                    float statValue = insulation.GetStatValue(stat, true);
                    if (subtract)
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
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null)
                    if (subtract)
                    {
                        return "StatsReport_InBed".Translate() + ": -" +
                        BedOffset(pawn).ToStringTemperature();
                    }
                    else
                    {
                        return "StatsReport_InBed".Translate() + ": +" +
                        BedOffset(pawn).ToStringTemperature();
                    }
            }
            return (string)null;
        }

        private float BedOffset(Pawn pawn)
        {
            Thing insulation = pawn.CurrentBed();
            float result;
            if (pawn.InBed())
            {
                result = insulation.GetStatValue(stat, true);
            }
            else
            {
                result = 0f;
            }
            return result;
        }
    }
}