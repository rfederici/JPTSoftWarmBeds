using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
        
        public Color BlanketColor = new Color(1f, 1f, 1f);
        
		public override void PostExposeData()
		{
            Scribe_Values.Look<bool>(ref loaded, "loaded", false, false);
            Scribe_Defs.Look<ThingDef>(ref loadedBedding, "loadedBedding");
            Scribe_Deep.Look<Thing>(ref bedding, "bedding", new object[0]);
            Scribe_Defs.Look<ThingDef>(ref blanketStuff, "blanketStuff");
            if (loaded && blanketDef != null)
            {
                Building_Blanket blanket = bedding as Building_Blanket;
                Scribe_Values.Look<bool>(ref blanket.hasColor, "hasColor", false, false);
            }
        }

        public override void PostSplitOff(Thing bedding)
        {
            if (blanketDef != null)
            {
                Building_Blanket blanket = this.bedding as Building_Blanket;
                if (blanket.hasColor)
                {
                    blanket.colorTwo = parent.Graphic.colorTwo;
                    parent.Notify_ColorChanged();
                }
            }
        }

        public CompProperties_MakeableBed Props
		{
			get
			{
				return (CompProperties_MakeableBed)props;
			}
		}

		public ThingDef LoadedBedding
		{
			get
			{
                return loadedBedding;
            }
        }

		public bool Loaded
		{
			get
			{
				return LoadedBedding != null;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
            allowedBedding = new ThingDef();
            allowedBedding = Props.beddingDef;
            blanketDef = new ThingDef();
            blanketDef = Props.blanketDef;
        }
         
        //modified from CompChangeableProjectiles 
        public void LoadBedding(ThingDef beddingDef, Thing bedding)
		{
            loaded = true;
            loadedBedding = beddingDef;
            blanketStuff = bedding.Stuff;
            //if (this.bedding.TryGetComp<CompColorable>().Active) //test
            //{
            //    blanketColor = bedding.TryGetComp<CompColorable>().Color;
            //}
            //else
            //{
            //    blanketColor = blanketStuff.stuffProps.color;
            //}
            if (blanketDef != null)
            {
                this.bedding = ThingMaker.MakeThing(blanketDef, blanketStuff);
                Building_Blanket blanket = this.bedding as Building_Blanket;
                //blanket.TryGetComp<CompColorable>().Color = blanketColor;
                blanket.hasColor = true;
                DrawBed();
            }
            parent.Notify_ColorChanged();
            //this.parent.def.building.bed_showSleeperBody = false;
        }

        //not present in CompChangeableProjectiles
        public void DrawBed() 
        {
            if (blanketDef != null)
            {
                Building_Blanket blanket = bedding as Building_Blanket;
                if (parent.DrawColorTwo == parent.DrawColor)
                {
                    blanket.colorTwo = BlanketColor;
                }
                else
                {
                    blanket.colorTwo = parent.Graphic.colorTwo;
                }
                bedding.Graphic.Draw(parent.DrawPos + Altitudes.AltIncVect, parent.Rotation, bedding);
            }
        }

        //modified from CompChangeableProjectiles to accept stuff
        public Thing RemoveBedding(ThingDef stuff)
        {
            Thing thing = ThingMaker.MakeThing(loadedBedding, stuff);
            loaded = false;
            loadedBedding = null;
            return thing;
        }

	}
}