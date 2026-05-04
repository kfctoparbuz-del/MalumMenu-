using HarmonyLib;
using AmongUs.Data;
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using UnityEngine;
using System;
using System.Security.Cryptography;
using InnerNet;
using System.Collections.Generic;

namespace MalumMenu;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetPlatformData))]
public static class Constants_GetPlatformData
{
    // Постфикс-патч Constants.GetPlatformData для подмены типа платформы пользователя
    public static void Postfix(ref PlatformSpecificData __result)
    {
        if (Utils.StringToPlatformType(MalumMenu.spoofPlatform.Value, out Platforms? platformType))
        {
            __result = new PlatformSpecificData
            {
                Platform = (Platforms)platformType,
                PlatformName = Constants.GetPlatformName()
            };
        }
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
public static class GameData_HandleDisconnect
{
    public static HashSet<int> disconnectQueue = new();

    // Префикс-патч GameData.HandleDisconnect для отслеживания успешных перегрузок
    public static void Prefix(PlayerControl player)
    {
        if (!CheatToggles.runOverload) return;

        NetworkedPlayerInfo playerData = player?.Data;
        if (playerData == null) return;

        bool isTarget = OverloadUI.currentTargets.Contains(playerData);

        if (isTarget) disconnectQueue.Add(playerData.ClientId);
    }

    // Постфикс-патч GameData.HandleDisconnect для отслеживания успешных перегрузок
    // (Избегает двойного подсчёта из-за состояния гонки)
    public static void Postfix(PlayerControl player)
    {
        if (!CheatToggles.runOverload) return;

        NetworkedPlayerInfo playerData = player?.Data;
        if (playerData == null) return;

        int clientId = player.Data.ClientId;

        if (disconnectQueue.Contains(clientId))
        {
            OverloadUI.numSuccesses++;

            if (CheatToggles.olLogDisconnect)
            {
                int total = OverloadUI.currentTargets.Count // Всё ещё подключённые цели
                            + OverloadUI.numSuccesses // Уже отключившиеся цели
                            - disconnectQueue.Count; // Ожидающие логи отключения (Избегает двойного подсчёта из-за состояния гонки)

                string colorStr = ColorUtility.ToHtmlStringRGB(Color.green);

                OverloadUI.LogConsole($"> <b><color=#{colorStr}>!! {playerData.DefaultOutfit.PlayerName} (ID : {playerData.ClientId}) Отключился !! - [{OverloadUI.numSuccesses}/{total}]</color></b>");
            }

            disconnectQueue.Remove(clientId);
        }
    }
}

[HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
public static class FreeChatInputField_UpdateCharCount
{
    // Постфикс-патч FreeChatInputField.UpdateCharCount для изменения отображения charCountText
    public static void Postfix(FreeChatInputField __instance)
    {
        // Работает только если CheatToggles.longerMessages включён
        if (!CheatToggles.longerMessages) return;

        // Обновление charCountText для учёта большего characterLimit
        int length = __instance.textArea.text.Length;
        __instance.charCountText.SetText($"{length}/{__instance.textArea.characterLimit}");

        if (length < 90) // Менее 75%
        {
            __instance.charCountText.color = Color.black;
        }
        else if (length < 120) // Менее 100%
        {
            __instance.charCountText.color = new Color(1f, 1f, 0f, 1f);
        }
        else // 100% или больше
        {
            __instance.charCountText.color = Color.red;
        }
    }
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
public static class ChatBubble_SetName
{
    public static void Postfix(ChatBubble __instance)
	{
        MalumESP.ChatNametags(__instance);
    }
}

[HarmonyPatch(typeof(SystemInfo), nameof(SystemInfo.deviceUniqueIdentifier), MethodType.Getter)]
public static class SystemInfo_deviceUniqueIdentifier_Getter
{
    // Постфикс-патч геттера SystemInfo.deviceUniqueIdentifier
    // Создан для скрытия реального уникального deviceId пользователя путём генерации случайного фальшивого
    public static void Postfix(ref string __result)
    {
        if (!MalumMenu.spoofDeviceId.Value) return;

        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        __result = BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShower_Start
{
    // Постфикс-патч VersionShower.Start для отображения версии MalumMenu
    public static void Postfix(VersionShower __instance)
    {
        if (MalumMenu.inStealthMode || MalumMenu.isPanicked) return;

        if (MalumMenu.supportedAU.Contains(Application.version)) // Проверяет, поддерживается ли версия Among Us
        {
            __instance.text.text =  $"MalumMenu v{MalumMenu.malumVersion} (v{Application.version})"; // Поддерживается
        }
        else
        {
            __instance.text.text =  $"MalumMenu v{MalumMenu.malumVersion} (<color=red>v{Application.version}</color>)"; // Не поддерживается
        }
    }
}

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTracker_Update
{
    // Постфикс-патч PingTracker.Update для отображения авторов MalumMenu и цветного текста пинга
    public static void Postfix(PingTracker __instance)
    {
        if (MalumMenu.inStealthMode)
        {
            __instance.text.alignment = TMPro.TextAlignmentOptions.TopLeft;

            return;
        }

        __instance.text.alignment = TMPro.TextAlignmentOptions.Center;

        int ping = Utils.GetPing();
        string pingText = Utils.GetColoredPingText($"ПИНГ: {ping} мс", ping);

        if (AmongUsClient.Instance.IsGameStarted)
        {
            __instance.aspectPosition.DistanceFromEdge = new Vector3(-0.21f, 0.50f, 0f);

            __instance.text.text = $"MalumMenu на Русском от Ernestrum ~ {pingText}";

            return;
        }

        __instance.text.text = $"MalumMenu на Русском от Ernestrum\n{pingText}";

    }
}

[HarmonyPatch(typeof(DisconnectPopup), nameof(DisconnectPopup.DoShow))]
public static class DisconnectPopup_DoShow
{
    // Постфикс-патч DisconnectPopup.DoShow для копирования кода лобби в буфер обмена при отключении
    public static void Postfix(DisconnectPopup __instance)
    {
        if (!CheatToggles.copyLobbyCodeOnDisconnect) return;

        GUIUtility.systemCopyBuffer = AmongUsClient_OnGameJoined.lastGameIdString;

        __instance.SetText(__instance._textArea.text + "\n\n<size=60%>Код лобби скопирован в буфер обмена</size>");
    }
}

[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.BanMinutesLeft), MethodType.Getter)]
public static class PlayerBanData_BanMinutesLeft_Getter
{
    // Постфикс-патч геттера PlayerBanData.BanMinutesLeft для снятия наказания за отключение
    public static void Postfix(PlayerBanData __instance, ref int __result)
    {
        if (!CheatToggles.avoidPenalties) return;

        __instance.BanPoints = 0f; // Удаляет все очки бана
        __result = 0; // Удаляет все минуты бана
    }
}

[HarmonyPatch(typeof(FullAccount), nameof(FullAccount.CanSetCustomName))]
public static class FullAccount_CanSetCustomName
{
    // Префикс-патч FullAccount.CanSetCustomName для разрешения использования пользовательских имён
    public static void Prefix(ref bool canSetName)
    {
        if (CheatToggles.unlockFeatures)
        {
            canSetName = true;
        }
    }
}

[HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CanPlayOnline))]
public static class AccountManager_CanPlayOnline
{
    // Префикс-патч AccountManager.CanPlayOnline для разрешения онлайн-игр
    public static void Postfix(ref bool __result)
    {
        if (CheatToggles.unlockFeatures)
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
public static class InnerNetClient_JoinGame
{
    // Префикс-патч InnerNetClient.JoinGame для разрешения онлайн-игр
    public static void Prefix()
    {
        if (CheatToggles.unlockFeatures)
        {
            DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
        }
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
public static class GameManager_CheckTaskCompletion
{
    // Префикс-патч GameManager.CheckTaskCompletion для предотвращения завершения текущей игры
    public static bool Prefix(ref bool __result)
    {
        if (!CheatToggles.noGameEnd) return true;

        __result = false;

        return false;
    }
}

[HarmonyPatch(typeof(Mushroom), nameof(Mushroom.FixedUpdate))]
public static class Mushroom_FixedUpdate
{
    public static void Postfix(Mushroom __instance)
    {
        MalumESP.SporeCloudVision(__instance);
    }
}

// Найдено здесь: https://github.com/g0aty/SickoMenu/blob/main/hooks/PlainDoor.cpp
[HarmonyPatch(typeof(DoorBreakerGame), nameof(DoorBreakerGame.Start))]
public static class DoorBreakerGame_Start
{
    // Префикс-патч DoorBreakerGame.Start для автоматического открытия двери при взаимодействии игрока с ней
    public static bool Prefix(DoorBreakerGame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        DoorsHandler.OpenDoor(__instance.MyDoor);
        __instance.MyDoor.SetDoorway(true);
        __instance.Close();

        return false;
    }
}

// Найдено здесь: https://github.com/g0aty/SickoMenu/blob/main/hooks/PlainDoor.cpp
[HarmonyPatch(typeof(DoorCardSwipeGame), nameof(DoorCardSwipeGame.Begin))]
public static class DoorCardSwipeGame_Begin
{
    // Префикс-патч DoorCardSwipeGame.Begin для автоматического открытия двери при взаимодействии игрока с ней
    public static bool Prefix(DoorCardSwipeGame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        DoorsHandler.OpenDoor(__instance.MyDoor);
        __instance.MyDoor.SetDoorway(true);
        __instance.Close();

        return false;
    }
}

// Найдено здесь: https://github.com/g0aty/SickoMenu/blob/main/hooks/PlainDoor.cpp
[HarmonyPatch(typeof(MushroomDoorSabotageMinigame), nameof(MushroomDoorSabotageMinigame.Begin))]
public static class MushroomDoorSabotageMinigame_Begin
{
    // Префикс-патч MushroomDoorSabotageMinigame.Begin для автоматического открытия двери при взаимодействии игрока с ней
    public static bool Prefix(MushroomDoorSabotageMinigame __instance)
    {
        if (!CheatToggles.autoOpenDoorsOnUse) return true;

        __instance.FixDoorAndCloseMinigame();

        return false;
    }
}

// ТРЕБУЕТ ИСПРАВЛЕНИЯ: Блокирует использование консолей, к которым имеет доступ
// самозванец (например, для исправления саботажей), когда чит отключён

// [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
// public static class Console_CanUse
// {
//     // Префикс-патч Console.CanUse для разрешения самозванцам выполнять задания
//     public static void Prefix(Console __instance)
//     {
//         __instance.AllowImpostor = CheatToggles.impostorTasks;
//     }
// }

[HarmonyPatch(typeof(IntroCutscene), "CoBegin")]
public static class IntroCutscene_CoBegin
{
    // Префикс-патч IntroCutscene.CoBegin для принудительной установки роли локального игрока на указанную роль
    public static void Prefix()
    {
        if (!Utils.isHost || !CheatToggles.forcedRole.HasValue) return;

        var forcedRole = CheatToggles.forcedRole.Value;

        // Если у LocalPlayer уже есть принудительная роль, ничего не делать
        if (PlayerControl.LocalPlayer.Data.RoleType == forcedRole)
        {
            return;
        }

        // Найти игрока с принудительной ролью для обмена ролями
        PlayerControl roleSwapTarget = null;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.RoleType != forcedRole) continue;
            roleSwapTarget = player;
            break;
        }

        DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, forcedRole);

        if (roleSwapTarget != null)
        {
            DestroyableSingleton<RoleManager>.Instance.SetRole(roleSwapTarget, PlayerControl.LocalPlayer.Data.RoleType);
        }
    }
}

// Найдено здесь: https://github.com/g0aty/SickoMenu/blob/main/hooks/LobbyBehaviour.cpp
[HarmonyPatch(typeof(GameContainer), nameof(GameContainer.SetupGameInfo))]
public static class GameContainer_SetupGameInfo
{
    // Постфикс-патч GameContainer.SetupGameInfo для отображения дополнительной информации при поиске игры:
    // имя хоста (например, Astral), код лобби (например, KLHCEG), платформа хоста (например, Epic) и возраст лобби в минутах (например, 4:20)
    public static void Postfix(GameContainer __instance)
    {
        if (!CheatToggles.seeLobbyInfo) return;

        // Иконка члена экипажа правильно выравнивается с этим
        const string separator = "<#0000>000000000000000</color>";

        var trueHostName = __instance.gameListing.TrueHostName;

        var age = __instance.gameListing.Age;
        var lobbyTime = $"Возраст: {age / 60}:{(age % 60 < 10 ? "0" : "")}{age % 60}";

        var platform = Utils.PlatformTypeToString(__instance.gameListing.Platform);

        // Устанавливает текст поля вместимости, чтобы включить новую информацию
        __instance.capacity.text = $"<size=40%>{separator}\n{trueHostName}\n{__instance.capacity.text}\n" +
                                   $"<#fb0>{GameCode.IntToGameName(__instance.gameListing.GameId)}</color>\n" +
                                   $"<#b0f>{platform}</color>\n{lobbyTime}\n{separator}</size>";
    }
}

[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
public static class BanMenu_SetVisible
{
    // Префикс-патч BanMenu.SetVisible для всегдашнего отображения кнопок кика и бана как у хоста
    public static bool Prefix(BanMenu __instance, bool show)
    {
        if (!Utils.isHost) return true;

        show &= PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null;

        __instance.BanButton.gameObject.SetActive(true);
        __instance.KickButton.gameObject.SetActive(true);
        __instance.MenuButton.gameObject.SetActive(show);

        return false;
    }
}

[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
public static class IGameOptionsExtensions_GetAdjustedNumImpostors
{
    // Префикс-патч IGameOptionsExtensions.GetAdjustedNumImpostors для снятия ограничений на количество самозванцев
    public static bool Prefix(IGameOptions __instance, ref int __result)
    {
        if (!CheatToggles.noOptionsLimits) return true;

        __result = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;

        return false;
    }
}

[HarmonyPatch(typeof(PlayerPurchasesData), nameof(PlayerPurchasesData.GetPurchase))]
public static class PlayerPurchasesData_GetPurchase
{
    // Постфикс-патч PlayerPurchasesData.GetPurchase для разблокировки всей косметики
    public static void Postfix(ref bool __result)
    {
        if (!CheatToggles.freeCosmetics) return;

        __result = true;
    }
}
