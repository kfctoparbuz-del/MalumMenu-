using UnityEngine;

namespace MalumMenu;

public class KeybindListener : MonoBehaviour
{
    public void Update()
    {
        if (MalumMenu.isPanicked) return;

        // Привязки клавиш не срабатывают при вводе текста в чате
        if (HudManager.InstanceExists && HudManager.Instance.Chat && HudManager.Instance.Chat.IsOpenOrOpening) return;

        // Проверка каждой привязки клавиш, нажал ли пользователь её, и переключение соответствующего чита
        foreach (var (name, key) in CheatToggles.Keybinds)
        {
            if (key == KeyCode.None) continue;
            if (!Input.GetKeyDown(key)) continue;

            if (!CheatToggles.ToggleFields.TryGetValue(name, out var field)) continue;

            var current = (bool)field.GetValue(null);
            field.SetValue(null, !current);
        }
    }
}
