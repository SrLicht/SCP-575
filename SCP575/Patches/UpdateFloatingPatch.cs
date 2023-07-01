using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using SCP575.Resources;

namespace SCP575.Patch
{
    [HarmonyPatch(typeof(FpcMotor), nameof(FpcMotor.UpdateFloating))]
    public class UpdateFloating
    {
        public static bool Prefix(FpcMotor __instance)
            => !Extensions.IsDummy(__instance.Hub);
    }
}
