using System.Linq;
using RimWorld;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class ButcherSmallCreature
        {
            public static void DefsLoaded() {
                var smallCorpses = ThingCategoryDefOf.Corpses.ThisAndChildCategoryDefs.SelectMany(x => x.childThingDefs).Where(
                    x =>
                        x.ingestible?.sourceDef != null &&
                        StatDefOf.MeatAmount.Worker.ShouldShowFor(StatRequest.For(x.ingestible.sourceDef, null)) &&
                        x.ingestible.sourceDef.GetStatValueAbstract(StatDefOf.MeatAmount) > 0 &&
                        x.ingestible.sourceDef.GetStatValueAbstract(StatDefOf.MeatAmount) <= 75);
                // second parameter is "exceptedFilters" but it's actually the filters to disallow apparently??
                ConvenientButcherRecipes.COAA_ButcherSmallCreatureFlesh.fixedIngredientFilter.SetDisallowAll(smallCorpses, new[] {SpecialFilter.AllowRotten});
            }
        }
    }
}