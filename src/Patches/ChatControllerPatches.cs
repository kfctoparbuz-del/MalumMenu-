using HarmonyLib;
using System;
using UnityEngine;
using System.Text.RegularExpressions;

namespace MalumMenu;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatController_AddChat
{
	// Префикс-патч ChatController.AddChat для получения сообщений призраков, если CheatSettings.seeGhosts включён, даже если LocalPlayer жив
	// По сути делает то же самое, что и оригинальный метод, с необходимыми изменениями
	public static bool Prefix(PlayerControl sourcePlayer, string chatText, bool censor, ChatController __instance)
    {
		// Просто выполнить оригинальный метод, если seeGhosts отключён или LocalPlayer уже мёртв
        if (!CheatToggles.seeGhosts || PlayerControl.LocalPlayer.Data.IsDead) return true;

        if (!sourcePlayer || !PlayerControl.LocalPlayer) return true;

		NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
		NetworkedPlayerInfo data2 = sourcePlayer.Data;

		if (data2 == null || data == null) return true; // Убрана проверка isDead для LocalPlayer

		ChatBubble pooledBubble = __instance.GetPooledBubble();

		try
		{
			pooledBubble.transform.SetParent(__instance.scroller.Inner);
			pooledBubble.transform.localScale = Vector3.one;
			bool flag = sourcePlayer == PlayerControl.LocalPlayer;
			if (flag)
			{
				pooledBubble.SetRight();
			}
			else
			{
				pooledBubble.SetLeft();
			}
			bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
			pooledBubble.SetCosmetics(data2);
			__instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2), null);
			if (censor && AmongUs.Data.DataManager.Settings.Multiplayer.CensorChat)
			{
				chatText = BlockedWords.CensorWords(chatText, false);
			}
			pooledBubble.SetText(chatText);
			pooledBubble.AlignChildren();
			__instance.AlignAllBubbles();
			if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null)
			{
				__instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
			}
			if (!flag && !__instance.IsOpenOrOpening)
			{
				SoundManager.Instance.PlaySound(__instance.messageSound, false).pitch = 0.5f + sourcePlayer.PlayerId / 15f;
				__instance.chatNotification.SetUp(sourcePlayer, chatText);
			}
		}
		catch (Exception message)
		{
			ChatController.Logger.Error(message.ToString(), null);
			__instance.chatBubblePool.Reclaim(pooledBubble);
		}

        return false; // Полностью пропускает оригинальный метод
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public static class ChatController_Update
{
    // Постфикс-патч ChatController.Update для разблокировки большей длины сообщений
    public static void Postfix(ChatController __instance)
    {
        //__instance.freeChatField.textArea.allowAllCharacters = CheatToggles.chatJailbreak; // На самом деле не используется кодом игры, но я всё равно включаю
        //__instance.freeChatField.textArea.AllowSymbols = true; // Разрешить отправку определённых символов
        //__instance.freeChatField.textArea.AllowEmail = CheatToggles.chatJailbreak; // Разрешить отправку email-адресов, когда chatJailbreak включён
        //__instance.freeChatField.textArea.AllowPaste = CheatToggles.chatJailbreak; // Разрешить вставку из буфера обмена в чат, когда chatJailbreak включён

        if (CheatToggles.longerMessages)
		{
			// Увеличение максимальной длины на 20 символов всё ещё избегает киков античита
            __instance.freeChatField.textArea.characterLimit = 120;
        }
		else
		{
            __instance.freeChatField.textArea.characterLimit = 100;
        }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatController_SendChat
{
    // Постфикс-патч ChatController.SendChat для разблокировки более низких лимитов частоты сообщений
    public static void Postfix(ChatController __instance)
    {
        if (!CheatToggles.lowerRateLimits) return;

		if (__instance.timeSinceLastMessage == 0f)
		{
			// Уменьшение лимита частоты максимум на 1 секунду всё ещё избегает киков античита
			__instance.timeSinceLastMessage += 1f;
		}
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendFreeChat))]
public static class ChatController_SendFreeChat
{
    // Префикс-патч ChatController.SendFreeChat для разрешения отправки URL без цензуры
    public static bool Prefix(ChatController __instance)
    {
		// Работает только если CheatSettings.bypassUrlBlock включён
        if (!CheatToggles.bypassUrlBlock) return true;

        string text = __instance.freeChatField.Text;

        // Замена точек в URL и email-адресах на запятые для обхода цензуры
        string modifiedText = CensorUrlsAndEmails(text);

        ChatController.Logger.Debug("SendFreeChat () :: Отправка сообщения: '" + modifiedText + "'", null);
        PlayerControl.LocalPlayer.RpcSendChat(modifiedText);

        return false;
    }

    private static string CensorUrlsAndEmails(string text)
    {
        // Регулярное выражение для поиска URL и email-адресов
        string pattern = @"(http[s]?://)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,6}(/[\w-./?%&=]*)?|([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+)";
        Regex regex = new Regex(pattern);

        // Замена точек в каждом совпадении
        return regex.Replace(text, match =>
        {
            var censored = match.Value;
            censored = censored.Replace('.', ',');
            return censored;
        });
    }
}
