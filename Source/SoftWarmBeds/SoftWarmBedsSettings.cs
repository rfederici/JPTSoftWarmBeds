using UnityEngine;
using Verse;

namespace SoftWarmBeds;

public class SoftWarmBedsSettings : ModSettings
{
    public static ColorDisplayOption colorDisplayOption = ColorDisplayOption.Pillow;

    public static float colorWash = 0.4f;

    public static bool manuallyUnmakeBed;

    public static void DoWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        if (listing.RadioButton("ColorDisplayOptionPillowLabel".Translate(),
                colorDisplayOption == ColorDisplayOption.Pillow, 0, "ColorDisplayOptionPillowTooltip".Translate()))
        {
            colorDisplayOption = ColorDisplayOption.Pillow;
        }

        if (listing.RadioButton("ColorDisplayOptionBlanketLabel".Translate(),
                colorDisplayOption == ColorDisplayOption.Blanket, 0, "ColorDisplayOptionBlanketTooltip".Translate()))
        {
            colorDisplayOption = ColorDisplayOption.Blanket;
        }

        listing.Gap();
        var colorWashPercent = (int)(Mathf.Round(colorWash * 10) * 10);
        switch (colorWash)
        {
            case < 0.05f:
                listing.Label($"{"ColorWashLevel".Translate() + ": "}{colorWashPercent}% (" +
                              "ColorWashNone".Translate() +
                              ")");
                break;
            case > 0.35f and < 0.45f:
                listing.Label($"{"ColorWashLevel".Translate() + ": "}{colorWashPercent}% (" +
                              "ColorWashNormal".Translate() + ")");
                break;
            case > 0.95f:
                listing.Label($"{"ColorWashLevel".Translate() + ": "}{colorWashPercent}% (" +
                              "ColorWashTotal".Translate() + ")");
                break;
            default:
                listing.Label($"{"ColorWashLevel".Translate() + ": "}{colorWashPercent}%");
                break;
        }

        colorWash = listing.Slider(colorWash, 0f, 1f);
        listing.CheckboxLabeled("manuallyUnmakeBed".Translate(), ref manuallyUnmakeBed,
            "manuallyUnmakeBedTooltip".Translate());
        listing.Gap();
        if (listing.ButtonText("Reset"))
        {
            colorDisplayOption = ColorDisplayOption.Pillow;
            colorWash = 0.4f;
            manuallyUnmakeBed = false;
        }

        if (SoftWarmBedsMod.currentVersion != null)
        {
            listing.Gap();
            GUI.contentColor = Color.gray;
            listing.Label("CurrentModVersion_Label".Translate(SoftWarmBedsMod.currentVersion));
            GUI.contentColor = Color.white;
        }

        listing.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref colorDisplayOption, "colorDisplayOption");
        Scribe_Values.Look(ref colorWash, "colorWash", 0.4f);
        Scribe_Values.Look(ref manuallyUnmakeBed, "manuallyUnmakeBed");
        base.ExposeData();
    }
}