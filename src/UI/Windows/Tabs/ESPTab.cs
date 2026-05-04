using UnityEngine;

namespace MalumMenu;

public class ESPTab : ITab
{
    public string name => "ESP";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawCamera();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawTracers();

        GUILayout.Space(15);

        DrawMinimap();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.seePlayerInfo = GUILayout.Toggle(CheatToggles.seePlayerInfo, " Видеть информацию об игроке");

        CheatToggles.seeRoles = GUILayout.Toggle(CheatToggles.seeRoles, " Видеть роли");

        CheatToggles.seeGhosts = GUILayout.Toggle(CheatToggles.seeGhosts, " Видеть призраков");

        CheatToggles.noShadows = GUILayout.Toggle(CheatToggles.noShadows, " Без теней");

        CheatToggles.taskArrows = GUILayout.Toggle(CheatToggles.taskArrows, " Стрелки к задачам");

        CheatToggles.revealVotes = GUILayout.Toggle(CheatToggles.revealVotes, " Раскрывать голоса");

        CheatToggles.seeLobbyInfo = GUILayout.Toggle(CheatToggles.seeLobbyInfo, " Видеть информацию лобби");
    }

    private void DrawCamera()
    {
        GUILayout.Label("Камера", GUIStylePreset.TabSubtitle);

        CheatToggles.zoomOut = GUILayout.Toggle(CheatToggles.zoomOut, " Отдалить");

        CheatToggles.spectate = GUILayout.Toggle(CheatToggles.spectate, " Наблюдение");

        CheatToggles.freecam = GUILayout.Toggle(CheatToggles.freecam, " Свободная камера");
    }

    private void DrawTracers()
    {
        GUILayout.Label("Трассеры", GUIStylePreset.TabSubtitle);

        CheatToggles.tracersCrew = GUILayout.Toggle(CheatToggles.tracersCrew, " Члены экипажа");

        CheatToggles.tracersImps = GUILayout.Toggle(CheatToggles.tracersImps, " Самозванцы");

        CheatToggles.tracersGhosts = GUILayout.Toggle(CheatToggles.tracersGhosts, " Призраки");

        CheatToggles.tracersBodies = GUILayout.Toggle(CheatToggles.tracersBodies, " Трупы");

        CheatToggles.colorBasedTracers = GUILayout.Toggle(CheatToggles.colorBasedTracers, " По цвету");

        CheatToggles.distanceBasedTracers = GUILayout.Toggle(CheatToggles.distanceBasedTracers, " По расстоянию");
    }

    private void DrawMinimap()
    {
        GUILayout.Label("Мини-карта", GUIStylePreset.TabSubtitle);

        CheatToggles.mapCrew = GUILayout.Toggle(CheatToggles.mapCrew, " Члены экипажа");

        CheatToggles.mapImps = GUILayout.Toggle(CheatToggles.mapImps, " Самозванцы");

        CheatToggles.mapGhosts = GUILayout.Toggle(CheatToggles.mapGhosts, " Призраки");

        CheatToggles.colorBasedMap = GUILayout.Toggle(CheatToggles.colorBasedMap, " По цвету");
    }
}
