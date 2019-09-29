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
            static readonly List<ThingDef> cannibalFoods = DefsFromType<ThingDef>(typeof(CannibalFoods));
            static readonly List<ThingDef> insectFoods = DefsFromType<ThingDef>(typeof(InsectFoods));
            static readonly List<ThingDef> stoves = new List<ThingDef> {RecipeUsers.ElectricStove, RecipeUsers.FueledStove};
            static readonly List<ThingDef> campfire = new List<ThingDef> {ThingDefOf.Campfire};

            public static void DefsLoaded() {
                foreach (var meat in ThingCategoryDefOf.MeatRaw.childThingDefs) {
                    foreach (var recipe in DefsFromType<RecipeDef>(typeof(InsectRecipes)))
                        recipe.fixedIngredientFilter.SetAllow(meat, insectMeats.Contains(meat));
                    foreach (var recipe in DefsFromType<RecipeDef>(typeof(CannibalRecipes)))
                        recipe.fixedIngredientFilter.SetAllow(meat, humanMeats.Contains(meat));
                }

                separateCannibalMeals.OnValueChanged += value => {
                    UpdateRecipes(value, stoves, typeof(CannibalRecipes));
                    UpdateRecipes(value, campfire, new List<RecipeDef> {CannibalRecipes.COAA_CookCannibalMealSimple, CannibalRecipes.COAA_MakeCannibalPemmican});

                    foreach (var recipe in DefsFromType<RecipeDef>(typeof(FoodRecipes)))
                    foreach (var humanMeat in humanMeats)
                        recipe.fixedIngredientFilter.SetAllow(humanMeat, !value);
                };
                separateCannibalMeals.OnValueChanged(separateCannibalMeals);

                separateInsectMeals.OnValueChanged += value => {
                    UpdateRecipes(value, stoves, typeof(InsectRecipes));
                    UpdateRecipes(value, campfire, new List<RecipeDef> {InsectRecipes.COAA_CookInsectMealSimple, InsectRecipes.COAA_MakeInsectPemmican});

                    foreach (var recipe in DefsFromType<RecipeDef>(typeof(FoodRecipes)))
                    foreach (var insectMeat in insectMeats)
                        recipe.fixedIngredientFilter.SetAllow(insectMeat, !value);
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