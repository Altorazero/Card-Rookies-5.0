using System;
using System.Collections.Generic;

public enum CardExecutionStatus { Running, Suspended, Completed, Cancelled }

public sealed class CardExecution
{
    public CardGraph Graph { get; }
    public ExecutionContext Context { get; }
    public CardNode CurrentNode { get; private set; }
    public CardExecutionStatus Status { get; private set; } = CardExecutionStatus.Running;
    public ISuspendPoint CurrentSuspendPoint { get; private set; }


    private readonly List<IGameEvent> _producedEvents = new();
    public IReadOnlyList<IGameEvent> ProducedEvents => _producedEvents;
    /// <summary>
    /// Узел использует это вместо EventContext.Raise, чтобы породить игровое
    /// последствие (урон, лечение и т.д.). Событие НЕ уходит в очередь немедленно —
    /// оно копится здесь и укладывается в EventQueue только когда весь граф
    /// розыгрыша карты завершён (см. CardExecution.Completed в PlayCardEventSystem).
    /// </summary>
    public void Emit(IGameEvent gameEvent) => _producedEvents.Add(gameEvent);

    public event Action<CardExecution> Suspended;
    public event Action<CardExecution> Resumed;
    public event Action<CardExecution> Completed;
    public event Action<CardExecution> Cancelled;

    private readonly NodeExecutorRegistry _registry;
    private readonly ICardExecutionHost _host;

    public CardExecution(CardGraph graph, ExecutionContext context, NodeExecutorRegistry registry, ICardExecutionHost host)
    {
        Graph = graph;
        Context = context;
        _registry = registry;
        _host = host;
        CurrentNode = graph.EntryNode;
    }

    public void Tick()
    {
        if (Status is CardExecutionStatus.Completed or CardExecutionStatus.Cancelled)
            return;

        Status = CardExecutionStatus.Running;

        while (CurrentNode != null)
        {
            var previousNode = CurrentNode;
            var executor = _registry.Get(CurrentNode);
            var outcome = executor(CurrentNode, this);

            switch (outcome.Kind)
            {
                case NodeOutcomeKind.Advance:
                    CurrentNode = outcome.Next;
                    break;

                case NodeOutcomeKind.Suspend:
                    Suspend(outcome.SuspendPoint);
                    return;

                case NodeOutcomeKind.Cancel:
                    Status = CardExecutionStatus.Cancelled;
                    Cancelled?.Invoke(this);
                    return;
            }

            if (CurrentNode == previousNode)
                throw new InvalidOperationException(
                    $"Infinite loop: node {previousNode.GetType().Name} advanced to itself.");
        }

        Status = CardExecutionStatus.Completed;
        Completed?.Invoke(this);
    }

    private void Suspend(ISuspendPoint point)
    {
        CurrentSuspendPoint = point;
        Status = CardExecutionStatus.Suspended;
        point.Resolved += OnSuspendResolved;
        point.Cancelled += OnSuspendCancelled;
        Suspended?.Invoke(this);
    }

    private void OnSuspendResolved()
    {
        DetachSuspendPoint();
        Resumed?.Invoke(this);
        _host.RequestContinue(this); // единственное место, откуда что-либо продолжает Tick снаружи
    }

    private void OnSuspendCancelled()
    {
        DetachSuspendPoint();
        Status = CardExecutionStatus.Cancelled;
        Cancelled?.Invoke(this);
        _host.RequestCancel(this);
    }

    private void DetachSuspendPoint()
    {
        CurrentSuspendPoint.Resolved -= OnSuspendResolved;
        CurrentSuspendPoint.Cancelled -= OnSuspendCancelled;
        CurrentSuspendPoint = null;
    }

    // Единая точка внешней отмены (игрок закрыл выбор карты руками и т.п.) —
    // вместо прямой мутации чужого состояния снаружи.
    public void RequestExternalCancel() => CurrentSuspendPoint?.Cancel();
}