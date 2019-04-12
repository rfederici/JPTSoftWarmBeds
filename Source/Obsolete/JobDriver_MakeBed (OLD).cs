

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
    public class JobDriver_MakeBed : JobDriver
    {
        //From TurretGun
        private static bool BedNeedsMaking(Building b)
        {
            Building_SoftWarmBed building_SoftWarmBed = b as Building_SoftWarmBed;
            if (building_SoftWarmBed == null)
            {
                return false;
            }
            CompMakeableBed CompMakeableBed = building_SoftWarmBed.TryGetComp<CompMakeableBed>();
            return CompMakeableBed != null && !CompMakeableBed.Loaded;
        }

        public static Thing FindBeddingForBed(Pawn pawn, Building_SoftWarmBed bed)
        {
            StorageSettings allowedBeddingsSettings = (!pawn.IsColonist) ? null : bed.TryGetComp<CompMakeableBed>().allowedBeddingsSettings;
            Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1, null, false) && (allowedBeddingsSettings == null || allowedBeddingsSettings.AllowedToAccept(t));
            return GenClosest.ClosestThingReachable(bed.Position, bed.Map, ThingRequest.ForGroup(ThingRequestGroup.Bedding), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 40f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toil gotoBed = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil loadIfNeeded = new Toil();
            loadIfNeeded.initAction = delegate ()
            {
                Pawn actor = loadIfNeeded.actor;
                Building building = (Building)actor.CurJob.targetA.Thing;
                Building_SoftWarmBed Building_SoftWarmBed = building as Building_SoftWarmBed;
                if (!JobDriver_MakeBed.BedNeedsMaking(building))
                {
                    this.JumpToToil(gotoBed);
                    return;
                }
                Thing thing = JobDriver_MakeBed.FindBeddingForBed(this.pawn, Building_SoftWarmBed);
                if (thing == null)
                {
                    if (actor.Faction == Faction.OfPlayer)
                    {
                        Messages.Message("MessageOutOfNearbyBeddingsFor".Translate(actor.LabelShort, Building_SoftWarmBed.Label, actor.Named("PAWN"), Building_SoftWarmBed.Named("BED")).CapitalizeFirst(), Building_SoftWarmBed, MessageTypeDefOf.NegativeEvent, true);
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                }
                actor.CurJob.targetB = thing;
                actor.CurJob.count = 1;
            };
            yield return loadIfNeeded;
            yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Pawn actor = loadIfNeeded.actor;
                    Building building = (Building)actor.CurJob.targetA.Thing;
                    Building_SoftWarmBed Building_SoftWarmBed = building as Building_SoftWarmBed;
                    //SoundDefOf.Artillery_BeddingLoaded.PlayOneShot(new TargetInfo(Building_SoftWarmBed.Position, Building_SoftWarmBed.Map, false));
                    Building_SoftWarmBed.TryGetComp<CompMakeableBed>().LoadBedding(actor.CurJob.targetB.Thing.def, 1);
                    actor.carryTracker.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
                }
            };
           // yield return gotoBed;
           //Toil man = new Toil();
           // man.tickAction = delegate ()
           //{
           //     Pawn actor = man.actor;
           //     Building building = (Building)actor.CurJob.targetA.Thing;
           //     if (JobDriver_MakeBed.BedNeedsMaking(building))
           //     {
           //         this.JumpToToil(loadIfNeeded);
           //         return;
           //     }
           //     building.GetComp<CompMannable>().ManForATick(actor);
           // };
           //man.defaultCompleteMode = ToilCompleteMode.Never;
           //man.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
           // yield return man;
            yield break;
        }

        //private const float BeddingSearchRadius = 40f;

        //private const int MaxPawnAmmoReservations = 10;
    }
}
