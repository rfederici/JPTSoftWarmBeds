using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace SoftWarmBeds
{
    [StaticConstructorOnStartup]
    public class Building_SoftWarmGuestBed : Building_SoftWarmBed, IStoreSettingsParent, IExposable
    {
        private static readonly Color guestFieldColor = new Color(170 / 255f, 79 / 255f, 255 / 255f);

        private static readonly Color sheetColorForGuests = new Color(89 / 255f, 55 / 255f, 121 / 255f);

        private static readonly List<IntVec3> guestField = new List<IntVec3>();

        public Pawn CurOccupant
        {
            get
            {
                var list = Map.thingGrid.ThingsListAt(Position);
                return list.OfType<Pawn>()
                    .Where(pawn => pawn.jobs.curJob != null)
                    .FirstOrDefault(pawn => pawn.jobs.curJob.def == JobDefOf.LayDown && pawn.jobs.curJob.targetA.Thing == this);
            }
        }

        public override Color DrawColor
        {
            get
            {
                if (def.MadeFromStuff)
                {
                    return base.DrawColor;
                }
                return DrawColorTwo;
            }
        }

        public override void Draw()
        {
            if (IsMade)
            {
                BedComp.DrawBed();
            }
            if (Medical) Medical = false;
            if (ForPrisoners) ForPrisoners = false;
        }

        public override Color DrawColorTwo { get { return sheetColorForGuests; } }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (var owner in owners.ToArray())
            {
                owner.ownership.UnclaimBed();
            }
            var room = Position.GetRoom(Map);
            base.DeSpawn(mode);
            if (room != null)
            {
                room.Notify_RoomShapeOrContainedBedsChanged();
            }
        }

        public override string GetInspectString()
        {
            base.GetInspectString();
            var stringBuilder = new StringBuilder();
            //stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append(InspectStringPartsFromComps());
            stringBuilder.AppendLine();
            stringBuilder.Append("ForGuestUse".Translate());

            stringBuilder.AppendLine();
            if (owners.Count == 0)
            {
                stringBuilder.Append("Owner".Translate() + ": " + "Nobody".Translate());
            }
            else if (owners.Count == 1)
            {
                stringBuilder.Append("Owner".Translate() + ": " + owners[0].LabelCap);
            }
            else
            {
                stringBuilder.Append("Owners".Translate() + ": ");
                bool notFirst = false;
                foreach (Pawn owner in owners)
                {
                    if (notFirst)
                    {
                        stringBuilder.Append(", ");
                    }
                    notFirst = true;
                    stringBuilder.Append(owner.Label);
                }
                //if(notFirst) stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }

        private static Thing BuildBed(Building_Bed bed, string defName)
        {
            ThingDef named = DefDatabase<ThingDef>.GetNamed(defName, true);
            return ThingMaker.MakeThing(named, bed.Stuff);
        }

        public static void Swap(Building_Bed bed)
        {
            //Log.Message(bed + " is " + bed.GetType());
            StorageSettings settings = null;
            int bedclass = 0;
            bool exception = bed.def.defName.Contains("SleepingSpot");
            if (bed is Building_SoftWarmGuestBed bed2 && !exception) //all guest beds but sleeping spots (those revert to default)
            {
                bedclass = 2; //transform to regular soft beds
                settings = bed2.settings;
            }
            else if (bed is Building_Bed && !(bed is Building_SoftWarmGuestBed)) //all regular beds & sleeping spots
            {
                bedclass = 1; //transform to guest variants (SoftWarmGuestBed)
                if (!exception)
                {
                    Building_SoftWarmBed bed1 = bed as Building_SoftWarmBed;
                    settings = bed1.settings;
                }
            }
            //Log.Message("Bedclass is " + bedclass);
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
            Building_Bed newBed = null;
            switch (bedclass)
            {
                case 1:
                    newBed = (Building_SoftWarmGuestBed)BuildBed(bed, bed.def.defName + "Guest");
                    //Log.Message("Swapping to Building_SoftWarmGuestBed");
                    break;
                case 2:
                    newBed = (Building_SoftWarmBed)BuildBed(bed, bed.def.defName.Split(new[] { "Guest" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    //Log.Message("Swapping to Building_SoftWarmBed");
                    break;
                default:
                    newBed = (Building_Bed)BuildBed(bed, bed.def.defName.Split(new[] { "Guest" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    //Log.Message("Swapping to Building_Bed");
                    break;
            }
            newBed.SetFactionDirect(bed.Faction);
            var spawnedBed = (Building_Bed)GenSpawn.Spawn(newBed, bed.Position, bed.Map, bed.Rotation);
            spawnedBed.HitPoints = bed.HitPoints;
            spawnedBed.ForPrisoners = bed.ForPrisoners;
            if (settings != null)
            {
                if (spawnedBed is Building_SoftWarmBed spawnedBed_base)
                {
                    spawnedBed_base.settings = settings;
                }
                if (spawnedBed is Building_SoftWarmGuestBed spawnedBed_guest)
                {
                    spawnedBed_guest.settings = settings;
                }
            }
            var compQuality = spawnedBed.TryGetComp<CompQuality>();
            if (compQuality != null) compQuality.SetQuality(bed.GetComp<CompQuality>().Quality, ArtGenerationContext.Outsider);

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

        //ADAPTED FROM HOSPITALITY
        public override IEnumerable<Gizmo> GetGizmos()
        {
            MethodInfo method = typeof(Building).GetMethod("GetGizmos");
            IntPtr ftn = method.MethodHandle.GetFunctionPointer();
            Func<IEnumerable<Gizmo>> func = (Func<IEnumerable<Gizmo>>)Activator.CreateInstance(typeof(Func<IEnumerable<Gizmo>>), new object[] { this, ftn });
            foreach (Gizmo gizmo in func())
            {
                yield return gizmo;
            }
            if (def.building.bed_humanlike)
            {
                Command_Toggle command_Toggle = new Command_Toggle
                {
                    defaultLabel = "CommandBedSetAsGuestLabel".Translate(),
                    defaultDesc = "CommandBedSetAsGuestDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/AsGuest", true),
                    isActive = (() => true),
                    toggleAction = delegate ()
                    {
                        Swap(this);
                    },
                    hotKey = KeyBindingDefOf.Misc4
                };
                yield return command_Toggle;
            
                if (IsMade)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "CommandUnmakeBed".Translate(),
                        defaultDesc = "CommandUnmakeBedDesc".Translate(),
                        icon = BedComp.LoadedBedding.uiIcon,
                        iconAngle = BedComp.LoadedBedding.uiIconAngle,
                        iconOffset = BedComp.LoadedBedding.uiIconOffset,
                        iconDrawScale = GenUI.IconDrawScale(BedComp.LoadedBedding),
                        action = delegate ()
                        {
                            Unmake();
                        }
                    };
                }
            }
            yield break;
        }

        public override void PostMake()
        {
            base.PostMake();
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDef.Named("GuestBeds"), KnowledgeAmount.Total);
        }

    }
}
