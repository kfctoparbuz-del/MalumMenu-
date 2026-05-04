namespace MalumMenu;

public static class MalumSabotageCheats
{
    private static bool _reactorSab;
    private static bool _oxygenSab;
    private static bool _commsSab;
    private static bool _elecSab;
    private static bool _unfixableLights;

    public static void HandleReactor(ShipStatus shipStatus, byte mapId)
    {
        switch (mapId)
        {
            case 2:
            {
                // Polus использует SystemTypes.Laboratory вместо SystemTypes.Reactor

                var labSys = shipStatus.Systems[SystemTypes.Laboratory].Cast<ReactorSystemType>();

                if (CheatToggles.reactorSab != _reactorSab)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Laboratory, _reactorSab ? (byte)16 : (byte)128);
                    _reactorSab = CheatToggles.reactorSab;
                }

                CheatToggles.reactorSab = _reactorSab = labSys.IsActive;
                break;
            }
            case 4:
            {
                // Airship использует HeliSabotageSystem для саботажа реактора

                var heliSys = shipStatus.Systems[SystemTypes.HeliSabotage].Cast<HeliSabotageSystem>();

                if (CheatToggles.reactorSab != _reactorSab)
                {
                    if (_reactorSab)
                    {
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 0); // Починить
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 1);
                    }
                    else
                    {
                        shipStatus.RpcUpdateSystem(SystemTypes.HeliSabotage, 128); // Саботировать
                    }

                    _reactorSab = CheatToggles.reactorSab;
                }

                CheatToggles.reactorSab = _reactorSab = heliSys.IsActive;
                break;
            }
            default:
            {
                // Другие карты ведут себя нормально

                var reactorSys = shipStatus.Systems[SystemTypes.Reactor].Cast<ReactorSystemType>();

                if (CheatToggles.reactorSab != _reactorSab)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Reactor, _reactorSab ? (byte)16 : (byte)128);
                    _reactorSab = CheatToggles.reactorSab;
                }

                CheatToggles.reactorSab = _reactorSab = reactorSys.IsActive;
                break;
            }
        }
    }

    public static void HandleOxygen(ShipStatus shipStatus, byte mapId)
    {
        if (mapId != 4 && mapId != 2 && mapId != 5) { // Карты без системы кислорода: Airship, MiraHQ, Fungle

            var oxygenSys = shipStatus.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();

            if (CheatToggles.oxygenSab != _oxygenSab)
            {
                shipStatus.RpcUpdateSystem(SystemTypes.LifeSupp, _oxygenSab ? (byte)16 : (byte)128);
                _oxygenSab = CheatToggles.oxygenSab;
            }

            CheatToggles.oxygenSab = _oxygenSab = oxygenSys.IsActive;

            return;

        }

        // Уведомить игрока, если он пытается активировать чит на карте без системы кислорода
        if (!CheatToggles.oxygenSab) return;
        HudManager.Instance.Notifier.AddDisconnectMessage("Система кислорода отсутствует на этой карте");
        CheatToggles.oxygenSab = false;
    }

    public static void HandleComms(ShipStatus shipStatus, byte mapId)
    {
        if (mapId is 1 or 5) // Fungle и Skeld используют HqHudSystemType вместо HudOverrideSystemType
        {

            var hqCommsSys = shipStatus.Systems[SystemTypes.Comms].Cast<HqHudSystemType>();

            if (CheatToggles.commsSab != _commsSab)
            {

                if (_commsSab)
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 0); // Починить
                    shipStatus.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                }
                else
                {
                    shipStatus.RpcUpdateSystem(SystemTypes.Comms, 128); // Саботировать
                }

                _commsSab = CheatToggles.commsSab;

            }

            CheatToggles.commsSab = _commsSab = hqCommsSys.IsActive;

        }
        else // Другие карты ведут себя нормально
        {

            var commsSys = shipStatus.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();

            if (CheatToggles.commsSab != _commsSab)
            {
                shipStatus.RpcUpdateSystem(SystemTypes.Comms, _commsSab ? (byte)16 : (byte)128);
                _commsSab = CheatToggles.commsSab;
            }

            CheatToggles.commsSab = _commsSab = commsSys.IsActive;

        }
    }

    public static void HandleElectrical(ShipStatus shipStatus, byte mapId)
    {
        if (mapId != 5) // На Fungle нет электрической системы
        {

            var elecSys = shipStatus.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();

            // Сначала обработать чит unfixableLights, чтобы читы не мешали друг другу
            HandleUnfixLights(shipStatus);

            if (CheatToggles.elecSab != _elecSab)
            {
                if (_elecSab)   // Починить
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var switchMask = 1 << (i & 0x1F);

                        if ((elecSys.ActualSwitches & switchMask) != (elecSys.ExpectedSwitches & switchMask))
                        {
                            shipStatus.RpcUpdateSystem(SystemTypes.Electrical, (byte)i);
                        }
                    }

                }
                else // Саботировать
                {
                    CheatToggles.unfixableLights = false; // Заменить чит unfixableLights, если он уже активен

                    byte b = 4;
                    for (var i = 0; i < 5; i++)
                    {
                        if (BoolRange.Next(0.5f))
                        {
                            b |= (byte)(1 << i);
                        }
                    }

                    shipStatus.RpcUpdateSystem(SystemTypes.Electrical, (byte)(b | 128));
                }

                _elecSab = CheatToggles.elecSab;
            }

            CheatToggles.elecSab = _elecSab = elecSys.IsActive && !_unfixableLights;

            return;

        }

        // Уведомить игрока, если он пытается активировать чит на карте без электрической системы
        if (!CheatToggles.elecSab && !CheatToggles.unfixableLights) return;

        HudManager.Instance.Notifier.AddDisconnectMessage("Электрическая система отсутствует на этой карте");
        CheatToggles.elecSab = CheatToggles.unfixableLights = false;
    }

    public static void HandleUnfixLights(ShipStatus shipStatus)
    {
        if (CheatToggles.unfixableLights == _unfixableLights) return;

        // По-видимому, большинство значений, которые вы вводите для amount в RpcUpdateSystem, полностью ломают свет
        // Их невозможно починить обычными средствами (переключение тумблеров)
        // Их можно починить только повторением RpcUpdateSystem с тем же значением

        if (!_unfixableLights)
        {
            CheatToggles.elecSab = false;
        }

        shipStatus.RpcUpdateSystem(SystemTypes.Electrical, 69); // Починить или саботировать

        _unfixableLights = CheatToggles.unfixableLights;
    }

    public static void HandleMushMix(ShipStatus shipStatus, byte mapId)
    {
        if (!CheatToggles.mushSab) return;

        if (mapId == 5) // MushroomMixup работает только на Fungle
        {

            shipStatus.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, 1); // Саботировать

        }
        else
        {
            // Уведомить игрока, если он пытается активировать чит на карте без грибов

            HudManager.Instance.Notifier.AddDisconnectMessage("Грибы отсутствуют на этой карте");
        }

        // Починить (сломано)
        // var mushSys = shipStatus.Systems[SystemTypes.MushroomMixupSabotage].Cast<MushroomMixupSabotageSystem>();
        // mushSys.Deteriorate(mushSys.currentSecondsUntilHeal);

        CheatToggles.mushSab = false;
    }

    public static void HandleSpores(FungleShipStatus shipStatus, byte mapId)
    {
        if (!CheatToggles.mushSpore) return;

        if (mapId == 5)
        {
            foreach (var mushroom in shipStatus.sporeMushrooms.Values)
            {
                PlayerControl.LocalPlayer.CmdCheckSporeTrigger(mushroom);
            }
        }
        else
        {
            HudManager.Instance.Notifier.AddDisconnectMessage("Грибы отсутствуют на этой карте");
        }

        CheatToggles.mushSpore = false;
    }

    public static void HandleDoors(ShipStatus shipStatus)
    {
        if (CheatToggles.closeAllDoors)
        {
            DoorsHandler.CloseAllDoors();
            CheatToggles.closeAllDoors = false;
        }
        if (CheatToggles.openAllDoors)
        {
            DoorsHandler.OpenAllDoors();
            CheatToggles.openAllDoors = false;
        }

        if (CheatToggles.spamCloseAllDoors)
        {
            DoorsHandler.CloseAllDoors();
        }
        if (CheatToggles.spamOpenAllDoors)
        {
            DoorsHandler.OpenAllDoors();
        }
    }

    public static void Process(ShipStatus shipStatus)
    {
        var currentMapID = Utils.GetCurrentMapID();

        // Обработка всех систем саботажа
        HandleReactor(shipStatus, currentMapID);
        HandleOxygen(shipStatus, currentMapID);
        HandleComms(shipStatus, currentMapID);
        HandleElectrical(shipStatus, currentMapID);
        HandleDoors(shipStatus);
    }

    public static void ProcessFungle(FungleShipStatus shipStatus)
    {
        var currentMapID = Utils.GetCurrentMapID();

        // Обработка систем саботажа на Fungle
        HandleMushMix(shipStatus, currentMapID);
        HandleSpores(shipStatus, currentMapID);
    }
}
