using System;
using Verse;

namespace SoftWarmBeds
{
    public class SpecialThingFilterWorker_SingleBedding : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return this.AlwaysMatches(t.def);
        }

        public override bool AlwaysMatches(ThingDef def)
        {
            return def.defName == "SingleBedding";
        }

        public override bool CanEverMatch(ThingDef def)
        {
            return this.AlwaysMatches(def);
        }
    }

    public class SpecialThingFilterWorker_DoubleBedding : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            return this.AlwaysMatches(t.def);
        }

        public override bool AlwaysMatches(ThingDef def)
        {
            return def.defName == "DoubleBedding";
        }

        public override bool CanEverMatch(ThingDef def)
        {
            return this.AlwaysMatches(def);
        }
    }
}