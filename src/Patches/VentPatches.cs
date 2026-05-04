using HarmonyLib;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class Vent_CanUse
{
    // Постфикс-патч Vent.CanUse для разрешения использования вентиляции, когда включён чит unlockVents
    public static void Postfix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data) return;
        if (PlayerControl.LocalPlayer.Data.Role.CanVent || PlayerControl.LocalPlayer.Data.IsDead) return;
        if (!CheatToggles.unlockVents) return;

        var @object = pc.Object;

        var center = @object.Collider.bounds.center;
        var position = __instance.transform.position;
        var num = Vector2.Distance(center, position);

        // Разрешить использование вентиляции, если вентиляция не слишком далеко и нет объектов, блокирующих путь игрока
        canUse = num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false);
        couldUse = true;
        __result = num;
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
public static class Vent_EnterVent
{
    // Постфикс-патч Vent.EnterVent для логирования в ConsoleUI, когда игрок заходит в вентиляцию,
    // а также комнаты, в которой он это сделал
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        if (!CheatToggles.logVents || !Utils.isShip) return;

        var (realPlayerName, displayPlayerName, isDisguised) = Utils.GetPlayerIdentity(pc);
        var room = Utils.GetRoomFromPosition(__instance.transform.position); //- (Vector3) pc.Collider.offset);
        var roomName = room != null ? room.RoomId.ToString() : "неизвестном месте";

        ConsoleUI.Log(isDisguised
            ? $"{realPlayerName} (как {displayPlayerName}) зашёл(а) в вентиляцию в {roomName}"
            : $"{realPlayerName} зашёл(а) в вентиляцию в {roomName}");
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
public static class Vent_ExitVent
{
    // Постфикс-патч Vent.ExitVent для логирования в ConsoleUI, когда игрок выходит из вентиляции,
    // а также комнаты, в которой он это сделал
    public static void Postfix(Vent __instance, PlayerControl pc)
    {
        if (!CheatToggles.logVents || !Utils.isShip) return;

        var (realPlayerName, displayPlayerName, isDisguised) = Utils.GetPlayerIdentity(pc);

        var room = Utils.GetRoomFromPosition(__instance.transform.position); //- (Vector3) pc.Collider.offset);
        var roomName = room != null ? room.RoomId.ToString() : "неизвестном месте";

        ConsoleUI.Log(isDisguised
            ? $"{realPlayerName} (как {displayPlayerName}) вышел(а) из вентиляции в {roomName}"
            : $"{realPlayerName} вышел(а) из вентиляции в {roomName}");
    }
}
