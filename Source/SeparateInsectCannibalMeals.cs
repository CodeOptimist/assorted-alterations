using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static readonly List<ThingDef> cannibalFoods = DefsFromType<ThingDef>(typeof(CannibalFoods));
        static readonly List<ThingDef> insectFoods = DefsFromType<ThingDef>(typeof(InsectFoods));

        [HarmonyPatch(typeof(FoodRestrictionDatabase), "GenerateStartingFoodRestrictions")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static class FoodRestrictionDatabase_GenerateStartingFoodRestrictions_Patch
        {
#if DEBUG
            [ReloadMethod]
#endif
            [HarmonyPostfix]
            static void ExcludeFromStartingRestrictions(FoodRestrictionDatabase __instance)
            {
                foreach (var restriction in __instance.AllFoodRestrictions)
                {
                    foreach (var food in cannibalFoods)
                        restriction.filter.SetAllow(food, false);
                    foreach (var food in insectFoods)
                        restriction.filter.SetAllow(food, false);
                }
            }
        }

        [HarmonyPatch(typeof(ThingSetMakerUtility), "CanGenerate")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
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