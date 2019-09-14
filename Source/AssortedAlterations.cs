using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib;
using HugsLib.Settings;
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

            SeparateInsectCannibalMeals.DefsLoaded();
            separateCannibalMeals.OnValueChanged += SeparateInsectCannibalMeals.OnValueChanged_separateCannibalMeals;
            separateCannibalMeals.OnValueChanged(separateCannibalMeals);
            separateInsectMeals.OnValueChanged += SeparateInsectCannibalMeals.OnValueChanged_separateInsectMeals;
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