using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using SCP575.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP575.Patch
{
    [HarmonyPatch(typeof(FpcMotor), nameof(FpcMotor.UpdateFloating))]
    public class UpdateFloating
    {
        public static bool Prefix(FpcMotor __instance)
            => !Extensions.IsDummy(__instance.Hub);
    }
}
