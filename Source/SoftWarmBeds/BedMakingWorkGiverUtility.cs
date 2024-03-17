using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace SoftWarmBeds;

public static class BedMakingWorkGiverUtility
{
    public static bool CanMakeBed(Pawn pawn, Building_Bed t, bool forced = false)
    {
        var CompMakeableBed = t.TryGetComp<CompMakeableBed>();
        if (CompMakeableBed == null || CompMakeableBed.Loaded)
        {
            return false;
        }

        if (t.IsForbidden(pawn))
        {
            return false;
        }

        LocalTargetInfo target = t;
        if (!pawn.CanReserve(target, 1, -1, null, forced))
        {
            return false;
        }

        if (t.Faction != pawn.Faction)
        {
            return false;
        }

        if (FindBestBedding(pawn, t) != null)
        {
            return true;
        }

        var beddingFilter = new ThingFilter();
        beddingFilter.SetAllow(t.TryGetComp<CompMakeableBed>().allowedBedding, true);
        JobFailReason.Is("NoSuitableBedding".Translate(beddingFilter.Summary));
        return false;
    }

    public static Job BedMakingJob(Pawn pawn, Building_Bed t, bool forced = false, JobDef customJob = null)
    {
        var t2 = FindBestBedding(pawn, t);
        return new Job(customJob ?? SoftWarmBeds_JobDefOf.MakeBed, t, t2);
    }

    private static Thing FindBestBedding(Pawn pawn, Building_Bed bed)
    {
        //Log.Message(pawn + " is looking for a bedding type " + bed.TryGetComp<CompMakeableBed>().blanketDef + " for " + bed);
        var beddingFilter = new ThingFilter();
        beddingFilter.SetAllow(bed.TryGetComp<CompMakeableBed>().allowedBedding, true);
        var stuffFilter = bed.TryGetComp<CompMakeableBed>().settings.filter;

        var position = pawn.Position;
        var map = pawn.Map;
        var bestThingRequest = beddingFilter.BestThingRequest;
        var peMode = PathEndMode.ClosestTouch;
        var traverseParams = TraverseParms.For(pawn);
        var validator = (Predicate<Thing>)Predicate;
        return GenClosest.ClosestThingReachable(position, map, bestThingRequest, peMode, traverseParams, 9999f,
            validator);

        bool Predicate(Thing x)
        {
            return !x.IsForbidden(pawn) && pawn.CanReserve(x) && beddingFilter.Allows(x) && stuffFilter.Allows(x.Stuff);
        }
    }
}