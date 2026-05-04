using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
public static class VoteBanSystem_AddVote
{
    // Префикс-патч VoteBanSystem.AddVote для мгновенного кика игрока, когда хост голосует за его кик
    public static bool Prefix(VoteBanSystem __instance, int srcClient, int clientId)
    {
        if (!Utils.isHost) return true;

        if (AmongUsClient.Instance.ClientId == srcClient)
        {
            AmongUsClient.Instance.KickPlayer(clientId, false);
        }

        return false;
    }
}

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.CmdAddVote))]
public static class VoteBanSystem_CmdAddVote
{
    // Префикс-патч VoteBanSystem.CmdAddVote для предотвращения отправки RPC AddVoteBan, когда хост голосует за кик игрока
    public static bool Prefix()
    {
        return !Utils.isHost;
    }
}
