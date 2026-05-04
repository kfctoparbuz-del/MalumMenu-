using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MalumMenu;

public class MenuUI : MonoBehaviour
{
    public static int windowHeight = 550;
    public static int windowWidth = 700;
    private Rect _windowRect;

    public static bool isGUIActive = false;
    private List<ITab> _tabs = new();
    private int _selectedTab;
    public static float hue; // Для режима RGB

    private void Start()
    {
        // Добавить все вкладки при запуске
        _tabs.Add(new MovementTab());
        _tabs.Add(new ESPTab());
        _tabs.Add(new RolesTab());
        _tabs.Add(new ShipTab());
        _tabs.Add(new ChatTab());
        _tabs.Add(new AnimationsTab());
        _tabs.Add(new OverloadTab());
        _tabs.Add(new ConsoleTab());
        _tabs.Add(new HostOnlyTab());
        _tabs.Add(new PassiveTab());
        _tabs.Add(new ModesTab());
        _tabs.Add(new ConfigTab());

        // Создание 2D области интерфейса меню
        _windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }
    public void InitStyles()
    {
        GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 15;
        GUI.skin.toggle.fontStyle = GUI.skin.button.fontStyle = GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.skin.window.fontStyle = FontStyle.Bold;
        GUI.skin.box.fontStyle = FontStyle.Bold;
    }

    private void Update()
    {

        if (Input.GetKeyDown(Utils.StringToKeycode(MalumMenu.menuKeybind.Value)))
        {
            // Включение или выключение GUI клавишей DELETE
            isGUIActive = !isGUIActive;

            if (MalumMenu.menuOpenOnMouse.Value)
            {
                // Телепортировать окно к курсору для немедленного использования
                Vector2 mousePosition = Input.mousePosition;
                _windowRect.position = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            }
        }

        if (CheatToggles.rgbMode)
        {
            hue += Time.deltaTime * 0.3f; // Регулировка скорости смены цвета, чем выше множитель, тем быстрее
            if (hue > 1f) hue -= 1f; // Возврат hue к 0, когда превышает 1
        }

        if (CheatToggles.stealthMode != MalumMenu.inStealthMode)
        {
            MalumMenu.inStealthMode = CheatToggles.stealthMode;

            Scene scene = SceneManager.GetActiveScene();

            if (scene.name == "MainMenu" || scene.name == "MatchMaking")
            {
                SceneManager.LoadScene(scene.name);
            }
        }

        if (CheatToggles.panicMode) Utils.Panic();

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = !(MalumMenu.inStealthMode || MalumMenu.isPanicked);

        if (CheatToggles.openConfig)
        {
            Utils.OpenConfigFile();
            CheatToggles.openConfig = false;
        }

        if (CheatToggles.reloadConfig)
        {
            MalumMenu.Plugin.Config.Reload();
            CheatToggles.reloadConfig = false;
        }

        if (CheatToggles.saveProfile)
        {
            CheatToggles.saveProfile = false; // Сначала выключить, чтобы избежать сохранения в профиль
            CheatToggles.SaveTogglesToProfile();
        }

        if (CheatToggles.loadProfile)
        {
            CheatToggles.LoadTogglesFromProfile();
            CheatToggles.loadProfile = false;
        }

        // Некоторые читы работают только при наличии LocalPlayer, поэтому они отключаются, если его нет
        if(!Utils.isPlayer)
        {
            CheatToggles.setFakeRole = false;
            CheatToggles.setFakeAlive = false;
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.teleportPlayer = false;
            CheatToggles.spectate = false;
            CheatToggles.freecam = false;
            CheatToggles.killPlayer = false;
            CheatToggles.callMeeting = false;

            if (CheatToggles.runOverload)
            {
                OverloadUI.StopOverload();
                OverloadHandler.ClearCustomTargets();
            }
        }

        // Некоторые читы работают только при наличии корабля, поэтому они отключаются, если его нет
        if(!Utils.isShip)
        {
            CheatToggles.sabotageMap = false;
            CheatToggles.unfixableLights = false;
            CheatToggles.completeMyTasks = false;
            CheatToggles.kickVents = false;
            CheatToggles.reportBody = false;
            CheatToggles.closeMeeting = false;
            CheatToggles.reactorSab = false;
            CheatToggles.oxygenSab = false;
            CheatToggles.commsSab = false;
            CheatToggles.elecSab = false;
            CheatToggles.mushSab = false;
            CheatToggles.closeAllDoors = false;
            CheatToggles.openAllDoors = false;
            CheatToggles.spamCloseAllDoors = false;
            CheatToggles.spamOpenAllDoors = false;
            CheatToggles.mushSpore = false;

            MalumCheats.StopShipAnimCheats();
        }

        if(!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.killPlayer = false;
            CheatToggles.ejectPlayer = false;
            CheatToggles.noKillCd = false;
            CheatToggles.killAnyone = false;
            CheatToggles.killVanished = false;
            CheatToggles.forceStartGame = false;
            CheatToggles.skipMeeting = false;
            CheatToggles.voteImmune = false;
            CheatToggles.noGameEnd = false;
            CheatToggles.showProtectMenu = false;
            CheatToggles.showRolesMenu = false;
            CheatToggles.noOptionsLimits = false;
        }

        // Некоторые читы работают только во время собрания, поэтому они отключаются, если собрания нет
        if (!Utils.isMeeting)
        {
            CheatToggles.skipMeeting = false;
            CheatToggles.ejectPlayer = false;
        }
        MalumPPMCheats.AlwaysImpostorCheck();
    }

    public void OnGUI()
    {
        if (!isGUIActive || MalumMenu.isPanicked) return;

        InitStyles();

        UIHelpers.ApplyUIColor();

        _windowRect = GUI.Window((int)WindowId.MenuUI, _windowRect, (GUI.WindowFunction)WindowFunction, "MalumMenu v" + MalumMenu.malumVersion);
    }

    public void WindowFunction(int windowID)
    {
        GUILayout.BeginHorizontal();

        // Левая панель выбора вкладок (15% ширины)
        GUILayout.BeginVertical(GUILayout.Width(windowWidth * 0.15f));
        for (var i = 0; i < _tabs.Count; i++)
        {
            Color standardColor = GUI.backgroundColor;

            if (_selectedTab == i)
            {
                GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            }

            if (GUILayout.Button(_tabs[i].name, GUIStylePreset.TabButton, GUILayout.Height(35)))
                _selectedTab = i;

            GUI.backgroundColor = standardColor;

        }
        GUILayout.EndVertical();

        // Вертикальная разделительная линия + невидимый пробел для создания промежутка между панелью вкладок и содержимым
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
        GUILayout.Space(10f);

        // Правая панель содержимого вкладки и элементов управления (85% ширины)
        GUILayout.BeginVertical(GUILayout.Width(windowWidth * 0.85f));

        // Содержимое конкретной вкладки
        if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
        {
            GUILayout.Label(_tabs[_selectedTab].name, GUIStylePreset.TabTitle);
            _tabs[_selectedTab].Draw();
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        // Окно можно перетаскивать
        GUI.DragWindow();
    }
}
