//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    public class CompMakeableBed : ThingComp
    {
        public ThingDef loadedBedding;

        public bool loaded = false;

		public ThingDef allowedBedding;

        public Thing bedding = null;

        public ThingDef blanketStuff = null;

        public ThingDef blanketDef = null;

		public override void PostExposeData()
		{
            Scribe_Values.Look<bool>(ref this.loaded, "loaded", false, false);
            Scribe_Defs.Look<ThingDef>(ref this.loadedBedding, "loadedBedding");
            Scribe_Deep.Look<Thing>(ref this.bedding, "bedding", new object[0]);
            Scribe_Defs.Look<ThingDef>(ref this.blanketStuff, "blanketStuff");
            if (this.loaded)
            {
            Building_Blanket blanket = this.bedding as Building_Blanket;
            Scribe_Values.Look<bool>(ref blanket.hasColor, "hasColor", false, false);
            }
        }

        public override void PostSplitOff(Thing bedding)
        {
            Building_Blanket blanket = this.bedding as Building_Blanket;
            if (blanket.hasColor)
            {
                blanket.colorTwo = this.parent.Graphic.colorTwo;
                this.parent.Notify_ColorChanged();
            }
        }

        public CompProperties_MakeableBed Props
		{
			get
			{
				return (CompProperties_MakeableBed)this.props;
			}
		}

		public ThingDef LoadedBedding
		{
			get
			{
                return this.loadedBedding;
            }
        }

		public bool Loaded
		{
			get
			{
				return this.LoadedBedding != null;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
            this.allowedBedding = new ThingDef();
            this.allowedBedding = this.Props.beddingDef;
            this.blanketDef = new ThingDef();
            this.blanketDef = this.Props.blanketDef;
        }
         
        //modified from CompChangeableProjectiles 
        public void LoadBedding(ThingDef beddingDef, Thing bedding)
		{
            this.loaded = true;
            this.loadedBedding = beddingDef;
            this.blanketStuff = bedding.Stuff;
            this.bedding = ThingMaker.MakeThing(this.blanketDef, this.blanketStuff);
            Building_Blanket blanket = this.bedding as Building_Blanket;
            blanket.hasColor = true;
            this.DrawBed();
            this.parent.Notify_ColorChanged();
            //this.parent.def.building.bed_showSleeperBody = false;
        }

        //not present in CompChangeableProjectiles
        public void DrawBed() 
        {
            Building_Blanket blanket = this.bedding as Building_Blanket;
            blanket.colorTwo = this.parent.Graphic.colorTwo;
            this.bedding.Graphic.Draw(this.parent.DrawPos + Altitudes.AltIncVect, this.parent.Rotation, this.bedding);
        }

        //modified from CompChangeableProjectiles to accept stuff
        public Thing RemoveBedding(ThingDef stuff)
        {
            Thing thing = ThingMaker.MakeThing(this.loadedBedding, stuff);
            this.loaded = false;
            this.loadedBedding = null;
            return thing;
        }

	}
}