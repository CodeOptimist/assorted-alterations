using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        class Mod_QualityBuilder_Forbidden
        {
            static readonly List<Assembly> qbAssemblies = LoadedModManager.RunningMods.SingleOrDefault(x => x.Identifier == "754637870")?.assemblies.loadedAssemblies;
            static readonly Type QbCompQualityBuilderType = qbAssemblies?.Select(x => x.GetType("QualityBuilder.CompQualityBuilder")).SingleOrDefault(x => x != null);
            static readonly MethodInfo QbPostSpawnSetupMethod = AccessTools.Method(QbCompQualityBuilderType, "PostSpawnSetup");

            static void GetForbidden(ref bool __state, ThingComp __instance) {
                if (!qualityBuilder_Forbidden) return;
                __state = __instance.parent.IsForbidden(Faction.OfPlayer);
            }

            static void RestoreForbidden(bool __state, ThingComp __instance) {
                if (!qualityBuilder_Forbidden) return;
                __instance.parent.SetForbidden(__state, false);
            }

            public static void DefsLoaded(HarmonyInstance harmonyInst) {
                if (QbCompQualityBuilderType == null || QbPostSpawnSetupMethod == null) return;   // || QbCompQualityBuilderType.Assembly.GetName().Version.ToString() != "1.0.8.0"
                harmonyInst.Patch(QbPostSpawnSetupMethod, prefix: new HarmonyMethod(typeof(Mod_QualityBuilder_Forbidden), nameof(GetForbidden)));
                harmonyInst.Patch(QbPostSpawnSetupMethod, postfix: new HarmonyMethod(typeof(Mod_QualityBuilder_Forbidden), nameof(RestoreForbidden)));
            }
        }
    }
}