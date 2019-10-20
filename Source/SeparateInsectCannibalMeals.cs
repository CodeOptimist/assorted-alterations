using System.Collections.Generic;
using Harmony;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class SeparateInsectCannibalMeals
        {
            static List<ThingDef> cannibalFoods, insectFoods, stoves, campfire;
            static List<RecipeDef> cannibalRecipes, insectRecipes;

            public static void DefsLoaded() {
                cannibalFoods = DefsFromType<ThingDef>(typeof(CannibalFoods));
                insectFoods = DefsFromType<ThingDef>(typeof(InsectFoods));
                stoves = new List<ThingDef> {RecipeUsers.ElectricStove, RecipeUsers.FueledStove};
                campfire = new List<ThingDef> {ThingDefOf.Campfire};
                cannibalRecipes = DefsFromType<RecipeDef>(typeof(CannibalRecipes));
                insectRecipes = DefsFromType<RecipeDef>(typeof(InsectRecipes));

                foreach (var meat in ThingCategoryDefOf.MeatRaw.childThingDefs) {
                    foreach (var recipe in insectRecipes)
                        recipe.fixedIngredientFilter.SetAllow(meat, insectMeats.Contains(meat));
                    foreach (var recipe in cannibalRecipes)
                        recipe.fixedIngredientFilter.SetAllow(meat, humanMeats.Contains(meat));
                }

                separateCannibalMeals.OnValueChanged += value => {
                    UpdateRecipes(value, stoves, cannibalRecipes);
                    UpdateRecipes(value, campfire, new List<RecipeDef> {CannibalRecipes.COAA_CookCannibalMealSimple, CannibalRecipes.COAA_MakeCannibalPemmican});
                };
                separateCannibalMeals.OnValueChanged(separateCannibalMeals);

                separateInsectMeals.OnValueChanged += value => {
                    UpdateRecipes(value, stoves, insectRecipes);
                    UpdateRecipes(value, campfire, new List<RecipeDef> {InsectRecipes.COAA_CookInsectMealSimple, InsectRecipes.COAA_MakeInsectPemmican});
                };
                separateInsectMeals.OnValueChanged(separateInsectMeals);
            }

            [HarmonyPatch(typeof(FoodRestrictionDatabase), "GenerateStartingFoodRestrictions")]
            static class FoodRestrictionDatabase_GenerateStartingFoodRestrictions_Patch
            {
                [HarmonyPostfix]
                static void ExcludeFromStartingRestrictions(FoodRestrictionDatabase __instance) {
                    foreach (var restriction in __instance.AllFoodRestrictions) {
                        foreach (var food in cannibalFoods)
                            restriction.filter.SetAllow(food, false);
                        foreach (var food in insectFoods)
                            restriction.filter.SetAllow(food, false);
                    }
                }
            }

            [HarmonyPatch(typeof(ThingSetMakerUtility), nameof(ThingSetMakerUtility.CanGenerate))]
            static class ThingSetMakerUtility_CanGenerate_Patch
            {
                [HarmonyPostfix]
                static void DenyGeneration(ref bool __result, ThingDef thingDef) {
                    if (cannibalFoods.Contains(thingDef) || insectFoods.Contains(thingDef))
                        __result = false;
                }
            }
        }
    }
}