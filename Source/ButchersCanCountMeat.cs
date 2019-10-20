using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class ButchersCanCountMeat
        {
            static List<RecipeDef> countableRecipes, convenientRecipes;

            public static void DefsLoaded() {
                countableRecipes = DefsFromType<RecipeDef>(typeof(CountableMeatRecipes));
                convenientRecipes = DefsFromType<RecipeDef>(typeof(ConvenientButcherRecipes));

                var butchers = new List<ThingDef> {RecipeUsers.TableButcher, RecipeUsers.ButcherSpot};
                convenientButcherRecipes.OnValueChanged += newValue => UpdateRecipes(newValue, butchers, convenientRecipes);
                convenientButcherRecipes.OnValueChanged(convenientButcherRecipes);
                countableMeatRecipes.OnValueChanged += newValue => UpdateRecipes(newValue, butchers, countableRecipes);
                countableMeatRecipes.OnValueChanged(countableMeatRecipes);
            }

            [HarmonyPatch(typeof(RecipeWorkerCounter), nameof(RecipeWorkerCounter.CanCountProducts))]
            static class RecipeWorkerCounter_CanCountProducts_Patch
            {
                [HarmonyPostfix]
                static void AllowCountMeat(ref bool __result, Bill_Production bill) {
                    if (countableRecipes.Contains(bill.recipe))
                        __result = true;
                }
            }

            [HarmonyPatch(typeof(RecipeWorkerCounter), nameof(RecipeWorkerCounter.CountProducts))]
            static class RecipeWorkerCounter_CountProducts_Patch
            {
                static readonly List<RecipeDef> insectRecipes = new List<RecipeDef> {
                    CountableMeatRecipes.COAA_ButcherInsectFlesh, CountableMeatRecipes.COAA_ButcherAnimalInsectFlesh, CountableMeatRecipes.COAA_ButcherInsectHumanFlesh
                };

                static readonly List<RecipeDef> humanRecipes = new List<RecipeDef> {
                    CountableMeatRecipes.COAA_ButcherHumanFlesh, CountableMeatRecipes.COAA_ButcherAnimalHumanFlesh, CountableMeatRecipes.COAA_ButcherInsectHumanFlesh
                };

                static readonly List<RecipeDef> animalRecipes = new List<RecipeDef> {
                    CountableMeatRecipes.COAA_ButcherAnimalFlesh, CountableMeatRecipes.COAA_ButcherAnimalInsectFlesh, CountableMeatRecipes.COAA_ButcherAnimalHumanFlesh
                };

                [HarmonyPrefix]
                static bool CountMeat(ref int __result, Bill_Production bill) {
                    if (!countableRecipes.Contains(bill.recipe))
                        return true;

                    var meatTypes = new List<List<ThingDef>>();
                    if (insectRecipes.Contains(bill.recipe))
                        meatTypes.Add(insectMeats);
                    if (humanRecipes.Contains(bill.recipe))
                        meatTypes.Add(humanMeats);
                    if (animalRecipes.Contains(bill.recipe))
                        meatTypes.Add(animalMeats);

                    var count = meatTypes.SelectMany(meatType => meatType).Sum(meat => bill.Map.resourceCounter.GetCount(meat));

                    __result = count;
                    return false;
                }
            }
        }
    }
}