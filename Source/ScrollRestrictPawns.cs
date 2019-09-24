using System.Linq;
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
            [HarmonyPatch(typeof(PawnColumnWorker_AllowedArea), "GetHeaderTip")]
            static class PawnColumnWorker_AllowedArea_GetHeaderTip_Patch
            {
                [HarmonyPostfix]
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

                    var areas = Find.CurrentMap.areaManager.AllAreas.Where(x => x.AssignableAsAllowed()).ToList();
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
                }
            }
        }
    }
}