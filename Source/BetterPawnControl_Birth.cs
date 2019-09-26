using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class BetterPawnControl_Birth
        {
            static readonly List<Assembly> bpcAssemblies = LoadedModManager.RunningMods.SingleOrDefault(x => x.Identifier == "1541460369")?.assemblies.loadedAssemblies;
            static readonly Type Bpc_AnimalManager = bpcAssemblies?.Select(x => x.GetType("BetterPawnControl.AnimalManager")).SingleOrDefault(x => x != null);
            static readonly Type Bpc_AnimalLink = bpcAssemblies?.Select(x => x.GetType("BetterPawnControl.AnimalLink")).SingleOrDefault(x => x != null);
            static readonly FieldInfo bpcAnimalLinksField = AccessTools.Field(Bpc_AnimalManager, "links");
            static readonly CodeInstructionComparer comparer = new CodeInstructionComparer();

            public static void DefsLoaded(HarmonyInstance HarmonyInst)
            {
                if (Bpc_AnimalManager == null || Bpc_AnimalLink == null) // || !Bpc_AnimalManager.Assembly.ImageRuntimeVersion.StartsWith("v2.0"))
                    return;

                HarmonyInst.Patch(
                    AccessTools.Method(typeof(Hediff_Pregnant), nameof(Hediff_Pregnant.DoBirthSpawn)),
                    transpiler: new HarmonyMethod(typeof(BetterPawnControl_Birth), nameof(Birth)));

                HarmonyInst.Patch(
                    AccessTools.Method(typeof(CompHatcher), nameof(CompHatcher.Hatch)),
                    transpiler: new HarmonyMethod(typeof(BetterPawnControl_Birth), nameof(Hatch)));
            }

            static void Born(Pawn pawn, Pawn parent)
            {
                if (!betterPawnControl_Birth) return;
                if (!(bpcAnimalLinksField.GetValue(null) is IList animalLinks)) return;

                var pawnLinks = (from object animalLink in animalLinks
                    let animal = (Pawn) AccessTools.Field(Bpc_AnimalLink, "animal").GetValue(animalLink)
                    where animal == parent
                    select Activator.CreateInstance(
                        Bpc_AnimalLink,
                        (int) AccessTools.Field(Bpc_AnimalLink, "zone").GetValue(animalLink),
                        pawn,
                        (Pawn) AccessTools.Field(Bpc_AnimalLink, "master").GetValue(animalLink),
                        (Area) AccessTools.Field(Bpc_AnimalLink, "area").GetValue(animalLink),
                        (bool) AccessTools.Field(Bpc_AnimalLink, "followDrafted").GetValue(animalLink),
                        (bool) AccessTools.Field(Bpc_AnimalLink, "followFieldwork").GetValue(animalLink),
                        (int) AccessTools.Field(Bpc_AnimalLink, "mapId").GetValue(animalLink))).ToList();

                foreach (var pawnLink in pawnLinks)
                    animalLinks.Add(pawnLink);
            }

            static IEnumerable<CodeInstruction> Birth(IEnumerable<CodeInstruction> instructions)
            {
                var playerSettingsField = AccessTools.Field(typeof(Pawn), "playerSettings");
                var sequence = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldfld, playerSettingsField),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, playerSettingsField),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Pawn_PlayerSettings), "AreaRestriction")?.GetGetMethod()),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Pawn_PlayerSettings), "AreaRestriction")?.GetSetMethod()),
                };

                //AccessTools.Property(typeof(Pawn_PlayerSettings), "AreaRestriction").GetGetMethod();

                var codes = instructions.ToList();
                for (var i = 0; i < codes.Count - sequence.Count; i++)
                    if (codes.GetRange(i, sequence.Count).SequenceEqual(sequence, comparer)) {
                        codes.InsertRange(
                            i + sequence.Count,
                            new[] {
                                codes[i - 1], codes[i + 1],
                                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BetterPawnControl_Birth), nameof(Born)))
                            });
                        break;
                    }

                return codes.AsEnumerable();
            }

            static IEnumerable<CodeInstruction> Hatch(IEnumerable<CodeInstruction> instructions)
            {
                var playerSettingsField = AccessTools.Field(typeof(Pawn), "playerSettings");
                var sequence = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldfld, playerSettingsField),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld), // leave out the field name in case it's ever renamed
                    new CodeInstruction(OpCodes.Ldfld, playerSettingsField),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Pawn_PlayerSettings), "AreaRestriction")?.GetGetMethod()),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Pawn_PlayerSettings), "AreaRestriction")?.GetSetMethod()),
                };

                var codes = instructions.ToList();
                for (var i = 0; i < codes.Count - sequence.Count; i++)
                    if (codes.GetRange(i, sequence.Count).SequenceEqual(sequence, comparer)) {
                        codes.InsertRange(
                            i + sequence.Count,
                            new[] {
                                codes[i - 1], codes[i + 1], codes[i + 2],
                                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BetterPawnControl_Birth), nameof(Born)))
                            });
                        break;
                    }

                return codes.AsEnumerable();
            }

            class CodeInstructionComparer : IEqualityComparer<CodeInstruction>
            {
                public bool Equals(CodeInstruction x, CodeInstruction y)
                {
                    if (ReferenceEquals(null, x)) return false;
                    if (ReferenceEquals(null, y)) return false;
                    if (ReferenceEquals(x, y)) return true;
                    if (!Equals(x.opcode, y.opcode)) return false;
                    if (Equals(x.operand, y.operand)) return true;

                    // consider null of Ldfld a wildcard
                    if (x.opcode == OpCodes.Ldfld && (x.operand == null || y.operand == null))
                        return true;
                    return false;
                }

                public int GetHashCode(CodeInstruction obj)
                {
                    return obj.GetHashCode();
                }
            }
        }
    }
}