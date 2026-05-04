using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetAnonymousVotes))]
public static class LogicOptions_GetAnonymousVotes
{
    // Постфикс-патч LogicOptions.GetAnonymousVotes для отключения анонимных голосов для чита revealVotes
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.revealVotes)
        {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetAnonymousVotes))]
public static class LogicOptionsNormal_GetAnonymousVotes
{
    // Постфикс-патч LogicOptionsNormal.GetAnonymousVotes для отключения анонимных голосов для чита revealVotes
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.revealVotes)
        {
            __result = false;
        }
    }
}
