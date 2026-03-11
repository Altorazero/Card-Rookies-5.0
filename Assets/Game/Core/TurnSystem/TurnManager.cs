using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Менеджер ходов. Регистрирует команды и управляет порядком ходов.
/// Ход передаётся от команды к команде в порядке регистрации.
///
/// Начало и конец хода — события (TurnStartEvent / TurnEndEvent).
/// Все методы имеют параметр <c>fireEvent</c> для возможности завершения хода без событий.
/// </summary>
public class TurnManager
{
    private readonly List<Geid> _teamOrder = new();
    private int _currentTeamIndex = 0;
    private int _turnNumber = 0;
    private readonly EventQueue _eventQueue;
    private readonly Geid _systemId = Geid.New;
    private bool _battleStarted = false;

    /// <summary>Команда, чей ход сейчас активен. Geid.Empty, если бой не начат.</summary>
    public Geid CurrentTeamId => _battleStarted && _teamOrder.Count > 0
        ? _teamOrder[_currentTeamIndex]
        : Geid.Empty;

    /// <summary>Текущий номер хода.</summary>
    public int TurnNumber => _turnNumber;

    /// <summary>Зарегистрированные команды в порядке регистрации.</summary>
    public IReadOnlyList<Geid> TeamOrder => _teamOrder;

    public TurnManager(EventQueue eventQueue)
    {
        _eventQueue = eventQueue;
    }

    /// <summary>
    /// Регистрирует команду. Порядок регистрации определяет порядок ходов.
    /// Команды регистрируются до начала боя.
    /// </summary>
    public void RegisterTeam(Geid teamId)
    {
        if (!_teamOrder.Contains(teamId))
            _teamOrder.Add(teamId);
        else
            Debug.LogWarning($"[TurnManager] Team {teamId} is already registered.");
    }

    /// <summary>
    /// Начинает бой: генерирует BattleStartEvent и передаёт ход первой зарегистрированной команде.
    /// </summary>
    /// <param name="fireEvents">Если false, события не генерируются.</param>
    public void StartBattle(bool fireEvents = true)
    {
        if (_teamOrder.Count == 0)
        {
            Debug.LogWarning("[TurnManager] Cannot start battle: no teams registered.");
            return;
        }
        _battleStarted = true;
        _currentTeamIndex = 0;
        _turnNumber = 1;

        if (fireEvents)
        {
            _eventQueue.Enqueue(new BattleStartEvent(_systemId));
            _eventQueue.Enqueue(new TurnStartEvent(_systemId, CurrentTeamId, _turnNumber));
        }

        Debug.Log($"[TurnManager] Battle started. Turn {_turnNumber}, team {CurrentTeamId}.");
    }

    /// <summary>
    /// Завершает свой ход. Работает только если <paramref name="teamId"/> — текущая команда.
    /// Нельзя завершить чужой ход этим методом.
    /// </summary>
    /// <param name="teamId">Команда, завершающая свой ход.</param>
    /// <param name="fireEvents">Если false, события TurnEnd/TurnStart не генерируются.</param>
    public void EndTurn(Geid teamId, bool fireEvents = true)
    {
        if (!_battleStarted)
        {
            Debug.LogWarning("[TurnManager] Battle has not started yet.");
            return;
        }
        if (CurrentTeamId != teamId)
        {
            Debug.LogWarning($"[TurnManager] Team {teamId} cannot end turn: it is not their turn (current: {CurrentTeamId}).");
            return;
        }
        AdvanceTurn(fireEvents);
    }

    /// <summary>
    /// Принудительно завершает ход команды <paramref name="teamId"/>, даже если сейчас не её ход.
    /// Если это текущая команда, ход передаётся следующей. 
    /// Если это не текущая команда, текущий ход всё равно завершается и ход передаётся 
    /// команде, следующей после <paramref name="teamId"/> в порядке регистрации.
    /// </summary>
    /// <param name="teamId">Команда, чей ход принудительно завершается.</param>
    /// <param name="fireEvents">Если false, события не генерируются.</param>
    public void ForceEndTurn(Geid teamId, bool fireEvents = true)
    {
        if (!_battleStarted)
        {
            Debug.LogWarning("[TurnManager] Battle has not started yet.");
            return;
        }

        int targetIndex = _teamOrder.IndexOf(teamId);
        if (targetIndex < 0)
        {
            Debug.LogWarning($"[TurnManager] Team {teamId} is not registered.");
            return;
        }

        Geid endingTeam = _teamOrder[_currentTeamIndex];
        if (fireEvents)
            _eventQueue.Enqueue(new TurnEndEvent(_systemId, endingTeam, _turnNumber));

        // Переходим к команде, следующей после teamId
        _currentTeamIndex = (targetIndex + 1) % _teamOrder.Count;
        _turnNumber++;

        if (fireEvents)
            _eventQueue.Enqueue(new TurnStartEvent(_systemId, CurrentTeamId, _turnNumber));

        Debug.Log($"[TurnManager] Force-ended turn of team {endingTeam}. Turn {_turnNumber}, now team {CurrentTeamId}.");
    }

    /// <summary>
    /// Завершает ход команды <paramref name="fromTeamId"/> и передаёт ход конкретной команде
    /// <paramref name="toTeamId"/>, минуя обычный порядок.
    /// </summary>
    /// <param name="fromTeamId">Команда, завершающая ход (должна быть текущей).</param>
    /// <param name="toTeamId">Команда, которой передаётся ход.</param>
    /// <param name="fireEvents">Если false, события не генерируются.</param>
    public void EndTurnAndPassTo(Geid fromTeamId, Geid toTeamId, bool fireEvents = true)
    {
        if (!_battleStarted)
        {
            Debug.LogWarning("[TurnManager] Battle has not started yet.");
            return;
        }
        if (CurrentTeamId != fromTeamId)
        {
            Debug.LogWarning($"[TurnManager] Team {fromTeamId} cannot end turn: it is not their turn (current: {CurrentTeamId}).");
            return;
        }
        int toIndex = _teamOrder.IndexOf(toTeamId);
        if (toIndex < 0)
        {
            Debug.LogWarning($"[TurnManager] Target team {toTeamId} is not registered.");
            return;
        }

        if (fireEvents)
            _eventQueue.Enqueue(new TurnEndEvent(_systemId, fromTeamId, _turnNumber));

        _currentTeamIndex = toIndex;
        _turnNumber++;

        if (fireEvents)
            _eventQueue.Enqueue(new TurnStartEvent(_systemId, CurrentTeamId, _turnNumber));

        Debug.Log($"[TurnManager] Team {fromTeamId} ended turn. Passed to {toTeamId}. Turn {_turnNumber}.");
    }
}
