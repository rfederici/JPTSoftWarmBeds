using UnityEngine;
using Verse;
using RimWorld;

namespace SoftWarmBeds
{
    public class Building_Blanket : ThingWithComps
    {
        public Color colorTwo;

        private Color linenDelta = new Color(1f, 1f, 1f);

        public float deltaFactor; //0.4f

        public Color sheetColor;

        public override Color DrawColorTwo
        {
            get
            {
                sheetColor = Color.Lerp(colorTwo, linenDelta, deltaFactor);
                return sheetColor;
            }
        }

        public override void Notify_ColorChanged()
        {
            base.Notify_ColorChanged();
            deltaFactor = Mathf.Round(SoftWarmBedsSettings.colorWash * 10) / 10;
        }

    }
}
