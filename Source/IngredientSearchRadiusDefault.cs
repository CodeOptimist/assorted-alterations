using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class IngredientSearchRadiusDefault
        {
            [HarmonyPatch(typeof(Bill_Production), MethodType.Constructor, typeof(RecipeDef))]
            static class Bill_Production_Bill_Production_Patch
            {
                [HarmonyPostfix]
                [SuppressMessage("ReSharper", "UnusedParameter.Local")]
                static void UseCustomIngredientRadius(Bill_Production __instance, RecipeDef recipe) {
                    __instance.ingredientSearchRadius = defaultSearchIngredientRadius;
                }
            }

            [HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
            static class Dialog_BillConfig_DoWindowContents_Patch
            {
                static readonly FieldInfo Bill = AccessTools.Field(typeof(Dialog_BillConfig), "bill");

                [HarmonyTranspiler]
                static IEnumerable<CodeInstruction> ButtonAfterLabel(IEnumerable<CodeInstruction> instructions) {
                    var listingIdx = -1;
                    var isInserted = false;

                    var codes = instructions.ToList();
                    for (var i = 0; i < codes.Count; i++) {
                        if (codes[i].operand is MethodInfo method && method == AccessTools.Method(typeof(Listing), nameof(Listing.Begin)))
                            listingIdx = i;

                        if (!isInserted && listingIdx != -1 && codes[i].operand is string str && str == "IngredientSearchRadius") {
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return codes[listingIdx - 2];
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dialog_BillConfig_DoWindowContents_Patch), nameof(DrawRadiusDefaultButton)));
                            isInserted = true;
                            continue;
                        }

                        yield return codes[i];
                    }
                }

                static void DrawRadiusDefaultButton(Dialog_BillConfig instance, Listing_Standard listing) {
                    var rect = listing.GetRect(0);
                    rect = new Rect(180f, rect.y - 2f, 65f, 24f);
                    if (Widgets.ButtonText(rect, "Default")) {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        var bill = (Bill_Production) Bill.GetValue(instance);
                        defaultSearchIngredientRadius.Value = bill.ingredientSearchRadius;
                    }
                }
            }
        }
    }
}