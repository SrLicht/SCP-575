using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.FirstPersonControl;
using SCP575.Resources;

namespace SCP575.Patch
{
    [HarmonyPatch(typeof(FpcMouseLook), nameof(FpcMouseLook.UpdateRotation))]
    public class DummyRotationPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label skip = generator.DefineLabel();
        
            newInstructions[newInstructions.Count - 1].labels.Add(skip);
        
            newInstructions.InsertRange(0, new List<CodeInstruction>()
            {
                new (OpCodes.Ldarg_0),
                new (OpCodes.Ldfld, AccessTools.Field(typeof(FpcMouseLook), nameof(FpcMouseLook._hub))),
                new (OpCodes.Call, AccessTools.Method(typeof(Extensions), nameof(Extensions.IsDummy))),
                new (OpCodes.Brtrue_S, skip)
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}