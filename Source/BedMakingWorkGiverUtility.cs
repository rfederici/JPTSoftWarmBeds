using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace SoftWarmBeds
{
    public static class BedMakingWorkGiverUtility
    {
        public static bool CanMakeBed(Pawn pawn, Thing t, bool forced = false)
        {
            CompMakeableBed CompMakeableBed = t.TryGetComp<CompMakeableBed>();
            if (CompMakeableBed == null || CompMakeableBed.Loaded)
            {
                return false;
            }
            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, -1, null, forced))
                {
                    if (t.Faction != pawn.Faction)
                    {
                        return false;
                    }
                    if (BedMakingWorkGiverUtility.FindBestBedding(pawn, t) == null)
                    {
                        ThingFilter beddingFilter = new ThingFilter();
                        beddingFilter.SetAllow(t.TryGetComp<CompMakeableBed>().allowedBedding, true);
                        JobFailReason.Is("NoSuitableBedding".Translate(beddingFilter.Summary), null);
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public static Job BedMakingJob(Pawn pawn, Thing t, bool forced = false, JobDef customJob = null)
        {
            Thing t2 = BedMakingWorkGiverUtility.FindBestBedding(pawn, t);
            return new Job(customJob ?? SoftWarmBeds_JobDefOf.MakeBed, t, t2);
        }

        private static Thing FindBestBedding(Pawn pawn, Thing bed)
        {
            //Log.Message(pawn + " is looking for a bedding type " + bed.TryGetComp<CompMakeableBed>().blanketDef + " for " + bed);
            ThingFilter beddingFilter = new ThingFilter();
            beddingFilter.SetAllow(bed.TryGetComp<CompMakeableBed>().allowedBedding, true);
            ThingFilter stuffFilter = new ThingFilter();
            if (bed is Building_SoftWarmBed)
            {
                Building_SoftWarmBed softWarmBed = bed as Building_SoftWarmBed;
                stuffFilter = softWarmBed.settings.filter;
            }
            if (bed is Building_SoftWarmGuestBed)
            {
                Building_SoftWarmGuestBed softWarmBed = bed as Building_SoftWarmGuestBed;
                stuffFilter = softWarmBed.settings.filter;
            }
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && beddingFilter.Allows(x) && stuffFilter.Allows(x.Stuff);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            ThingRequest bestThingRequest = beddingFilter.BestThingRequest;
            PathEndMode peMode = PathEndMode.ClosestTouch;
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(position, map, bestThingRequest, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}
