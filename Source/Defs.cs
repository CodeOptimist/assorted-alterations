using RimWorld;
using Verse;

#pragma warning disable 649
namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        [DefOf]
        static class RecipeUsers
        {
            public static ThingDef TableButcher;
            public static ThingDef ButcherSpot;
            public static ThingDef ElectricStove;
            public static ThingDef FueledStove;

            static RecipeUsers()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(RecipeUsers));
            }
        }

        [DefOf]
        static class FoodRecipes
        {
            public static RecipeDef CookMealSimple;
            public static RecipeDef CookMealFine;
            public static RecipeDef CookMealLavish;
            public static RecipeDef CookMealSurvival;
            public static RecipeDef Make_Pemmican;

            static FoodRecipes()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(FoodRecipes));
            }
        }

        [DefOf]
        static class Meats
        {
            public static ThingDef Meat_Human;
            public static ThingDef Meat_Megaspider;

            static Meats()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(Meats));
            }
        }

        [DefOf]
        static class ConvenientButcherRecipes
        {
            public static RecipeDef COAA_ButcherSmallCreatureFlesh;

            static ConvenientButcherRecipes()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(ConvenientButcherRecipes));
            }
        }

        [DefOf]
        static class CountableMeatRecipes
        {
            public static RecipeDef COAA_ButcherAnimalFlesh;
            public static RecipeDef COAA_ButcherInsectFlesh;
            public static RecipeDef COAA_ButcherHumanFlesh;
            public static RecipeDef COAA_ButcherAnimalInsectFlesh;
            public static RecipeDef COAA_ButcherAnimalHumanFlesh;
            public static RecipeDef COAA_ButcherInsectHumanFlesh;

            static CountableMeatRecipes()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(CountableMeatRecipes));
            }
        }

        [DefOf]
        static class CannibalRecipes
        {
            public static RecipeDef COAA_CookCannibalMealSimple;
            public static RecipeDef COAA_CookCannibalMealFine;
            public static RecipeDef COAA_CookCannibalMealLavish;
            public static RecipeDef COAA_CookCannibalMealSurvival;
            public static RecipeDef COAA_MakeCannibalPemmican;

            static CannibalRecipes()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(CannibalRecipes));
            }
        }

        [DefOf]
        static class InsectRecipes
        {
            public static RecipeDef COAA_CookInsectMealSimple;
            public static RecipeDef COAA_CookInsectMealFine;
            public static RecipeDef COAA_CookInsectMealLavish;
            public static RecipeDef COAA_CookInsectMealSurvival;
            public static RecipeDef COAA_MakeInsectPemmican;

            static InsectRecipes()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(InsectRecipes));
            }
        }

        [DefOf]
        static class CannibalFoods
        {
            public static ThingDef COAA_CannibalMealSurvivalPack;
            public static ThingDef COAA_CannibalMealSimple;
            public static ThingDef COAA_CannibalMealFine;
            public static ThingDef COAA_CannibalMealLavish;
            public static ThingDef COAA_CannibalPemmican;

            static CannibalFoods()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(CannibalFoods));
            }
        }

        [DefOf]
        static class InsectFoods
        {
            public static ThingDef COAA_InsectMealSurvivalPack;
            public static ThingDef COAA_InsectMealSimple;
            public static ThingDef COAA_InsectMealFine;
            public static ThingDef COAA_InsectMealLavish;
            public static ThingDef COAA_InsectPemmican;

            static InsectFoods()
            {
                DefOfHelper.EnsureInitializedInCtor(typeof(InsectFoods));
            }
        }
    }
}
#pragma warning restore 649