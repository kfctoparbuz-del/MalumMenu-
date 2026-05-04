using UnityEngine;
using System;

namespace MalumMenu;

public class MovementTab : ITab
{
    public string name => "Движение";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawTeleport();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.noClip = GUILayout.Toggle(CheatToggles.noClip, " Проход сквозь стены");

        CheatToggles.invertControls = GUILayout.Toggle(CheatToggles.invertControls, " Инвертировать управление");

        try
        {
            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = GUILayout.HorizontalSlider(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, 0f, 20f, GUILayout.Width(250f));
                Utils.SnapSpeedToDefault(0.05f, true);
                GUILayout.Label($"Текущая скорость: {PlayerControl.LocalPlayer?.MyPhysics.GhostSpeed} {(Utils.IsSpeedDefault(true) ? "(По умолчанию)" : "")}");
            }
            else
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = GUILayout.HorizontalSlider(PlayerControl.LocalPlayer.MyPhysics.Speed, 0f, 20f, GUILayout.Width(250f));
                Utils.SnapSpeedToDefault(0.05f);
                GUILayout.Label($"Текущая скорость: {PlayerControl.LocalPlayer?.MyPhysics.Speed} {(Utils.IsSpeedDefault() ? "(По умолчанию)" : "")}");
            }
        } catch (NullReferenceException) {}
    }

    private void DrawTeleport()
    {
        GUILayout.Label("Телепортация", GUIStylePreset.TabSubtitle);

        CheatToggles.teleportCursor = GUILayout.Toggle(CheatToggles.teleportCursor, " к курсору");

        CheatToggles.teleportPlayer = GUILayout.Toggle(CheatToggles.teleportPlayer, " к игроку");
    }
}
