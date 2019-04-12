

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




using UnityEngine;

using Verse;
using Verse.AI;          // Needed when you do something with the AI


using RimWorld;
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

// Note: If the usings are not found, (red line under it,) look into the folder '/Source-DLLs' and follow the instructions in the text files

// Now the program starts:
namespace SoftWarmBeds
{

        public abstract class JobGiver_MakeBeds : ThinkNode_JobGiver
        {
            public override ThinkNode DeepCopy(bool resolve = true)
            {
                JobGiver_MakeBeds jobGiver_MakeBeds = (JobGiver_MakeBeds)base.DeepCopy(resolve);
                jobGiver_MakeBeds.maxDistFromPoint = this.maxDistFromPoint;
                return jobGiver_MakeBeds;
            }

            protected override Job TryGiveJob(Pawn pawn)
            {
                Log.Message("Tentando dar o servico");
                Predicate<Thing> validator = (Thing t) => t.def.HasComp(typeof(CompMakeableBed)) && pawn.CanReserve(t, 1, -1, null, false) && JobDriver_MakeBed.FindBeddingForBed(pawn, (Building_SoftWarmBed)t) != null;
                Thing thing = GenClosest.ClosestThingReachable(this.GetRoot(pawn), pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), this.maxDistFromPoint, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                if (thing != null)
                {
                    Log.Message("Dando o serviço");
                    return new Job(SoftWarmBeds_JobDefOf.MakeBed, thing)
                    {
                        expiryInterval = 2000,
                        checkOverrideOnExpire = true
                    };
                }
                return null;
            }
            [DefOf]
            public class SoftWarmBeds_JobDefOf
        {
            public static JobDef MakeBed;
            }
        
            protected abstract IntVec3 GetRoot(Pawn pawn);

            public float maxDistFromPoint = -1f;

        }
}
