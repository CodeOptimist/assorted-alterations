using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace AssortedAlterations
{
    partial class AssortedAlterations
    {
        static class PauseOnBeginAssault
        {
            [HarmonyPatch(typeof(LordJob_StageThenAttack), nameof(LordJob_StageThenAttack.CreateGraph))]
            static class LordJob_StageThenAttack_CreateGraph_Patch
            {
                [HarmonyTranspiler]
                static IEnumerable<CodeInstruction> PauseOnBeginAssault(IEnumerable<CodeInstruction> instructions) {
                    var msgIdx = -1;
                    var isInserted = false;

                    var codes = instructions.ToList();
                    for (var i = 0; i < codes.Count; i++) {
                        if (codes[i].operand is string str && str == "MessageRaidersBeginningAssault")
                            msgIdx = i;

                        if (!isInserted && msgIdx != -1 && codes[i].operand is MethodInfo method && method == AccessTools.Method(typeof(Transition), nameof(Transition.AddPreAction))) {
                            yield return codes[i];
                            yield return codes[msgIdx - 1];   // instance we're calling AddPreAction on, e.g. lodloc.s 4
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldftn, new Action(() => {
                                if (pauseOnBeginAssault)
                                    Find.TickManager.Pause();
                            }).Method);
                            yield return new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(Action), new[] {typeof(object), typeof(IntPtr)}));
                            yield return new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(TransitionAction_Custom), new[] {typeof(Action)}));
                            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Transition), nameof(Transition.AddPreAction)));
                            isInserted = true;
                            continue;
                        }

                        yield return codes[i];
                    }
                }
            }
        }
    }
}