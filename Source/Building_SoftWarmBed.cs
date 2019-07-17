using System;
using System.Collections.Generic;
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
        
        public bool IsMade
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

        public override void Tick()
        {
            base.Tick();
            if (IsMade)
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

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
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
                    return base.DrawColor;
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
