using System;
using HarmonyLib;

namespace MalumMenu;

// Читы GuestMode закомментированы, так как они сломаны в последних обновлениях

// [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.StartInitialLoginFlow))]
// public static class EOSManager_StartInitialLoginFlow
// {
//     /// <summary>
//     /// Префикс-патч EOSManager.StartInitialLoginFlow для автоматической игры с гостевым аккаунтом
//     /// при загрузке игры с включённым guestMode
//     /// </summary>
//     /// <param name="__instance">Экземпляр <c>EOSManager</c>.</param>
//     /// <returns><c>false</c> для пропуска оригинального метода, <c>true</c> для выполнения оригинального метода.</returns>
//     public static bool Prefix(EOSManager __instance)
//     {
//         // Всегда удалять старые гостевые аккаунты, чтобы избежать всплывающего окна слияния аккаунтов
//         __instance.DeleteDeviceID(new System.Action(__instance.EndMergeGuestAccountFlow));

//         // Войти в новую временную учётную запись, если пользователь играет в гостевом режиме
//         if (!MalumMenu.guestMode.Value) return true;
//         __instance.StartTempAccountFlow();
//         __instance.CloseStartupWaitScreen();

//         return false;
//     }
// }

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.HasServerTimestamp), MethodType.Getter)]
public static class EOSManager_HasServerTimestamp_Getter
{
    // Постфикс-патч геттера EOSManager.HasServerTimestamp для обеспечения возможности подмены даты
    public static void Postfix(ref bool __result)
    {
        if (!CheatToggles.spoofAprilFoolsDate) return;

        __result = true;
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.ApproximateServerTime), MethodType.Getter)]
public static class EOSManager_ApproximateServerTime_Getter
{
    // Постфикс-патч геттера EOSManager.ApproximateServerTime для подмены даты на 1 апреля, 7:01 UTC
    public static void Postfix(ref Il2CppSystem.DateTime __result)
    {
        if (!CheatToggles.spoofAprilFoolsDate) return;

        var managedDate = new DateTime(DateTime.UtcNow.Year, 4, 1, 7, 1, 0, DateTimeKind.Utc);
        __result = new Il2CppSystem.DateTime(managedDate.Ticks);
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFreechatAllowed))]
public static class EOSManager_IsFreechatAllowed
{
    // Префикс-патч EOSManager.IsFreechatAllowed для разблокировки свободного чата
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.unlockFeatures)
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFriendsListAllowed))]
public static class EOSManager_IsFriendsListAllowed
{
    // Префикс-патч EOSManager.IsFriendsListAllowed для разблокировки списка друзей
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.unlockFeatures)
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
public static class EOSManager_IsAllowedOnline
{
    // Префикс-патч EOSManager.IsAllowedOnline для разрешения онлайн-игр
    public static void Prefix(ref bool canOnline)
    {
        if (CheatToggles.unlockFeatures)
        {
            canOnline = true;
        }
    }
}

[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsMinorOrWaiting))]
public static class EOSManager_IsMinorOrWaiting
{
    // Префикс-патч EOSManager.IsMinorOrWaiting для снятия статуса несовершеннолетнего
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.unlockFeatures)
        {
            __result = false;
        }
    }
}
