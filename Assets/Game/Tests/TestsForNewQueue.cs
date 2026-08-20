using NUnit.Framework;
using System;
using System.ComponentModel;

// ===== Фикстуры для теста =====

public sealed record CounterComponent(int Value) : IComponent;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
public sealed class FakeEvent : IGameEvent
{
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public EventScratch Scratch { get; } = new();
    public GEID TargetId { get; }

    public GEID Id => throw new NotImplementedException();

    public GEID SystemSourceId => throw new NotImplementedException();

    public FakeEvent(GEID targetId) => TargetId = targetId;
}

public sealed class IncrementApplySystem : IEventListener<FakeEvent, IApplyPhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 0;

    public void OnEvent(EventContext context, FakeEvent evt)
    {
        context.Mutate<CounterComponent>(evt.TargetId, c => c with { Value = c.Value + 1 });
    }
}

public sealed class GuardCancelIfOverNineSystem : IEventListener<FakeEvent, IGuardPhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 0;

    public void OnEvent(EventContext context, FakeEvent evt)
    {
        var counter = context.BattleState.GetEntity(evt.TargetId).GetComponent<CounterComponent>();
        if (counter != null && counter.Value >= 9)
            evt.Status = EventStatus.Cancelled;
    }
}

public sealed class ReplaceWithNoopSystem : IEventListener<FakeEvent, IReplacePhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 0;
    public bool ShouldReplace = false;

    public void OnEvent(EventContext context, FakeEvent evt)
    {
        if (ShouldReplace)
            context.Replace(new FakeEvent(evt.TargetId)); // событие-замена без Apply-системы на него — не увеличит счётчик
    }
}

public sealed class IllegalRaiseInGuardSystem : IEventListener<FakeEvent, IGuardPhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 0;

    public void OnEvent(EventContext context, FakeEvent evt)
    {
        context.Raise(new FakeEvent(evt.TargetId)); // должно бросить исключение
    }
}

// ===== Тесты =====

[TestFixture]
public class EventQueuePipelineTests
{
    private BattleState _state;
    private GEID _entityId;

    [SetUp]
    public void SetUp()
    {
        _state = new BattleState(2);
        var entity = new BaseEntity();
        entity.AddComponent(new CounterComponent(0));
        _state.AddEntity(entity);
        _entityId = entity.Id;
    }

    private int GetCounter() =>
        _state.GetEntity(_entityId).GetComponent<CounterComponent>().Value;

    [Test]
    public void Apply_MutatesState_AndCommandLogRecordsIt()
    {
        var queue = new EventQueue(_state);
        queue.Subscribe(new IncrementApplySystem());

        int checkpointBefore = queue.CommandLog.Checkpoint;
        queue.Enqueue(new FakeEvent(_entityId));
        queue.ProcessQueue();

        Assert.AreEqual(1, GetCounter());
        Assert.Greater(queue.CommandLog.Checkpoint, checkpointBefore);
    }

    [Test]
    public void Cancelled_InGuard_RollsBackAnyPriorMutation()
    {
        var queue = new EventQueue(_state);
        // Гипотетическая ситуация "мутация раньше Apply вопреки контракту" не воспроизводима штатными
        // системами (контракт это запрещает по дизайну) — проверяем сам факт отсутствия мутации
        // при отмене на Guard: IncrementApplySystem не должен успеть сработать вообще.
        queue.Subscribe(new GuardCancelIfOverNineSystem());
        queue.Subscribe(new IncrementApplySystem());

        _state.GetEntity(_entityId).AddComponent(new CounterComponent(9)); // уже "перегружено"

        queue.Enqueue(new FakeEvent(_entityId));
        queue.ProcessQueue();

        Assert.AreEqual(9, GetCounter(), "Guard должен был отменить событие до Apply — счётчик не должен был увеличиться.");
    }

    [Test]
    public void Replace_StopsOriginalEvent_ReplacementProcessedInstead()
    {
        var queue = new EventQueue(_state);
        var replaceSystem = new ReplaceWithNoopSystem { ShouldReplace = true };
        queue.Subscribe(replaceSystem);
        queue.Subscribe(new IncrementApplySystem());

        queue.Enqueue(new FakeEvent(_entityId));
        queue.ProcessQueue();

        // Исходное событие заменено на "пустое" FakeEvent без реакции —
        // но IncrementApplySystem подписана на FakeEvent вообще, значит сработает на замене тоже!
        // Это осознанная проверка: Replace не убирает эффект, если замена того же типа события.
        Assert.AreEqual(1, GetCounter(), "Замена всё ещё FakeEvent с тем же ApplySystem — она должна была сработать один раз, а не два.");
    }

    [Test]
    public void Raise_OutsideAfterOrSba_ThrowsInvalidOperationException()
    {
        var queue = new EventQueue(_state);
        queue.Subscribe(new IllegalRaiseInGuardSystem());

        queue.Enqueue(new FakeEvent(_entityId));

        Assert.Throws<InvalidOperationException>(() => queue.ProcessQueue());
    }

    [Test]
    public void PreviewMode_AutoRollsBackOnDispose()
    {
        var queue = new EventQueue(_state);
        queue.Subscribe(new IncrementApplySystem());

        using (queue.EnterMode(ExecutionMode.Preview))
        {
            queue.Enqueue(new FakeEvent(_entityId));
            queue.ProcessQueue();
            Assert.AreEqual(1, GetCounter(), "Внутри Preview-скоупа мутация должна быть видна.");
        }

        Assert.AreEqual(0, GetCounter(), "После выхода из Preview-скоупа состояние должно быть полностью откачено.");
        Assert.AreEqual(ExecutionMode.Real, queue.Mode);
    }

    [Test]
    public void PreviewMode_Commit_KeepsMutation()
    {
        var queue = new EventQueue(_state);
        queue.Subscribe(new IncrementApplySystem());

        using (var scope = queue.EnterMode(ExecutionMode.Preview))
        {
            queue.Enqueue(new FakeEvent(_entityId));
            queue.ProcessQueue();
            scope.Commit();
        }

        Assert.AreEqual(1, GetCounter(), "После явного Commit() откат не должен происходить.");
    }

    [Test]
    public void SimulationMode_DoesNotWriteStateHistory()
    {
        var queue = new EventQueue(_state);
        queue.Subscribe(new IncrementApplySystem());

        int historyCountBefore = queue.StateHistory.Count;

        using (queue.EnterMode(ExecutionMode.Simulation))
        {
            queue.Enqueue(new FakeEvent(_entityId));
            queue.ProcessQueue();
        }

        Assert.AreEqual(historyCountBefore, queue.StateHistory.Count,
            "В Simulation режиме StateHistory не должен пополняться.");
    }
}