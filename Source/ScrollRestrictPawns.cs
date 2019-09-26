using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class ScrollRestrictPawns
        {
            // TD Enhancement Pack
            static readonly List<Assembly> tdAssemblies = LoadedModManager.RunningMods.SingleOrDefault(x => x.Identifier == "1339135272")?.assemblies.loadedAssemblies;
            static readonly Type Td_Settings = tdAssemblies?.Select(x => x.GetType("TD_Enhancement_Pack.Settings")).SingleOrDefault(x => x != null);
            static readonly ModSettings tdSettings = AccessTools.Method(Td_Settings, "Get")?.Invoke(null, new object[] { }) as ModSettings;
            static readonly Type Td_MapComponent_AreaOrder = tdAssemblies?.Select(x => x.GetType("TD_Enhancement_Pack.MapComponent_AreaOrder")).SingleOrDefault(x => x != null);

            [HarmonyPatch(typeof(PawnColumnWorker_AllowedArea), "GetHeaderTip")]
            static class PawnColumnWorker_AllowedArea_GetHeaderTip_Patch
            {
                [HarmonyPostfix]
                [SuppressMessage("ReSharper", "UnusedParameter.Local")]
                static void GetHeaderTip(ref string __result, PawnTable table)
                {
                    if (!scrollRestrictPawns)
                        return;
                    __result += "\n" + "COAA_AllowedAreaShiftScrollTip".Translate();
                }
            }

            [HarmonyPatch(typeof(PawnColumnWorker_AllowedArea), "DoCell")]
            static class PawnColumnWorker_AllowedArea_DoCell_Patch
            {
                [HarmonyPostfix]
                static void ShiftScrollAssign(Rect rect, Pawn pawn, PawnTable table)
                {
                    if (!scrollRestrictPawns)
                        return;
                    if (!Event.current.shift || Event.current.type != EventType.ScrollWheel || !Mouse.IsOver(rect))
                        return;

                    var areasEnumerable = Find.CurrentMap.areaManager.AllAreas.Where(x => x.AssignableAsAllowed());

                    // TD Enhancement Pack
                    if (tdSettings != null && Td_MapComponent_AreaOrder != null) {
                        var tdMapComponentAreaOrder =
                            (MapComponent) AccessTools.Method(typeof(Map), nameof(Map.GetComponent)).MakeGenericMethod(Td_MapComponent_AreaOrder).Invoke(Find.CurrentMap, new object[] { });
                        var tdAreaForTypes = (bool?) AccessTools.Field(Td_Settings, "areaForTypes")?.GetValue(tdSettings);
                        if (tdAreaForTypes == true) {
                            if (Find.UIRoot.windows.WindowOfType<MainTabWindow_Restrict>() != null) {
                                if (AccessTools.Field(Td_MapComponent_AreaOrder, "notForColonists") is FieldInfo notForColonistsField)
                                    areasEnumerable = areasEnumerable.Where(x => !((HashSet<Area>) notForColonistsField.GetValue(tdMapComponentAreaOrder)).Contains(x));
                            } else if (Find.UIRoot.windows.WindowOfType<MainTabWindow_Animals>() != null) {
                                if (AccessTools.Field(Td_MapComponent_AreaOrder, "notForAnimals") is FieldInfo notForAnimalsField)
                                    areasEnumerable = areasEnumerable.Where(x => !((HashSet<Area>) notForAnimalsField.GetValue(tdMapComponentAreaOrder)).Contains(x));
                            }
                        }
                    }

                    var areas = areasEnumerable.ToList();
                    areas.Insert(0, null); // include Unrestricted
                    var pawnAreaIdx = 0;
                    for (var i = 0; i < areas.Count; i++)
                        if (pawn.playerSettings.AreaRestriction == areas[i]) {
                            pawnAreaIdx = i;
                            break;
                        }

                    var next = areas[(pawnAreaIdx + 1) % areas.Count];
                    var prev = areas[(pawnAreaIdx - 1 + areas.Count) % areas.Count];
                    foreach (var _pawn in table.PawnsListForReading)
                        _pawn.playerSettings.AreaRestriction = Event.current.delta.y > 0 ? next : prev;
                    SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
                    Event.current.Use(); // don't scroll normally
                }
            }
        }
    }
}