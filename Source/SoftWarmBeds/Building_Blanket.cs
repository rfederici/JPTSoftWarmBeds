using UnityEngine;
using Verse;

namespace SoftWarmBeds;

public class Building_Blanket : ThingWithComps
{
    public Color colorTwo;

    private readonly Color linenDelta = new Color(1f, 1f, 1f);

    public float deltaFactor => Mathf.Round(SoftWarmBedsSettings.colorWash * 10) / 10;

    public override Color DrawColorTwo => Color.Lerp(colorTwo, linenDelta, deltaFactor);
}