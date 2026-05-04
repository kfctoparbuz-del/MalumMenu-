using UnityEngine;

namespace MalumMenu;

public class AnimationsTab : ITab
{
    public string name => "Анимации";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawClientSided();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.animShields = GUILayout.Toggle(CheatToggles.animShields, " Щиты");

        CheatToggles.animAsteroids = GUILayout.Toggle(CheatToggles.animAsteroids, " Астероиды");

        CheatToggles.animEmptyGarbage = GUILayout.Toggle(CheatToggles.animEmptyGarbage, " Выброс мусора");

        CheatToggles.animMedScan = GUILayout.Toggle(CheatToggles.animMedScan, " Сканирование в медотсеке");

        CheatToggles.animCamsInUse = GUILayout.Toggle(CheatToggles.animCamsInUse, " Использование камер");

        // CheatToggles.animPet = GUILayout.Toggle(CheatToggles.animPet, " Питомец");
    }

    private void DrawClientSided()
    {
        GUILayout.Label("Только для клиента", GUIStylePreset.TabSubtitle);

        CheatToggles.moonWalk = GUILayout.Toggle(CheatToggles.moonWalk, " Лунная походка");
    }
}
