using HarmonyLib;
using System.Collections.Generic;

namespace MalumMenu;

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
public static class MapBehaviour_ShowNormalMap
{
    // Постфикс-патч MapBehaviour.ShowNormalMap для создания иконок herePoint для каждого игрока
    public static void Postfix(MapBehaviour __instance)
    {
        MinimapHandler.minimapActive = MinimapHandler.IsCheatEnabled();

        if (!MinimapHandler.minimapActive)
        {
            return; // Выполняется только если чит мини-карты включён
        }

        __instance.ColorControl.SetColor(Palette.Purple); // Пользовательский цвет карты

        __instance.DisableTrackerOverlays();

        // Уничтожить старые иконки игроков (herePoints)
        try
        {
            MinimapHandler.herePoints.ForEach(x => UnityEngine.Object.Destroy(x.sprite.gameObject));
            MinimapHandler.herePoints.Clear();
        }
        catch { }

        // и создать новые для каждого игрока
        var temp = new List<HerePoint>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.AmOwner) // Локальный игрок всегда обрабатывается нормально
            {
                var herePoint = UnityEngine.Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent);

                temp.Add(new HerePoint(player, herePoint));
            }
        }
        MinimapHandler.herePoints = temp;

    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
public static class MapBehaviour_FixedUpdate
{
    // Постфикс-патч MapBehaviour.FixedUpdate для обновления цвета и позиции каждой иконки herePoint на карте в соответствии с соответствующим игроком
    public static void Postfix(MapBehaviour __instance)
    {
        // Сброс карты, если чит мини-карты отключён
        if (MinimapHandler.IsCheatEnabled() != MinimapHandler.minimapActive)
        {
            if (!__instance.infectedOverlay.gameObject.active) // Не затрагивать карту саботажа
            {
                __instance.Close();
                __instance.ShowNormalMap();
            }
        }

        // Правильная обработка каждой иконки herePoint на карте
        var temp = MinimapHandler.herePoints;
        foreach (var herePoint in temp)
        {
            MinimapHandler.HandleHerePoint(herePoint);
        }

        foreach (var herePoint in MinimapHandler.herePointsToRemove)
        {
            MinimapHandler.herePoints.Remove(herePoint);
        }

    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
public static class MapBehaviour_Close
{
    // Постфикс-патч MapBehaviour.Close для очистки всех иконок herePoint
    public static void Postfix(MapBehaviour __instance)
    {
        try
        {
            MinimapHandler.herePoints.ForEach(x => UnityEngine.Object.Destroy(x.sprite.gameObject));
            MinimapHandler.herePoints.Clear();
        }
        catch { }
    }
}
