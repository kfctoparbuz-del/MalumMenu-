using HarmonyLib;

namespace MalumMenu;

// Найдено здесь: https://github.com/astra1dev/AUnlocker/blob/main/src/OptionsPatches.cs

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
public static class NumberOption_Increase
{
    // Увеличивает значение числовой опции игры без ограничений
    public static bool Prefix(NumberOption __instance)
    {
        if (!CheatToggles.noOptionsLimits) return true;

        // Избегает обхода ограничений на количество импостеров и скорость игроков в играх не HideNSeek
        // из-за ограничений античита
        if (!Utils.isHideNSeek && __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed) return true;

        __instance.Value += __instance.Increment;
        __instance.UpdateValue();
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();

        return false;
    }
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
public static class NumberOption_Decrease
{
    // Уменьшает значение числовой опции игры без ограничений
    public static bool Prefix(NumberOption __instance)
    {
        if (!CheatToggles.noOptionsLimits) return true;

        // Избегает обхода ограничений на количество импостеров и скорость игроков в играх не HideNSeek
        // из-за ограничений античита
        if (!Utils.isHideNSeek && __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed) return true;

        __instance.Value -= __instance.Increment;
        __instance.UpdateValue();
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();

        return false;
    }
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
public static class NumberOption_Initialize
{
    // Устанавливает допустимый диапазон числовой опции игры практически неограниченным
    public static void Postfix(NumberOption __instance)
    {
        if (!CheatToggles.noOptionsLimits) return;

        // Избегает обхода ограничений на количество импостеров и скорость игроков в играх не HideNSeek
        // из-за ограничений античита
        if (!Utils.isHideNSeek && __instance.Title is StringNames.GameNumImpostors or StringNames.GamePlayerSpeed) return;

        __instance.ValidRange = new FloatRange(-999f, 999f);
    }
}
