using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace SoftWarmBeds
{
    //Instruction to draw pawn body when on a bed that's unmade
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt", new Type[] { typeof(Vector3) })]
    public static class PawnOnBedRenderer_Patch
    {
        public static void Postfix(PawnRenderer __instance, Pawn ___pawn, Vector3 drawLoc)
        {
            Building_Bed softWarmBed = ___pawn.CurrentBed();
            CompMakeableBed bedComp = softWarmBed.TryGetComp<CompMakeableBed>();
            if (___pawn.RaceProps.Humanlike && bedComp != null)//___pawn.CurrentBed() is Building_SoftWarmBed)
            {
                if (!bedComp.Loaded)
                {
                    // Thanks to @Zamu & @Mehni!
                    var rotDrawMode = (RotDrawMode)AccessTools.Property(typeof(PawnRenderer), "CurRotDrawMode").GetGetMethod(true).Invoke(__instance, new object[0]);
                    MethodInfo layingFacing = AccessTools.Method(type: typeof(PawnRenderer), name: "LayingFacing");
                    Rot4 rot = (Rot4)layingFacing.Invoke(__instance, new object[0]);
                    float angle;
                    Vector3 rootLoc;
                    Rot4 rotation = softWarmBed.Rotation;
                    rotation.AsInt += 2;
                    angle = rotation.AsAngle;
                    AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)softWarmBed.def.altitudeLayer, 15);
                    Vector3 vector2 = ___pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
                    Vector3 vector3 = vector2;
                    vector3.y += 0.02734375f;
                    float d = -__instance.BaseHeadOffsetAt(Rot4.South).z;
                    Vector3 a = rotation.FacingCell.ToVector3();
                    rootLoc = vector2 + a * d;
                    rootLoc.y += 0.0078125f;
                    MethodInfo renderPawnInternal = AccessTools.Method(type: typeof(PawnRenderer), name: "RenderPawnInternal", parameters: new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) });
                    renderPawnInternal.Invoke(__instance, new object[] { rootLoc, angle, true, rot, rot, rotDrawMode, false, false, false });
                }
            }
        }
    }

    //Makes bed thoughts consider the bed stats when judging confortable temperature (while juggling Hospitality functions!)
    [HarmonyPatch(typeof(Toils_LayDown), "ApplyBedThoughts")]
    [HarmonyBefore(new string[] { "HugsLib.Hospitality" })]
    public class ApplyBedThoughts_Patch
    {
        static bool isHospitalityLoaded = false;

        // reflection cache
        static Type compGuestType;
        static MethodInfo getCompGuestInfo;
        static MethodInfo isGuestInfo;
        static Func<object, object> getCompGuestBed;

        public static void InitializeHospitalityReflections() 
        {
            if (isHospitalityLoaded) return;
            isHospitalityLoaded = true;
            compGuestType = AccessTools.TypeByName("Hospitality.CompGuest");
            getCompGuestInfo = AccessTools.Method(typeof(ThingWithComps), "GetComp").MakeGenericMethod(compGuestType);
            isGuestInfo = AccessTools.Method("Hospitality.GuestUtility:IsGuest", new[] { typeof(Pawn), typeof(bool) });
            var param = Expression.Parameter(typeof(object));
            var fieldExp = Expression.Field(
                Expression.Convert(param, compGuestType),
                compGuestType.GetField("bed", BindingFlags.Public | BindingFlags.Instance));
            getCompGuestBed = Expression.Lambda<Func<object, object>>(Expression.Convert(fieldExp, typeof(object)), param).Compile();
        }

        public static bool Prefix(object __instance, Pawn actor)
        {
            bool result;
            if (actor.needs.mood == null)
            {
                result = false;
            }
            else if (actor.CurrentBed() == null)
            {
                result = true;
            }
            else
            {
                //Log.Message("ApplyBedThoughts is relevant for " + actor);
                Building_Bed bed = new Building_Bed();
                bed = actor.CurrentBed();
                bool hospitalityIsGuest = false;
                if (isHospitalityLoaded)
                {
                    //Correcting for Hospitality 1.0.25
                    hospitalityIsGuest = AddedBedIsOwned(__instance, actor, bed);
                    //
                }
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
                if (actor.GetRoom(RegionType.Set_Passable).PsychologicallyOutdoors)
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside, null);
                }
                if (bed == null || bed.CostListAdjusted().Count == 0)
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround, null);
                }
                //taking bed insulation into consideration:
                //Log.Message("bedStats: cold = " + bed.GetStatValue(BedInsulationCold.Bed_Insulation_Cold, true)+", heat ="+ bed.GetStatValue(BedInsulationHeat.Bed_Insulation_Heat));
                float minTempInBed = actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) - bed.GetStatValue(BedInsulationCold.Bed_Insulation_Cold, true);
                float maxTempInBed = actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null) + bed.GetStatValue(BedInsulationHeat.Bed_Insulation_Heat, true);
                if (actor.AmbientTemperature < minTempInBed)
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold, null);
                    //Log.Message("Temperature is " + actor.AmbientTemperature + " and bed insulation is " + minTempInBed + ",  so " + actor + " gains cold memory");
                }
                if (actor.AmbientTemperature > maxTempInBed)
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat, null);
                    //Log.Message("Temperature is " + actor.AmbientTemperature + " and bed insulation is " + maxTempInBed + ",  so " + actor + " gains heat memory");
                }
                //adapted for hospitality:
                if (bed != null && (hospitalityIsGuest || bed == actor.ownership.OwnedBed) && !bed.ForPrisoners && !actor.story.traits.HasTrait(TraitDefOf.Ascetic))
                {
                    ThoughtDef thoughtDef = null;
                    Room room = bed.GetRoom();

                    //Correcting for Hospitality 1.0.25
                    if (hospitalityIsGuest)
                    {
                        bool hospitalityOnlyOneBed = room.ContainedBeds.Count() == 1;
                        if (room.Role == DefDatabase<RoomRoleDef>.GetNamed("GuestRoom", true))// easier to replicate GuestUtility.roleDefGuestRoom than reflecting it!
                        {
                            thoughtDef = hospitalityOnlyOneBed ? ThoughtDefOf.SleptInBedroom : ThoughtDefOf.SleptInBarracks;
                        }
                    }
                    else if (room.Role == RoomRoleDefOf.Bedroom)
                    {
                        thoughtDef = ThoughtDefOf.SleptInBedroom;
                    }
                    else if (room.Role == RoomRoleDefOf.Barracks)
                    {
                        thoughtDef = ThoughtDefOf.SleptInBarracks;
                    }
                    if (thoughtDef != null)
                    {
                        int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(bed.GetRoom(RegionType.Set_Passable).GetStat(RoomStatDefOf.Impressiveness));
                        if (thoughtDef.stages[scoreStageIndex] != null)
                        {
                            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex), null);
                        }
                    }
                }
                result = false;
            }
            return result;
        }

        //corrected for Hospitality 1.0.34
        private static bool AddedBedIsOwned(object __instance, Pawn pawn, Building_Bed building_Bed)
        {
            bool isGuest = (bool)isGuestInfo.Invoke(__instance, new object[] { pawn , true });
            return isGuest ? (GetGuestBed(__instance, pawn) == building_Bed) : (building_Bed == pawn.ownership.OwnedBed);
        }

        private static Building_Bed GetGuestBed(object __instance, Pawn pawn)
        {
            var comp = getCompGuestInfo.Invoke(pawn, new object[] { });
            Building_Bed bed = (Building_Bed)getCompGuestBed(comp);

            //Log.Message("Calculated GetGuestBed is " + bed.ToString());
            return (comp != null) ? bed : null;
        }
    }

    //Makes the bed stats cover for a possible lack of apparel when calculating comfortable temperature range for pawns in bed
    [HarmonyPatch(typeof(GenTemperature), "ComfortableTemperatureRange", new Type[] { typeof(Pawn) })]
    public class ComfortableTemperatureRange_Patch
    {
        public static void Postfix(Pawn p, ref FloatRange __result)
        {
            if (p.InBed())
            {
                Building_Bed bed = p.CurrentBed();
                float InsulationCold = bed.GetStatValue(BedInsulationCold.Bed_Insulation_Cold, true);
                float InsulationHeat = bed.GetStatValue(BedInsulationHeat.Bed_Insulation_Heat, true);
                if (InsulationCold != 0 || InsulationHeat != 0)
                {
                    ThingDef raceDef = p.kindDef.race;
                    FloatRange altResult = new FloatRange(raceDef.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null), raceDef.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null));
                    altResult.min -= InsulationCold;
                    altResult.max += InsulationHeat;
                    if (__result.min > altResult.min) __result.min = altResult.min;
                    if (__result.max < altResult.max) __result.max = altResult.max;
                    //Log.Message(bed+" insulation cold is "+bed.GetStatValue(BedInsulationCold.Bed_Insulation_Cold, true));
                    //Log.Message("comfortable range modified for " + p + " by bed" + bed + ": " + __result.ToString());
                }
            }
        }
    }

    //Adjusts the info report on comfortable temperatures - explanation part
    [HarmonyPatch(typeof(StatPart_ApparelStatOffset), "ExplanationPart")]
    public class ExplanationPart_Patch
    {
        public static string Postfix(string original, StatRequest req, StatDef ___apparelStat)
        {
            if (req.HasThing && req.Thing != null)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && pawn.InBed())
                {
                    StringBuilder alteredText = new StringBuilder();
                    bool subtract = ___apparelStat == StatDefOf.Insulation_Cold;
                    StatDef modifier = subtract ? BedInsulationCold.Bed_Insulation_Cold : BedInsulationHeat.Bed_Insulation_Heat;
                    float bedStatValue = pawn.CurrentBed().GetStatValue(modifier, true);
                    float bedOffset = subtract ? bedStatValue * -1 : bedStatValue;
                    string signal = subtract ? null : "+";
                    alteredText.AppendLine("StatsReport_BedInsulation".Translate() + ": " + signal + bedOffset.ToStringTemperature());
                    return alteredText.ToString();
                }
            }
            return original;
        }
    }

    //Adjusts the info report on comfortable temperatures - final value
    [HarmonyPatch(typeof(StatsReportUtility), "StatsToDraw", new Type[] { typeof(Thing) })]
    public class StatsToDraw_Patch
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> original, Thing thing)
        {
            foreach (StatDrawEntry entry in original)
            {
                Pawn pawn = thing as Pawn;
                StatDef statDef = entry.stat;
                if (pawn != null && pawn.InBed() && (statDef == StatDefOf.ComfyTemperatureMin || statDef == StatDefOf.ComfyTemperatureMax))
                {
                    if (!statDef.Worker.IsDisabledFor(thing))
                    {
                        //sneak transform:
                        float statValue = statDef.Worker.GetValueUnfinalized(StatRequest.For(thing), true);
                        bool subtract = statDef == StatDefOf.ComfyTemperatureMin;
                        StatDef modifier = subtract ? BedInsulationCold.Bed_Insulation_Cold : BedInsulationHeat.Bed_Insulation_Heat;
                        float bedStatValue = pawn.CurrentBed().GetStatValue(modifier, true);
                        float bedOffset = subtract ? bedStatValue * -1 : bedStatValue;
                        statValue += bedOffset;
                        //
                        if (statDef.showOnDefaultValue || statValue != statDef.defaultBaseValue)
                        {
                            yield return new StatDrawEntry(statDef.category, statDef, statValue, StatRequest.For(thing), ToStringNumberSense.Undefined, null, false);
                        }
                    }
                    else
                    {
                        yield return new StatDrawEntry(statDef.category, statDef);
                    }
                }
                else
                {
                    yield return entry;
                }
            }
        }
    }

    //Adjusts the info report on comfortable temperatures - InfoCard
    [HarmonyPatch(typeof(StatPart_ApparelStatOffset), "GetInfoCardHyperlinks")]
    public class GetInfoCardHyperlinks_Patch
    {
        public static IEnumerable<Dialog_InfoCard.Hyperlink> Postfix(IEnumerable<Dialog_InfoCard.Hyperlink> original, StatRequest req, StatDef ___apparelStat)
        {
            if (req.HasThing && req.Thing != null)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && pawn.InBed())
                {
                    yield return new Dialog_InfoCard.Hyperlink(pawn.CurrentBed(), -1);
                }
            }
            yield break;
        }
    }

    //Adds info on used bedding material to the inspector pane
    [HarmonyPatch(typeof(Building_Bed), "GetInspectString")]
    public class GetInspectString_Patch
    {
        public static void Postfix(object __instance, ref string __result)
        {
            if (__instance is Building_Bed bed)
            {
                CompMakeableBed bedComp = bed.TryGetComp<CompMakeableBed>();
                if (bedComp != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine();
                    if (bedComp.Loaded)
                    {
                        stringBuilder.AppendLine("BedMade".Translate(bedComp.blanketStuff.LabelCap, bedComp.blanketStuff));
                    }
                    else
                    {
                        stringBuilder.AppendLine("BedNotMade".Translate());
                    }
                    __result += stringBuilder.ToString().TrimEndNewlines();
                }
            }
        }
    }

    //Command to draw the blanket
    [HarmonyPatch(typeof(Building), "Draw")]
    public class Draw_Patch
    {
        public static void Postfix(object __instance)
        {
            if (__instance is Building_Bed bed)
            {
                CompMakeableBed bedComp = bed.TryGetComp<CompMakeableBed>();
                if (bedComp != null && bedComp.loaded)
                {
                    bedComp.DrawBed();
                }
            }
        }
    }

    //Instructions to deal with the used bedding on de-spawn
    [HarmonyPatch(typeof(Building_Bed), "DeSpawn")]
    public class DeSpawn_Patch
    {
        public static void Prefix(object __instance)
        {
            if (__instance is Building_Bed bed)
            {
                CompMakeableBed bedComp = bed.TryGetComp<CompMakeableBed>();
                if (bedComp != null && bedComp.loaded && bedComp.NotTheBlanket)
                {
                    bedComp.Unmake();
                }
            }
        }
    }

    //Tweak to the bed's secondary color
    [HarmonyPatch(typeof(Building_Bed), "DrawColorTwo", MethodType.Getter)] // Wait for a Harmony fix!
    public class DrawColorTwo_Patch
    {
        public static void Postfix(object __instance, ref Color __result)
        {
            if (__instance is Building_Bed bed)
            {
                CompMakeableBed bedComp = bed.TryGetComp<CompMakeableBed>();
                if (bedComp != null || bed.def.MadeFromStuff) // unmakeable non-stuffed beds aren't affected
                {
                    bool forPrisoners = bed.ForPrisoners;
                    bool medical = bed.Medical;
                    bool invertedColorDisplay = (SoftWarmBedsSettings.colorDisplayOption == ColorDisplayOption.Blanket);
                    if (!forPrisoners && !medical && !invertedColorDisplay)
                    {
                        if (bedComp != null && bedComp.loaded && bedComp.blanketDef == null) // bedding color for beds that are made
                        {
                            __result = bedComp.blanketStuff.stuffProps.color;
                        }
                        else if (bed.def.MadeFromStuff) // stuff color for umade beds & bedrolls
                        {
                            __result = bed.DrawColor;
                        }
                    }
                }
            }
        }
    }

    //Command to update the blanket color when needed
    [HarmonyPatch(typeof(Thing), "Notify_ColorChanged")]
    public class Notify_ColorChanged_Patch
    {
        public static void Postfix(object __instance)
        {
            if (__instance is Building_Bed bed)
            {
                CompMakeableBed bedComp = bed.TryGetComp<CompMakeableBed>();
                if (bedComp != null && bedComp.loaded && bedComp.blanketDef != null)
                {
                    bedComp.bedding.Notify_ColorChanged();
                }
            }
        }
    }

    //Interface to Hospitality for seamless guest bed switching
    [StaticConstructorOnStartup]
    public static class Hospitality_Patch
    {
        static Hospitality_Patch()
        {
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("Orion.Hospitality")))
            {
                var harmonyInstance = new Harmony("JPT_SoftWarmBeds.Hospitality");

                Log.Message("[SoftWarmBeds] Hospitality detected! Adapting...");

                harmonyInstance.Patch(original: AccessTools.Method("Hospitality.Building_GuestBed:Swap", new[] { typeof(Building_Bed) }),
                    prefix: new HarmonyMethod(typeof(Hospitality_Patch), nameof(SwapPatch)), postfix: null, transpiler: null);

                harmonyInstance.Patch(original: AccessTools.Method("Hospitality.Building_GuestBed:GetInspectString"),
                    prefix: null, postfix: new HarmonyMethod(typeof(GetInspectString_Patch), nameof(GetInspectString_Patch.Postfix)), transpiler: null);

                ApplyBedThoughts_Patch.InitializeHospitalityReflections();
            }
        }

        public static bool SwapPatch(object __instance, Building_Bed bed)
        {
            CompMakeableBed bedComp = bed.TryGetComp<CompMakeableBed>();
            if (bedComp != null)
            {
                bedComp.NotTheBlanket = false;
                Swap(__instance, bed, bedComp.settings, bedComp);
                return false;
            }
            return true;
        }

        public static void Swap(object __instance, Building_Bed bed, StorageSettings settings, CompMakeableBed compMakeable)
        {
            ThingDef bedLoadedBedding = null;
            Thing bedBedding = null;
            if (compMakeable != null)
            {
                if (compMakeable.Loaded)
                {
                    bedLoadedBedding = compMakeable.loadedBedding;
                    bedBedding = compMakeable.bedding;
                }
            }
            //reflection info
            Type guestBed = AccessTools.TypeByName("Hospitality.Building_GuestBed");
            MethodInfo makeBedinfo = AccessTools.Method(guestBed, "MakeBed", new[] { typeof(Building_Bed), typeof(string) });
            //
            Building_Bed newBed;
            string newName;
            if (bed.GetType() == guestBed)
            {
                newName = bed.def.defName.Split(new string[] { "Guest" }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            else
            {
                newName = bed.def.defName + "Guest";
            }
            // Thanks again to @Zamu for figuring out it was actually very simple!
            newBed = (Building_Bed)makeBedinfo.Invoke(__instance, new object[] { bed, newName });
            newBed.SetFactionDirect(bed.Faction);
            var spawnedBed = (Building_Bed)GenSpawn.Spawn(newBed, bed.Position, bed.Map, bed.Rotation);
            spawnedBed.HitPoints = bed.HitPoints;
            spawnedBed.ForPrisoners = bed.ForPrisoners;
            var SpawnedCompQuality = spawnedBed.TryGetComp<CompQuality>();
            if (SpawnedCompQuality != null) SpawnedCompQuality.SetQuality(bed.GetComp<CompQuality>().Quality, ArtGenerationContext.Outsider);
            var SpawnedCompMakeable = spawnedBed.TryGetComp<CompMakeableBed>();
            if (SpawnedCompMakeable != null)
            {
                SpawnedCompMakeable.settings = settings;
                if (bedLoadedBedding != null)
                {
                    SpawnedCompMakeable.LoadBedding(bedLoadedBedding, bedBedding);
                }
            }
            Find.Selector.Select(spawnedBed, false, true);
        }
    }

    //Preventing people without beds from using the default BedRestEffectiveness value (80%). Switching to the minimun value instead. 
    [HarmonyPatch(typeof(Need_Rest), "TickResting")]
    public class Need_Rest_Patch
    {
        public static bool Prefix(float restEffectiveness, Pawn ___pawn)
        {
            if (___pawn.RaceProps.Humanlike && ___pawn.CurrentBed() == null && ___pawn.Faction != null && ___pawn.Faction.IsPlayer && restEffectiveness == StatDefOf.BedRestEffectiveness.valueIfMissing)
            {
                ___pawn.needs.rest.TickResting(StatDefOf.BedRestEffectiveness.minValue);
                return false;
            }
            return true;
        }
    }
	
	//Preventing Beddings from being targeted by apparel recipes regardless of not being either tainted or clean.
    //Solution by NanoCE
	[HarmonyPatch(typeof(SpecialThingFilterWorker_NonDeadmansApparel), "Matches")]
	public static class Patch_SpecialThingFilterWorkerMatches
	{
		public static void Postfix(ref bool __result, ref Thing t)
		{
			if(t != null && t is Bedding)
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(SpecialThingFilterWorker_DeadmansApparel), "Matches")]
	public static class Patch_SpecialThingFilterWorker_DeadmansApparel
	{
		public static void Postfix(ref bool __result, ref Thing t)
		{
			if (t != null && t is Bedding)
			{
				__result = false;
			}
		}
	}

    //Prevents VFE - Vikings Beds from curing Hypotermia if they're not made
    [StaticConstructorOnStartup]
    public static class VFEV_Patch
    {
        static VFEV_Patch()
        {
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing.StartsWith("OskarPotocki.VFE.Vikings")))
            {
                var harmonyInstance = new Harmony("JPT_SoftWarmBeds.VFEV");

                Log.Message("[SoftWarmBeds] Vanilla Factions Expanded - Vikings detected! Adapting...");

                harmonyInstance.Patch(original: AccessTools.Method("VFEV.CompCureHypothermia:CompTickRare"),
                    prefix: new HarmonyMethod(typeof(VFEV_Patch), nameof(Prefix)), postfix: null, transpiler: null);
            }
        }

        public static bool Prefix(object __instance)
        {
            if (__instance is ThingComp compInstance)
            {
                if (compInstance.parent is Building_Bed bed)
                {
                    CompMakeableBed bedComp = bed.TryGetComp<CompMakeableBed>();
                    if (bedComp != null)
                    {
                        return !bedComp.loaded;
                    }
                }
            }
            return true;
        }
    }
}