using System;
using UnityEngine;

namespace MalumMenu;

public class OverloadTab : ITab
{
    public string name => "Нагрузка";

    private GUIStyle _sliderSubtitle;
    private int _maxStrength = 1000;
    private float _maxCooldown = 1f;
    private float _fpsEstimate = 0f;
    private float _rawCooldown;
    private float _rawStrength;

    public void Draw()
    {
        InitStyles();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawSettingsToggle();

        GUILayout.EndVertical();

        if (CheatToggles.showOverloadSettings)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(MenuUI.windowWidth * 0.75f));

            DrawSettingsSection();

            GUILayout.EndVertical();
        }
    }

    private void InitStyles()
    {
        if (_sliderSubtitle == null)
        {
            _sliderSubtitle = new(GUIStylePreset.TabSubtitle)
            {
                fontStyle = FontStyle.Normal
            };
        }
    }

    private void DrawGeneral()
    {
        CheatToggles.showOverload = GUILayout.Toggle(CheatToggles.showOverload, " Показать меню нагрузки");
    }

    private void DrawSettingsToggle()
    {
        GUILayout.Label("Настройки", GUIStylePreset.TabSubtitle);

        CheatToggles.showOverloadSettings = GUILayout.Toggle(CheatToggles.showOverloadSettings, " Показать настройки нагрузки");
    }

    private void DrawSettingsSection()
    {
        GUILayout.Space(15);

        GUILayout.BeginHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        CheatToggles.olAutoAdapt = GUILayout.Toggle(CheatToggles.olAutoAdapt, " Автоадаптация");

        int ping = Utils.GetPing();
        string pingStr = $"ПИНГ : {ping} мс";
        GUILayout.Label(Utils.GetColoredPingText(pingStr, ping));

        int strength = OverloadHandler.strength;
        float cooldown = OverloadHandler.cooldown;

        float numExecutionsPerSec;
        string extraStr = "";

        if (cooldown > Time.unscaledDeltaTime) // numExecutionsPerSec будет ниже FPS
        {
            numExecutionsPerSec = 1f / cooldown;
        }
        else // numExecutionsPerSec будет выше FPS
        {
            // FPS колеблется слишком часто, поэтому обновляем только после значительного изменения (> 5)

            float fps = Utils.GetFps();
            if (Math.Abs(fps - _fpsEstimate) > 5f)
            {
                _fpsEstimate = fps;
            }

            numExecutionsPerSec = (int)_fpsEstimate; // numExecutionsPerSec ограничен FPS независимо от задержки
            extraStr = " (Ограничение FPS)";
        }

        int numTargetsPerSec = OverloadUI.currentTargets.Count <= numExecutionsPerSec ? OverloadUI.currentTargets.Count : (int)numExecutionsPerSec; // numTargetsPerSec ограничен numExecutionsPerSec

        int rpcPerTarget = numTargetsPerSec > 0 ? (int)(strength * numExecutionsPerSec / numTargetsPerSec) :
                                            (int)(strength * numExecutionsPerSec);

        string rpcStr = CheatToggles.olShowRpcTotal
                        ? $"{rpcPerTarget*Math.Max(1, numTargetsPerSec)}"
                        : $"{rpcPerTarget}x{numTargetsPerSec}";

        CheatToggles.olShowRpcTotal = GUILayout.Toggle(CheatToggles.olShowRpcTotal, $" RPC/с : {rpcStr}{extraStr}");

        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        DrawSettingsSliders();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.35f));

        GUILayout.Label("Общие", GUIStylePreset.TabSubtitle);

        CheatToggles.olAutoStart = GUILayout.Toggle(CheatToggles.olAutoStart, " Автозапуск когда готово");

        CheatToggles.olAutoStop = GUILayout.Toggle(CheatToggles.olAutoStop, " Автоостановка когда завершено");

        CheatToggles.olLockTargets = GUILayout.Toggle(CheatToggles.olLockTargets, " Блокировать цели при запуске");

        CheatToggles.olKillSwitch = GUILayout.Toggle(CheatToggles.olKillSwitch, " Аварийный выключатель при лагах");

        if (CheatToggles.olKillSwitch)
        {
            Color standardBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            bool isPressed = GUILayout.Button($"{OverloadUI.killSwitchThreshold} мс", GUILayout.Width(70f));
            if (isPressed)
            {
                if (OverloadUI.killSwitchThreshold >= 3000) // Макс KS = 3000 мс
                {
                    OverloadUI.killSwitchThreshold = 500; // Мин KS = 500 мс
                }
                else
                {
                    OverloadUI.killSwitchThreshold = OverloadUI.killSwitchThreshold + 500; // Увеличение с шагом 500 мс
                }
            }

            GUI.backgroundColor = standardBackgroundColor;
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Логи", GUIStylePreset.TabSubtitle);

        CheatToggles.olLogStartStop = GUILayout.Toggle(CheatToggles.olLogStartStop, " Логировать ЗАПУСК и ОСТАНОВКУ");

        CheatToggles.olLogAddRemove = GUILayout.Toggle(CheatToggles.olLogAddRemove, " Логировать ДОБАВЛЕНИЕ и УДАЛЕНИЕ");

        CheatToggles.olLogAttack = GUILayout.Toggle(CheatToggles.olLogAttack, " Логировать нагрузку");

        CheatToggles.olLogDisconnect = GUILayout.Toggle(CheatToggles.olLogDisconnect, " Логировать отключение");

        CheatToggles.olVerboseLogs = GUILayout.Toggle(CheatToggles.olVerboseLogs, " Подробные логи атак");

        CheatToggles.olAutoClear = GUILayout.Toggle(CheatToggles.olAutoClear, " Автоочистка при запуске");

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(15);
    }

    private void DrawSettingsSliders()
    {
        GUILayout.Label($"Сила : {_rawStrength}", _sliderSubtitle);
        GUILayout.Space(1);

        GUILayout.BeginHorizontal();

        float inputStrength = GUILayout.HorizontalSlider(_rawStrength, 1, _maxStrength, GUILayout.Width(350f));

        if (inputStrength != _rawStrength)
        {
            CheatToggles.olAutoAdapt = false; // Отключить AutoAdapt, если пользователь вводит вручную
            _rawStrength = inputStrength;
        }

        GUILayout.Space(5);
        bool isPressedMaxStrength = GUILayout.Button($"{_maxStrength}", GUILayout.Width(50f));

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label($"Задержка : {_rawCooldown:F2}", _sliderSubtitle);
        GUILayout.Space(1);

        GUILayout.BeginHorizontal();

        float inputCooldown = GUILayout.HorizontalSlider(_rawCooldown, 0f, _maxCooldown, GUILayout.Width(350f));

        if (inputCooldown != _rawCooldown)
        {
            CheatToggles.olAutoAdapt = false; // Отключить AutoAdapt, если пользователь вводит вручную
            _rawCooldown = inputCooldown;
        }

        GUILayout.Space(5);
        bool isPressedMaxCooldown = GUILayout.Button($"{_maxCooldown:F0}", GUILayout.Width(50f));

        GUILayout.EndHorizontal();

        if (!CheatToggles.olAutoAdapt)
        {
            float strengthStep = _maxStrength / 100f; // Шаги слайдера составляют 1/100 от максимальной силы
            int clampStrength = Mathf.RoundToInt(Mathf.Clamp(Mathf.Round(_rawStrength / strengthStep) * strengthStep, 1, _maxStrength));
            OverloadHandler.strength = clampStrength;

            float cooldownStep = _maxCooldown / 100f; // Шаги слайдера составляют 1/100 от максимальной задержки
            float clampCooldown = Mathf.Round(_rawCooldown / cooldownStep) * cooldownStep;
            OverloadHandler.cooldown = clampCooldown;
        }

        // Регулировка границ, чтобы слайдеры никогда не выходили за пределы

        while (_maxStrength < OverloadHandler.strength)
        {
            _maxStrength *= 10;
        }

        while (_maxCooldown < OverloadHandler.cooldown)
        {
            _maxCooldown *= 10;
        }

        if (isPressedMaxStrength)
        {
            if (_maxStrength >= 1000) // Макс _maxStrength = 1000 RPC
            {
                CheatToggles.olAutoAdapt = false; // Отключить AutoAdapt, если пользователь вводит вручную

                OverloadHandler.strength = Mathf.RoundToInt(OverloadHandler.strength/10f); // Корректировка значения с учётом изменения максимума (÷10)

                _maxStrength = 100; // Мин _maxStrength = 100 RPC
            }
            else
            {
                CheatToggles.olAutoAdapt = false; // Отключить AutoAdapt, если пользователь вводит вручную

                OverloadHandler.strength *= 10; // Корректировка значения с учётом изменения максимума (x10)

                _maxStrength *= 10; // Увеличение с шагом x10
            }
        }

        if (isPressedMaxCooldown)
        {
            if (_maxCooldown >= 10f) // Макс _maxCooldown = 10с
            {
                CheatToggles.olAutoAdapt = false; // Отключить AutoAdapt, если пользователь вводит вручную

                OverloadHandler.cooldown /= 10f; // Корректировка значения с учётом изменения максимума (÷10)

                _maxCooldown = 1f; // Мин _maxCooldown = 1с
            }
            else
            {
                CheatToggles.olAutoAdapt = false; // Отключить AutoAdapt, если пользователь вводит вручную

                OverloadHandler.cooldown *= 10; // Корректировка значения с учётом изменения максимума (x10)

                _maxCooldown *= 10; // Увеличение с шагом x10
            }
        }

        // Обновление значений слайдеров в соответствии с фактическими значениями

        _rawStrength = OverloadHandler.strength;
        _rawCooldown = OverloadHandler.cooldown;
    }
}
