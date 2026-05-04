using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace MalumMenu;
public static class MalumCheats
{
    private static bool _isScanAnimActive;
    private static bool _isCamsAnimActive;

    public static void CloseMeetingCheat()
    {
        if (!CheatToggles.closeMeeting) return;

        if (Utils.isMeeting) // Закрывает окно MeetingHud, если оно открыто
        {

            // Уничтожение игрового объекта окна MeetingHud
            MeetingHud.Instance.DespawnOnDestroy = false;
            Object.Destroy(MeetingHud.Instance.gameObject);

            // Игровой процесс должен быть повторно включён
            DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f, false));
            PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
            ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
            DestroyableSingleton<HudManager>.Instance.SetMapButtonEnabled(true);
            DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
            ControllerManager.Instance.CloseAndResetAll();

        }
        else if (ExileController.Instance) // Завершает кат-сцену изгнания, если она проигрывается
        {
            ExileController.Instance.ReEnableGameplay();
            ExileController.Instance.WrapUp();
        }

        CheatToggles.closeMeeting = false;
    }

    public static void SkipMeetingCheat()
    {
        if (!CheatToggles.skipMeeting) return;

        if (Utils.isMeeting)
        {
            MeetingHud.Instance.RpcVotingComplete(new Il2CppStructArray<MeetingHud.VoterState>(0L), null, true);
        }

        CheatToggles.skipMeeting = false;
    }

    public static void CallMeetingCheat()
    {
        if (!CheatToggles.callMeeting) return;

        if (Utils.isHost)
        {
            // То же, что и PlayerControl.ReportDeadBody, но без дополнительных проверок
            MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
            PlayerControl.LocalPlayer.RpcStartMeeting(null);
        }
        else
        {
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        }

        CheatToggles.callMeeting = false;
    }

    public static void ForceStartGameCheat()
    {
        if (!CheatToggles.forceStartGame) return;

        if (Utils.isHost && Utils.isLobby)
        {
            AmongUsClient.Instance.SendStartGame();
        }

        CheatToggles.forceStartGame = false;
    }

    public static void NoKillCdCheat(PlayerControl playerControl)
    {
        if (CheatToggles.noKillCd && playerControl.killTimer > 0f)
        {
            playerControl.SetKillTimer(0f);
        }
    }

    public static void CompleteMyTasksCheat()
    {
        if (CheatToggles.completeMyTasks)
        {
            foreach (var task in PlayerControl.LocalPlayer.myTasks)
            {
                Utils.CompleteTask(task);
            }

            CheatToggles.completeMyTasks = false;
        }
    }

    public static void OpenSabotageMapCheat()
    {
        if (!CheatToggles.sabotageMap) return;

        DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
        {
            Mode = MapOptions.Modes.Sabotage
        });

        CheatToggles.sabotageMap = false;
    }

    public static void HandleEngineerCheats(EngineerRole engineerRole)
    {
        if (CheatToggles.endlessVentTime)
        {
            // Делает время в вентиляции невероятно долгим (float.MaxValue), чтобы оно никогда не заканчивалось
            engineerRole.inVentTimeRemaining = float.MaxValue;
        }
        else if (engineerRole.inVentTimeRemaining > engineerRole.GetCooldown())
        {
            // Время в вентиляции сбрасывается до нормального значения после отключения чита
            engineerRole.inVentTimeRemaining = engineerRole.GetCooldown();
        }

        if (CheatToggles.noVentCooldown)
        {
            if (engineerRole.cooldownSecondsRemaining > 0f)
            {
                engineerRole.cooldownSecondsRemaining = 0f;

                DestroyableSingleton<HudManager>.Instance.AbilityButton.ResetCoolDown();
                DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCooldownFill(0f);
            }
        }
    }

    public static void HandleShapeshifterCheats(ShapeshifterRole shapeshifterRole)
    {
        if (CheatToggles.endlessSsDuration)
        {
            // Делает длительность превращения невероятно долгой (float.MaxValue), чтобы она никогда не заканчивалась
            shapeshifterRole.durationSecondsRemaining = float.MaxValue;
        }
        else if (shapeshifterRole.durationSecondsRemaining > GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.ShapeshifterDuration))
        {
            // Длительность превращения сбрасывается до нормального значения после отключения чита
            shapeshifterRole.durationSecondsRemaining = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.ShapeshifterDuration);

        }
    }

    public static void HandleScientistCheats(ScientistRole scientistRole)
    {
        if (CheatToggles.noVitalsCooldown)
        {
            scientistRole.currentCooldown = 0f;
        }

        if (CheatToggles.endlessBattery)
        {
            // Делает заряд виталов невероятно долгим (float.MaxValue), чтобы он никогда не заканчивался
            scientistRole.currentCharge = float.MaxValue;
        }
        else if (scientistRole.currentCharge > scientistRole.RoleCooldownValue)
        {
            // Заряд батареи сбрасывается до нормального значения после отключения чита
            scientistRole.currentCharge = scientistRole.RoleCooldownValue;
        }
    }

    public static void HandleTrackerCheats(TrackerRole trackerRole)
    {
        if (CheatToggles.noTrackingCooldown)
        {
            trackerRole.cooldownSecondsRemaining = 0f;
            trackerRole.delaySecondsRemaining = 0f;

            DestroyableSingleton<HudManager>.Instance.AbilityButton.ResetCoolDown();
            DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCooldownFill(0f);
        }

        if (CheatToggles.noTrackingDelay && MapBehaviour.Instance != null)
        {
            MapBehaviour.Instance.trackedPointDelayTime = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDelay);
        }

        if (CheatToggles.endlessTracking)
        {
            // Делает заряд виталов невероятно долгим (float.MaxValue), чтобы он никогда не заканчивался
            trackerRole.durationSecondsRemaining = float.MaxValue;
        }
        else if (trackerRole.durationSecondsRemaining > GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDuration))
        {
            // Заряд батареи сбрасывается до нормального значения после отключения чита
            trackerRole.durationSecondsRemaining = GameManager.Instance.LogicOptions.GetRoleFloat(FloatOptionNames.TrackerDuration);
        }
    }

    public static void UseVentCheat(HudManager hudManager)
    {
        // try-catch для предотвращения ошибок, когда роль равна null
        try
        {

			// Инженерам и самозванцам не нужен этот чит, поэтому он для них отключён
			// Нахождение в вентиляции призраком вызывает проблемы, поэтому оно также отключено

			if (!PlayerControl.LocalPlayer.Data.Role.CanVent && !PlayerControl.LocalPlayer.Data.IsDead)
            {
				hudManager.ImpostorVentButton.gameObject.SetActive(CheatToggles.unlockVents);
			}

        } catch { }
    }

    public static void WalkInVentCheat()
    {
        try
        {
            if (!CheatToggles.walkInVents) return;

            PlayerControl.LocalPlayer.inVent = false;
            PlayerControl.LocalPlayer.moveable = true;

        } catch { }
    }

    public static void KickVentsCheat()
    {
        if (!CheatToggles.kickVents) return;

        foreach(var vent in ShipStatus.Instance.AllVents)
        {
            VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, vent.Id);
        }

        CheatToggles.kickVents = false;
    }

    public static void KillAllCheat()
    {
        if (!CheatToggles.killAll) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Убийство в лобби отключено из-за слишком большого количества багов");
        }
        else
        {
            // Убить всех игроков путём отправки успешного RPC-вызова MurderPlayer
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
            }
        }

        CheatToggles.killAll = false;
    }

    public static void KillAllCrewCheat()
    {
        if (!CheatToggles.killAllCrew) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Убийство в лобби отключено из-за слишком большого количества багов");
        }
        else
        {
            // Убить всех игроков путём отправки успешного RPC-вызова MurderPlayer
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.TeamType == RoleTeamTypes.Crewmate)
                {
                    Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
                }
            }
        }

        CheatToggles.killAllCrew = false;
    }

    public static void KillAllImpsCheat()
    {
        if (!CheatToggles.killAllImps) return;

        if (Utils.isLobby)
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Убийство в лобби отключено из-за слишком большого количества багов");
        }
        else
        {
            // Убить всех игроков путём отправки успешного RPC-вызова MurderPlayer
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.TeamType == RoleTeamTypes.Impostor)
                {
                    Utils.MurderPlayer(player, MurderResultFlags.Succeeded);
                }
            }
        }

        CheatToggles.killAllImps = false;
    }

    public static void ProtectCheat()
    {
        if (!Utils.isHost || Utils.isLobby) return;

        foreach (var player in ProtectUI.playersToProtect)
        {
            if (player.protectedByGuardianId == -1) // -1 означает, что защита в данный момент не активна
            {
                //PlayerControl.LocalPlayer.TurnOnProtection(true, PlayerControl.LocalPlayer.cosmetics.ColorId, PlayerControl.LocalPlayer.PlayerId);
                PlayerControl.LocalPlayer.RpcProtectPlayer(player, PlayerControl.LocalPlayer.cosmetics.ColorId);
            }
        }
    }

    public static void TeleportCursorCheat()
    {
        if (PlayerControl.LocalPlayer?.NetTransform == null || Camera.main == null) return;
        if (!CheatToggles.teleportCursor) return;

        // Телепортировать игрока на позицию курсора в мире при правом клике
        if (Input.GetMouseButtonDown(1))
        {
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    public static void NoClipCheat()
    {
        try
        {

            PlayerControl.LocalPlayer.Collider.enabled = !(CheatToggles.noClip || PlayerControl.LocalPlayer.onLadder);

        } catch { }
    }

    public static void PlayScannerCheat()
    {
        if (CheatToggles.animMedScan && !_isScanAnimActive)
        {
            Utils.ForceSetScanner(PlayerControl.LocalPlayer, true);
            _isScanAnimActive = true;
        }
        else if (!CheatToggles.animMedScan && _isScanAnimActive)
        {
            Utils.ForceSetScanner(PlayerControl.LocalPlayer, false);
            _isScanAnimActive = false;
        }
    }

    public static void PlayAnimationCheat()
    {
        if (CheatToggles.animPet && Utils.isPlayer && PlayerControl.LocalPlayer.cosmetics != null && PlayerControl.LocalPlayer.cosmetics.CurrentPet != null)
        {
            // Не перемещать LocalPlayer, просто отправить RPC, чтобы другие видели анимацию глажки питомца
            RpcPetMessage rpcMessage = new(PlayerControl.LocalPlayer.MyPhysics.NetId,
                PlayerControl.LocalPlayer.cosmetics.CurrentPet.PettingPlayerPosition,
                PlayerControl.LocalPlayer.cosmetics.CurrentPet.transform.position);
            AmongUsClient.Instance.LateBroadcastReliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
        }

        byte mapId = Utils.GetCurrentMapID();

        if (mapId == byte.MaxValue) return;

        var map = (MapNames)mapId;

        if (CheatToggles.animShields)
        {
            if (map is MapNames.Skeld or MapNames.Dleks)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.PrimeShields);
            }
            CheatToggles.animShields = false;
        }

        if (CheatToggles.animAsteroids)
        {
            if (map is MapNames.Skeld or MapNames.Dleks or MapNames.Polus)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.ClearAsteroids);
            }
            else
            {
                CheatToggles.animAsteroids = false;
            }
        }

        if (CheatToggles.animEmptyGarbage)
        {
            if (map is MapNames.Skeld or MapNames.Dleks)
            {
                Utils.ForcePlayAnimation((byte)TaskTypes.EmptyGarbage);
            }

            CheatToggles.animEmptyGarbage = false;
        }

        if (map is not (MapNames.MiraHQ or MapNames.Fungle))
        {
            if (CheatToggles.animCamsInUse && !_isCamsAnimActive)
            {
                // ShipStatus.Instance.UpdateSystem(SystemTypes.Security, PlayerControl.LocalPlayer, (byte)(CheatToggles.animCamsInUse ? 1 : 0));
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 1);
                _isCamsAnimActive = true;
            }
            else if (!CheatToggles.animCamsInUse && _isCamsAnimActive)
            {
                // Выключить камеры, если чит использовался ранее и теперь отключён
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 0);
                _isCamsAnimActive = false;
            }
        }
        else
        {
            CheatToggles.animCamsInUse = false;
        }
    }

    public static void StopShipAnimCheats()
    {
        CheatToggles.animShields = false;
        CheatToggles.animAsteroids = false;
        CheatToggles.animEmptyGarbage = false;
        CheatToggles.animMedScan = false;
        CheatToggles.animCamsInUse = false;

        // Это гарантирует, что анимации камер и сканирования не останутся отмеченными как активные, если игрок
        // отключится, пока переключатели включены (так как это может вызвать необычные RPC в лобби)

        _isCamsAnimActive = false;
        _isScanAnimActive = false;
    }
}
