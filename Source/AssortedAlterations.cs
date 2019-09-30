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
        static SettingHandle<float> defaultSearchIngredientRadius;

        static SettingHandle<bool> denyDistantSupplyJobs,
            countableMeatRecipes,
            convenientButcherRecipes,
            separateCannibalMeals,
            separateInsectMeals,
            scrollRestrictPawns,
            betterPawnControl_Birth,
            pauseOnBeginAssault;

        static List<ThingDef> insectMeats, humanMeats, animalMeats;
        public override string ModIdentifier => "COAssortedAlterations";

        public override void DefsLoaded() {
            var meats = ThingCategoryDefOf.MeatRaw.childThingDefs;
            insectMeats = meats.Where(x => x.ingestible?.sourceDef?.race?.FleshType == FleshTypeDefOf.Insectoid).ToList();
            humanMeats = meats.Where(x => x.ingestible?.sourceDef?.race?.Humanlike == true).ToList();
            animalMeats = meats.Except(insectMeats).Except(humanMeats).ToList();

            SettingHandle<T> GetSettingHandle<T>(string settingName, T defaultValue) {
                return Settings.GetHandle(settingName, $"COAA_{settingName}Setting_title".Translate(), $"COAA_{settingName}Setting_description".Translate(), defaultValue);
            }

            defaultSearchIngredientRadius = GetSettingHandle("defaultSearchIngredientRadius", 999f);
            denyDistantSupplyJobs = GetSettingHandle("denyDistantSupplyJobs", ModLister.HasActiveModWithName("Pick Up And Haul"));
            countableMeatRecipes = GetSettingHandle("countableMeatRecipes", true);
            convenientButcherRecipes = GetSettingHandle("convenientButcherRecipes", true);
            separateCannibalMeals = GetSettingHandle("separateCannibalMeals", true);
            separateInsectMeals = GetSettingHandle("separateInsectMeals", true);
            scrollRestrictPawns = GetSettingHandle("scrollRestrictPawns", true);
            pauseOnBeginAssault = GetSettingHandle("pauseOnBeginAssault", true);
            betterPawnControl_Birth = GetSettingHandle("betterPawnControl_Birth", true);

            ButcherSmallCreature.DefsLoaded();
            ButchersCanCountMeat.DefsLoaded();
            SeparateInsectCannibalMeals.DefsLoaded();
            BetterPawnControl_Birth.DefsLoaded(HarmonyInst);
        }

        static List<T> DefsFromType<T>(Type type) {
            return type.GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (T) x.GetValue(null)).ToList();
        }

        static void UpdateRecipes(bool isAdd, IEnumerable<ThingDef> recipeUsers, Type recipesType) {
            UpdateRecipes(isAdd, recipeUsers, DefsFromType<RecipeDef>(recipesType));
        }

        static void UpdateRecipes(bool isAdd, IEnumerable<ThingDef> recipeUsers, List<RecipeDef> recipes) {
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