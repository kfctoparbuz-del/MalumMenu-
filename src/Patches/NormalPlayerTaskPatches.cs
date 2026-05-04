using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
public static class NormalPlayerTask_Initialize
{
    // Постфикс-патч NormalPlayerTask.Initialize для создания стрелок для задач, у которых их нет
    public static void Postfix(NormalPlayerTask __instance)
    {
        // Настройка цели стрелки для задачи UploadData на Airship отдельно
        if (__instance.TaskType == TaskTypes.UploadData && (MapNames)Utils.GetCurrentMapID() == MapNames.Airship)
        {
            if (__instance.taskStep == 0)
            {
                var airshipTask = __instance.GetComponent<AirshipUploadTask>();
                var consolePositions = airshipTask.FindValidConsolesPositions();

                // AirshipUploadTask использует массив Arrows[] вместо унаследованного поля Arrow
                for (var i = 0; i < consolePositions.Count && i < airshipTask.Arrows.Length; i++)
                {
                    // Уже есть две существующие стрелки, нам просто нужно установить цель одной из них на шаге 0
                    airshipTask.Arrows[i].target = consolePositions[i];
                }

                airshipTask.LocationDirty = true;

                return;
            }
        }

        ArrowHandler.EnsureArrowExists(__instance);

        if (!ArrowHandler.NeedsSpecialTarget(__instance))
        {
            __instance.UpdateArrowAndLocation();
        }
        else if (ArrowHandler.IsOwnedAndIncomplete(__instance))
        {
            ArrowHandler.SetArrowTargetForSpecialTasks(__instance);
        }
    }
}

[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
public static class NormalPlayerTask_FixedUpdate
{
    // Постфикс-патч NormalPlayerTask.FixedUpdate для управления видимостью стрелок
    public static void Postfix(NormalPlayerTask __instance)
    {
        if (__instance.Arrow == null) return;

        if (!CheatToggles.taskArrows)
        {
            // Скрыть стрелки, если taskStep == 0 (стандартное поведение)
            if (__instance.taskStep == 0)
            {
                __instance.Arrow.gameObject.SetActive(false);
            }

            return;
        }

        if (ArrowHandler.IsOwnedAndIncomplete(__instance))
        {
            if (ArrowHandler.NeedsSpecialTarget(__instance))
            {
                ArrowHandler.SetArrowTargetForSpecialTasks(__instance);
            }

            __instance.Arrow.gameObject.SetActive(true);
        }
    }
}

[HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.FixedUpdate))]
public static class AirshipUploadTask_FixedUpdate_Patch
{
    // Постфикс-патч AirshipUploadTask.FixedUpdate для сохранения видимости стрелок, когда taskStep == 0 или активен саботаж связи
    public static void Postfix(AirshipUploadTask __instance)
    {
        // Этот патч, к сожалению, необходим, потому что AirshipUploadTask переопределяет NormalPlayerTask.FixedUpdate
        if (__instance.Arrows == null) return;

        if (!CheatToggles.taskArrows)
        {
            // Деактивировать все стрелки, если taskStep == 0 (стандартное поведение)
            if (__instance.taskStep != 0) return;

            foreach (var arrow in __instance.Arrows)
            {
                arrow.gameObject.SetActive(false);
            }

            return;
        }

        var consolePositions = __instance.FindValidConsolesPositions();

        for (var i = 0; i < __instance.Arrows.Length; i++)
        {
            // Активировать только стрелки, соответствующие действительным позициям консолей
            __instance.Arrows[i].gameObject.SetActive(i < consolePositions.Count && __instance.Owner != null && __instance.Owner.AmOwner);
        }
    }
}
