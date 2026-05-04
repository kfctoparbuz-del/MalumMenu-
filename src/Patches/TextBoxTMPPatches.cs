using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Update))]
public static class TextBoxTMP_Update
{
    // Постфикс-патч TextBoxTMP.Update для разрешения копирования, вставки и вырезания текста между полем чата и буфером обмена устройства
    public static void Postfix(TextBoxTMP __instance)
    {
        if (!CheatToggles.unlockClipboard || !__instance.hasFocus) return;

        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            GUIUtility.systemCopyBuffer = __instance.text;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Utils.isPastingInput = true;

            __instance.SetText(__instance.text + GUIUtility.systemCopyBuffer);

            Utils.isPastingInput = false;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            GUIUtility.systemCopyBuffer = __instance.text;
            __instance.SetText("");
        }
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public static class TextBoxTMP_IsCharAllowed
{
    private static int _currentCharPos = 0;

    // Префикс-патч TextBoxTMP.IsCharAllowed для разблокировки дополнительных символов
    public static bool Prefix(TextBoxTMP __instance, ref bool __result)
    {
        // Если пользователь пишет через IME композицию, всегда разрешать вводимые символы
        // Исправляет проблемы для пользователей CJK языков

        string compositionString = Input.compositionString;
        if (compositionString.Length > 0)
        {
            __result = true;
            return false;
        }

        // Если пользователь вставил текст, читать из буфера обмена. В противном случае использовать вводимый текст
        var input = Utils.isPastingInput ? GUIUtility.systemCopyBuffer : Input.inputString;

        // Разрешать все символы, если нет пользовательского ввода, так как проверка не требуется
        if (input.Length == 0)
        {
            __result = true;
            return false;
        }

        // Восстановить полную строку, обрабатываемую TextBoxTMP.SetText

        string currentText = __instance.text ?? string.Empty;

        int caretPos = Mathf.Clamp(__instance.caretPos, 0, currentText.Length);

        string text = currentText.Insert(caretPos, input);

        // Получить символ, который в данный момент проверяется, отслеживая
        // каждый вызов TextBoxTMP.IsCharAllowed в цикле foreach TextBoxTMP.SetText

        _currentCharPos = Mathf.Clamp(_currentCharPos, 0, text.Length - 1);

        char currentChar = text[_currentCharPos];

        if (_currentCharPos >= text.Length - 1)
        {
            _currentCharPos = 0; // Сброс позиции, когда цикл завершается
        }
        else
        {
            _currentCharPos++; // Увеличение позиции до следующего символа в цикле
        }

        if (CheatToggles.unlockCharacters)
        {
            // Заблокированные символы, чтобы не нарушать ввод текста / не получить кик от античита
            HashSet<char> blockedSymbols = new() { '\b', '\r', '>', '<', '[' };

            if (blockedSymbols.Contains(currentChar))
            {
                __result = false;
                return false;
            }

            __result = true;
        }
        else // Обычная логика IsCharAllowed
        {
            if (__instance.IpMode)
            {
                __result = (currentChar >= '0' && currentChar <= '9') || currentChar == '.';
                return false;
            }

            __result = currentChar == ' ' ||
            (currentChar >= 'A' && currentChar <= 'Z') ||
            (currentChar >= 'a' && currentChar <= 'z') ||
            (currentChar >= '0' && currentChar <= '9') ||
            (currentChar >= 'À' && currentChar <= 'ÿ') ||
            (currentChar >= 'Ѐ' && currentChar <= 'џ') ||
            (currentChar >= '぀' && currentChar <= '㆟') ||
            (currentChar >= 'ⱡ' && currentChar <= '힣') ||
            (__instance.AllowSymbols && TextBoxTMP.SymbolChars.Contains(currentChar)) ||
            (__instance.AllowEmail && TextBoxTMP.EmailChars.Contains(currentChar));
        }

        return false;
    }
}
