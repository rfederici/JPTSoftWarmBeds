//using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace SoftWarmBeds
{
    public class WorkGiver_MakeBeds : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (Building_SoftWarmBed softWarmBed in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_SoftWarmBed>())
            {
                yield return softWarmBed as Thing;
            }
            yield break;
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public virtual JobDef JobStandard
        {
            get
            {
                return SoftWarmBeds_JobDefOf.MakeBed;
            }
        }

        public virtual bool CanMakeBedThing(Thing t)
        {
            return (t is Building_SoftWarmBed);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return this.CanMakeBedThing(t) && BedMakingWorkGiverUtility.CanMakeBed(pawn, t, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return BedMakingWorkGiverUtility.BedMakingJob(pawn, t, forced, this.JobStandard);
        }

    }
}
