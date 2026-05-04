using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace MalumMenu;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class MalumMenu : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static MalumMenu Plugin;
    public new static ManualLogSource Log;

    public static MenuUI menuUI;
    public static ConsoleUI consoleUI;
    public static RolesUI rolesUI;
    public static OverloadUI overloadUI;
    public static DoorsUI doorsUI;
    public static TasksUI tasksUI;
    public static ProtectUI protectUI;
    public static KeybindListener keybindListener;

    public static string malumVersion = "3.1.0";
    public static List<string> supportedAU = new List<string> { "2026.3.31" };
    public static bool isPanicked = false;
    public static bool inStealthMode = false;

    public static ConfigEntry<string> menuKeybind;
    public static ConfigEntry<string> menuHtmlColor;
    public static ConfigEntry<bool> menuOpenOnMouse;
    public static ConfigEntry<bool> menuKeepSubwindowsOpen;
    public static ConfigEntry<string> spoofLevel;
    public static ConfigEntry<string> spoofPlatform;
    public static ConfigEntry<bool> spoofDeviceId;
    public static ConfigEntry<bool> noTelemetry;
    public static ConfigEntry<string> guestFriendCode;
    public static ConfigEntry<bool> guestMode;
    public static ConfigEntry<bool> autoLoadProfile;
    public static ConfigEntry<string> configEditor;
    public static ConfigEntry<int> adaptMaxStrength;
    public static ConfigEntry<float> adaptMaxCooldown;
    public static ConfigEntry<float> attackLogDelay;
    public static ConfigEntry<int> defaultStrength;
    public static ConfigEntry<float> defaultCooldown;
    public static ConfigEntry<int> killSwitchLvl;

    public override void Load()
    {
        Log = base.Log;
        Plugin = this;

        // Загружает настройки конфигурации
        menuKeybind = Config.Bind("MalumMenu.GUI",
                                "Keybind",
                                "Delete",
                                "Клавиша клавиатуры, используемая для включения и выключения GUI. Список поддерживаемых кодов клавиш: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");

        menuHtmlColor = Config.Bind("MalumMenu.GUI",
                                "Color",
                                "",
                                "Пользовательский цвет для вашего GUI MalumMenu. Поддерживает HTML-цветовые коды");

        menuOpenOnMouse = Config.Bind("MalumMenu.GUI",
                                "OpenOnMouse",
                                false,
                                "При включении GUI MalumMenu всегда будет открываться в текущей позиции мыши");

        menuKeepSubwindowsOpen = Config.Bind("MalumMenu.GUI",
                                "KeepSubwindowsOpen",
                                false,
                                "При включении закрытие GUI MalumMenu не будет автоматически закрывать его подокна");

        autoLoadProfile = Config.Bind("MalumMenu.Profile",
                                "AutoLoadProfile",
                                false,
                                "При включении ваш сохранённый профиль клавиш и переключателей будет автоматически загружаться при запуске игры");

        configEditor = Config.Bind("MalumMenu.Config",
                                "ConfigEditor",
                                "notepad.exe",
                                "Программа, используемая для открытия файла конфигурации при использовании переключателя Open Config. Может быть любым исполняемым файлом, но рекомендуется использовать текстовый редактор");

        // Настройки GuestMode закомментированы, так как читы сломаны в последних обновлениях

        // guestMode = Config.Bind("MalumMenu.GuestMode",
        //                         "GuestMode",
        //                         false,
        //                         "При включении каждый раз при запуске игры будет создаваться новый гостевой аккаунт, что позволяет обходить баны аккаунтов и обнаружение PUID");

        // guestFriendCode = Config.Bind("MalumMenu.GuestMode",
        //                         "FriendName",
        //                         "",
        //                         "Имя пользователя, которое будет использоваться при установке кода друга для вашего гостевого аккаунта. ВАЖНО: Можно использовать только с GuestMode, должно быть ≤ 10 символов и не может включать специальные символы/дискриминатор (#1234)");

        spoofLevel = Config.Bind("MalumMenu.Spoofing",
                                "Level",
                                "",
                                "Пользовательский уровень игрока, отображаемый другим в онлайн-играх, чтобы скрыть вашу реальную платформу. ВАЖНО: Пользовательские уровни могут быть только от 1 до 100001. Десятичные числа не работают");

        spoofPlatform = Config.Bind("MalumMenu.Spoofing",
                                "Platform",
                                "",
                                "Пользовательская игровая платформа, отображаемая другим в онлайн-лобби, чтобы скрыть вашу реальную платформу. Список поддерживаемых платформ: https://skeld.js.org/enums/_skeldjs_constant.Platform.html");

        spoofDeviceId = Config.Bind("MalumMenu.Privacy",
                                "HideDeviceId",
                                true,
                                "При включении будет скрывать ваш уникальный deviceId от Among Us, что потенциально может помочь обойти аппаратные баны в будущем");

        noTelemetry = Config.Bind("MalumMenu.Privacy",
                                "NoTelemetry",
                                true,
                                "При включении остановит сбор аналитики ваших игр Among Us и их отправку в Innersloth с помощью Unity Analytics");

        adaptMaxStrength = Config.Bind("MalumMenu.Overload",
                                "AdaptMaxStrength",
                                500,
                                new ConfigDescription(
                                    "Максимальное общее количество RPC, отправленных за один цикл перегрузки в режиме AutoAdapt. Автоматически распределяется между целями и уменьшается в зависимости от пинга. ВАЖНО: Допустимо только от 1 до 1000 RPC",
                                    new AcceptableValueRange<int>(1, 1000)
                                ));

        adaptMaxCooldown = Config.Bind("MalumMenu.Overload",
                                "AdaptMaxCooldown",
                                1f,
                                new ConfigDescription(
                                    "Максимальное время (в секундах) для завершения одного полного цикла перегрузки в режиме AutoAdapt. Автоматически распределяется между целями (больше целей = короче задержка на цель). ВАЖНО: Допустимо только от 0с до 10с",
                                    new AcceptableValueRange<float>(0f, 10f)
                                ));

        attackLogDelay = Config.Bind("MalumMenu.Overload",
                                "AttackLogDelay",
                                2f,
                                "Минимальное время (в секундах) между логами атак в обычном (не подробном) режиме");

        defaultStrength = Config.Bind("MalumMenu.Overload",
                                "DefaultStrength",
                                500,
                                new ConfigDescription(
                                    "Стандартное количество некорректных RPC, отправляемых каждой цели за цикл перегрузки. Переопределяется, если включён режим AutoAdapt. ВАЖНО: Допустимо только от 1 до 1000 RPC",
                                    new AcceptableValueRange<int>(1, 1000)
                                ));

        defaultCooldown = Config.Bind("MalumMenu.Overload",
                                "DefaultCooldown",
                                1f,
                                new ConfigDescription(
                                    "Стандартная задержка (в секундах) между каждой целью во время цикла перегрузки. Переопределяется, если включён режим AutoAdapt. ВАЖНО: Допустимо только от 0с до 10с",
                                    new AcceptableValueRange<float>(0f, 10f)
                                ));

        killSwitchLvl = Config.Bind("MalumMenu.Overload",
                                "DefaultKillSwitchLevel",
                                1,
                                new ConfigDescription(
                                    "Стандартный уровень, используемый аварийным выключателем. Каждый уровень добавляет 500 мс к максимально допустимому пингу перед остановкой перегрузки. Помогает избежать лагов/отключений. ВАЖНО: Допустимо только от уровня 1 (500 мс) до 6 (3000 мс)",
                                    new AcceptableValueRange<int>(1, 6)
                                ));

        // Включено по умолчанию
        CheatToggles.unlockFeatures = true;
        CheatToggles.freeCosmetics = true;
        CheatToggles.avoidPenalties = true;

        // Включено по умолчанию
        CheatToggles.olAutoAdapt = true;
        CheatToggles.olKillSwitch = true;
        CheatToggles.olAutoStop = true;
        CheatToggles.olAutoClear = true;
        CheatToggles.olLogStartStop = true;
        CheatToggles.olLogAttack = true;
        CheatToggles.olLogAddRemove = true;
        CheatToggles.olLogDisconnect = true;

        Harmony.PatchAll();

        // UI
        menuUI = AddComponent<MenuUI>();
        consoleUI = AddComponent<ConsoleUI>();
        overloadUI = AddComponent<OverloadUI>();
        doorsUI = AddComponent<DoorsUI>();
        tasksUI = AddComponent<TasksUI>();
        protectUI = AddComponent<ProtectUI>();
        // rolesUI = AddComponent<RolesUI>();

        // Компоненты
        keybindListener = AddComponent<KeybindListener>();

        // Отключает телеметрию (полностью не проверено, работает ли, но согласно документации Unity должно)
        if (noTelemetry.Value)
        {
            Analytics.enabled = false;
            Analytics.deviceStatsEnabled = false;
            PerformanceReporting.enabled = false;
        }

        // Загружает профиль при запуске
        if (autoLoadProfile.Value)
        {
            CheatToggles.LoadTogglesFromProfile();
        }

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu" && !(inStealthMode || isPanicked))
            {
                // Предупреждение о неподдерживаемых версиях AU
                if (!supportedAU.Contains(Application.version))
                {
                    Utils.ShowPopup("\nЭта версия MalumMenu и ваша версия Among Us несовместимы!\n\nУстановите правильную версию игры или MalumMenu для работы без ошибок.");
                }
            }
        }));
    }
}
