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
        static class Mod_BetterPawnControl_Birth
        {
            static readonly List<Assembly> bpcAssemblies = LoadedModManager.RunningMods.SingleOrDefault(x => x.Identifier == "1541460369")?.assemblies.loadedAssemblies;
            static readonly Type BpcAnimalManagerType = bpcAssemblies?.Select(x => x.GetType("BetterPawnControl.AnimalManager")).SingleOrDefault(x => x != null);
            static readonly Type BpcAnimalLinkType = bpcAssemblies?.Select(x => x.GetType("BetterPawnControl.AnimalLink")).SingleOrDefault(x => x != null);
            static readonly FieldInfo BpcAnimalLinks = AccessTools.Field(BpcAnimalManagerType, "links");
            static readonly FieldInfo PlayerSettings = AccessTools.Field(typeof(Pawn), nameof(Pawn.playerSettings));
            static readonly PropertyInfo AreaRestriction = AccessTools.Property(typeof(Pawn_PlayerSettings), nameof(Pawn_PlayerSettings.AreaRestriction));
            static readonly CodeInstructionComparer comparer = new CodeInstructionComparer();

            public static void DefsLoaded(HarmonyInstance harmonyInst) {
                if (BpcAnimalManagerType == null || BpcAnimalLinkType == null)  // || !BpcAnimalManagerType.Assembly.GetName().Version.ToString().StartsWith("1.9.2.")
                    return;
                if (PlayerSettings == null || AreaRestriction == null)
                    return;

                harmonyInst.Patch(
                    AccessTools.Method(typeof(Hediff_Pregnant), nameof(Hediff_Pregnant.DoBirthSpawn)),
                    transpiler: new HarmonyMethod(typeof(Mod_BetterPawnControl_Birth), nameof(Birth)));

                harmonyInst.Patch(
                    AccessTools.Method(typeof(CompHatcher), nameof(CompHatcher.Hatch)),
                    transpiler: new HarmonyMethod(typeof(Mod_BetterPawnControl_Birth), nameof(Hatch)));
            }

            static void Born(Pawn pawn, Pawn parent) {
                if (!betterPawnControl_Birth) return;
                if (!(BpcAnimalLinks?.GetValue(null) is IList animalLinks)) return;

                var pawnLinks = (from object animalLink in animalLinks
                    let animal = (Pawn) AccessTools.Field(BpcAnimalLinkType, "animal").GetValue(animalLink)
                    let isObedient = pawn.training?.HasLearned(TrainableDefOf.Obedience)
                    where animal == parent
                    select Activator.CreateInstance(
                        BpcAnimalLinkType,
                        (int) AccessTools.Field(BpcAnimalLinkType, "zone").GetValue(animalLink),
                        pawn,
                        isObedient == true ? (Pawn) AccessTools.Field(BpcAnimalLinkType, "master").GetValue(animalLink) : null,
                        (Area) AccessTools.Field(BpcAnimalLinkType, "area").GetValue(animalLink),
                        pawn.playerSettings?.followDrafted ?? default,
                        pawn.playerSettings?.followFieldwork ?? default,
                        (int) AccessTools.Field(BpcAnimalLinkType, "mapId").GetValue(animalLink))).ToList();

                foreach (var pawnLink in pawnLinks)
                    animalLinks.Add(pawnLink);
            }

            static IEnumerable<CodeInstruction> Birth(IEnumerable<CodeInstruction> instructions) {
                var sequence = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldfld, PlayerSettings),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, PlayerSettings),
                    new CodeInstruction(OpCodes.Callvirt, AreaRestriction?.GetGetMethod()),
                    new CodeInstruction(OpCodes.Callvirt, AreaRestriction?.GetSetMethod()),
                };

                var isInserted = false;
                var codes = instructions.ToList();
                for (var i = 0; i < codes.Count; i++) {
                    if (!isInserted && codes.GetRange(i, sequence.Count).SequenceEqual(sequence, comparer)) {
                        yield return codes[i];
                        yield return codes[i - 1];
                        yield return codes[i + 1];
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Mod_BetterPawnControl_Birth), nameof(Born)));
                        isInserted = true;
                        continue;
                    }
                    yield return codes[i];
                }
            }

            static IEnumerable<CodeInstruction> Hatch(IEnumerable<CodeInstruction> instructions) {
                var sequence = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldfld, PlayerSettings),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld), // leave out the field name in case it's ever renamed
                    new CodeInstruction(OpCodes.Ldfld, PlayerSettings),
                    new CodeInstruction(OpCodes.Callvirt, AreaRestriction?.GetGetMethod()),
                    new CodeInstruction(OpCodes.Callvirt, AreaRestriction?.GetSetMethod()),
                };

                var isInserted = false;
                var codes = instructions.ToList();
                for (var i = 0; i < codes.Count; i++) {
                    if (!isInserted && codes.GetRange(i, sequence.Count).SequenceEqual(sequence, comparer)) {
                        yield return codes[i];
                        yield return codes[i - 1];
                        yield return codes[i + 1];
                        yield return codes[i + 2];
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Mod_BetterPawnControl_Birth), nameof(Born)));
                        isInserted = true;
                        continue;
                    }
                    yield return codes[i];
                }
            }

            class CodeInstructionComparer : IEqualityComparer<CodeInstruction>
            {
                public bool Equals(CodeInstruction x, CodeInstruction y) {
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

                public int GetHashCode(CodeInstruction obj) {
                    return obj.GetHashCode();
                }
            }
        }
    }
}