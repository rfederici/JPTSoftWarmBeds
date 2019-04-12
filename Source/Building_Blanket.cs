//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    public class Building_Blanket : ThingWithComps
    {
        public Color colorTwo;

        private Color linenDelta = new Color(1f, 1f, 1f);

        private float deltaFactor = 0.4f;

        public Color sheetColor;

        public bool hasColor = false;

        public override Color DrawColorTwo
        {
            get
            {
                if (!hasColor)
                {
                    return base.DrawColorTwo;
                }
                this.sheetColor = Color.Lerp(colorTwo, linenDelta, deltaFactor);
                return this.sheetColor;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.hasColor, "hasColor", false, false);
        }
    }
}
