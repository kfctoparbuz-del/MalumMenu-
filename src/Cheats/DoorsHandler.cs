using System.Collections.Generic;
using System.Linq;

namespace MalumMenu;

public static class DoorsHandler
{
    // Возвращает список всех комнат, в которых есть двери
    public static List<SystemTypes> GetRoomsWithDoors()
    {
        if (!Utils.isShip || ShipStatus.Instance.AllDoors.Count <= 0) return new List<SystemTypes>();

        return ShipStatus.Instance.AllDoors.Select(d => d.Room).Distinct().ToList();
    }

    // Возвращает список всех дверей в указанной комнате
    public static List<OpenableDoor> GetDoorsInRoom(SystemTypes room)
    {
        if (!Utils.isShip || ShipStatus.Instance.AllDoors.Count <= 0) return new List<OpenableDoor>();

        return ShipStatus.Instance.AllDoors.Where(d => d.Room == room).ToList();
    }

    // Возвращает агрегированный статус дверей в указанной комнате
    public static string GetStatusOfDoorsInRoom(SystemTypes room, bool colorize)
    {
        var doorsInRoom = GetDoorsInRoom(room);
        if (doorsInRoom.Count <= 0) return "Н/Д";
        if (doorsInRoom.All(d => d.IsOpen)) return colorize ? "<color=#00FF00>Открыты</color>" : "Открыты";
        if (doorsInRoom.All(d => !d.IsOpen)) return colorize ? "<color=#FF0000>Закрыты</color>" : "Закрыты";
        return colorize ? "<color=#FFFF00>Смешанные</color>" : "Смешанные";
    }

    // Открывает все двери в указанной комнате
    public static void OpenDoorsInRoom(SystemTypes doorRoom)
    {
        foreach (var door in GetDoorsInRoom(doorRoom))
        {
            OpenDoor(door);
        }
    }

    // Закрывает все двери в указанной комнате
    public static void CloseDoorsInRoom(SystemTypes doorRoom)
    {
        try { ShipStatus.Instance.RpcCloseDoorsOfType(doorRoom); } catch { }
    }

    // Открывает все двери на карте
    public static void OpenAllDoors()
    {
        foreach (var door in ShipStatus.Instance.AllDoors)
        {
            OpenDoor(door);
        }
    }

    // Закрывает все двери на карте
    public static void CloseAllDoors()
    {
        foreach (var door in ShipStatus.Instance.AllDoors)
        {
            try { ShipStatus.Instance.RpcCloseDoorsOfType(door.Room); } catch { }
        }
    }

    // Открывает конкретную дверь
    public static void OpenDoor(OpenableDoor openableDoor)
    {
        try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(openableDoor.Id | 64)); } catch { }
    }
}
