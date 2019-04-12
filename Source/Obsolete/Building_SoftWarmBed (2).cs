

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




using UnityEngine;
//using VerseBase;           // Material/Graphics handling functions are found here
using Verse;
//using Verse.AI;          // Needed when you do something with the AI


using RimWorld;
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

// Note: If the usings are not found, (red line under it,) look into the folder '/Source-DLLs' and follow the instructions in the text files


// Now the program starts:
namespace SoftWarmBeds
{
    [StaticConstructorOnStartup]
    public class Building_SoftWarmBed : Building_Bed//, IStoreSettingsParent
    {

        private float curRotationInt;

        private bool IsMade
        {
            get
            {
                CompMakeableBed CompMakeableBed = this.TryGetComp<CompMakeableBed>();
                return CompMakeableBed != null && CompMakeableBed.Loaded;
            }
        }

        private bool Occupied
        {
            get
            {
                return this.CurOccupants != null;
            }
        }

        //public override void SpawnSetup(Map map, bool respawningAfterLoad)
        //{
        //	base.SpawnSetup(map, respawningAfterLoad);
        //	this.powerComp = base.GetComp<CompPowerTrader>();
        //	this.mannableComp = base.GetComp<CompMannable>();
        //}

        public Thing bedding ;   

      //  public override void PostMake()
      //  {
      //      base.PostMake();
    		//this.settings = new StorageSettings(this);
      //  }

    //public void MakeBed(Thing thing)
    //{
    //    ThingDef stuff = thing.Stuff;
    //    this.bedding = ThingMaker.MakeThing(this.def.building.turretGunDef, stuff);
    //    //this.UpdateStats(stuff);
    //}

    //private void UpdateStats(ThingDef stuff)
    //{
    //    //if (this.bedding != null)
    //    //{
    //    float oldstat = this.GetStatValue(BedInsulationCold.Bed_Insulation_Cold, true);//<- Isso funciona quando o Stat está ok no XML
    //    float addstat = stuff.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Cold, null);//<-Isso funciona, não mexa!                                                                             //float oldstat = this.GetStatValue(BeddingStuffPowerCold.Bedding_StuffPower_Insulation_Cold, true);
    //    Log.Message("detectados: cama:"+oldstat+" cobertor:"+addstat);
    //    float newstat = new float();
    //    //this.def.SetStatBaseValue(BeddingStuffPowerCold.Bedding_StuffPower_Insulation_Cold, newstat);//<- mudou o stat para todas as camas!
    //    newstat = oldstat += stuff.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Cold, null);//<-Isso funciona, não mexa!
    //    //this.def.workerInt.InitSetStatInitSetStat (newstat);
    //    Log.Message("modificado para:"+newstat);
    //    //if (this.subtract)
    //    //{
    //    //     val -= statValue;
    //    // }
    //    //else`v 
    //    //{
    //    //    val += statValue;
    //    //}
    //}

    public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<int>(ref this.burstCooldownTicksLeft, "burstCooldownTicksLeft", 0, false);
            //Scribe_Values.Look<int>(ref this.burstWarmupTicksLeft, "burstWarmupTicksLeft", 0, false);
            //Scribe_TargetInfo.Look(ref this.currentTargetInt, "currentTarget");
            //Scribe_Values.Look<bool>(ref this.holdFire, "holdFire", false, false);
            Scribe_Deep.Look<Thing>(ref this.bedding, "bedding", new object[0]);
            //if (Scribe.mode == LoadSaveMode.PostLoadInit)
            //{
            //	BackCompatibility.TurretPostLoadInit(this);
            //	this.UpdateGunVerbs();
            //}
        }

        public override void Tick()
        {
            base.Tick();
            if (this.IsMade && !this.Occupied)
            {
                CompMakeableBed CompMakeableBed = this.TryGetComp<CompMakeableBed>();
                if (!CompMakeableBed.allowedBeddingsSettings.AllowedToAccept(CompMakeableBed.LoadedBedding))
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
            CompMakeableBed compMakeableBed = this.TryGetComp<CompMakeableBed>();
            if (compMakeableBed != null)
            {
                if (compMakeableBed.Loaded)
                {
                    stringBuilder.AppendLine("BedMade".Translate(compMakeableBed.bedding.Stuff.LabelCap, compMakeableBed.bedding.Stuff));//this.bedding.Stuff.LabelCap, this.bedding.Stuff));
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
                CompMakeableBed compMakeableBed = this.TryGetComp<CompMakeableBed>();
                compMakeableBed.DrawBed();
                //this.bedding.Graphic.Draw(this.DrawPos + Altitudes.AltIncVect, this.Rotation, this.bedding);
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
                CompMakeableBed changeableBedding = this.TryGetComp<CompMakeableBed>();
                yield return new Command_Action
                {
                    defaultLabel = "CommandUnmakeBed".Translate(),
                    defaultDesc = "CommandUnmakeBedDesc".Translate(),
                    icon = changeableBedding.LoadedBedding.uiIcon,
                    iconAngle = changeableBedding.LoadedBedding.uiIconAngle,
                    iconOffset = changeableBedding.LoadedBedding.uiIconOffset,
                    iconDrawScale = GenUI.IconDrawScale(changeableBedding.LoadedBedding),
                    action = delegate ()
                    {
                        this.Unmake();
                    }
                };
            }
            yield break;
        }

        private void Unmake()
        {
            ThingDef stuff = this.TryGetComp<CompMakeableBed>().blanketStuff;
            GenPlace.TryPlaceThing(this.TryGetComp<CompMakeableBed>().RemoveBedding(stuff), base.Position, base.Map, ThingPlaceMode.Near, null, null);
        }

  //      public bool StorageTabVisible
		//{
		//	get
		//	{
		//		return true;
		//	}
		//}

  //      public StorageSettings GetStoreSettings()
		//{
		//	return this.settings;
		//}

  //      public StorageSettings GetParentStoreSettings()
		//{
		//	return this.def.building.defaultStorageSettings;
		//}

  //      public StorageSettings settings;

    }

}
