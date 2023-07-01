using HarmonyLib;
using SCP575.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP575.Patch
{
    /*[HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.InstanceMode), MethodType.Setter)]
    public class InstanceModeSetterPatch
    {
        public static bool Prefix(CharacterClassManager __instance)
            => !Extensions.IsDummy(__instance._hub);
    }*/
}
