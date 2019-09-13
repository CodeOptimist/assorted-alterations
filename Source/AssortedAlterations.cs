using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations : ModBase
    {
        public static SettingHandle<float> defaultSearchIngredientRadius;
        public static SettingHandle<bool> denyDistantSupplyJobs;
        public static SettingHandle<bool> countableMeatRecipes;
        public static SettingHandle<bool> convenientButcherRecipes;
        public static SettingHandle<bool> separateCannibalMeals;
        public static SettingHandle<bool> separateInsectMeals;
        public override string ModIdentifier => "COAssortedAlterations";

        public override void DefsLoaded()
        {
            defaultSearchIngredientRadius = Settings.GetHandle(
                "defaultSearchIngredientRadius",
                "defaultSearchIngredientRadiusSetting_title".Translate(),
                "defaultSearchIngredientRadiusSetting_description".Translate(),
                999f);
            denyDistantSupplyJobs = Settings.GetHandle(
                "denyDistantSupplyJobs",
                "denyDistantSupplyJobsSetting_title".Translate(),
                "denyDistantSupplyJobsSetting_description".Translate(),
                ModLister.HasActiveModWithName("Pick Up And Haul"));
            countableMeatRecipes = Settings.GetHandle(
                "countableMeatRecipes",
                "countableMeatRecipesSetting_title".Translate(),
                "countableMeatRecipesSetting_description".Translate(),
                true);
            convenientButcherRecipes = Settings.GetHandle(
                "convenientButcherRecipes",
                "convenientButcherRecipesSetting_title".Translate(),
                "convenientButcherRecipesSetting_description".Translate(),
                true);
            separateCannibalMeals = Settings.GetHandle(
                "separateCannibalMeals",
                "separateCannibalMealsSetting_title".Translate(),
                "separateCannibalMealsSetting_description".Translate(),
                true);
            separateInsectMeals = Settings.GetHandle(
                "separateInsectMeals",
                "separateInsectMealsSetting_title".Translate(),
                "separateInsectMealsSetting_description".Translate(),
                true);

            var butchers = new List<ThingDef> {RecipeUsers.TableButcher, RecipeUsers.ButcherSpot};
            convenientButcherRecipes.OnValueChanged = value => UpdateRecipes(value, butchers, typeof(ConvenientButcherRecipes));
            convenientButcherRecipes.OnValueChanged(convenientButcherRecipes);
            countableMeatRecipes.OnValueChanged = value => UpdateRecipes(value, butchers, typeof(CountableMeatRecipes));
            countableMeatRecipes.OnValueChanged(countableMeatRecipes);
            var stoves = new List<ThingDef> {RecipeUsers.ElectricStove, RecipeUsers.FueledStove};
            var campfire = new List<ThingDef> {ThingDefOf.Campfire};
            separateCannibalMeals.OnValueChanged = value =>
            {
                UpdateRecipes(value, stoves, typeof(CannibalRecipes));
                UpdateRecipes(value, campfire, new List<RecipeDef> {CannibalRecipes.COAA_CookCannibalMealSimple, CannibalRecipes.COAA_MakeCannibalPemmican});

                foreach (var recipe in DefsFromType<RecipeDef>(typeof(FoodRecipes)))
                    recipe.fixedIngredientFilter.SetAllow(Meats.Meat_Human, !value);
            };
            separateCannibalMeals.OnValueChanged(separateCannibalMeals);
            separateInsectMeals.OnValueChanged = value =>
            {
                UpdateRecipes(value, stoves, typeof(InsectRecipes));
                UpdateRecipes(value, campfire, new List<RecipeDef> {InsectRecipes.COAA_CookInsectMealSimple, InsectRecipes.COAA_MakeInsectPemmican});

                foreach (var recipe in DefsFromType<RecipeDef>(typeof(FoodRecipes)))
                    recipe.fixedIngredientFilter.SetAllow(Meats.Meat_Megaspider, !value);
            };
            separateInsectMeals.OnValueChanged(separateInsectMeals);
        }

        static List<T> DefsFromType<T>(Type type)
        {
            return type.GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (T) x.GetValue(null)).ToList();
        }

        static void UpdateRecipes(bool isAdd, IEnumerable<ThingDef> recipeUsers, Type recipesType)
        {
            UpdateRecipes(isAdd, recipeUsers, DefsFromType<RecipeDef>(recipesType));
        }

        static void UpdateRecipes(bool isAdd, IEnumerable<ThingDef> recipeUsers, List<RecipeDef> recipes)
        {
            if (isAdd)
                foreach (var recipeUser in recipeUsers)
                    recipeUser.AllRecipes.AddRange(recipes);
            else
                foreach (var recipeUser in recipeUsers)
                foreach (var recipe in recipes)
                    recipeUser.AllRecipes.Remove(recipe);
        }
    }
}