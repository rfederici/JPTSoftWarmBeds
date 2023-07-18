using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SoftWarmBeds;

public class JobDriver_MakeBed : JobDriver
{
    private const TargetIndex BeddingInd = TargetIndex.B;

    private const TargetIndex MakeableInd = TargetIndex.A;

    private const int MakingDuration = 180;

    protected Thing Bed => job.GetTarget(TargetIndex.A).Thing;

    protected CompMakeableBed bedComp => Bed.TryGetComp<CompMakeableBed>();

    protected Thing Bedding => job.GetTarget(TargetIndex.B).Thing;

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
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        AddEndCondition(() => !bedComp.Loaded ? JobCondition.Ongoing : JobCondition.Succeeded);
        job.count = 1;
        var reserveBedding = Toils_Reserve.Reserve(TargetIndex.B);
        yield return reserveBedding;
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true)
            .FailOnDestroyedNullOrForbidden(TargetIndex.B);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveBedding, TargetIndex.B, TargetIndex.None, true);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(MakingDuration).FailOnDestroyedNullOrForbidden(TargetIndex.B)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
            .WithProgressBarToilDelay(TargetIndex.A);
        var makeTheBed = new Toil();
        makeTheBed.initAction = delegate
        {
            var actor = makeTheBed.actor;
            bedComp.LoadBedding(actor.CurJob.targetB.Thing);
            actor.carryTracker.innerContainer.ClearAndDestroyContents();
        };
        makeTheBed.defaultCompleteMode = ToilCompleteMode.Instant;
        makeTheBed.FailOnDespawnedOrNull(TargetIndex.A);
        yield return makeTheBed;
    }
}