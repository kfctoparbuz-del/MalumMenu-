using UnityEngine;

namespace MalumMenu;

public class ShipTab : ITab
{
    public string name => "Корабль";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawSabotage();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawVents();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.unfixableLights = GUILayout.Toggle(CheatToggles.unfixableLights, " Неисправимый свет");

        // CheatToggles.reportBody = GUILayout.Toggle(CheatToggles.reportBody, " Сообщить о теле");

        CheatToggles.callMeeting = GUILayout.Toggle(CheatToggles.callMeeting, " Созвать собрание");

        CheatToggles.closeMeeting = GUILayout.Toggle(CheatToggles.closeMeeting, " Закрыть собрание");

        CheatToggles.autoOpenDoorsOnUse = GUILayout.Toggle(CheatToggles.autoOpenDoorsOnUse, " Авто-открытие дверей при использовании");
    }

    private void DrawSabotage()
    {
        GUILayout.Label("Саботаж", GUIStylePreset.TabSubtitle);

        CheatToggles.reactorSab = GUILayout.Toggle(CheatToggles.reactorSab, " Реактор");

        CheatToggles.oxygenSab = GUILayout.Toggle(CheatToggles.oxygenSab, " Кислород");

        CheatToggles.elecSab = GUILayout.Toggle(CheatToggles.elecSab, " Свет");

        CheatToggles.commsSab = GUILayout.Toggle(CheatToggles.commsSab, " Связь");

        CheatToggles.showDoorsMenu = GUILayout.Toggle(CheatToggles.showDoorsMenu, " Показать меню дверей");

        CheatToggles.mushSab = GUILayout.Toggle(CheatToggles.mushSab, " Грибная неразбериха");

        CheatToggles.mushSpore = GUILayout.Toggle(CheatToggles.mushSpore, " Активировать споры");

        CheatToggles.sabotageMap = GUILayout.Toggle(CheatToggles.sabotageMap, " Открыть карту саботажа");
    }

    private void DrawVents()
    {
        GUILayout.Label("Вентиляция", GUIStylePreset.TabSubtitle);

        CheatToggles.unlockVents = GUILayout.Toggle(CheatToggles.unlockVents, " Разблокировать вентиляцию");

        CheatToggles.kickVents = GUILayout.Toggle(CheatToggles.kickVents, " Выгнать всех из вентиляции");

        CheatToggles.walkInVents = GUILayout.Toggle(CheatToggles.walkInVents, " Ходить в вентиляции");
    }
}
