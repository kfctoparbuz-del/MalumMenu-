using AmongUs.Data;

namespace MalumMenu;
public static class MalumSpoof
{
    public static void SpoofLevel()
    {
        // Парсинг записи конфигурации Spoofing.Level и преобразование её в uint
        if (!string.IsNullOrEmpty(MalumMenu.spoofLevel.Value) &&
            uint.TryParse(MalumMenu.spoofLevel.Value, out uint parsedLevel) &&
            parsedLevel != DataManager.Player.Stats.Level)
        {

            // Сохранение подменённого уровня с помощью DataManager
            DataManager.Player.stats.level = parsedLevel - 1;
            DataManager.Player.Save();
        }
    }

    public static string SpoofFriendCode()
    {
        string friendCode = MalumMenu.guestFriendCode.Value;
        if (string.IsNullOrWhiteSpace(friendCode))
        {
            friendCode = DestroyableSingleton<AccountManager>.Instance.GetRandomName();
        }
        return friendCode;
    }
}
