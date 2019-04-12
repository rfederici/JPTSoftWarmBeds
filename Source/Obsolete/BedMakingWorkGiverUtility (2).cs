

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




using UnityEngine;

using Verse;
using Verse.AI;          // Needed when you do something with the AI
using Verse.Sound;       // Needed when you do something with Sound

using RimWorld;
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

// Note: If the usings are not found, (red line under it,) look into the folder '/Source-DLLs' and follow the instructions in the text files

// Now the program starts:
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
            //bool flag = !forced;
            //if (flag && !CompMakeableBed.ShouldAutoRefuelNow)
            //{
            //    return false;
            //}
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
                        ThingFilter beddingFilter = t.TryGetComp<CompMakeableBed>().allowedBeddingsSettings.filter;
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

            //List<Thing> source = BedMakingWorkGiverUtility.FindAllBeddings(pawn, t);
            //Job job = new Job(customJob ?? SoftWarmBeds_JobDefOf.MakeBed, t);
            //job.targetQueueB = (from f in source
            //                    select new LocalTargetInfo(f)).ToList<LocalTargetInfo>();
            //return job;
        }

        private static Thing FindBestBedding(Pawn pawn, Thing bed)
        {
            ThingFilter filter = bed.TryGetComp<CompMakeableBed>().allowedBeddingsSettings.filter;
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            ThingRequest bestThingRequest = filter.BestThingRequest;
            PathEndMode peMode = PathEndMode.ClosestTouch;
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(position, map, bestThingRequest, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }

        //private static List<Thing> FindAllBeddings(Pawn pawn, Thing bed)
        //{
        //    int quantity = 1; //bed.TryGetComp<CompMakeableBed>().GetFuelCountToFullyRefuel();
        //    ThingFilter filter = bed.TryGetComp<CompMakeableBed>().allowedBeddingsSettings.filter;
        //    Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
        //    IntVec3 position = bed.Position;
        //    Region region = position.GetRegion(pawn.Map, RegionType.Set_Passable);
        //    TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
        //    RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, false);
        //    List<Thing> chosenThings = new List<Thing>();
        //    int accumulatedQuantity = 0;
        //    RegionProcessor regionProcessor = delegate (Region r)
        //    {
        //        List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            Thing thing = list[i];
        //            if (validator(thing))
        //            {
        //                if (!chosenThings.Contains(thing))
        //                {
        //                    if (ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn))
        //                    {
        //                        chosenThings.Add(thing);
        //                        accumulatedQuantity += thing.stackCount;
        //                        if (accumulatedQuantity >= quantity)
        //                        {
        //                            return true;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        return false;
        //    };
        //    RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 99999, RegionType.Set_Passable);
        //    if (accumulatedQuantity >= quantity)
        //    {
        //        return chosenThings;
        //    }
        //    return null;
        //}

        //TESTE FILTRO

        //private static Thing FindBestBedding(Pawn pawn, Thing bed)
        //{
        //    ThingFilter beddingsFilter = bed.TryGetComp<CompMakeableBed>().allowedBeddingsSettings.filter;
        //    //Building_SoftWarmBed softWarmBed = bed as Building_SoftWarmBed;
        //    //ThingFilter stuffFilter = softWarmBed.def.building.fixedStorageSettings.filter;
        //    //ThingFilter x = new ThingFilter
        //    //{
        //    //foreach (ThingDef thingDef in stuffFilter)
        //    //{
        //    //    if (thingDef.stuffProps != null && thingDef.stuffProps.categories.Contains(stuffToFilter))
        //    //}
        //    Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && beddingsFilter.Allows(x);// && stuffFilter.Allows(x.Stuff);
        //    IntVec3 position = pawn.Position;
        //    Map map = pawn.Map;
        //    ThingRequest bestThingRequest = beddingsFilter.BestThingRequest;
        //    PathEndMode peMode = PathEndMode.ClosestTouch;
        //    TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
        //    Predicate<Thing> validator = predicate;
        //    return GenClosest.ClosestThingReachable(position, map, bestThingRequest, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        //}

        //public ThingRequest MyThingRequest
        //{
        //    get
        //    {
        //        if (this.allowedDefs.Count == 1)
        //        {
        //            return ThingRequest.ForDef(this.allowedDefs.First<ThingDef>());
        //        }
        //        bool flag = true;
        //        bool flag2 = true;
        //        foreach (ThingDef thingDef in this.allowedDefs)
        //        {
        //            if (!thingDef.EverHaulable)
        //            {
        //                flag = false;
        //            }
        //            if (thingDef.category != ThingCategory.Pawn)
        //            {
        //                flag2 = false;
        //            }
        //        }
        //        if (flag)
        //        {
        //            return ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);
        //        }
        //        if (flag2)
        //        {
        //            return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
        //        }
        //        return ThingRequest.ForGroup(ThingRequestGroup.Everything);
        //    }
        //}
    }
}
