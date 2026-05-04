using HarmonyLib;
using System;

namespace MalumMenu;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManager_Start
{
	// Постфикс-патч HudManager.Start для предоставления доступа к мини-карте также и самозванцам
	public static void Postfix(HudManager __instance)
	{
		__instance.MapButton.OnClick.RemoveAllListeners(); // Удалить предыдущее действие OnClick

		// Всегда открывать обычную карту при нажатии кнопки карты
		// Для доступа к карте саботажа можно использовать кнопку саботажа
		__instance.MapButton.OnClick.AddListener((Action) (() =>
        {
			__instance.ToggleMapVisible(new MapOptions
			{
				Mode = MapOptions.Modes.Normal
			});

		}));
	}
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update
{
	public static void Postfix(HudManager __instance)
    {
		__instance.ShadowQuad.gameObject.SetActive(!MalumESP.IsFullbrightActive()); // Полная яркость

		if (Utils.IsChatUiActive()) // Всегда чат
		{
			__instance.Chat.gameObject.SetActive(true);
		}
		else
		{
			Utils.CloseChat();
			__instance.Chat.gameObject.SetActive(false);
		}

		MalumCheats.UseVentCheat(__instance);
		MalumESP.ZoomOut(__instance);
		MalumESP.FreecamCheat();

		// Закрыть меню выбора игрока, если нет включенного чита PPM
		if (PlayerPickMenu.playerpickMenu != null && CheatToggles.ShouldPPMClose())
		{
            PlayerPickMenu.playerpickMenu.Close();
        }
    }
}
