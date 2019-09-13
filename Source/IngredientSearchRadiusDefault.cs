using System.Diagnostics.CodeAnalysis;
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
        [HarmonyPatch(typeof(Bill_Production), MethodType.Constructor, typeof(RecipeDef))]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static class Bill_Production_Bill_Production_Patch
        {
            [HarmonyPostfix]
            [SuppressMessage("ReSharper", "UnusedParameter.Local")]
            static void UseCustomIngredientRadius(Bill_Production __instance, RecipeDef recipe)
            {
                __instance.ingredientSearchRadius = defaultSearchIngredientRadius;
            }
        }

        [HarmonyPatch(typeof(Dialog_BillConfig), "DoWindowContents")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static class BillConfig_DoWindowContents_Patch
        {
            static readonly FieldInfo BillGetter = typeof(Dialog_BillConfig).GetField("bill", BindingFlags.Instance | BindingFlags.NonPublic);

#if DEBUG
            [ReloadMethod]
#endif
            [HarmonyPostfix]
            static void DrawRadiusDefaultButton(Dialog_BillConfig __instance, Rect inRect)
            {
                var bill = (Bill_Production) BillGetter.GetValue(__instance);
                var rect = new Rect(inRect.xMin + 700f, inRect.yMin + 506f, 65f, 24f);
                if (Widgets.ButtonText(rect, "Default"))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    defaultSearchIngredientRadius.Value = bill.ingredientSearchRadius;
                }
            }
        }
    }
}