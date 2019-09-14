using Harmony;
using Reloader;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class ButchersCanCountMeat
        {
            [HarmonyPatch(typeof(RecipeWorkerCounter), "CanCountProducts")]
            class RecipeWorkerCounter_CanCountProducts_Patch
            {
#if DEBUG
                [ReloadMethod]
#endif
                [HarmonyPostfix]
                static void AllowCountMeat(ref bool __result, Bill_Production bill)
                {
                    if (bill.recipe.specialProducts != null && bill.recipe.specialProducts.Contains(SpecialProductType.Butchery) && bill.recipe.defName.StartsWith("COAA_"))
                        __result = true;
                }
            }

            [HarmonyPatch(typeof(RecipeWorkerCounter), "CountProducts")]
            class RecipeWorkerCounter_CountProducts_Patch
            {
#if DEBUG
                [ReloadMethod]
#endif
                [HarmonyPrefix]
                static bool CountMeat(ref int __result, Bill_Production bill)
                {
                    if (bill.recipe.specialProducts == null || !bill.recipe.specialProducts.Contains(SpecialProductType.Butchery) || !bill.recipe.defName.StartsWith("COAA_"))
                        return true;

                    var recipe = bill.recipe.defName;
                    var isAnimalRecipe = recipe == "COAA_ButcherAnimalFlesh" || recipe == "COAA_ButcherAnimalInsectFlesh" || recipe == "COAA_ButcherAnimalHumanFlesh";
                    var isInsectRecipe = recipe == "COAA_ButcherInsectFlesh" || recipe == "COAA_ButcherAnimalInsectFlesh" || recipe == "COAA_ButcherInsectHumanFlesh";
                    var isHumanRecipe = recipe == "COAA_ButcherHumanFlesh" || recipe == "COAA_ButcherAnimalHumanFlesh" || recipe == "COAA_ButcherInsectHumanFlesh";
                    var isCustomRecipe = isAnimalRecipe || isInsectRecipe || isHumanRecipe;

                    var count = 0;
                    foreach (var child in ThingCategoryDefOf.MeatRaw.childThingDefs)
                    {
                        var isCounted = true;
                        if (isCustomRecipe)
                            switch (child.defName)
                            {
                                case "Meat_Human":
                                    isCounted = isHumanRecipe;
                                    break;
                                case "Meat_Megaspider":
                                    isCounted = isInsectRecipe;
                                    break;
                                default:
                                    isCounted = isAnimalRecipe;
                                    break;
                            }

                        if (isCounted)
                            count += bill.Map.resourceCounter.GetCount(child);
                    }

                    __result = count;
                    return false;
                }
            }
        }
    }
}