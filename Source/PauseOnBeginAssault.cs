using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
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
            static int msgIdx = -1;

            [HarmonyPatch(typeof(LordJob_StageThenAttack), nameof(LordJob_StageThenAttack.CreateGraph))]
            static class LordJob_StageThenAttack_CreateGraph_Patch
            {

                [HarmonyTranspiler]
                static IEnumerable<CodeInstruction> PauseOnBeginAssault(IEnumerable<CodeInstruction> instructions)
                {
                    var codes = instructions.ToList();
                    for (var i = 0; i < codes.Count; i++) {
                        if (codes[i].operand is string str && str == "MessageRaidersBeginningAssault")
                            msgIdx = i;
                        if (msgIdx == -1) continue;
                        if (codes[i].operand is MethodInfo methodInfo && methodInfo == AccessTools.Method(typeof(Transition), nameof(Transition.AddPreAction))) {
                            void Pause()
                            {
                                if (pauseOnBeginAssault)
                                    Find.TickManager.Pause();
                            }

                            codes.InsertRange(
                                i + 1, new List<CodeInstruction> {
                                    codes[msgIdx - 1], // instance we're calling AddPreAction on, e.g. lodloc.s 4
                                    new CodeInstruction(OpCodes.Ldarg_0),
                                    new CodeInstruction(OpCodes.Ldftn, new Action(Pause).Method),
                                    new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(Action), new[] {typeof(object), typeof(IntPtr)})),
                                    new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(TransitionAction_Custom), new[] {typeof(Action)})),
                                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Transition), nameof(Transition.AddPreAction))),
                                });
                            break;
                        }
                    }

                    return codes.AsEnumerable();
                }
            }
        }
    }
}
