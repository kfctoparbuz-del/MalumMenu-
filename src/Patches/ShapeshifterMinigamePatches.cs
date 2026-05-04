using AmongUs.Data;
using HarmonyLib;
using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace MalumMenu;

[HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
public static class ShapeshifterMinigame_Begin
{
    // Префикс-патч ShapeshifterMinigame.Begin для реализации логики меню выбора игрока
    public static bool Prefix(ShapeshifterMinigame __instance)
    {
        if (!PlayerPickMenu.isActive) return true; // Открыть обычное меню оборотня, если не активно

        // Пользовательский список игроков, установленный openPlayerPickMenu
        List<NetworkedPlayerInfo> playerList = PlayerPickMenu.customPlayerList;

        __instance.potentialVictims = new List<ShapeshifterPanel>();

        List<UiElement> selectableElements = new List<UiElement>();

        for (int i = 0; i < playerList.Count; i++)
        {
            NetworkedPlayerInfo playerData = playerList[i];

            int num = i % 3;
            int num2 = i / 3;
            ShapeshifterPanel shapeshifterPanel = Object.Instantiate(__instance.PanelPrefab, __instance.transform);
            shapeshifterPanel.transform.localPosition = new Vector3(__instance.XStart + num * __instance.XOffset, __instance.YStart + num2 * __instance.YOffset, -1f);

            shapeshifterPanel.SetPlayer(i, playerData, (Il2CppSystem.Action) (() =>
            {
                PlayerPickMenu.targetPlayerData = playerData; // Сохранить выбранного игрока

                PlayerPickMenu.customAction.Invoke(); // Пользовательское действие, установленное openPlayerPickMenu

                __instance.Close();
            }));

            if (playerData.Object != null)
            {
                shapeshifterPanel.NameText.text = Utils.GetNameTag(playerData, playerData.DefaultOutfit.PlayerName);

                // Перемещение и изменение размера именной таблички, чтобы предотвратить перекрытие с текстом для дальтоников
                if (CheatToggles.seeRoles && CheatToggles.seePlayerInfo)
                {
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }
                else if (CheatToggles.seeRoles || CheatToggles.seePlayerInfo)
                {
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
                else
                {
                    // Сброс позиции и масштаба именной таблички к значениям по умолчанию (они странные, но ладно)
                    shapeshifterPanel.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    shapeshifterPanel.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            }

            __instance.potentialVictims.Add(shapeshifterPanel);

            selectableElements.Add(shapeshifterPanel.Button);
        }

        ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, selectableElements, false);

        PlayerPickMenu.isActive = false;

        return false; // Пропустить оригинальный метод, когда активно

    }
}

[HarmonyPatch(typeof(ShapeshifterPanel), nameof(ShapeshifterPanel.SetPlayer))]
public static class ShapeshifterPanel_SetPlayer
{
    // Префикс-патч ShapeshifterPanel.SetPlayer для разрешения использования PlayerPickMenu в лобби
    public static bool Prefix(ShapeshifterPanel __instance, int index, NetworkedPlayerInfo playerInfo, Il2CppSystem.Action onShift)
    {
        if (!PlayerPickMenu.isActive) return true; // Открыть обычное меню оборотня, если не активно

        __instance.shapeshift = onShift;

        __instance.PlayerIcon.SetFlipX(false);
        __instance.PlayerIcon.ToggleName(false);

        SpriteRenderer[] componentsInChildren = __instance.GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in componentsInChildren)
        {
            spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, index + 2);
        }

        __instance.PlayerIcon.SetMaskLayer(index + 2);
        __instance.PlayerIcon.UpdateFromEitherPlayerDataOrCache(playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null);

        __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);

        // Пропускает использование пользовательских табличек с именами, потому что они ломают PlayerPickMenu в лобби

        __instance.NameText.text = playerInfo.PlayerName;

        DataManager.Settings.Accessibility.OnColorBlindModeChanged += (Il2CppSystem.Action)__instance.SetColorblindText;
        __instance.SetColorblindText();

        return false; // Пропускает оригинальный метод, когда активно
    }
}
