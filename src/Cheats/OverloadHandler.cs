using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace MalumMenu;
public static class OverloadHandler
{
    public static float cooldown;
    public static int strength;
    private static HashSet<int> _customTargets = new();
    private static float _timer;
    private static float _attackLogTimer;
    private static Dictionary<int, int> _rpcCounters = new();
    private static int _nextTarget = int.MinValue;
    private static bool _hasRun;

    public static void Run()
    {
        if (!CheatToggles.runOverload || OverloadUI.currentTargets.Count <= 0)
        {
            _timer = cooldown;
            _attackLogTimer = MalumMenu.attackLogDelay.Value;
            _nextTarget = int.MinValue;
            _hasRun = false;
            _rpcCounters.Clear();
            return;
        }

        _timer += Time.unscaledDeltaTime;
        _attackLogTimer += Time.unscaledDeltaTime;

        if (_timer >= cooldown)
        {
            // Если выбраны все возможные цели...

            if (OverloadUI.maxPossibleTargets == OverloadUI.currentTargets.Count)
            {
                int broadcastId = -1; // ... эффективнее рассылать RPC широковещательно

                Utils.Overload(broadcastId, strength);
                _timer -= cooldown;

                if (CheatToggles.olLogAttack)
                {
                    string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                    if (!CheatToggles.olVerboseLogs)
                    {
                        _rpcCounters.TryAdd(broadcastId, 0);

                        _rpcCounters.TryGetValue(broadcastId, out var rpcCount);

                        int newRpcCount = rpcCount + strength;

                        // Логировать количество отправленных RPC с момента последнего лога
                        // Логирование происходит не чаще одного раза за интервал attackLogDelay (в секундах)

                        if (_attackLogTimer >= MalumMenu.attackLogDelay.Value)
                        {
                            OverloadUI.LogConsole($"> <b><color=#{colorStr}>Отправлено {newRpcCount} некорректных RPC всем игрокам (ID : {broadcastId})</color></b>");

                            _attackLogTimer -= MalumMenu.attackLogDelay.Value;

                            _rpcCounters.Clear();
                        }
                        else
                        {
                            _rpcCounters[broadcastId] = newRpcCount;
                        }
                    }
                    else // Логировать каждую отдельную отправку при использовании подробного логирования
                    {
                        OverloadUI.LogConsole($"> <b><color=#{colorStr}>Отправлено {strength} некорректных RPC всем игрокам (ID : {broadcastId})</color></b>");
                    }
                }

                return;
            }

            var currentTargets = OverloadUI.currentTargets;

            foreach (NetworkedPlayerInfo targetData in currentTargets)
            {
                int clientId = targetData.ClientId;

                if (!_hasRun)
                {
                    if (_nextTarget == int.MinValue || clientId == _nextTarget) // Нет отмеченной цели (новый цикл) ИЛИ clientId является отмеченной целью
                    {
                        Utils.Overload(clientId, strength);
                        _timer -= cooldown;

                        if (CheatToggles.olLogAttack)
                        {
                            string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                            if (!CheatToggles.olVerboseLogs)
                            {
                                _rpcCounters.TryAdd(clientId, 0);

                                _rpcCounters.TryGetValue(clientId, out var rpcCount);

                                int newRpcCount = rpcCount + strength;

                                _rpcCounters[clientId] = newRpcCount;
                            }
                            else // Логировать каждую отдельную отправку при использовании подробного логирования
                            {
                                OverloadUI.LogConsole($"> <b><color=#{colorStr}>Отправлено {strength} некорректных RPC {targetData.DefaultOutfit.PlayerName} (ID : {clientId})</color></b>");
                            }
                        }

                        _hasRun = true; // Отметить, что атака была выполнена в этой итерации
                    }
                }
                else // Если атака уже была выполнена в этой итерации...
                    // (всегда будет предыдущий игрок последовательно)
                {
                    // Отметить текущего игрока как цель для следующей итерации
                    _nextTarget = clientId;
                    _hasRun = false;

                    // Завершить, чтобы следующая итерация могла начаться сразу после задержки
                    return;
                }
            }

            // После полной итерации (цикл завершён)...

            // ... (1) Сбросить состояние, чтобы начать цикл заново с первого игрока

            _nextTarget = int.MinValue;
            _hasRun = false;

            // ... (2) Логировать количество отправленных RPC с момента последнего лога для всех currentTargets
            // Логирование происходит не чаще одного раза за интервал attackLogDelay (в секундах)

            if (!CheatToggles.olVerboseLogs)
            {
                if (_attackLogTimer >= MalumMenu.attackLogDelay.Value)
                {
                    string colorStr = ColorUtility.ToHtmlStringRGB(Palette.Orange);

                    foreach (KeyValuePair<int, int> entry in _rpcCounters)
                    {
                        int clientId = entry.Key;
                        int rpcCount = entry.Value;

                        NetworkedPlayerInfo playerData = OverloadUI.currentTargets.FirstOrDefault(pd => pd.ClientId == clientId);

                        if (playerData != null)
                        {
                            OverloadUI.LogConsole($"> <b><color=#{colorStr}>Отправлено {rpcCount} некорректных RPC {playerData.DefaultOutfit.PlayerName} (ID : {clientId})</color></b>");
                        }
                    }

                    _attackLogTimer -= MalumMenu.attackLogDelay.Value;

                    _rpcCounters.Clear();
                }
            }
            else
            {
                _rpcCounters.Clear();
            }
        }
    }

    public static void AddCustomTarget(NetworkedPlayerInfo playerData)
    {
        int clientId = playerData.ClientId;
        _customTargets.Add(clientId);
    }

    public static void RemoveCustomTarget(NetworkedPlayerInfo playerData)
    {
        int clientId = playerData.ClientId;
        _customTargets.Remove(clientId);
    }

    public static bool IsCustomTarget(NetworkedPlayerInfo playerData)
    {
        return _customTargets.Contains(playerData.ClientId);
    }

    public static (HashSet<TargetType> targetTypes, bool isTarget) GetTarget(NetworkedPlayerInfo playerData)
    {
        bool isTarget = false;
        var targetTypes = new HashSet<TargetType>();

        if (CheatToggles.overloadAll)
        {
            targetTypes.Add(TargetType.All);
            isTarget = true;
        }

        bool hostTarget = CheatToggles.overloadHost && AmongUsClient.Instance.HostId == playerData.ClientId;
        if (hostTarget)
        {
            targetTypes.Add(TargetType.Host);
            isTarget = true;
        }

        if (playerData.Role != null)
        {
            RoleTeamTypes roleTeamType = playerData.Role.TeamType;

            bool crewTarget = CheatToggles.overloadCrew && roleTeamType.Equals(RoleTeamTypes.Crewmate);
            if (crewTarget)
            {
                targetTypes.Add(TargetType.Crewmate);
                isTarget = true;
            }

            bool impTarget = CheatToggles.overloadImps && roleTeamType.Equals(RoleTeamTypes.Impostor);
            if (impTarget)
            {
                targetTypes.Add(TargetType.Impostor);
                isTarget = true;
            }
        }

        bool customTarget = IsCustomTarget(playerData);
        if (customTarget)
        {
            targetTypes.Add(TargetType.Custom);
            isTarget = true;
        }

        if (!isTarget)
        {
            targetTypes.Add(TargetType.None);
        }

        return (targetTypes, isTarget);
    }

    public static void ClearCustomTargets()
    {
        _customTargets.Clear();
    }

    // Проходит по всем указанным игрокам и
    // добавляет всех, кто отмечен как цели и соответствует targetType, в _customTargets
    public static void PopulateCustomTargets(PlayerControl[] players, TargetType targetType)
    {
        int playerCount = players.Length;

        for (int i = 0; i < playerCount; i++)
        {
            NetworkedPlayerInfo playerData = players[i].Data;
            var playerTarget = GetTarget(playerData);
            bool isTarget = playerTarget.isTarget;

            if (isTarget && !IsCustomTarget(playerData))
            {
                HashSet<TargetType> currentTargetTypes = playerTarget.targetTypes;
                if (currentTargetTypes.Contains(targetType))
                {
                    AddCustomTarget(playerData);
                }
            }
        }
    }

    // Возвращает адаптированные силу и задержку, используя количество currentTargets и пинг AmongUsClient
    // Должен балансировать их для достижения низкой задержки, но эффективной отдачи
    public static (int strength, float cooldown) CalculateAdaptedValues()
    {
        int targetCount = OverloadUI.maxPossibleTargets == OverloadUI.currentTargets.Count
                        ? 1 // Широковещательный режим считается как одна цель
                        : Math.Max(1, OverloadUI.currentTargets.Count); // Предотвращает деление на ноль

        float maxCooldown = MalumMenu.adaptMaxCooldown.Value;
        float cooldown = maxCooldown / targetCount;

        int pingLevel = Math.Max(1, Utils.GetPing() / 100); // 0-99 мс = Ур. 1, 100-199 мс = Ур. 2, 200-299 мс = Ур. 3, ...

        int maxStrength = MalumMenu.adaptMaxStrength.Value;
        int strength = Math.Max(1, maxStrength / pingLevel / targetCount);

        return (strength, cooldown);
    }

    public enum TargetType
    {
        None,
        All,
        Custom,
        Host,
        Impostor,
        Crewmate
    }
}
