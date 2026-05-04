using UnityEngine;

namespace MalumMenu;

public class RolesTab : ITab
{
    public string name => "Роли";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawImpostor();

        GUILayout.Space(15);

        DrawShapeshifter();

        GUILayout.Space(15);

        DrawCrewmate();

        GUILayout.Space(15);

        DrawTracker();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawEngineer();

        GUILayout.Space(15);

        DrawScientist();

        GUILayout.Space(15);

        DrawDetective();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.setFakeRole = GUILayout.Toggle(CheatToggles.setFakeRole, " Фейк роль");

        CheatToggles.setFakeAlive = GUILayout.Toggle(CheatToggles.setFakeAlive, " Фейково ожить");
    }

    private void DrawImpostor()
    {
        GUILayout.Label("Самозванец", GUIStylePreset.TabSubtitle);

        CheatToggles.killReach = GUILayout.Toggle(CheatToggles.killReach, " Дальность убийства");

        // CheatToggles.impostorTasks = GUILayout.Toggle(CheatToggles.impostorTasks, " Разрешить задания");
    }

    private void DrawShapeshifter()
    {
        GUILayout.Label("Оборотень", GUIStylePreset.TabSubtitle);

        CheatToggles.noShapeshiftAnim = GUILayout.Toggle(CheatToggles.noShapeshiftAnim, " Без анимации превращения");

        CheatToggles.endlessSsDuration = GUILayout.Toggle(CheatToggles.endlessSsDuration, " Быть морфом вечно");
    }

    private void DrawCrewmate()
    {
        GUILayout.Label("Член экипажа", GUIStylePreset.TabSubtitle);

        CheatToggles.showTasksMenu = GUILayout.Toggle(CheatToggles.showTasksMenu, " Показать меню заданий");
    }

    private void DrawTracker()
    {
        GUILayout.Label("Следопыт", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessTracking = GUILayout.Toggle(CheatToggles.endlessTracking, " Бесконечное отслеживание");

        CheatToggles.noTrackingDelay = GUILayout.Toggle(CheatToggles.noTrackingDelay, " Без задержки отслеживания");

        CheatToggles.noTrackingCooldown = GUILayout.Toggle(CheatToggles.noTrackingCooldown, " Без перезарядки отслеживания");

        CheatToggles.trackReach = GUILayout.Toggle(CheatToggles.trackReach, " Дальность отслеживания");
    }

    private void DrawEngineer()
    {
        GUILayout.Label("Инженер", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessVentTime = GUILayout.Toggle(CheatToggles.endlessVentTime, " Бесконечное время в люке");

        CheatToggles.noVentCooldown = GUILayout.Toggle(CheatToggles.noVentCooldown, " Без кд люка");
    }

    private void DrawScientist()
    {
        GUILayout.Label("Учёный", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessBattery = GUILayout.Toggle(CheatToggles.endlessBattery, " Бесконечный аккумулятор");

        CheatToggles.noVitalsCooldown = GUILayout.Toggle(CheatToggles.noVitalsCooldown, " Без перезарядки виталов");
    }

    private void DrawDetective()
    {
        GUILayout.Label("Детектив", GUIStylePreset.TabSubtitle);

        CheatToggles.interrogateReach = GUILayout.Toggle(CheatToggles.interrogateReach, " Дальность допроса");
    }
}
