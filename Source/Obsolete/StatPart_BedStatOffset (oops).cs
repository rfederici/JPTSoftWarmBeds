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
                //Pawn pawn = req.Thing as Pawn;
                //Thing insulation = pawn.CurrentBed();
                //if (pawn != null && insulation != null)
                //{
                //    float statValue = insulation.GetStatValue(this.stat, true);
                //    if (this.subtract)
                //    {
                //        val -= statValue;
                //    }
                //    else
                //    {
                //        val += statValue;
                //    }
                //}
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && pawn.CurrentBed() != null)
                {
                    float statValue = BedOffset(pawn);
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
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null)
                    if (this.subtract)
                    {
                        return "StatsReport_InBed".Translate() + ": -" +
                        this.BedOffset(pawn).ToStringTemperature();
                    }
                    else
                    {
                        return "StatsReport_InBed".Translate() + ": +" +
                        this.BedOffset(pawn).ToStringTemperature();
                    }
            }
            return (string)null;
        }

        private float BedOffset(Pawn pawn)
        {
            Thing insulation = null;
            CompMakeableBed BedComp = pawn.CurrentBed().TryGetComp<CompMakeableBed>();
            if (BedComp != null)
            {
                if (BedComp.Loaded)
                {
                    insulation = BedComp.bedding; // comp = stat from bedding
                }
                else
                {
                    return 0f; // unmade bed = zero
                }
            }
            else
            {
                if (pawn.CurrentBed().Stuff != null)
                {
                    insulation = pawn.CurrentBed(); ; // no comp (Bedroll)
                }
                else
                {
                    return 0f; // no comp, no stuff (SleepingSpot)
                }
            }

            float result;
            if (pawn.InBed())
            {
                result = insulation.GetStatValue(this.stat, true);
            }
            else
            {
                result = 0f;
            }
            return result;
        }
    }
}