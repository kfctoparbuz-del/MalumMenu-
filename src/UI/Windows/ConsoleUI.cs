using Il2CppSystem;
using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

public class ConsoleUI : MonoBehaviour
{
    public static int windowHeight = 350;
    public static int windowWidth = 550;
    private Rect _windowRect;

    private GUIStyle _logStyle;
    private static Vector2 _scrollPosition = Vector2.zero;
    private static List<string> _logEntries = new();
    private const int MaxLogEntries = 300;

    private void Start()
    {
        // Создание 2D области интерфейса консоли
        _windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showConsole || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

        _logStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 16
        };

        UIHelpers.ApplyUIColor();

        _windowRect = GUI.Window((int)WindowId.ConsoleUI, _windowRect, (GUI.WindowFunction)ConsoleWindow, "Консоль");
    }

    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginVertical(GUI.skin.box);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

        foreach (var log in _logEntries)
        {
            GUILayout.Label(log, _logStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Очистить лог", GUILayout.Width(260)))
        {
            _logEntries.Clear();
        }

        if (GUILayout.Button("Копировать лог в буфер обмена"))
        {
            GUIUtility.systemCopyBuffer = String.Join("\n", _logEntries.ToArray());
        }

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    public static void Log(string message)
    {
        if (_logEntries.Count >= MaxLogEntries) // Ограничение количества записей в логе для контроля использования памяти
        {
            _logEntries.RemoveAt(0); // Удаление самой старой записи лога
        }

        _logEntries.Add(message);

        // Прокрутка вниз
        _scrollPosition.y = float.MaxValue;
    }
}
