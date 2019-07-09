using Harmony;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SoftWarmBeds
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            
            try
            {
                ((Action)(() =>
                {

                    if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Hospitality"))
                    {
                        HarmonyInstance.DEBUG = true;
                        HarmonyInstance harmonyInstance = HarmonyInstance.Create("JPT_SoftWarmBeds.Hospitality");
                        Log.Message("[SoftWarmBeds] Hospitality detected! Adapting...");

                        harmonyInstance.Patch(original: AccessTools.Method(type: typeof(Building_Bed), name: "GetGizmos"),
                            prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Building_Bed_GetGizmos_Postfix)), transpiler: null);

                        //harmonyInstance.Patch(original: AccessTools.Method(type: typeof(Building_Bed), name: "ForPrisoners"),
                        //    prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Building_Bed_ForPrisoners_Postfix)), transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName(name: "Hospitality.Harmony.Building_Bed_Patch+GetGizmos"), name: "Postfix"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(Building_Bed_GetGizmos_Postfix_Prefix)), postfix: null, transpiler: null);

                        //harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.Harmony.Building_Bed_Patch+ForPrisoners"), name: "Postfix"),
                        //    prefix: new HarmonyMethod(type: patchType, name: nameof(Building_Bed_ForPrisoners_Postfix_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.Hospitality_SpecialInjector"), name: "CreateGuestBedDefs"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(SpecialInjector_CreateGuestBedDefs_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.GuestUtility"), name: "FindBedFor"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(GuestUtility_FindBedFor_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.GuestUtility"), name: "GetGuestBeds"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(GuestUtility_GetGuestBeds_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.GuestUtility"), name: "BedCheck"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(GuestUtility_BedCheck_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.Harmony.RestUtility_Patch+CanUseBedEver"), name: "Postfix"), prefix: null, postfix: null,
                            transpiler: new HarmonyMethod(type: patchType, name: nameof(Harmony_RestUtility_Patch_CanUseBedEver_Transpiler)));

                        //harmonyInstance.Patch(original: AccessTools.Method(type: patch6, name: "Replacement"),
                        //    prefix: new HarmonyMethod(type: patchType, name: nameof(Harmony_Toils_LayDown_Patch_ApplyBedThoughts_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.IncidentWorker_VisitorGroup"), name: "CheckCanCome"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(IncidentWorker_VisitorGroup_CheckCanCome_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.IncidentWorker_VisitorGroup"), name: "ShowAskMayComeDialog"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(IncidentWorker_VisitorGroup_ShowAskMayComeDialog_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.JobGiver_Sleep"), name: "TryIssueJobPackage"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(JobGiver_Sleep_TryIssueJobPackage_Prefix)), postfix: null, transpiler: null);

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.RoomRoleWorker_GuestRoom"), name: "GetScore"), prefix: null, postfix: null,
                            transpiler: new HarmonyMethod(type: patchType, name: nameof(RoomRoleWorker_GuestRoom_GetScore_Transpiler)));

                        harmonyInstance.Patch(original: AccessTools.Method(type: AccessTools.TypeByName("Hospitality.ThoughtWorker_Beds"), name: "CurrentStateInternal"),
                            prefix: new HarmonyMethod(type: patchType, name: nameof(ThoughtWorker_Beds_CurrentStateInternal_Prefix)), postfix: null, transpiler: null);

                    }
                }))();
            }
            catch (TypeLoadException ex) { }

        }

        //replaces Hospitality Gizmo
        //[HarmonyAfter(new string[] { "HugsLib.Hospitality" })]
        public static void Building_Bed_GetGizmos_Postfix(Building_Bed __instance, ref IEnumerable<Gizmo> __result)
        {
            //if (__instance.TryGetComp<CompMakeableBed>() != null)
            //if (__instance is Building_SoftWarmGuestBed || __instance is Building_SoftWarmBed)
            //{
                __result = Process(__instance, __result);
            //}
            //else
            //{
            //    //start reflection info
            //    var type = AccessTools.TypeByName("Hospitality.Harmony.Building_Bed_Patch+GetGizmos");
            //    MethodInfo process = AccessTools.Method(type, "Process", new[] { typeof(Building_Bed), typeof(IEnumerable<Gizmo>)});
            //    //end reflection info
            //    __result = (IEnumerable<Gizmo>)process.Invoke(__instance, new object[] { __instance, __result });
            //}
        }

        private static IEnumerable<Gizmo> Process(Building_Bed __instance, IEnumerable<Gizmo> __result)
        {
            //Log.Message("Patching Building_Bed_GetGizmos_Prefix");
            if (!__instance.ForPrisoners && !__instance.Medical && __instance.def.building.bed_humanlike)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandBedSetAsGuestLabel".Translate(),
                    defaultDesc = "JP Esteve Aqui",//"CommandBedSetAsGuestDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/AsGuest"),
                    isActive = () => false,
                    toggleAction = () => Building_SoftWarmGuestBed.Swap(__instance),
                    hotKey = KeyBindingDefOf.Misc4
                };
            }
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }
        }

        //Cancels Hospitality Gizmo postfix:
        public static bool Building_Bed_GetGizmos_Postfix_Prefix(Building_Bed __instance)//, ref IEnumerable<Gizmo> __result)
        {
            return false;
        }
          
        public static void Building_Bed_ForPrisoners_Postfix(Building_Bed __instance)
        {
            if (!__instance.ForPrisoners) return;

            if (__instance is Building_SoftWarmGuestBed)
            {
                Building_SoftWarmGuestBed.Swap(__instance);
            }
        }

        public static bool Building_Bed_ForPrisoners_Postfix_Prefix(Building_Bed __instance)
        {
            return false;
        }

        public static bool SpecialInjector_CreateGuestBedDefs_Prefix(ThingDef[] bedDefs, CompProperties_Facility[] facilities, object __instance)
        {
            StringBuilder stringBuilder = new StringBuilder("Created (soft & warm) guest beds for the following beds: ");
            FieldInfo[] fields = typeof(ThingDef).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (ThingDef thingDef in bedDefs)
            {
                ThingDef thingDef2 = new ThingDef();
                foreach (FieldInfo fieldInfo in fields)
                {
                    fieldInfo.SetValue(thingDef2, fieldInfo.GetValue(thingDef));
                }
                MethodInfo copyComps = AccessTools.Method("Hospitality_SpecialInjector:CopyComps", new[] { typeof(ThingDef), typeof(ThingDef) });
                copyComps.Invoke(__instance, new object[] { thingDef2, thingDef });
                ThingDef thingDef3 = thingDef2;
                thingDef3.defName += "Guest";
                thingDef2.label = "GuestBedFormat".Translate(thingDef2.label);
                thingDef2.thingClass = typeof(Building_SoftWarmGuestBed);
                thingDef2.shortHash = 0;
                thingDef2.minifiedDef = null;
                thingDef2.tradeability = Tradeability.None;
                thingDef2.scatterableOnMapGen = false;
                typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[]
                {
            thingDef2,
            typeof(ThingDef)
                });
                DefDatabase<ThingDef>.Add(thingDef2);
                stringBuilder.Append(thingDef.defName + ", ");
                foreach (CompProperties_Facility compProperties_Facility in facilities)
                {
                    compProperties_Facility.linkableBuildings.Add(thingDef2);
                }
            }
            Log.Message(stringBuilder.ToString().TrimEnd(new char[] { ' ', ',' }), false);
            return false;
        }

        public static bool GuestUtility_FindBedFor_Prefix(Pawn __instance)
        {
            FindSoftWarmBedFor(__instance);
            return false;
        }

        public static Building_SoftWarmGuestBed FindSoftWarmBedFor(this Pawn pawn)
        {

            bool BedValidator(Thing t)
            {
                if (!(t is Building_SoftWarmGuestBed)) return false;
                if (!pawn.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some)) return false;
                var b = (Building_SoftWarmGuestBed)t;
                if (b.CurOccupant != null) return false;
                if (b.ForPrisoners) return false;
                Find.Maps.ForEach(m => m.reservationManager.ReleaseAllForTarget(b));
                return (!b.IsForbidden(pawn) && !b.IsBurning());
            }

            var bed = (Building_SoftWarmGuestBed)GenClosest.ClosestThingReachable(pawn.Position, pawn.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(pawn), 500f, BedValidator);
            return bed;
        }

        public static bool GuestUtility_GetGuestBeds_Prefix(Map __instance, Area area)
        {
            GetSoftWarmGuestBeds(__instance, area);
            return false;
        }

        public static IEnumerable<Building_SoftWarmGuestBed> GetSoftWarmGuestBeds(this Map map, Area area = null)
        {
            if (map == null) return new Building_SoftWarmGuestBed[0];
            if (area == null) return map.listerBuildings.AllBuildingsColonistOfClass<Building_SoftWarmGuestBed>();
            return map.listerBuildings.AllBuildingsColonistOfClass<Building_SoftWarmGuestBed>().Where(b => area[b.Position]);
        }

        public static bool GuestUtility_BedCheck_Prefix(Map map, object __instance)
        {
            SoftWarmBedCheck(map, __instance);
            return false;
        }

        public static bool SoftWarmBedCheck(Map map, object __instance)
        {
            if (map == null) return false;
            //start reflection info
            var type = AccessTools.TypeByName("Hospitality.Hospitality_MapComponent");
            MethodInfo mapComponent = AccessTools.Method(type, "Instance", new[] { typeof(Map) });
            var mapComp = mapComponent.Invoke(__instance, new object[] { map });
            FieldInfo refuseGuestsUntilWeHaveBedsInfo = AccessTools.Field(type, "refuseGuestsUntilWeHaveBeds");
            bool refuseGuestsUntilWeHaveBeds = (bool)refuseGuestsUntilWeHaveBedsInfo.GetValue(mapComp);
            //end reflection info
            //if (!mapComp.refuseGuestsUntilWeHaveBeds) return true;
            //replaced by reflection:
            if (!refuseGuestsUntilWeHaveBeds) return true;
            if (!map.listerBuildings.AllBuildingsColonistOfClass<Building_SoftWarmGuestBed>().Any()) return false;
            // We have beds now!
            refuseGuestsUntilWeHaveBeds = false;
            return true;
        }

        //public static bool Harmony_RestUtility_Patch_CanUseBedEver_Prefix(Pawn p, ThingDef bedDef, ref bool __result, object __instance)
        //{
        //    if (bedDef.thingClass == typeof(SoftWarmBeds.Building_SoftWarmGuestBed))
        //    {
        //        //start reflection info
        //        var type = AccessTools.TypeByName("Hospitality.GuestUtility");
        //        MethodInfo isGuest = AccessTools.Method(type, "IsGuest", new[] { typeof(Pawn) });
        //        bool pIsGuest = (bool)isGuest.Invoke(__instance, new object[] { p });
        //        //end reflection info

        //        __result &= pIsGuest;

        //    }
        //    return false;
        //}

        public static IEnumerable<CodeInstruction> Harmony_RestUtility_Patch_CanUseBedEver_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Log.Message("Transpiling Inject");
            Type bedInfo = AccessTools.TypeByName("Hospitality.Building_GuestBed");

            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            foreach (CodeInstruction instruction in codeInstructions)
            {
                //Log.Message("procurando linha " + OpCodes.Ldtoken + " " + bedInfo);
                if (instruction.opcode == OpCodes.Ldtoken && instruction.operand == bedInfo)
                {
                    //Log.Message("instruction found, patching " + bedInfo + " into " + AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmGuestBed"));
                    yield return new CodeInstruction(opcode: OpCodes.Ldtoken, operand: AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmGuestBed"));

                }
                else
                {
                    yield return instruction;
                }

            }
        }

        //Referência à Guest Bed removida!
        //public static bool Harmony_Toils_LayDown_Patch_ApplyBedThoughts_Prefix(Pawn ___pawn, object __instance)
        //{
        //    SoftWarmReplacement(___pawn, __instance);
        //    return false;
        //}


        //public static bool SoftWarmReplacement(Pawn actor, object __instance)
        //{
        //    bool result;
        //    if (actor.needs.mood == null)
        //    {
        //        result = false;
        //    }
        //    else
        //    {
        //        //start reflection info
        //        var type = AccessTools.TypeByName("Hospitality.GuestUtility");
        //        MethodInfo isGuest = AccessTools.Method(type, "IsGuest", new[] { typeof(Pawn) });
        //        bool actorIsGuest = (bool)isGuest.Invoke(__instance, new object[] { actor });
        //        //end reflection info
        //        Building_Bed building_Bed = actorIsGuest ? GetSoftWarmGuestBeds2(actor, __instance).FirstOrDefault<Building_SoftWarmGuestBed>() : actor.CurrentBed();
        //        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
        //        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
        //        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
        //        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
        //        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
        //        actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
        //        bool psychologicallyOutdoors = actor.GetRoom(RegionType.Set_Passable).PsychologicallyOutdoors;
        //        if (psychologicallyOutdoors)
        //        {
        //            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside, null);
        //        }
        //        bool flag2 = building_Bed == null || building_Bed.CostListAdjusted().Count == 0;
        //        if (flag2)
        //        {
        //            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround, null);
        //        }
        //        bool flag3 = actor.AmbientTemperature < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null);
        //        if (flag3)
        //        {
        //            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold, null);
        //        }
        //        bool flag4 = actor.AmbientTemperature > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null);
        //        if (flag4)
        //        {
        //            actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat, null);
        //        }
        //        //start reflection info
        //        var type2 = AccessTools.TypeByName("Hospitality.Toils_LayDown_Patch.ApplyBedThoughts");
        //        MethodInfo addedBedIsOwned = AccessTools.Method(type, "AddedBedIsOwned", new[] { typeof(Pawn), typeof(Building_Bed) });
        //        bool actorAddedBedIsOwned = (bool)addedBedIsOwned.Invoke(__instance, new object[] { actor, building_Bed });
        //        //end reflection info
        //        bool flag5 = building_Bed != null && actorAddedBedIsOwned && !building_Bed.ForPrisoners && !actor.story.traits.HasTrait(TraitDefOf.Ascetic);
        //        if (flag5)
        //        {
        //            ThoughtDef thoughtDef = null;
        //            bool flag6 = building_Bed.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.Bedroom;
        //            if (flag6)
        //            {
        //                thoughtDef = ThoughtDefOf.SleptInBedroom;
        //            }
        //            else
        //            {
        //                bool flag7 = building_Bed.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.Barracks;
        //                if (flag7)
        //                {
        //                    thoughtDef = ThoughtDefOf.SleptInBarracks;
        //                }
        //            }
        //            bool flag8 = thoughtDef != null;
        //            if (flag8)
        //            {
        //                int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom(RegionType.Set_Passable).GetStat(RoomStatDefOf.Impressiveness));
        //                bool flag9 = thoughtDef.stages[scoreStageIndex] != null;
        //                if (flag9)
        //                {
        //                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex), null);
        //                }
        //            }
        //        }
        //        result = false;
        //    }
        //    return result;
        //}

        public static IEnumerable<Building_SoftWarmGuestBed> GetSoftWarmGuestBeds(this Pawn pawn, object __instance)
        {
            IEnumerable<Building_SoftWarmGuestBed> result;
            if (pawn == null)
            {
                result = new Building_SoftWarmGuestBed[0];
            }
            else
            {
                //start reflection info
                var type = AccessTools.TypeByName("Hospitality.GuestUtility");
                MethodInfo getGuestArea = AccessTools.Method(type, "GetGuestArea", new[] { typeof(Pawn) });
                Area pawnGuestArea = (Area)getGuestArea.Invoke(__instance, new object[] { pawn });
                //end reflection info
                Area area = pawnGuestArea;
                if (area == null)
                {
                    result = pawn.MapHeld.listerBuildings.AllBuildingsColonistOfClass<Building_SoftWarmGuestBed>();
                }
                else
                {
                    result = from b in pawn.MapHeld.listerBuildings.AllBuildingsColonistOfClass<Building_SoftWarmGuestBed>()
                             where area[b.Position]
                             select b;
                }
            }
            return result;
        }

        public static bool IncidentWorker_VisitorGroup_CheckCanCome_Prefix(Map map, Faction faction, string reasons, object __instance)
        {
            SoftWarmCheckCanCome(map, faction, out reasons, __instance);
            return false;
        }

        private static bool SoftWarmCheckCanCome(Map map, Faction faction, out string reasons, object __instance)
        {
            //start reflection info
            var type = AccessTools.TypeByName("Hospitality.IncidentWorker_VisitorGroup");
            MethodInfo isFogged = AccessTools.Method(type, "IsFogged", new[] { typeof(Pawn) });
            //end reflection info
            bool flag = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout);
            Faction[] array = (from p in map.mapPawns.AllPawnsSpawned
                               where !p.Dead && !p.IsPrisoner && p.Faction != null && !p.Downed && !(bool)isFogged.Invoke(__instance, new object[] { p })
                               select p.Faction into p
                               where p.HostileTo(Faction.OfPlayer) || p.HostileTo(faction)
                               select p).ToArray<Faction>();
            bool flag2 = map.GameConditionManager.ConditionIsActive(GameConditionDefOf.VolcanicWinter);
            bool flag3 = faction.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) && faction.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp);
            bool flag4 = map.listerBuildings.AllBuildingsColonistOfClass<Building_SoftWarmGuestBed>().Any<Building_SoftWarmGuestBed>();
            reasons = null;
            bool flag5 = flag3 && !flag && !flag2 && !array.Any<Faction>() && flag4;
            bool result;
            if (flag5)
            {
                result = true;
            }
            else
            {
                List<string> list = new List<string>();
                bool flag6 = !flag4;
                if (flag6)
                {
                    list.Add("- " + "VisitorsArrivedReasonNoBeds".Translate());
                }
                bool flag7 = flag;
                if (flag7)
                {
                    list.Add("- " + GameConditionDefOf.ToxicFallout.LabelCap);
                }
                bool flag8 = flag2;
                if (flag8)
                {
                    list.Add("- " + GameConditionDefOf.VolcanicWinter.LabelCap);
                }
                bool flag9 = !flag3;
                if (flag9)
                {
                    list.Add("- " + "Temperature".Translate());
                }
                foreach (Faction faction2 in array)
                {
                    list.Add("- " + faction2.def.pawnsPlural.CapitalizeFirst());
                }
                reasons = list.Distinct<string>().Aggregate((string a, string b) => a + "\n" + b);
                result = false;
            }
            return result;
        }

        public static bool IncidentWorker_VisitorGroup_ShowAskMayComeDialog_Prefix(Faction faction, Map map, string reasons, Direction8Way spawnDirection, Action allow, Action refuse, object __instance)
        {
            SoftWarmShowAskMayComeDialog(faction, map, reasons, spawnDirection, allow, refuse, __instance);
            return false;
        }

        private static void SoftWarmShowAskMayComeDialog(Faction faction, Map map, string reasons, Direction8Way spawnDirection, Action allow, Action refuse, object __instance)
        {
            string text = "VisitorsArrivedDesc".Translate(faction, reasons);
            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("VisitorsArrivedAccept".Translate());
            diaOption.action = allow;
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            DiaOption diaOption2 = new DiaOption("VisitorsArrivedRefuse".Translate());
            diaOption2.action = refuse;
            diaOption2.resolveTree = true;
            diaNode.options.Add(diaOption2);
            bool flag = !map.listerBuildings.AllBuildingsColonistOfClass<Building_SoftWarmGuestBed>().Any<Building_SoftWarmGuestBed>();
            if (flag)
            {
                DiaOption diaOption3 = new DiaOption("VisitorsArrivedRefuseUntilBeds".Translate());
                diaOption3.action = delegate ()
                {
                    //start reflection info
                    var type = AccessTools.TypeByName("Hospitality.GuestUtility");
                    MethodInfo refuseGuestsUntilWeHaveBeds = AccessTools.Method(type, "RefuseGuestsUntilWeHaveBeds", new[] { typeof(Map) });
                    //end reflection info
                    refuseGuestsUntilWeHaveBeds.Invoke(__instance, new object[] { map });
                    refuse();
                };
                diaOption3.resolveTree = true;
                diaNode.options.Add(diaOption3);
            }
            string label = ((MapParent)map.ParentHolder).Label;
            string title = "VisitorsArrivedTitle".Translate(label, spawnDirection.LabelShort());
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, title));
        }

        public static bool JobGiver_Sleep_TryIssueJobPackage_Prefix(ThinkNode __instance, Pawn pawn, JobIssueParams jobParams)
        {
            SoftWarmTryIssueJobPackage(__instance, pawn, jobParams);
            return false;
        }

        public static ThinkResult SoftWarmTryIssueJobPackage(ThinkNode __instance, Pawn pawn, JobIssueParams jobParams)
        {
            ThinkResult result;
            if (pawn.CurJob != null)
            {
                result = new ThinkResult(pawn.CurJob, __instance, null, false);
            }
            else
            {
                Pawn_NeedsTracker needs = pawn.needs;
                //bool flag2 = ((needs != null) ? needs.rest : null) == null;
                if ((needs?.rest) == null)
                {
                    if (pawn.needs == null)
                    {
                        Log.ErrorOnce(pawn.Name.ToStringShort + " has no needs", 453636 + pawn.thingIDNumber, false);
                    }
                    if (pawn.needs.rest == null)
                    {
                        Log.ErrorOnce(pawn.Name.ToStringShort + " has no rest need", 357474 + pawn.thingIDNumber, false);
                    }
                    result = ThinkResult.NoJob;
                }
                else
                {
                    if (pawn.mindState == null)
                    {
                        Log.ErrorOnce(pawn.Name.ToStringShort + " has no mindstate", 23892 + pawn.thingIDNumber, false);
                        pawn.mindState = new Pawn_MindState(pawn);
                    }
                    if (Find.TickManager.TicksGame - pawn.mindState.lastDisturbanceTick < 400)
                    {
                        Log.Message(pawn.Name.ToStringShort + " can't sleep - got disturbed", false);
                        result = ThinkResult.NoJob;
                    }
                    else
                    {
                        Building_SoftWarmGuestBed building_GuestBed = FindSoftWarmBedFor(pawn);
                        if (building_GuestBed != null)
                        {
                            result = new ThinkResult(new Job(JobDefOf.LayDown, building_GuestBed), __instance, null, false);
                        }
                        else
                        {
                            IntVec3 c = CellFinder.RandomClosewalkCellNear(pawn.mindState.duty.focus.Cell, pawn.MapHeld, 4, null);
                            if (!pawn.CanReserve(c, 1, -1, null, false))
                            {
                                result = ThinkResult.NoJob;
                            }
                            else
                            {
                                result = new ThinkResult(new Job(JobDefOf.LayDown, c), __instance, null, false);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static IEnumerable<CodeInstruction> RoomRoleWorker_GuestRoom_GetScore_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Log.Message("Transpiling GetScore");
            Type bedInfo = AccessTools.TypeByName("Hospitality.Building_GuestBed");

            CodeInstruction[] codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            foreach (CodeInstruction instruction in codeInstructions)
            {
                //Log.Message("procurando linha " + OpCodes.Isinst + " " + bedInfo);
                if (instruction.opcode == OpCodes.Isinst && instruction.operand == bedInfo)
                {
                    //Log.Message("instruction found, patching " + bedInfo + " into " + AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmGuestBed"));
                    yield return new CodeInstruction(opcode: OpCodes.Isinst, operand: AccessTools.TypeByName("SoftWarmBeds.Building_SoftWarmGuestBed"));

                }
                else
                {
                    yield return instruction;
                }

            }
        }

        public static bool ThoughtWorker_Beds_CurrentStateInternal_Prefix(object __instance, Pawn pawn)
        {
            SoftWarmCurrentStateInternal(__instance, pawn);
            return false;
        }

        public static ThoughtState SoftWarmCurrentStateInternal(object __instance, Pawn pawn)
        {
            try
            {
                //start reflection info
                var guestUtility = AccessTools.TypeByName("Hospitality.GuestUtility");
                //end reflection info

                if (pawn == null) return ThoughtState.Inactive;
                if (pawn.thingIDNumber == 0) return ThoughtState.Inactive;

                if (Current.ProgramState != ProgramState.Playing)
                {
                    return ThoughtState.Inactive;
                }
                //start reflection info
                MethodInfo isGuest = AccessTools.Method(guestUtility, "IsGuest", new[] { typeof(Pawn) });
                bool pawnIsGuest = (bool)isGuest.Invoke(__instance, new object[] { pawn });
                //end reflection info
                if (!pawnIsGuest) return ThoughtState.Inactive;

                //replaced by reflection:
                Type type = AccessTools.TypeByName("Hospitality.CompGuest");
                MethodInfo getComp = AccessTools.Method(typeof(ThingWithComps), "GetComp", new[] { type });
                var compGuest = getComp.Invoke(pawn, new object[] { });
                //
                if (compGuest == null) return ThoughtState.Inactive;

                //replaced by reflection:
                FieldInfo compArrivedInfo = AccessTools.Field(type, "arrived");
                bool compArrived = (bool)compArrivedInfo.GetValue(compGuest);
                //
                if (!compArrived) return ThoughtState.Inactive;

                //start reflection info
                MethodInfo getGuestArea = AccessTools.Method(guestUtility, "GetGuestArea", new[] { typeof(Pawn) });
                Area area = (Area)getGuestArea.Invoke(__instance, new object[] { pawn });
                var thoughtWorker = AccessTools.TypeByName("Hospitality.ThoughtWorker_Beds");
                MethodInfo staysInArea = AccessTools.Method(thoughtWorker, "StaysInArea", new[] { typeof(Pawn), typeof(Area) });
                //end reflection info

                var visitors = pawn.MapHeld.lordManager.lords.Where(l => l?.ownedPawns != null).SelectMany(l => l.ownedPawns).Count(p => (bool)staysInArea.Invoke(__instance, new object[] { p, area }));//StaysInArea(p, area));
                var bedCount = pawn.MapHeld.GetSoftWarmGuestBeds(area).Count(b => b?.def.useHitPoints == true);

                if (bedCount == 0) return ThoughtState.ActiveAtStage(0);
                if (bedCount < visitors && !pawn.InBed()) return ThoughtState.ActiveAtStage(1);
                if (bedCount > visitors * 1.3f && bedCount > visitors + 3) return ThoughtState.ActiveAtStage(3);
                return ThoughtState.ActiveAtStage(2);
            }
            catch (Exception e)
            {
                Log.Warning(e.Message);
                return ThoughtState.Inactive;
            }
        }

    }
}



