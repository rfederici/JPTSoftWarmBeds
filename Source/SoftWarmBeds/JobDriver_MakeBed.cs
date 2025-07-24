using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SoftWarmBeds;

public class JobDriver_MakeBed : JobDriver
{
    private const TargetIndex BeddingInd = TargetIndex.B;

    private const TargetIndex MakeableInd = TargetIndex.A;

    private const int MakingDuration = 180;

    private Thing Bed => job.GetTarget(MakeableInd).Thing;

    private CompMakeableBed bedComp => Bed.TryGetComp<CompMakeableBed>();

    private Thing Bedding => job.GetTarget(BeddingInd).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var toilPawn = pawn;
        LocalTargetInfo target = Bed;
        var toilJob = job;
        bool result;
        if (toilPawn.Reserve(target, toilJob, 1, -1, null, errorOnFailed))
        {
            toilPawn = pawn;
            target = Bedding;
            toilJob = job;
            result = toilPawn.Reserve(target, toilJob, 1, -1, null, errorOnFailed);
        }
        else
        {
            result = false;
        }

        return result;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(MakeableInd);
        AddEndCondition(() => !bedComp.Loaded ? JobCondition.Ongoing : JobCondition.Succeeded);
        job.count = 1;
        var reserveBedding = Toils_Reserve.Reserve(BeddingInd);
        yield return reserveBedding;
        yield return Toils_Goto.GotoThing(BeddingInd, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(BeddingInd).FailOnSomeonePhysicallyInteracting(BeddingInd);
        yield return Toils_Haul.StartCarryThing(BeddingInd, false, true)
            .FailOnDestroyedNullOrForbidden(BeddingInd);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveBedding, BeddingInd, TargetIndex.None, true);
        yield return Toils_Goto.GotoThing(MakeableInd, PathEndMode.Touch);
        yield return Toils_General.Wait(MakingDuration).FailOnDestroyedNullOrForbidden(BeddingInd)
            .FailOnDestroyedNullOrForbidden(MakeableInd).FailOnCannotTouch(MakeableInd, PathEndMode.Touch)
            .WithProgressBarToilDelay(MakeableInd);
        var makeTheBed = new Toil();
        makeTheBed.initAction = delegate
        {
            var actor = makeTheBed.actor;
            bedComp.LoadBedding(actor.CurJob.targetB.Thing);
            actor.carryTracker.innerContainer.ClearAndDestroyContents();
        };
        makeTheBed.defaultCompleteMode = ToilCompleteMode.Instant;
        makeTheBed.FailOnDespawnedOrNull(MakeableInd);
        yield return makeTheBed;
    }
}