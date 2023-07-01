namespace SCP575.Patch
{
    /*[HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.InstanceMode), MethodType.Setter)]
    public class InstanceModeSetterPatch
    {
        public static bool Prefix(CharacterClassManager __instance)
            => !Extensions.IsDummy(__instance._hub);
    }*/
}
