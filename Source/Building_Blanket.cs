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
                sheetColor = Color.Lerp(colorTwo, linenDelta, deltaFactor);
                return sheetColor;
            }
        }

    }
}
