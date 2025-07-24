using UnityEngine;
using Verse;

namespace SoftWarmBeds;

public class Building_Blanket : ThingWithComps
{
    private readonly Color linenDelta = new(1f, 1f, 1f);
    public Color colorTwo;

    private static float DeltaFactor => Mathf.Round(SoftWarmBedsSettings.ColorWash * 10) / 10;

    public override Color DrawColorTwo => Color.Lerp(colorTwo, linenDelta, DeltaFactor);
}