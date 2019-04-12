using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    [StaticConstructorOnStartup]
    public class Building_SoftWarmBed : Building_Bed, IStoreSettingsParent, IExposable
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
                return this.BedComp != null && this.BedComp.Loaded;
            }
        }

        private bool Occupied
        {
            get
            {
                return this.CurOccupants != null;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
        	base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void PostMake()
        {
            base.PostMake();
    		this.settings = new StorageSettings(this);
            if (this.def.building.defaultStorageSettings != null)
            {
               this.settings.CopyFrom(this.def.building.defaultStorageSettings);
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<StorageSettings>(ref this.settings, "settings", new object[]
			{
				this
			});
        }

        public override void Tick()
        {
            base.Tick();
            if (this.IsMade)// && !this.Occupied)
            {
                if (!this.settings.filter.Allows(this.BedComp.blanketStuff))
                {
                   this.Unmake();
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
            if (this.BedComp != null)
            {
                if (this.BedComp.Loaded)
                {
                    stringBuilder.AppendLine("BedMade".Translate(this.BedComp.bedding.Stuff.LabelCap, this.BedComp.bedding.Stuff));
                }
                else
                {
                    stringBuilder.AppendLine("BedNotMade".Translate());
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        private float CurRotation
        {
            get
            {
                return this.curRotationInt;
            }
            set
            {
                this.curRotationInt = value;
                if (this.curRotationInt > 360f)
                {
                    this.curRotationInt -= 360f;
                }
                if (this.curRotationInt < 0f)
                {
                    this.curRotationInt += 360f;
                }
            }
        }

        public override void Draw()
        {
            if (this.IsMade)
            {
                this.BedComp.DrawBed();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            if (this.IsMade)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandUnmakeBed".Translate(),
                    defaultDesc = "CommandUnmakeBedDesc".Translate(),
                    icon = this.BedComp.LoadedBedding.uiIcon,
                    iconAngle = this.BedComp.LoadedBedding.uiIconAngle,
                    iconOffset = this.BedComp.LoadedBedding.uiIconOffset,
                    iconDrawScale = GenUI.IconDrawScale(this.BedComp.LoadedBedding),
                    action = delegate ()
                    {
                        this.Unmake();
                    }
                };
            }
            yield break;
        }

        public void Unmake()
        {
            ThingDef stuff = this.BedComp.blanketStuff;
            GenPlace.TryPlaceThing(this.BedComp.RemoveBedding(stuff), base.Position, base.Map, ThingPlaceMode.Near, null, null);
            this.Notify_ColorChanged();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.IsMade)
            {
                this.Unmake();
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
			return this.settings;
		}

        public StorageSettings GetParentStoreSettings()
	    {
			return this.def.building.defaultStorageSettings;
		}

        public StorageSettings settings;

        private static Color SheetColorNormal = new Color(1f, 1f, 1f);
              
        public override Color DrawColorTwo
		{
			get
			{
                bool forPrisoners = this.ForPrisoners;
                bool medical = this.Medical;
                if (!forPrisoners && !medical)
                {
                    if (this.IsMade)
                    {
                        return SheetColorNormal;
                    }
                    return base.DrawColor;
                }
                return base.DrawColorTwo;
                
            }
		}

        public override void Notify_ColorChanged()
		{   
            if (this.IsMade)
            {
               this.BedComp.bedding.Notify_ColorChanged();
            }
            base.Notify_ColorChanged();

		}

    }
}
