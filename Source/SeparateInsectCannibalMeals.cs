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

            public static void DefsLoaded()
            {
                cannibalFoods = DefsFromType<ThingDef>(typeof(CannibalFoods));
                insectFoods = DefsFromType<ThingDef>(typeof(InsectFoods));
                stoves = new List<ThingDef> {RecipeUsers.ElectricStove, RecipeUsers.FueledStove};
                campfire = new List<ThingDef> {ThingDefOf.Campfire};

                foreach (var meat in ThingCategoryDefOf.MeatRaw.childThingDefs) {
                    foreach (var recipe in DefsFromType<RecipeDef>(typeof(InsectRecipes)))
                        recipe.fixedIngredientFilter.SetAllow(meat, insectMeats.Contains(meat));
                    foreach (var recipe in DefsFromType<RecipeDef>(typeof(CannibalRecipes)))
                        recipe.fixedIngredientFilter.SetAllow(meat, humanMeats.Contains(meat));
                }
            }

            public static void OnValueChanged_separateInsectMeals(bool newvalue)
            {
                UpdateRecipes(newvalue, stoves, typeof(InsectRecipes));
                UpdateRecipes(newvalue, campfire, new List<RecipeDef> {InsectRecipes.COAA_CookInsectMealSimple, InsectRecipes.COAA_MakeInsectPemmican});

                foreach (var recipe in DefsFromType<RecipeDef>(typeof(FoodRecipes)))
                foreach (var insectMeat in insectMeats)
                    recipe.fixedIngredientFilter.SetAllow(insectMeat, !newvalue);
            }

            public static void OnValueChanged_separateCannibalMeals(bool newvalue)
            {
                UpdateRecipes(newvalue, stoves, typeof(CannibalRecipes));
                UpdateRecipes(newvalue, campfire, new List<RecipeDef> {CannibalRecipes.COAA_CookCannibalMealSimple, CannibalRecipes.COAA_MakeCannibalPemmican});

                foreach (var recipe in DefsFromType<RecipeDef>(typeof(FoodRecipes)))
                foreach (var humanMeat in humanMeats)
                    recipe.fixedIngredientFilter.SetAllow(humanMeat, !newvalue);
            }

            [HarmonyPatch(typeof(FoodRestrictionDatabase), "GenerateStartingFoodRestrictions")]
            static class FoodRestrictionDatabase_GenerateStartingFoodRestrictions_Patch
            {
                [HarmonyPostfix]
                static void ExcludeFromStartingRestrictions(FoodRestrictionDatabase __instance)
                {
                    foreach (var restriction in __instance.AllFoodRestrictions) {
                        foreach (var food in cannibalFoods)
                            restriction.filter.SetAllow(food, false);
                        foreach (var food in insectFoods)
                            restriction.filter.SetAllow(food, false);
                    }
                }
            }

            [HarmonyPatch(typeof(ThingSetMakerUtility), "CanGenerate")]
            static class ThingSetMakerUtility_CanGenerate_Patch
            {
                [HarmonyPostfix]
                static void DenyGeneration(ref bool __result, ThingDef thingDef)
                {
                    if (cannibalFoods.Contains(thingDef) || insectFoods.Contains(thingDef))
                        __result = false;
                }
            }
        }
    }
}