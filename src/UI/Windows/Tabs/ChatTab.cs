using UnityEngine;

namespace MalumMenu;

public class ChatTab : ITab
{
    public string name => "Чат";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawTextbox();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.enableChat = GUILayout.Toggle(CheatToggles.enableChat, " Включить чат");

        CheatToggles.bypassUrlBlock = GUILayout.Toggle(CheatToggles.bypassUrlBlock, " Обход блокировки URL");

        CheatToggles.lowerRateLimits = GUILayout.Toggle(CheatToggles.lowerRateLimits, " Снизить лимиты частоты");
    }

    private void DrawTextbox()
    {
        GUILayout.Label("Текстовое поле", GUIStylePreset.TabSubtitle);

        CheatToggles.unlockCharacters = GUILayout.Toggle(CheatToggles.unlockCharacters, " Разблокировать доп. символы");

        CheatToggles.longerMessages = GUILayout.Toggle(CheatToggles.longerMessages, " Разрешить длинные сообщения");

        CheatToggles.unlockClipboard = GUILayout.Toggle(CheatToggles.unlockClipboard, " Разблокировать буфер обмена");
    }
}
