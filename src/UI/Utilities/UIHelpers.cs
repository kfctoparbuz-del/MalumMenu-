using UnityEngine;

namespace MalumMenu;

public static class UIHelpers
{
    public static void ApplyUIColor()
    {
        if (CheatToggles.rgbMode)
        {
            GUI.backgroundColor = Color.HSVToRGB(MenuUI.hue, 1f, 1f); // Установка цвета фона на основе оттенка
        }
        else
        {
            var configHtmlColor = MalumMenu.menuHtmlColor.Value;

            if (!ColorUtility.TryParseHtmlString(configHtmlColor, out var uiColor))
            {
                if (!configHtmlColor.StartsWith("#"))
                {
                    if (ColorUtility.TryParseHtmlString("#" + configHtmlColor, out uiColor))
                    {
                        GUI.backgroundColor = uiColor;
                    }
                }
            }
            else
            {
                GUI.backgroundColor = uiColor;
            }
        }
    }
}
