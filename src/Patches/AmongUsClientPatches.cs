using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
public static class AmongUsClient_Update
{
    public static void Postfix()
    {
        MalumSpoof.SpoofLevel();

        // Читы GuestMode закомментированы, так как они сломаны в последних обновлениях

        // Код для обработки временных аккаунтов так же, как и полных, включая доступ к кодам друзей
        // if (!EOSManager.Instance.loginFlowFinished || !MalumMenu.guestMode.Value) return;
        // DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;

        // if (!string.IsNullOrWhiteSpace(EOSManager.Instance.FriendCode)) return;
        // var friendCode = MalumSpoof.spoofFriendCode();
        // var editUsername = EOSManager.Instance.editAccountUsername;
        // editUsername.UsernameText.SetText(friendCode);
        // editUsername.SaveUsername();
        // EOSManager.Instance.FriendCode = friendCode;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public static class AmongUsClient_OnGameJoined
{
    // Постфикс-патч AmongUsClient.OnGameJoined для сохранения строки ID последней присоединённой игры
    public static string lastGameIdString = "";

    public static void Postfix(string gameIdString)
    {
        lastGameIdString = gameIdString;
    }
}
