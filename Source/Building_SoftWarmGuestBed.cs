using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Hospitality;

namespace SoftWarmBeds
{
    [StaticConstructorOnStartup]
    public class Building_SoftWarmGuestBed : Hospitality.Building_GuestBed, IStoreSettingsParent, IExposable
    {
       
        private float curRotationInt;
        
        protected CompMakeableBed BedComp
		{
			get
			{
				return this.TryGetComp<CompMakeableBed>();
			}
		}
        
        private bool IsMade
        {
            get
            {
                return BedComp != null && BedComp.Loaded;
            }
        }

        private bool Occupied
        {
            get
            {
                return CurOccupants != null;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
        	base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void PostMake()
        {
            base.PostMake();
            SetUpStorageSettings();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<StorageSettings>(ref settings, "settings", new object[] { this });
            if (settings == null)
            {
                SetUpStorageSettings();
            }
        }

        public void SetUpStorageSettings()
        {
            if (GetParentStoreSettings() != null)
            {
                settings = new StorageSettings(this);
                settings.CopyFrom(GetParentStoreSettings());
            }
        }
        
        public new static void Swap(Building_Bed bed)
        {
            //Log.Message(bed + " is " + bed.GetType());
            StorageSettings settings = null;
            int bedclass = 0;
            bool exception = bed.def.defName.Contains("SleepingSpot");
            if (bed is Building_SoftWarmGuestBed bed2 && !exception) //all guest beds but sleeping spots (those revert to default)
            {
                bedclass = 2; //transform to regular soft beds
                settings = bed2.settings;
                bed2.NotTheBlanket = false;
            }
            else if (bed is Building_Bed && !(bed is Building_SoftWarmGuestBed)) //all regular beds & sleeping spots
            {
                bedclass = 1; //transform to guest variants (SoftWarmGuestBed)
                if (!exception)
                {
                    Building_SoftWarmBed bed1 = bed as Building_SoftWarmBed;
                    settings = bed1.settings;
                    bed1.NotTheBlanket = false;
                }
            }
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
                    //Log.Message("Swapping to Building_SoftWarmGuestBed");
                    newBed = (Building_SoftWarmGuestBed)BuildBed(bed, bed.def.defName + "Guest");
                    break;
                case 2:
                    //Log.Message("Swapping to Building_SoftWarmBed");
                    newBed = (Building_SoftWarmBed)BuildBed(bed, bed.def.defName.Split(new[] { "Guest" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    break;
                default:
                    //Log.Message("Swapping to Building_Bed");
                    newBed = (Building_Bed)BuildBed(bed, bed.def.defName.Split(new[] { "Guest" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    break;
            }
            newBed.SetFactionDirect(bed.Faction);
            //var spawnedBed = (Building_Bed)GenSpawn.Spawn(newBed, bed.Position, bed.Map, bed.Rotation);
            var spawnedBed = GenSpawn.Spawn(newBed, bed.Position, bed.Map, bed.Rotation) as Building_Bed;
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

        private static Thing BuildBed(Building_Bed bed, string defName)
        {
            ThingDef named = DefDatabase<ThingDef>.GetNamed(defName, true);
            return ThingMaker.MakeThing(named, bed.Stuff);
        }

        public override void Tick()
        {
            base.Tick();
            if (IsMade)// && !this.Occupied)
            {
                if (!settings.filter.Allows(BedComp.blanketStuff))
                {
                   Unmake();
                } 
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string inspectString = base.GetInspectString();
            if (!inspectString.NullOrEmpty())
            {
                stringBuilder.AppendLine(inspectString);
            }
            if (BedComp != null)
            {
                if (BedComp.Loaded)
                {
                    stringBuilder.AppendLine("BedMade".Translate(BedComp.bedding.Stuff.LabelCap, BedComp.bedding.Stuff));
                }
                else
                {
                    stringBuilder.AppendLine("BedNotMade".Translate());
                }
            }
            base.GetInspectString();
            return stringBuilder.ToString().TrimEndNewlines();
        }

        private float CurRotation
        {
            get
            {
                return curRotationInt;
            }
            set
            {
                curRotationInt = value;
                if (curRotationInt > 360f)
                {
                    curRotationInt -= 360f;
                }
                if (curRotationInt < 0f)
                {
                    curRotationInt += 360f;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (IsMade)
            {
                BedComp.DrawBed();
            }
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
            //IEnumerator<Gizmo> enumerator = null;
            if (def.building.bed_humanlike)
            {
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.defaultLabel = "CommandBedSetAsGuestLabel".Translate();
                command_Toggle.defaultDesc = "CommandBedSetAsGuestDesc".Translate();
                command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/AsGuest", true);
                command_Toggle.isActive = (() => true);
                command_Toggle.toggleAction = delegate ()
                {
                    Swap(this);
                };
                command_Toggle.hotKey = KeyBindingDefOf.Misc4;
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

        public void Unmake()
        {
            ThingDef stuff = BedComp.blanketStuff;
            GenPlace.TryPlaceThing(BedComp.RemoveBedding(stuff), base.Position, base.Map, ThingPlaceMode.Near, null, null);
            Notify_ColorChanged();
        }

        public bool NotTheBlanket = true;

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (IsMade && NotTheBlanket)
            {
                Unmake();
            }
            base.DeSpawn(mode);
        }

        public bool StorageTabVisible
		{
		    get
			{
				return true;
			}
		}

        public StorageSettings GetStoreSettings()
		{
			return settings;
		}

        public StorageSettings GetParentStoreSettings()
	    {
            return def.building.defaultStorageSettings;
        }

        public StorageSettings settings;

        public override Color DrawColorTwo
        {
            get
            {
                bool forPrisoners = ForPrisoners;
                bool medical = Medical;
                bool invertedColorDisplay = (SoftWarmBedsSettings.colorDisplayOption == ColorDisplayOption.Blanket);
                if (!forPrisoners && !medical && !invertedColorDisplay)
                {
                    if (IsMade && BedComp.blanketDef == null)
                    {
                        return BedComp.blanketStuff.stuffProps.color;
                    }
                }
                return base.DrawColorTwo;
            }
        }

        public override void Notify_ColorChanged()
        {
            base.Notify_ColorChanged();
            if (IsMade && BedComp.blanketDef != null)
            {
                BedComp.bedding.Notify_ColorChanged();
            }
        }

    }
}
