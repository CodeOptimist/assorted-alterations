using RimWorld;
using Verse;

#pragma warning disable 649
namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        [DefOf]
        static class SpecialFilter
        {
            public static SpecialThingFilterDef AllowRotten;

            static SpecialFilter() {
                DefOfHelper.EnsureInitializedInCtor(typeof(SpecialFilter));
            }
        }

        [DefOf]
        static class RecipeUsers
        {
            public static ThingDef TableButcher, ButcherSpot, ElectricStove, FueledStove;

            static RecipeUsers() {
                DefOfHelper.EnsureInitializedInCtor(typeof(RecipeUsers));
            }
        }

        [DefOf]
        static class FoodRecipes
        {
            public static RecipeDef CookMealSimple, CookMealFine, CookMealLavish, CookMealSurvival, Make_Pemmican;

            static FoodRecipes() {
                DefOfHelper.EnsureInitializedInCtor(typeof(FoodRecipes));
            }
        }

        [DefOf]
        static class ConvenientButcherRecipes
        {
            public static RecipeDef COAA_ButcherSmallCreatureFlesh;

            static ConvenientButcherRecipes() {
                DefOfHelper.EnsureInitializedInCtor(typeof(ConvenientButcherRecipes));
            }
        }

        [DefOf]
        static class CountableMeatRecipes
        {
            public static RecipeDef COAA_ButcherAnimalFlesh, COAA_ButcherInsectFlesh, COAA_ButcherHumanFlesh, COAA_ButcherAnimalInsectFlesh, COAA_ButcherAnimalHumanFlesh, COAA_ButcherInsectHumanFlesh;

            static CountableMeatRecipes() {
                DefOfHelper.EnsureInitializedInCtor(typeof(CountableMeatRecipes));
            }
        }

        [DefOf]
        static class CannibalRecipes
        {
            public static RecipeDef COAA_CookCannibalMealSimple, COAA_CookCannibalMealFine, COAA_CookCannibalMealLavish, COAA_CookCannibalMealSurvival, COAA_MakeCannibalPemmican;

            static CannibalRecipes() {
                DefOfHelper.EnsureInitializedInCtor(typeof(CannibalRecipes));
            }
        }

        [DefOf]
        static class InsectRecipes
        {
            public static RecipeDef COAA_CookInsectMealSimple, COAA_CookInsectMealFine, COAA_CookInsectMealLavish, COAA_CookInsectMealSurvival, COAA_MakeInsectPemmican;

            static InsectRecipes() {
                DefOfHelper.EnsureInitializedInCtor(typeof(InsectRecipes));
            }
        }

        [DefOf]
        static class CannibalFoods
        {
            public static ThingDef COAA_CannibalMealSurvivalPack, COAA_CannibalMealSimple, COAA_CannibalMealFine, COAA_CannibalMealLavish, COAA_CannibalPemmican;

            static CannibalFoods() {
                DefOfHelper.EnsureInitializedInCtor(typeof(CannibalFoods));
            }
        }

        [DefOf]
        static class InsectFoods
        {
            public static ThingDef COAA_InsectMealSurvivalPack, COAA_InsectMealSimple, COAA_InsectMealFine, COAA_InsectMealLavish, COAA_InsectPemmican;

            static InsectFoods() {
                DefOfHelper.EnsureInitializedInCtor(typeof(InsectFoods));
            }
        }
    }
}
#pragma warning restore 649