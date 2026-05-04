using UnityEngine;

namespace MalumMenu;

public class ConfigTab : ITab
{
    public string name => "Конфигурация";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.openConfig = GUILayout.Toggle(CheatToggles.openConfig, " Открыть конфиг");

        CheatToggles.reloadConfig = GUILayout.Toggle(CheatToggles.reloadConfig, " Сбросить конфиг");

        CheatToggles.saveProfile = GUILayout.Toggle(CheatToggles.saveProfile, " Сохранить в профиль");

        CheatToggles.loadProfile = GUILayout.Toggle(CheatToggles.loadProfile, " Загрузить из профиля");
    }
}
