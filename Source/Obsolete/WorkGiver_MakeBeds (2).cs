

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




using UnityEngine;

using Verse;
using Verse.AI;            // Needed when you do something with the AI


using RimWorld;
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

// Note: If the usings are not found, (red line under it,) look into the folder '/Source-DLLs' and follow the instructions in the text files

// Now the program starts:
namespace SoftWarmBeds
{
    public class WorkGiver_MakeBeds : WorkGiver_Scanner
    {          

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDefOf.Bed);
            }
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

        //public virtual JobDef JobAtomic
        //{
        //    get
        //    {
        //        return JobDefOf.RefuelAtomic;
        //    }
        //}

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
