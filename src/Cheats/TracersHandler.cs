using UnityEngine;

namespace MalumMenu;

public static class TracersHandler
{
    // Рисует трассер от LocalPlayer до другого игрока.
    public static void DrawPlayerTracer(PlayerPhysics playerPhysics)
    {
        try
        {
            var color = Color.clear; // Все трассеры по умолчанию невидимы

            if (!playerPhysics.myPlayer.Data.IsDead)
            {
                if (CheatToggles.tracersCrew && !playerPhysics.myPlayer.Data.Role.IsImpostor ||
                    CheatToggles.tracersImps && playerPhysics.myPlayer.Data.Role.IsImpostor)
                {
                    if (CheatToggles.distanceBasedTracers)
                    {
                        color = GetDistanceBasedColor(playerPhysics.myPlayer.transform.position);
                    }
                    else if (CheatToggles.colorBasedTracers)
                    {
                        color = playerPhysics.myPlayer.Data.Color; // Трассер по цвету
                    }
                    else
                    {
                        color = playerPhysics.myPlayer.Data.Role.TeamColor; // Трассер по команде
                    }
                }
            }
            else
            {
                if (CheatToggles.tracersGhosts)
                {
                    if (CheatToggles.distanceBasedTracers)
                    {
                        color = GetDistanceBasedColor(playerPhysics.myPlayer.transform.position);
                    }
                    else if (CheatToggles.colorBasedTracers)
                    {
                        color = playerPhysics.myPlayer.Data.Color; // Трассер по цвету
                    }
                    else
                    {
                        color = Palette.White; // Трассер призрака (белый)
                    }
                }
            }

            // Рисует трассер между игроком и LocalPlayer, используя соответствующий цвет
            Utils.DrawTracer(playerPhysics.myPlayer.gameObject, PlayerControl.LocalPlayer.gameObject, color);
        } catch { }
    }

    // Рисует трассер от LocalPlayer до трупа. Рисует трассеры только для незаявленных трупов.
    public static void DrawBodyTracer(DeadBody deadBody)
    {
        var color = Color.clear; // Все трассеры по умолчанию невидимы

        if (CheatToggles.tracersBodies)
        {
            if (CheatToggles.distanceBasedTracers)
            {
                color = GetDistanceBasedColor(deadBody.transform.position);
            }
            else if (CheatToggles.colorBasedTracers)
            {
                color = GameData.Instance.GetPlayerById(deadBody.ParentId).Color; // Трассер по цвету
            }
            else
            {
                color = Color.yellow; // Трассер трупа (жёлтый)
            }
        }

        // Рисует трассер между трупом и LocalPlayer, используя соответствующий цвет
        Utils.DrawTracer(deadBody.gameObject, PlayerControl.LocalPlayer.gameObject, color);
    }

    // Получает цвет на основе расстояния между LocalPlayer и целевой позицией.
    // Близкие расстояния — красный, средние — жёлтый, дальние — зелёный.
    private static Color GetDistanceBasedColor(Vector3 targetPosition)
    {
        const float maxDistance = 20f; // Зелёный на расстоянии 20+ единиц
        const float minDistance = 2f;  // Красный на расстоянии 2 единицы или меньше

        var distance = Vector3.Distance(targetPosition, PlayerControl.LocalPlayer.transform.position);
        var normalized = Mathf.InverseLerp(minDistance, maxDistance, distance);

        // Интерполяция: Красный (близко) -> Жёлтый (средне) -> Зелёный (далеко)
        return normalized < 0.5f
            ? Color.Lerp(Color.red, Color.yellow, normalized * 2f)
            : Color.Lerp(Color.yellow, Color.green, (normalized - 0.5f) * 2f);
    }
}
