using UnityEngine;

namespace MalumMenu;

public class HostOnlyTab : ITab
{
    public string name => "Только для хоста";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawMurder();

        GUILayout.Space(15);

        DrawGameState();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawMeetings();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.killVanished = GUILayout.Toggle(CheatToggles.killVanished, " Убивать в невидимости");

        CheatToggles.killAnyone = GUILayout.Toggle(CheatToggles.killAnyone, " Убивать кого угодно");

        CheatToggles.noKillCd = GUILayout.Toggle(CheatToggles.noKillCd, " Без перезарядки убийства");

        CheatToggles.showProtectMenu = GUILayout.Toggle(CheatToggles.showProtectMenu, " Показать меню защиты");

        // CheatToggles.forceRole = GUILayout.Toggle(CheatToggles.forceRole, " Принудительная роль");

        // CheatToggles.noOptionsLimits = GUILayout.Toggle(CheatToggles.noOptionsLimits, " Без ограничений опций");
    }

    private void DrawMurder()
    {
        GUILayout.Label("Убийство", GUIStylePreset.TabSubtitle);

        CheatToggles.killPlayer = GUILayout.Toggle(CheatToggles.killPlayer, " Убить игрока");

        CheatToggles.telekillPlayer = GUILayout.Toggle(CheatToggles.telekillPlayer, " Телекилл игрока");

        CheatToggles.killAllCrew = GUILayout.Toggle(CheatToggles.killAllCrew, " Убить всех членов экипажа");

        CheatToggles.killAllImps = GUILayout.Toggle(CheatToggles.killAllImps, " Убить всех самозванцев");

        CheatToggles.killAll = GUILayout.Toggle(CheatToggles.killAll, " Убить всех");
    }

    private void DrawGameState()
    {
        GUILayout.Label("Состояние игры", GUIStylePreset.TabSubtitle);

        CheatToggles.forceStartGame = GUILayout.Toggle(CheatToggles.forceStartGame, " Принудительный запуск игры");

        CheatToggles.noGameEnd = GUILayout.Toggle(CheatToggles.noGameEnd, " Без завершения игры");
    }

    private void DrawMeetings()
    {
        GUILayout.Label("Собрания", GUIStylePreset.TabSubtitle);

        CheatToggles.skipMeeting = GUILayout.Toggle(CheatToggles.skipMeeting, " Пропустить собрание");

        CheatToggles.voteImmune = GUILayout.Toggle(CheatToggles.voteImmune, " Неуязвимость к голосованию");

        CheatToggles.ejectPlayer = GUILayout.Toggle(CheatToggles.ejectPlayer, " Выгнать игрока");
    }
}
