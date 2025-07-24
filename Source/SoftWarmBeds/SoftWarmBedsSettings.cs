using UnityEngine;
using Verse;

namespace SoftWarmBeds;

public class SoftWarmBedsSettings : ModSettings
{
    public static ColorDisplayOption ColorDisplayOption = ColorDisplayOption.Pillow;

    public static float ColorWash = 0.4f;

    public static bool ManuallyUnmakeBed;

    public static void DoWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        if (listing.RadioButton("ColorDisplayOptionPillowLabel".Translate(),
                ColorDisplayOption == ColorDisplayOption.Pillow, 0, "ColorDisplayOptionPillowTooltip".Translate()))
        {
            ColorDisplayOption = ColorDisplayOption.Pillow;
        }

        if (listing.RadioButton("ColorDisplayOptionBlanketLabel".Translate(),
                ColorDisplayOption == ColorDisplayOption.Blanket, 0, "ColorDisplayOptionBlanketTooltip".Translate()))
        {
            ColorDisplayOption = ColorDisplayOption.Blanket;
        }

        listing.Gap();
        var colorWashPercent = (int)(Mathf.Round(ColorWash * 10) * 10);
        switch (ColorWash)
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

        ColorWash = listing.Slider(ColorWash, 0f, 1f);
        listing.CheckboxLabeled("manuallyUnmakeBed".Translate(), ref ManuallyUnmakeBed,
            "manuallyUnmakeBedTooltip".Translate());
        listing.Gap();
        if (listing.ButtonText("Reset"))
        {
            ColorDisplayOption = ColorDisplayOption.Pillow;
            ColorWash = 0.4f;
            ManuallyUnmakeBed = false;
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
        Scribe_Values.Look(ref ColorDisplayOption, "colorDisplayOption");
        Scribe_Values.Look(ref ColorWash, "colorWash", 0.4f);
        Scribe_Values.Look(ref ManuallyUnmakeBed, "manuallyUnmakeBed");
        base.ExposeData();
    }
}