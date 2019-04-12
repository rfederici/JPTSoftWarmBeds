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
        public override void TransformValue(StatRequest req, ref float value)
        {
            if (req.HasThing)
            {
                CompMakeableBed BedComp = req.Thing.TryGetComp<CompMakeableBed>();
                if (BedComp.Loaded)
                {
                    ThingDef BedStuff = BedComp.blanketStuff;
                    //if (multiplierStat != null)
                    //{
                    //    factor = SelectValue(req, multiplierStat);
                    //    i++;
                    //}
                    //if (additiveStat != null)
                    //{
                    //    addend = SelectValue(req, additiveStat);
                    //    i++
                    //}
                    float factor = (multiplierStat != null) ? SelectValue(req, multiplierStat) : 0f;
                    float addend = (additiveStat != null) ? SelectValue(req, additiveStat) : 0f;
                    Log.Message("defined: factor: " + factor + " addend " + addend);
                    if (additiveStat != null)
                    {
                        if (multiplierStat != null)
                        {
                            value += factor * addend;
                            goto Done;
                        };
                        value += addend;
                        goto Done;
                    }
                    value *= factor;

                    Done:
                    Factor = factor;
                    Addend = addend;
                    //factor = 0f;
                    //addend = 0f;
                    return;
                }
                return;
            }
            return;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing)
            {
                CompMakeableBed BedComp = req.Thing.TryGetComp<CompMakeableBed>();
                if (BedComp.Loaded)
                {
                    ThingDef BedStuff = BedComp.blanketStuff;
                    StringBuilder stringBuilder = new StringBuilder();
                    if (additiveStat != null)
                    {
                        string text = BedStuff.LabelCap;
                        string number = this.Addend.ToStringByStyle(this.parentStat.ToStringStyleUnfinalized, ToStringNumberSense.Absolute); //(additiveStat != null) ? "0" : 
                        stringBuilder.AppendLine(string.Concat(new string[]
                        {
                        "StatsReport_Material".Translate(),
                        " (",
                        text,
                        "): ",
                        number
                        }));
                        if (multiplierStat != null)
                        {
                            stringBuilder.AppendLine("StatsReport_StuffEffectMultiplier".Translate() + ": x" + number);
                        }
                    }
                    string textB = BedStuff.label;
                    stringBuilder.AppendLine("StatsReport_StuffEffectMultiplier".Translate() + " (" + textB + "): x" + this.Factor.ToStringPercent("F0"));
                    return stringBuilder.ToString().TrimEndNewlines();
                }
                return null;
            }
            return null;
        }

        //private float GetMultiplier(StatRequest req)
        //{
        //    if (req.HasThing && multiplierStat !=null)
        //    {
        //        CompMakeableBed BedComp = req.Thing.TryGetComp<CompMakeableBed>();
        //        ThingDef BedStuff = BedComp.blanketStuff;
        //        if (BedStuff.StatBaseDefined(multiplierStat))
        //        {
        //            return BedStuff.GetStatValueAbstract(this.multiplierStat, null);
        //        }
        //        return req.Thing.GetStatValue(this.multiplierStat, true);
        //    }
        //    //return req.Def.GetStatValueAbstract(this.multiplierStat, null);
        //    return 0f;
        //}

        //private float GetAdditive(StatRequest req)
        //{
        //    if (req.HasThing && additiveStat != null)
        //    {
        //        CompMakeableBed BedComp = req.Thing.TryGetComp<CompMakeableBed>();
        //        ThingDef BedStuff = BedComp.blanketStuff;
        //        if (BedStuff.StatBaseDefined(additiveStat))
        //        {
        //            return BedStuff.GetStatValueAbstract(this.additiveStat, null);
        //        }
        //        return req.Thing.GetStatValue(this.additiveStat, true);
        //    }
        //    //return req.Def.GetStatValueAbstract(this.additiveStat, null);
        //    return 0f;
        //}

        private float SelectValue(StatRequest req, StatDef stat)
        {
            if (stat != null)
            {
                CompMakeableBed BedComp = req.Thing.TryGetComp<CompMakeableBed>();
                ThingDef BedStuff = BedComp.blanketStuff;
                if (Both)
                {
                    if (stat == additiveStat)
                    {
                        return BedStuff.GetStatValueAbstract(stat, null);
                    }
                    //return req.Thing.GetStatValue(stat, true);
                    return req.Def.GetStatValueAbstract(stat, null);
                }
                return BedStuff.GetStatValueAbstract(stat, null);
            }
            Log.Message(stat + " tried selecting without target");
            return 0f;
        }

        private bool Both
        {
            get
            {
                return additiveStat != null && multiplierStat != null;
            }
            
        }

        private float Factor;

        private float Addend;
        
        private StatDef additiveStat = null;

        private StatDef multiplierStat = null;

    }
}