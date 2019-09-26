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
            static readonly CodeInstructionComparer comparer = new CodeInstructionComparer();
            static readonly Assembly bpcAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "BetterPawnControl");
            static readonly Type AnimalLinkType = bpcAssembly.GetType("BetterPawnControl.AnimalLink");
            static readonly FieldInfo animalLinksField = AccessTools.Field(bpcAssembly.GetType("BetterPawnControl.AnimalManager"), "links");

            public static void DefsLoaded(HarmonyInstance HarmonyInst)
            {
                if (bpcAssembly == null || !bpcAssembly.ImageRuntimeVersion.StartsWith("v2.0."))
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
                if (!(animalLinksField.GetValue(null) is IList animalLinks)) return;

                var pawnLinks = (from object animalLink in animalLinks
                    let animal = (Pawn) AccessTools.Field(AnimalLinkType, "animal").GetValue(animalLink)
                    where animal == parent
                    select Activator.CreateInstance(
                        AnimalLinkType,
                        (int) AccessTools.Field(AnimalLinkType, "zone").GetValue(animalLink),
                        pawn,
                        (Pawn) AccessTools.Field(AnimalLinkType, "master").GetValue(animalLink),
                        (Area) AccessTools.Field(AnimalLinkType, "area").GetValue(animalLink),
                        (bool) AccessTools.Field(AnimalLinkType, "followDrafted").GetValue(animalLink),
                        (bool) AccessTools.Field(AnimalLinkType, "followFieldwork").GetValue(animalLink),
                        (int) AccessTools.Field(AnimalLinkType, "mapId").GetValue(animalLink))).ToList();

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
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_PlayerSettings), "get_AreaRestriction")),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_PlayerSettings), "set_AreaRestriction"))
                };

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
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_PlayerSettings), "get_AreaRestriction")),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_PlayerSettings), "set_AreaRestriction"))
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