using System.Diagnostics.CodeAnalysis;
using Harmony;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static IConstructible constructible;

        [HarmonyPatch(typeof(WorkGiver_ConstructDeliverResources), "ResourceDeliverJobFor")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static class WorkGiver_ConstructDeliverResources_ResourceDeliverJobFor_Patch
        {
#if DEBUG
            [ReloadMethod]
#endif
            [HarmonyPrefix]
            [SuppressMessage("ReSharper", "UnusedParameter.Local")]
            static bool GetConstructible(Pawn pawn, IConstructible c)
            {
                constructible = c;
                return true;
            }
        }

        [HarmonyPatch(typeof(WorkGiver_ConstructDeliverResources), "ResourceValidator")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static class WorkGiver_ConstructDeliverResources_ResourceValidator_Patch
        {
#if DEBUG
            [ReloadMethod]
#endif
            [HarmonyPostfix]
            [SuppressMessage("ReSharper", "UnusedParameter.Local")]
            static void DenySupplyingDistantResources(ref bool __result, Pawn pawn, ThingDefCountClass need, Thing th)
            {
                if (!__result)
                    return;
                if (!(AssortedAlterations.constructible is Thing constructible))
                    return;
                if (th.IsInValidStorage())
                    return;

                // ReSharper disable once UnusedVariable
                if (!StoreUtility.TryFindBestBetterStorageFor(th, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var storeCell, out var haulDest))
                    return;

                var supplyFromThisDist = th.Position.DistanceTo(constructible.Position);
                var supplyFromStoreDist = storeCell.DistanceTo(constructible.Position);

                // if this is closer to what we're constructing than if we first hauled it to storage
                if (supplyFromThisDist < supplyFromStoreDist)
                    return; // go ahead and supply directly from this

                // otherwise ...
                // this thing would be closer to our constructible once stored, so let's exclude it from consideration so that a
                // Haul (rather than Supply) job will retrieve it *and* any adjacent resources via Mehni's "Pick Up And Haul"
                // https://steamcommunity.com/sharedfiles/filedetails/?id=1279012058

                // and with kevlou's "While You're Up" if our pawn is sufficiently close we'll end up grabbing this *anyway*
                // simply on our WAY to the stockpile we'll now be supplying from (provided we're headed that way, e.g. it has resources)
                // https://steamcommunity.com/sharedfiles/filedetails/?id=1544626521
                __result = false;
            }
        }
    }
}