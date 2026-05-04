using UnityEngine;

namespace MalumMenu;

public class ModesTab : ITab
{
    public string name => "Режимы";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.rgbMode = GUILayout.Toggle(CheatToggles.rgbMode, " RGB-режим");

        CheatToggles.stealthMode = GUILayout.Toggle(CheatToggles.stealthMode, " Скрытный режим");

        CheatToggles.panicMode = GUILayout.Toggle(CheatToggles.panicMode, " Панический режим");
    }
}
