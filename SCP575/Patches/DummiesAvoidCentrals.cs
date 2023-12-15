using CentralAuth;
using HarmonyLib;
using NorthwoodLib.Pools;
using SCP575.API.Extensions;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SCP575.Patches
{
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.InstanceMode), MethodType.Setter)]
    internal class DummiesAvoidCentrals
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label skip = generator.DefineLabel();

            newInstructions[0].labels.Add(skip);

            newInstructions.InsertRange(0, new List<CodeInstruction>()
            {
                new(OpCodes.Ldsfld, AccessTools.Field(typeof(Dummies), nameof(Dummies.AllDummies))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld,
                    AccessTools.Field(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager._hub))),
                new(OpCodes.Callvirt,
                    AccessTools.Method(typeof(HashSet<ReferenceHub>), nameof(HashSet<ReferenceHub>.Contains))),
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
