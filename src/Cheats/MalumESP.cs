using UnityEngine;
using Sentry.Internal.Extensions;

namespace MalumMenu;
public static class MalumESP
{
    private static bool _freecamActive;
    private static bool _resolutionChangeNeeded;
    public static void SporeCloudVision(Mushroom mushroom)
    {
        if (CheatToggles.noShadows)
        {
            // Изменение позиции по оси Z грибных облаков, чтобы игроки отображались над ними

            mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, -1);
            return;
        }

        // Нормальная позиция по оси Z: 5f
        mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, 5f);
    }

    public static bool IsFullbrightActive()
    {
        // Полная яркость автоматически активируется при отдалении камеры, наблюдении за другими игроками или "свободной камере"
        // Это сделано для избежания проблем с тенями

        return CheatToggles.noShadows || Camera.main.orthographicSize > 3f || Camera.main.gameObject.GetComponent<FollowerCamera>().Target != PlayerControl.LocalPlayer;
    }

    public static void ZoomOut(HudManager hudManager)
    {
        if (CheatToggles.zoomOut)
        {
            if (hudManager.Chat.IsOpenOrOpening || PlayerCustomizationMenu.Instance || (Utils.isLobby && (FriendsListUI.Instance.IsOpen ||
                GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.active || GameStartManager.Instance.RulesEditPanel))) return;

            _resolutionChangeNeeded = true;

            if (Input.GetAxis("Mouse ScrollWheel") < 0f ) // Отдалить
            {

                // Нужно настроить как основную камеру, так и камеру UI

                Camera.main.orthographicSize++;
                hudManager.UICamera.orthographicSize++;

                // Utils.AdjustResolution(), похоже, требуется для правильной синхронизации UI игры
                // после изменения orthographicSize

                Utils.AdjustResolution();

            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0f )
            {
                // Приблизить
                if (!(Camera.main.orthographicSize > 3f)) return; // Никогда не опускаться ниже стандартного orthographicSize: 3f

                Camera.main.orthographicSize--;
                hudManager.UICamera.orthographicSize--;

                Utils.AdjustResolution();
            }
        }
        else
        {
            // orthographicSize сбрасывается до стандартного значения: 3f
            Camera.main.orthographicSize = 3f;
            hudManager.UICamera.orthographicSize = 3f;

            // Utils.AdjustResolution() вызывается в последний раз для предотвращения проблем с UI
            if (_resolutionChangeNeeded)
            {
                Utils.AdjustResolution();
                _resolutionChangeNeeded = false;
            }
        }
    }

    public static void MeetingNametags(MeetingHud meetingHud)
    {
        try
        {
            foreach (var playerState in meetingHud.playerStates)
            {
                // Получение NetworkedPlayerInfo для каждого playerState
                var data = GameData.Instance.GetPlayerById(playerState.TargetPlayerId);

                if (data.IsNull() || data.Disconnected || data.Outfits[PlayerOutfitType.Default].IsNull()) continue;

                // Соответствующее обновление таблички с именем игрока
                playerState.NameText.text = Utils.GetNameTag(data, data.DefaultOutfit.PlayerName);

                // Перемещение и изменение размера таблички с именем для предотвращения перекрытия с текстом для дальтоников
                if (CheatToggles.seeRoles && CheatToggles.seePlayerInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                    playerState.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }
                else if (CheatToggles.seeRoles || CheatToggles.seePlayerInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
                else
                {
                    // Сброс позиции и масштаба таблички с именем к значениям по умолчанию (они странные, но ладно)
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            }
        } catch { }
    }

    public static void PlayerNametags(PlayerPhysics playerPhysics)
    {
        try
        {
            playerPhysics.myPlayer.cosmetics.SetName(Utils.GetNameTag(playerPhysics.myPlayer.Data, playerPhysics.myPlayer.CurrentOutfit.PlayerName));
            // Перемещение текста имени вверх для предотвращения перекрытия с текстом для дальтоников
            if (CheatToggles.seeRoles && CheatToggles.seePlayerInfo)
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.186f, 0f);
            }
            else if (CheatToggles.seeRoles || CheatToggles.seePlayerInfo)
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.093f, 0f);
            }
            else
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
        } catch { }
    }

    public static void ChatNametags(ChatBubble chatBubble)
    {
        try
        {
            // Соответствующее обновление таблички с именем игрока
            chatBubble.NameText.text = Utils.GetNameTag(chatBubble.playerInfo, chatBubble.NameText.text, true);

            // Регулировка размера пузыря чата под новую табличку с именем для предотвращения проблем
            chatBubble.NameText.ForceMeshUpdate(true, true);
            chatBubble.Background.size = new Vector2(5.52f, 0.2f + chatBubble.NameText.GetNotDumbRenderedHeight() + chatBubble.TextArea.GetNotDumbRenderedHeight());
            chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);

        } catch { }
    }

    public static void SeeGhostsCheat(PlayerPhysics playerPhysics)
    {
        try{

            if(playerPhysics.myPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                playerPhysics.myPlayer.Visible = CheatToggles.seeGhosts;
            }

        }catch{}
    }

    public static void FreecamCheat()
    {
        if (CheatToggles.freecam)
        {
            // Полное отключение FollowerCamera
            if (!_freecamActive)
            {

                Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = false;
                Camera.main.gameObject.GetComponent<FollowerCamera>().Target = null;

                _freecamActive = true;

            }

            // Предотвращение движения игрока во время свободной камеры
            PlayerControl.LocalPlayer.moveable = false;

            // Получение ввода с клавиатуры
            var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);

            // Изменение позиции камеры в зависимости от ввода с клавиатуры
            // Скорость: 10f
            Camera.main.transform.position = Camera.main.transform.position + movement * 10f * Time.deltaTime;

        }
        else
        {
            // Повторное включение FollowerCamera и движения после отключения свободной камеры
            if (!_freecamActive) return;
            PlayerControl.LocalPlayer.moveable = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            _freecamActive = false;
        }
    }
}
