using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SoftWarmBeds
{
    public class Building_SoftWarmGuestBed : Building_SoftWarmBed
    {
        private static readonly Color guestFieldColor = new Color(170/255f, 79/255f, 255/255f);

        private static readonly Color sheetColorForGuests = new Color(89/255f, 55/255f, 121/255f);

        private static readonly List<IntVec3> guestField = new List<IntVec3>();

        public Pawn CurOccupant
        {
            get
            {
				List<Thing> source = base.Map.thingGrid.ThingsListAt(base.Position);
				return (from pawn in source.OfType<Pawn>()
				where pawn.jobs.curJob != null
				select pawn).FirstOrDefault((Pawn pawn) => pawn.jobs.curJob.def == JobDefOf.LayDown && pawn.jobs.curJob.targetA.Thing == this);
            }
        }

        public override Color DrawColor
        {
            get
            {
                bool madeFromStuff = this.def.MadeFromStuff;
                Color result;
                if (madeFromStuff)
                {
                    result = base.DrawColor;
                }
                else
                {
                    result = this.DrawColorTwo;
                }
                return result;
            }
        }

        public override void Draw()
        {
            base.Draw();
            bool medical = base.Medical;
            if (medical)
            {
                base.Medical = false;
            }
            bool forPrisoners = base.ForPrisoners;
            if (forPrisoners)
            {
                base.ForPrisoners = false;
            }
        }

        public override Color DrawColorTwo
        {
            get
            {
                return Building_SoftWarmGuestBed.sheetColorForGuests;
            }
        }

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			foreach (Pawn pawn in this.owners.ToArray())
			{
				pawn.ownership.UnclaimBed();
			}
			Room room = base.Position.GetRoom(base.Map, RegionType.Set_Passable);
			base.DeSpawn(mode);
			bool flag = room != null;
			if (flag)
			{
				room.Notify_RoomShapeOrContainedBedsChanged();
			}
		}

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append(InspectStringPartsFromComps());
            stringBuilder.AppendLine();
            stringBuilder.Append("ForGuestUse".Translate());
            
            stringBuilder.AppendLine();
            bool flag = this.owners.Count == 0;
            if (flag)
            {
                stringBuilder.Append("Owner".Translate() + ": " + "Nobody".Translate());
            }
            else
            {
                bool flag2 = this.owners.Count == 1;
                if (flag2)
                {
                    stringBuilder.Append("Owner".Translate() + ": " + this.owners[0].LabelCap);
                }
                else
                {
                    stringBuilder.Append("Owners".Translate() + ": ");
                    bool flag3 = false;
                    foreach (Pawn pawn in this.owners)
                    {
                        bool flag4 = flag3;
                        if (flag4)
                        {
                            stringBuilder.Append(", ");
                        }
                        flag3 = true;
                        stringBuilder.Append(pawn.Label);
                    }
                }
            }
            return stringBuilder.ToString();
        }

        public override void PostMake()
        {
            base.PostMake();
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDef.Named("GuestBeds"), KnowledgeAmount.Total);
        }

        public static void Swap(Building_SoftWarmBed bed)
        {
            //bed.Unmake();
            CompMakeableBed compMakeable = bed.TryGetComp<CompMakeableBed>();
            ThingDef bedLoadedBedding = null;
            Thing bedBedding = null;
            if (compMakeable != null)
            {
                if (compMakeable.Loaded)
                {
                    bedLoadedBedding = compMakeable.loadedBedding;
                    bedBedding = compMakeable.bedding;
                }
            }
            //Log.Message("Bed acquired, comp:" + compMakeable + " loadedBedding:" + bedLoadedBedding + " bedding:" + bedBedding);
            Building_SoftWarmBed newBed;
            if (bed is Building_SoftWarmGuestBed)
            {
                newBed = (Building_SoftWarmBed)Building_SoftWarmGuestBed.MakeBed(bed, bed.def.defName.Split(new[] {"Guest"}, StringSplitOptions.RemoveEmptyEntries)[0]);
            }
            else
            {
                newBed = (Building_SoftWarmGuestBed)Building_SoftWarmGuestBed.MakeBed(bed, bed.def.defName + "Guest");
            }
            newBed.SetFactionDirect(bed.Faction);

            var spawnedBed = (Building_SoftWarmBed)GenSpawn.Spawn(newBed, bed.Position, bed.Map, bed.Rotation);
            spawnedBed.HitPoints = bed.HitPoints;
            spawnedBed.ForPrisoners = bed.ForPrisoners;
            spawnedBed.settings = bed.settings;

            var compQuality = spawnedBed.TryGetComp<CompQuality>();
            if(compQuality != null) compQuality.SetQuality(bed.GetComp<CompQuality>().Quality, ArtGenerationContext.Outsider);

            var spawnedBlanket = spawnedBed.TryGetComp<CompMakeableBed>();
            if (spawnedBlanket != null)
            {
                //Log.Message("new bed has comp");
                if (bedLoadedBedding != null)
                {
                    //Log.Message("new bed has blanket:" + bedLoadedBedding + " made from " + bedBedding.Stuff);
                    spawnedBlanket.LoadBedding(bedLoadedBedding, bedBedding);
                }
            }
            Find.Selector.Select(spawnedBed, false, true);
        }

        private static Thing MakeBed(Building_SoftWarmBed bed, string defName)
        {
            ThingDef newDef = DefDatabase<ThingDef>.GetNamed(defName);
            //Log.Message("making new bed from" + newDef + " in " + bed.Stuff);
            return ThingMaker.MakeThing(newDef, bed.Stuff);
        }
    }
}
