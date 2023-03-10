using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;

namespace SCP575.Patch
{
    /// <summary>
    /// Thanks Jesus-QC#4544 for the patch.
    /// </summary>
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.InstanceMode), MethodType.Setter)]
    public class DummysAvoidCentralServers
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label skip = generator.DefineLabel();

            newInstructions[0].labels.Add(skip);

            newInstructions.InsertRange(0, new List<CodeInstruction>()
            {
                new(OpCodes.Ldsfld, AccessTools.Field(typeof(Scp575), nameof(Scp575.Dummies))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld,
                    AccessTools.Field(typeof(CharacterClassManager), nameof(CharacterClassManager._hub))),
                new(OpCodes.Callvirt,
                    AccessTools.Method(typeof(List<ReferenceHub>), nameof(List<ReferenceHub>.Contains))),
                new(OpCodes.Brfalse_S, skip),
                new(OpCodes.Ldc_I4_2),
                new(OpCodes.Starg_S, 1),
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}