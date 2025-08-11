using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SoftWarmBeds;

public class CompMakeableBed : CompFlickable, IStoreSettingsParent
{
    private readonly FieldInfo baseSwitchOnIntInfo = AccessTools.Field(typeof(CompFlickable), "switchOnInt");

    private readonly FieldInfo baseWantSwitchInfo = AccessTools.Field(typeof(CompFlickable), "wantSwitchOn");
    private readonly Color blanketDefaultColor = new(1f, 1f, 1f);
    private ThingDef allowedBedding;
    private Thing blanket;
    private ThingDef blanketDef;
    private ThingDef blanketStuff;
    private float curRotationInt;
    private bool loaded;
    private ThingDef loadedBedding;
    private bool notTheBlanket = true;
    private StorageSettings settings;

    private bool SwitchOnInt
    {
        get => (bool)baseSwitchOnIntInfo.GetValue(this);
        set => baseSwitchOnIntInfo.SetValue(this, value);
    }

    private bool WantSwitchOn
    {
        get => (bool)baseWantSwitchInfo.GetValue(this);
        set => baseWantSwitchInfo.SetValue(this, value);
    }

    public bool Loaded => loaded;
    
    // Additional properties for external access
    public bool IsLoaded => loaded;

    private ThingDef LoadedBedding => loadedBedding;
    
    // Public properties for external access
    public ThingDef AllowedBedding => allowedBedding;
    public Thing Blanket => blanket;
    public ThingDef BlanketDef => blanketDef;
    public ThingDef BlanketStuff => blanketStuff;
    public bool NotTheBlanket 
    { 
        get => notTheBlanket; 
        set => notTheBlanket = value; 
    }
    public StorageSettings Settings => settings;
    
    // Public properties for Odyssey compatibility
    public ThingDef LoadedBeddingDef => loadedBedding;
    public ThingDef BlanketStuffDef => blanketStuff;

    public CompProperties_MakeableBed Props => (CompProperties_MakeableBed)props;

    private Building_Bed BaseBed => parent as Building_Bed;

    private float CurRotation
    {
        get => curRotationInt;
        set
        {
            curRotationInt = value;
            if (curRotationInt > 360f) curRotationInt -= 360f;
            if (curRotationInt < 0f) curRotationInt += 360f;
        }
    }

    private bool Occupied => BaseBed.CurOccupants != null;

    public void Notify_SettingsChanged()
    {
    }

    public bool StorageTabVisible => true;

    public StorageSettings GetParentStoreSettings()
    {
        return parent.def.building.fixedStorageSettings; //defaultStorageSettings;
    }

    public StorageSettings GetStoreSettings()
    {
        return settings;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in StorageSettingsClipboard.CopyPasteGizmosFor(settings)) yield return gizmo;

        if (!loaded) yield break;

        if (SoftWarmBedsSettings.ManuallyUnmakeBed)
        {
            Props.commandTexture = Props.beddingDef.graphicData.texPath;
            foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
        }
        else
        {
            var unmake = new Command_Action
            {
                defaultLabel = Props.commandLabelKey.Translate(),
                defaultDesc = Props.commandDescKey.Translate(),
                icon = LoadedBedding.uiIcon,
                iconAngle = LoadedBedding.uiIconAngle,
                iconOffset = LoadedBedding.uiIconOffset,
                iconDrawScale = GenUI.IconDrawScale(LoadedBedding),
                action = Unmake
            };
            yield return unmake;
        }
    }

    public override void CompTick()
    {
        if (!Loaded || settings.filter.Allows(blanketStuff)) return;
        if (!SoftWarmBedsSettings.ManuallyUnmakeBed || (SwitchOnInt && WantSwitchOn)) Unmake();
    }

    //not present in CompChangeableProjectiles
    private void drawBed()
    {
        if (blanketDef == null || blanket == null) return;

        if (blanket is Building_Blanket buildingBlanket)
        {
            if (SoftWarmBedsSettings.ColorDisplayOption == ColorDisplayOption.Blanket)
            {
                buildingBlanket.DrawColor = parent.Graphic.colorTwo;
                buildingBlanket.colorTwo = blanketStuff.stuffProps.color;
            }
            else
            {
                buildingBlanket.DrawColor = blanketStuff.stuffProps.color;
                buildingBlanket.colorTwo = parent.DrawColorTwo == parent.DrawColor
                    ? blanketDefaultColor
                    : parent.Graphic.colorTwo;
            }
        }

        blanket.Graphic.Draw(parent.DrawPos + Altitudes.AltIncVect, parent.Rotation, blanket);
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (Loaded) drawBed();
    }

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        allowedBedding = Props.beddingDef;
        blanketDef = Props.blanketDef;
        setUpStorageSettings();
    }

    //modified from CompChangeableProjectiles
    public void LoadBedding(Thing bedding)
    {
        loaded = true;
        loadedBedding = bedding.def;
        blanketStuff = bedding.Stuff;
        if (blanketDef != null)
        {
            blanket = ThingMaker.MakeThing(blanketDef, blanketStuff);
            if (BaseBed.Faction != null) drawBed();
        }

        parent.Notify_ColorChanged();
        WantSwitchOn = true;
        SwitchOnInt = true;
    }

    public void LoadBedding(ThingDef stuff)
    {
        var bedding = ThingMaker.MakeThing(Props.beddingDef, stuff);
        LoadBedding(bedding);
    }

    public void LoadBedding(ThingDef beddingDef, ThingDef stuff)
    {
            blanket = ThingMaker.MakeThing(blanketDef, blanketStuff);
            if (BaseBed.Faction != null)
            {
                drawBed();
            }
        }

        parent.Notify_ColorChanged();
        WantSwitchOn = true;
        SwitchOnInt = true;
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref loaded, "loaded");
        Scribe_Defs.Look(ref loadedBedding, "loadedBedding");
        Scribe_Deep.Look(ref blanket, "bedding");
        Scribe_Defs.Look(ref blanketStuff, "blanketStuff");
        Scribe_Deep.Look(ref settings, "settings", this);
        if (settings == null) setUpStorageSettings();
    }

    public override void PostSplitOff(Thing bedding)
    {
        if (blanketDef == null || blanket == null)
        if (blanketDef == null || blanket == null) return;

        if (blanket is not Building_Blanket buildingBlanket)
        {
            return;
        }
        if (blanket is not Building_Blanket buildingBlanket) return;

        buildingBlanket.colorTwo = parent.Graphic.colorTwo;
        parent.Notify_ColorChanged();
    }

    //modified from CompChangeableProjectiles to accept stuff
    private Thing removeBedding(ThingDef stuff)
    {
        var thing = ThingMaker.MakeThing(loadedBedding, stuff);
        loaded = false;
        loadedBedding = null;
        return thing;
    }

    private void setUpStorageSettings()
    {
        if (GetParentStoreSettings() == null) return;

        settings = new StorageSettings(this);
        settings.CopyFrom(GetParentStoreSettings());
    }

    public void Unmake()
    {
        if (SoftWarmBedsSettings.ManuallyUnmakeBed)
        {
            WantSwitchOn = !WantSwitchOn;
            FlickUtility.UpdateFlickDesignation(parent);
        }
        else doUnmake();
    }

    public override void ReceiveCompSignal(string signal)
    {
        if (SoftWarmBedsSettings.ManuallyUnmakeBed && Loaded && !WantSwitchOn)
        if (SoftWarmBedsSettings.ManuallyUnmakeBed && Loaded && !WantSwitchOn) doUnmake();
    }

    private void doUnmake()
    {
        var stuff = blanketStuff;
        GenPlace.TryPlaceThing(removeBedding(blanketStuff), BaseBed.Position, BaseBed.Map, ThingPlaceMode.Near);
        BaseBed.Notify_ColorChanged();
    }
}