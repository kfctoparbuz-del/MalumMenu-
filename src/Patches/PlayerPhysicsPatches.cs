using System;
using HarmonyLib;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
public static class PlayerPhysics_LateUpdate
{
    public static void Postfix(PlayerPhysics __instance)
    {
        MalumESP.PlayerNametags(__instance);
        MalumESP.SeeGhostsCheat(__instance);

        MalumCheats.NoClipCheat();
        MalumCheats.ProtectCheat();
        MalumCheats.KillAllCheat();
        MalumCheats.KillAllCrewCheat();
        MalumCheats.KillAllImpsCheat();
        MalumCheats.ForceStartGameCheat();
        MalumCheats.TeleportCursorCheat();
        MalumCheats.CompleteMyTasksCheat();
        MalumCheats.PlayAnimationCheat();
        MalumCheats.PlayScannerCheat();

        MalumPPMCheats.EjectPlayerPPM();
        MalumPPMCheats.SpectatePPM();
        MalumPPMCheats.KillPlayerPPM();
        MalumPPMCheats.TelekillPlayerPPM();
        MalumPPMCheats.TeleportPlayerPPM();
        MalumPPMCheats.SetFakeRolePPM();
        MalumPPMCheats.SetFakeAlivePPM();
        // MalumPPMCheats.ForceRolePPM();

        // Эта проверка гарантирует, что выполняется только один запуск за кадр,
        // чтобы прогресс OverloadHandler._timer оставался точным
        if (__instance.AmOwner)
        {
            OverloadHandler.Run();
        }

        TracersHandler.DrawPlayerTracer(__instance);

        GameObject[] bodyObjects = GameObject.FindGameObjectsWithTag("DeadBody");
        foreach(GameObject bodyObject in bodyObjects) // Находит и перебирает все трупы
        {
            DeadBody deadBody = bodyObject.GetComponent<DeadBody>();

            if (!deadBody || deadBody.Reported) continue;  // Рисовать трассеры только для незаявленных трупов
            TracersHandler.DrawBodyTracer(deadBody);
        }

        try
        {
            if (CheatToggles.invertControls)
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = -Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
            else
            {
                PlayerControl.LocalPlayer.MyPhysics.Speed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.Speed);
                PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = Mathf.Abs(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed);
            }
        } catch (NullReferenceException) { }
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
public static class PlayerPhysics_HandleAnimation
{
    // Префикс-патч PlayerPhysics.HandleAnimation для отключения анимации ходьбы
    public static bool Prefix(PlayerPhysics __instance)
    {
        if (CheatToggles.moonWalk && __instance.AmOwner)
        {
            __instance.ResetAnimState();

            return false;
        }

        return true;
    }
}
