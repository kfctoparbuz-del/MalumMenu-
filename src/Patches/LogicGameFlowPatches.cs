using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class LogicGameFlowNormal_CheckEndCriteria
{
    // Префикс-патч LogicGameFlowNormal.CheckEndCriteria для предотвращения завершения текущей игры
    public static bool Prefix()
    {
        return !CheatToggles.noGameEnd;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
public static class LogicGameFlowNormal_IsGameOverDueToDeath
{
    // Постфикс-патч LogicGameFlowNormal.IsGameOverDueToDeath для предотвращения зависания игры
    // после исключения, которое должно было вызвать завершение игры
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.noGameEnd)
        {
            __result = false;
        }

    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
public static class LogicGameFlowHnS_CheckEndCriteria
{
    // Префикс-патч LogicGameFlowHnS.CheckEndCriteria для предотвращения завершения текущей игры в режиме HnS
    public static bool Prefix()
    {
        return !CheatToggles.noGameEnd;
    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.IsGameOverDueToDeath))]
public static class LogicGameFlowHnS_IsGameOverDueToDeath
{
    // Постфикс-патч LogicGameFlowNormal.IsGameOverDueToDeath для предотвращения зависания игры в режиме HnS
    // после исключения, которое должно было вызвать завершение игры
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.noGameEnd)
        {
            __result = false;
        }

    }
}
