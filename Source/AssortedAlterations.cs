using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations : ModBase
    {
        static SettingHandle<float> defaultSearchIngredientRadius;
        static SettingHandle<bool> denyDistantSupplyJobs, countableMeatRecipes, convenientButcherRecipes, separateCannibalMeals, separateInsectMeals, scrollRestrictPawns;
        static List<ThingDef> insectMeats, humanMeats, animalMeats;
        static ModLogger _logger;
        public override string ModIdentifier => "COAssortedAlterations";

        public override void DefsLoaded()
        {
            _logger = Logger;
            var meats = ThingCategoryDefOf.MeatRaw.childThingDefs;
            insectMeats = meats.Where(x => x.ingestible?.sourceDef?.race != null && x.ingestible.sourceDef.race.FleshType == FleshTypeDefOf.Insectoid).ToList();
            humanMeats = meats.Where(x => x.ingestible?.sourceDef?.race != null && x.ingestible.sourceDef.race.Humanlike).ToList();
            animalMeats = meats.Except(insectMeats).Except(humanMeats).ToList();
            
            defaultSearchIngredientRadius = Settings.GetHandle(
                "defaultSearchIngredientRadius",
                "COAA_defaultSearchIngredientRadiusSetting_title".Translate(),
                "COAA_defaultSearchIngredientRadiusSetting_description".Translate(),
                999f);
            denyDistantSupplyJobs = Settings.GetHandle(
                "denyDistantSupplyJobs",
                "COAA_denyDistantSupplyJobsSetting_title".Translate(),
                "COAA_denyDistantSupplyJobsSetting_description".Translate(),
                ModLister.HasActiveModWithName("Pick Up And Haul"));
            countableMeatRecipes = Settings.GetHandle(
                "countableMeatRecipes",
                "COAA_countableMeatRecipesSetting_title".Translate(),
                "COAA_countableMeatRecipesSetting_description".Translate(),
                true);
            convenientButcherRecipes = Settings.GetHandle(
                "convenientButcherRecipes",
                "COAA_convenientButcherRecipesSetting_title".Translate(),
                "COAA_convenientButcherRecipesSetting_description".Translate(),
                true);
            separateCannibalMeals = Settings.GetHandle(
                "separateCannibalMeals",
                "COAA_separateCannibalMealsSetting_title".Translate(),
                "COAA_separateCannibalMealsSetting_description".Translate(),
                true);
            separateInsectMeals = Settings.GetHandle(
                "separateInsectMeals",
                "COAA_separateInsectMealsSetting_title".Translate(),
                "COAA_separateInsectMealsSetting_description".Translate(),
                true);
            scrollRestrictPawns = Settings.GetHandle(
                "scrollRestrictPawns",
                "COAA_scrollRestrictPawnsSetting_title".Translate(),
                "COAA_scrollRestrictPawnsSetting_description".Translate(),
                true);

            ButcherSmallCreature.DefsLoaded();

            ButchersCanCountMeat.DefsLoaded();
            convenientButcherRecipes.OnValueChanged += ButchersCanCountMeat.OnValueChanged_convenientButcherRecipes;
            convenientButcherRecipes.OnValueChanged(convenientButcherRecipes);
            countableMeatRecipes.OnValueChanged += ButchersCanCountMeat.OnValueChanged_countableMeatRecipes;
            countableMeatRecipes.OnValueChanged(countableMeatRecipes);

            SeparateInsectCannibalMeals.DefsLoaded();
            separateCannibalMeals.OnValueChanged += SeparateInsectCannibalMeals.OnValueChanged_separateCannibalMeals;
            separateCannibalMeals.OnValueChanged(separateCannibalMeals);
            separateInsectMeals.OnValueChanged += SeparateInsectCannibalMeals.OnValueChanged_separateInsectMeals;
            separateInsectMeals.OnValueChanged(separateInsectMeals);
        }

        static void Debug(params object[] strings)
        {
#if DEBUG
            _logger.Trace(strings);
#endif
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