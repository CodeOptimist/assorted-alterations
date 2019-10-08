using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        class RestockIndicator
        {
            class CompProperties : WorldObjectCompProperties
            {
                public CompProperties() {
                    compClass = typeof(Comp);
                }
            }

            class Comp : WorldObjectComp
            {
                static readonly FieldInfo LastStockGenerationTicks = AccessTools.Field(typeof(SettlementBase_TraderTracker), "lastStockGenerationTicks");
                static readonly PropertyInfo RegenerateStockEveryDays = AccessTools.Property(typeof(SettlementBase_TraderTracker), "RegenerateStockEveryDays");

                public override string CompInspectStringExtra() {
                    if (!(parent is Settlement settlement)) return null;
                    var lastStockGenerationTicks = (int) LastStockGenerationTicks.GetValue(settlement.trader);
                    if (lastStockGenerationTicks == -1) return null;
                    var regenerateStockEveryDays = (int) RegenerateStockEveryDays.GetValue(settlement.trader, null);
                    var remainingTicks = regenerateStockEveryDays * 60000 - (Find.TickManager.TicksGame - lastStockGenerationTicks);
                    if (remainingTicks <= 0) return null;
                    var result = $"Restock in: {remainingTicks.ToStringTicksToPeriod()}";
                    return result;
                }
            }

            public static void DefsLoaded() {
                if (!restockIndicator) return;
                if (WorldObjectDefOf.Settlement.comps.Any(x => x.compClass == typeof(CompProperties))) return;
                WorldObjectDefOf.Settlement.comps.Add(new CompProperties());
            }
        }
    }
}