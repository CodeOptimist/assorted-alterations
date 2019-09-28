using System.Collections.Generic;
using Harmony;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class ButchersCanCountMeat
        {
            static List<RecipeDef> countableRecipes, animalRecipes, insectRecipes, humanRecipes;
            static List<ThingDef> butchers;

            public static void DefsLoaded()
            {
                countableRecipes = DefsFromType<RecipeDef>(typeof(CountableMeatRecipes));
                animalRecipes = new List<RecipeDef>
                    {CountableMeatRecipes.COAA_ButcherAnimalFlesh, CountableMeatRecipes.COAA_ButcherAnimalInsectFlesh, CountableMeatRecipes.COAA_ButcherAnimalHumanFlesh};
                insectRecipes = new List<RecipeDef>
                    {CountableMeatRecipes.COAA_ButcherInsectFlesh, CountableMeatRecipes.COAA_ButcherAnimalInsectFlesh, CountableMeatRecipes.COAA_ButcherInsectHumanFlesh};
                humanRecipes = new List<RecipeDef>
                    {CountableMeatRecipes.COAA_ButcherHumanFlesh, CountableMeatRecipes.COAA_ButcherAnimalHumanFlesh, CountableMeatRecipes.COAA_ButcherInsectHumanFlesh};
                butchers = new List<ThingDef> { RecipeUsers.TableButcher, RecipeUsers.ButcherSpot };
            }

            public static void OnValueChanged_countableMeatRecipes(bool newvalue)
            {
                UpdateRecipes(newvalue, butchers, typeof(CountableMeatRecipes));
            }

            public static void OnValueChanged_convenientButcherRecipes(bool newvalue)
            {
                UpdateRecipes(newvalue, butchers, typeof(ConvenientButcherRecipes));
            }

            [HarmonyPatch(typeof(RecipeWorkerCounter), "CanCountProducts")]
            class RecipeWorkerCounter_CanCountProducts_Patch
            {
                [HarmonyPostfix]
                static void AllowCountMeat(ref bool __result, Bill_Production bill)
                {
                    if (countableRecipes.Contains(bill.recipe))
                        __result = true;
                }
            }

            [HarmonyPatch(typeof(RecipeWorkerCounter), "CountProducts")]
            class RecipeWorkerCounter_CountProducts_Patch
            {
                [HarmonyPrefix]
                static bool CountMeat(ref int __result, Bill_Production bill)
                {
                    if (!countableRecipes.Contains(bill.recipe))
                        return true;

                    var meatTypes = new List<List<ThingDef>>();
                    if (insectRecipes.Contains(bill.recipe))
                        meatTypes.Add(insectMeats);
                    if (humanRecipes.Contains(bill.recipe))
                        meatTypes.Add(humanMeats);
                    if (animalRecipes.Contains(bill.recipe))
                        meatTypes.Add(animalMeats);

                    var count = 0;
                    foreach (var meatType in meatTypes)
                    foreach (var meat in meatType)
                        count += bill.Map.resourceCounter.GetCount(meat);

                    __result = count;
                    return false;
                }
            }
        }
    }
}