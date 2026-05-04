using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace MalumMenu;

public class OverloadUI : MonoBehaviour
{
    public static int numSuccesses;
    public static int maxPossibleTargets;
    public static int killSwitchThreshold;
    public static HashSet<NetworkedPlayerInfo> currentTargets = new HashSet<NetworkedPlayerInfo>(new NetPlayerInfoCidComparer());
    private HashSet<NetworkedPlayerInfo> _tmpTargets = new HashSet<NetworkedPlayerInfo>(new NetPlayerInfoCidComparer());
    private bool _areTargetsUnlocked => !CheatToggles.runOverload || !CheatToggles.olLockTargets;
    private bool _hasAutoStarted;

    private Rect _windowRect = new(320, 10, 595, 500);
    private GUIStyle _targetButtonStyle;
    private GUIStyle _normalButtonStyle;
    private GUIStyle _logStyle;

    // Элементы консоли атаки
    private static Vector2 _scrollPosition = Vector2.zero;
    private static List<string> _logEntries = new();
    private const int MaxLogEntries = 300;

    private void Start()
    {
        killSwitchThreshold = 500 * MalumMenu.killSwitchLvl.Value;

        if (!CheatToggles.olAutoAdapt)
        {
            OverloadHandler.strength = MalumMenu.defaultStrength.Value;
            OverloadHandler.cooldown = MalumMenu.defaultCooldown.Value;
        }
    }

    private void Update()
    {
        var players = PlayerControl.AllPlayerControls.ToArray().Where(player => player?.Data != null && !player.AmOwner).ToArray();
        maxPossibleTargets = players.Length;

        if (!Utils.isFreePlay)
        {
            for (int i = 0; i < maxPossibleTargets; i++)
            {
                NetworkedPlayerInfo playerData = players[i].Data;
                var playerTarget = OverloadHandler.GetTarget(playerData);

                bool isTarget = playerTarget.isTarget;

                if (_areTargetsUnlocked)
                {
                    if (isTarget)
                    {
                        _tmpTargets.Add(playerData);

                        if (CheatToggles.runOverload && CheatToggles.olLogAddRemove && !currentTargets.Contains(playerData))
                        {
                            string colorStr = ColorUtility.ToHtmlStringRGB(Color.blue);
                            LogConsole($"> <b><color=#{colorStr}>ДОБАВЛЕН : {playerData.DefaultOutfit.PlayerName} (ID : {playerData.ClientId})</color></b>");
                        }
                    }
                    else
                    {
                        if (CheatToggles.runOverload && CheatToggles.olLogAddRemove && currentTargets.Contains(playerData))
                        {
                            string colorStr = ColorUtility.ToHtmlStringRGB(Color.blue);
                            LogConsole($"> <b><color=#{colorStr}>УДАЛЁН : {playerData.DefaultOutfit.PlayerName} (ID : {playerData.ClientId})</color></b>");
                        }
                    }
                }
                else
                {
                    if (currentTargets.Contains(playerData))
                    {
                        _tmpTargets.Add(playerData);
                    }
                }
            }
        }

        // Замена HashSet (currentTargets <-> _tmpTargets) возможна только если AmClient
        // Иначе currentTargets очищается слишком рано при отключении с активной атакой,
        // что приводит к неверному подсчёту общего количества в сообщении СТОП

        if (Utils.isClient && AmongUsClient.Instance.AmClient)
        {
            var old = currentTargets;
            currentTargets = _tmpTargets;
            _tmpTargets = old;
        }
        else
        {
            // currentTargets очищается здесь, если лог СТОП выполнен / не нужен (атака выключена)

            if (!CheatToggles.runOverload) currentTargets.Clear();

            _hasAutoStarted = false;
        }

        _tmpTargets.Clear();

        if (CheatToggles.olAutoAdapt)
        {
            var adaptedValues = OverloadHandler.CalculateAdaptedValues();

            OverloadHandler.strength = adaptedValues.strength;
            OverloadHandler.cooldown = adaptedValues.cooldown;
        }

        int numCurrentTargets = currentTargets.Count;

        if (CheatToggles.runOverload)
        {
            bool doAutoStop = CheatToggles.olAutoStop && numCurrentTargets <= 0;

            bool isLagging = Utils.GetPing() > killSwitchThreshold;
            bool doKillSwitch = CheatToggles.olKillSwitch && isLagging;

            if (doAutoStop || doKillSwitch)
            {
                string extraStr = doKillSwitch ? " : ! Аварийный выключатель !" : "";
                StopOverload(extraStr);
            }
        }
        else
        {
            if (Utils.isPlayer && CheatToggles.olAutoStart && !_hasAutoStarted && numCurrentTargets > 0)
            {
                _hasAutoStarted = true;
                StartOverload();
            }
        }
    }

    private void OnGUI()
    {
        if (!CheatToggles.showOverload || !MenuUI.isGUIActive || MalumMenu.isPanicked) return;

        InitStyles();

        UIHelpers.ApplyUIColor();

        _windowRect = GUI.Window((int)WindowId.OverloadUI, _windowRect, (GUI.WindowFunction)OverloadWindow, "Атака");
    }

    private void OverloadWindow(int windowID)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space(15f);

        GUILayout.BeginVertical();

        GUILayout.Space(5f);

        var players = PlayerControl.AllPlayerControls.ToArray().Where(player => player?.Data != null && !player.AmOwner).ToArray();
        var playerCount = players.Length;

        GUILayout.BeginHorizontal();

        if (playerCount > 0 && !Utils.isFreePlay)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(false));

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

            DrawPlayers(players, playerCount);

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(10f);
        }

        GUILayout.BeginVertical();

        DrawSelectionToggles();

        if (CheatToggles.overloadReset)
        {
            CheatToggles.overloadAll = false;
            CheatToggles.overloadHost = false;
            CheatToggles.overloadCrew = false;
            CheatToggles.overloadImps = false;

            OverloadHandler.ClearCustomTargets();

            CheatToggles.overloadReset = false;
        }

        GUILayout.EndVertical();

        GUILayout.Space(40f);

        GUILayout.EndHorizontal();

        GUILayout.Space(10f);

        GUILayout.Box("", GUIStylePreset.DarkSeparator, GUILayout.Height(1f), GUILayout.Width(420f));

        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();

        DrawStateButtons();

        GUILayout.Space(3f);

        DrawStateLabel();

        GUILayout.EndHorizontal();

        GUILayout.Space(20f);

        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(539f));

        DrawConsole();

        GUILayout.EndVertical();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private void InitStyles()
    {
        if (_targetButtonStyle == null)
        {
            _targetButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Italic
            };
        }

        if (_normalButtonStyle == null)
        {
            _normalButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold
            };
        }

        if (_logStyle == null)
        {
            _logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15
            };
        }
    }

    public static void LogConsole(string message)
    {
        if (_logEntries.Count >= MaxLogEntries)
        {
            _logEntries.RemoveAt(0);
        }

        _logEntries.Add(message);

        _scrollPosition.y = float.MaxValue;
    }

    public static void StartOverload()
    {
        CheatToggles.runOverload = true;

        if (CheatToggles.olAutoClear)
        {
            _logEntries.Clear();
        }

        if (CheatToggles.olLogStartStop)
        {
            string colorStr = ColorUtility.ToHtmlStringRGB(Color.red);
            string pluralStr = currentTargets.Count != 1 ? "ей" : "ь";
            string allStr = maxPossibleTargets > 0 && currentTargets.Count == maxPossibleTargets ? " - ВСЕ" : "";

            LogConsole($"> <b><color=#{colorStr}>ЗАПУСК : [{currentTargets.Count}] Цель{pluralStr}{allStr}</color></b>");
        }

        numSuccesses = 0;
    }

    public static void StopOverload(string extraStr = "")
    {
        if (CheatToggles.olLogStartStop)
        {
            int total = currentTargets.Count+numSuccesses;
            string colorStr = ColorUtility.ToHtmlStringRGB(Color.red);
            LogConsole($"> <b><color=#{colorStr}>СТОП : [{numSuccesses} / {total}] Кикнуто{extraStr}</color></b>");
        }

        // runOverload должен быть выключен ПОСЛЕ того, как сообщение СТОП было записано в лог
        // Иначе currentTargets очищается слишком рано при отключении с активной атакой,
        // что приводит к неверному подсчёту общего количества в сообщении СТОП

        CheatToggles.runOverload = false;

        numSuccesses = 0;
    }

    private void DrawPlayers(PlayerControl[] players, int playerCount)
    {
        for (int i = 0; i < playerCount; i++)
        {
            int num = i + 1;

            NetworkedPlayerInfo playerData = players[i].Data;

            var playerTarget = OverloadHandler.GetTarget(playerData);
            bool isTarget = playerTarget.isTarget;

            Color playerBackgroundColor = playerData.Color;
            Color playerContentColor = Color.Lerp(playerBackgroundColor, Color.white, 0.5f);

            Color standardBackgroundColor = GUI.backgroundColor;
            Color standardContentColor = GUI.contentColor;

            GUI.backgroundColor = isTarget ? Color.black : playerBackgroundColor;
            GUI.contentColor = playerContentColor;

            GUIStyle style = isTarget ? _targetButtonStyle : _normalButtonStyle;

            bool isPressed = GUILayout.Button(playerData.DefaultOutfit.PlayerName, style, GUILayout.Width(140f));

            if (isPressed && _areTargetsUnlocked)
            {
                if (isTarget)
                {
                    // Если удаляемая цель была добавлена фильтром, этот фильтр также отключается
                    // Любые другие цели, добавленные тем же фильтром, заново добавляются как пользовательские, чтобы
                    // была удалена только нужная цель

                    HashSet<OverloadHandler.TargetType> targetTypes = playerTarget.targetTypes;

                    if (targetTypes.Contains(OverloadHandler.TargetType.All))
                    {
                        OverloadHandler.PopulateCustomTargets(players, OverloadHandler.TargetType.All);
                        CheatToggles.overloadAll = false;
                    }

                    if (targetTypes.Contains(OverloadHandler.TargetType.Host))
                    {
                        OverloadHandler.PopulateCustomTargets(players, OverloadHandler.TargetType.Host);
                        CheatToggles.overloadHost = false;
                    }

                    if (targetTypes.Contains(OverloadHandler.TargetType.Crewmate))
                    {
                        OverloadHandler.PopulateCustomTargets(players, OverloadHandler.TargetType.Crewmate);
                        CheatToggles.overloadCrew = false;
                    }
                    else if (targetTypes.Contains(OverloadHandler.TargetType.Impostor))
                    {
                        OverloadHandler.PopulateCustomTargets(players, OverloadHandler.TargetType.Impostor);
                        CheatToggles.overloadImps = false;
                    }

                    OverloadHandler.RemoveCustomTarget(playerData);
                }
                else
                {
                    OverloadHandler.AddCustomTarget(playerData);
                }
            }

            // Сброс цвета UI
            GUI.backgroundColor = standardBackgroundColor;
            GUI.contentColor = standardContentColor;

            // UI показывает ряды из 3 кнопок (1 кнопка на игрока)
            if (num % 3 == 0 && num < playerCount)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            }
        }
    }

    private void DrawSelectionToggles()
    {
        bool newOverloadAll = GUILayout.Toggle(CheatToggles.overloadAll, " Все");
        CheatToggles.overloadAll = _areTargetsUnlocked ? newOverloadAll : false;

        bool newOverloadHost = GUILayout.Toggle(CheatToggles.overloadHost, " Хост");
        CheatToggles.overloadHost = _areTargetsUnlocked ? newOverloadHost : false;

        bool newOverloadCrew = GUILayout.Toggle(CheatToggles.overloadCrew, " Члены экипажа");
        CheatToggles.overloadCrew = _areTargetsUnlocked ? newOverloadCrew : false;

        bool newOverloadImps = GUILayout.Toggle(CheatToggles.overloadImps, " Самозванцы");
        CheatToggles.overloadImps = _areTargetsUnlocked ? newOverloadImps : false;

        bool newOverloadReset = GUILayout.Toggle(CheatToggles.overloadReset, " Сбросить");
        CheatToggles.overloadReset = _areTargetsUnlocked ? newOverloadReset : false;
    }

    private void DrawStateButtons()
    {
        Color standardBackgroundColor = GUI.backgroundColor;

        bool startEnabled = !CheatToggles.runOverload && Utils.isPlayer;

        Color startBackgroundColor = Color.green;
        GUI.backgroundColor = startEnabled ? startBackgroundColor : Color.black;

        if (GUILayout.Button("ЗАПУСК", GUILayout.Width(140f)) && startEnabled)
        {
            StartOverload();
        }

        // Сброс цвета UI
        GUI.backgroundColor = standardBackgroundColor;

        // Проверка Utils.isPlayer не нужна, так как проверка MenuUI уже обеспечивает её для runOverload
        bool stopEnabled = CheatToggles.runOverload;

        Color stopBackgroundColor = Color.red;
        GUI.backgroundColor = stopEnabled ? stopBackgroundColor : Color.black;

        if (GUILayout.Button("СТОП", GUILayout.Width(140f)) && stopEnabled)
        {
            StopOverload();
        }

        // Сброс цвета UI
        GUI.backgroundColor = standardBackgroundColor;
    }

    private void DrawStateLabel()
    {
        if (CheatToggles.runOverload)
        {
            Color onColor = Color.Lerp(Palette.AcceptedGreen, Color.white, 0.5f);
            string colorStr = ColorUtility.ToHtmlStringRGB(onColor);

            string firstStr = $"<b><color=#{colorStr}> Вкл : ";
            string middleStr;
            string finalStr = "</color></b>";

            if (currentTargets.Count > 0)
            {
                string pluralStr = currentTargets.Count != 1 ? "ей" : "ь";
                middleStr = $"Атака {currentTargets.Count} цел{pluralStr}";
            }
            else
            {
                middleStr = "Ожидание";
            }

            GUILayout.Label($"{firstStr}{middleStr}{finalStr}");
        }
        else
        {
            Color offColor = Color.Lerp(Palette.DisabledGrey, Color.white, 0.6f);
            string colorStr = ColorUtility.ToHtmlStringRGB(offColor);

            string middleStr = "";
            if (currentTargets.Count > 0)
            {
                string pluralStr = currentTargets.Count != 1 ? "ей" : "ь";
                middleStr = $" : {currentTargets.Count} цел{pluralStr} выбрано";
            }

            GUILayout.Label($"<b><color=#{colorStr}> Выкл{middleStr}</color></b>");
        }
    }

    private void DrawConsole()
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

        foreach (var log in _logEntries)
        {
            GUILayout.Label(log, _logStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

        if (GUILayout.Button("Очистить лог"))
        {
            _logEntries.Clear();
        }

        GUILayout.EndHorizontal();
    }
}
