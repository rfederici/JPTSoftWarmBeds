using System.Linq;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace SoftWarmBeds;

public class SoftWarmBedsMod : Mod
{
    public static string currentVersion;
    private SoftWarmBedsSettings settings;

    public SoftWarmBedsMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<SoftWarmBedsSettings>();

        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        SoftWarmBedsSettings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "SoftWarmBeds".Translate();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        if (Current.Game == null || Current.ProgramState != ProgramState.Playing)
        {
            return;
        }

        foreach (var bed in Find.Maps.SelectMany(map =>
                     map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>()))
        {
            bed.Notify_ColorChanged();
        }
    }
}