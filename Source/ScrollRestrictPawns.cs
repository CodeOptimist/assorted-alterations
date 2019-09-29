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
            static readonly Type Td_DoAllowedAreaSelectors_Patch = tdAssemblies?.Select(x => x.GetType("TD_Enhancement_Pack.DoAllowedAreaSelectors_AreaOrder")).SingleOrDefault(x => x != null);

            [HarmonyPatch(typeof(PawnColumnWorker_AllowedArea), "GetHeaderTip")]
            static class PawnColumnWorker_AllowedArea_GetHeaderTip_Patch
            {
                [HarmonyPostfix]
                [SuppressMessage("ReSharper", "UnusedParameter.Local")]
                static void GetHeaderTip(ref string __result, PawnTable table) {
                    if (!scrollRestrictPawns)
                        return;
                    __result += "\n" + "COAA_AllowedAreaShiftScrollTip".Translate();
                }
            }

            [HarmonyPatch(typeof(PawnColumnWorker_AllowedArea), nameof(PawnColumnWorker_AllowedArea.DoCell))]
            static class PawnColumnWorker_AllowedArea_DoCell_Patch
            {
                [HarmonyPostfix]
                static void ShiftScrollAssign(Rect rect, Pawn pawn, PawnTable table) {
                    if (!scrollRestrictPawns)
                        return;
                    if (!Event.current.shift || Event.current.type != EventType.ScrollWheel || !Mouse.IsOver(rect))
                        return;

                    bool AssignableAsAllowed(Area area) {
                        // TD Enhancement Pack
                        if (AccessTools.Method(Td_DoAllowedAreaSelectors_Patch, "AssignableAsAllowedForPawn") is MethodInfo methodInfo)
                            return (bool) methodInfo.Invoke(null, new object[] {area, pawn});
                        return area.AssignableAsAllowed();
                    }

                    var areas = Find.CurrentMap.areaManager.AllAreas.Where(AssignableAsAllowed).ToList();
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