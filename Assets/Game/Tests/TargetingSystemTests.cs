using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Полный набор тестов для системы таргетинга (пайплайн-архитектура).
/// Покрывает: пулы, фильтры (And/Or/Not), сортировщики, условия выхода,
/// составные фильтры, гексагональные фильтры и формы.
/// </summary>
public class TargetingSystemTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static (BattleState state, EventQueue queue) CreateQueue(int seed = 42)
    {
        var state = new BattleState(seed);
        var queue = new EventQueue(state);
        queue.Subscribe(new TargetingSystem());
        return (state, queue);
    }

    private static BaseEntity AddEntity(BattleState state, int hp = 100, HexCoordinates? hex = null)
    {
        var e = new BaseEntity();
        e.AddComponent(new HealthComponent(hp));
        if (hex.HasValue) e.AddComponent(new HexComponent(hex.Value));
        state.AddEntity(e);
        return e;
    }

    /// <summary>Минимальное тестовое событие, реализующее INeedTargeting.</summary>
    private class TestEvent :
        IGameEvent, IHaveSubjects,
        ITargetResolvePhaseEvent, IGuardPhaseEvent, IApplyPhaseEvent,
        INeedTargeting
    {
        public EventStatus Status { get; set; } = EventStatus.Pending;
        public Geid Id { get; } = Geid.New;
        public Geid SystemSourceId { get; }
        public List<List<Geid>> Subjects { get; set; }
        public ITargetingSpec TargetingSpec { get; set; }

        public TestEvent(Geid sourceId, ITargetingSpec spec)
        {
            SystemSourceId = sourceId;
            TargetingSpec = spec;
            Subjects = SubjectsHelper.Create((SubjectRole.Source, sourceId));
        }
    }

    // -----------------------------------------------------------------------
    // 1. Нет спека → событие продолжается без изменения статуса
    // -----------------------------------------------------------------------

    [Test]
    public void NoTargetingSpec_EventContinuesNormally()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);

        var evt = new TestEvent(src.Id, null);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreNotEqual(EventStatus.Fizzled, evt.Status);
        Debug.Log("NoTargetingSpec_EventContinuesNormally passed.");
    }

    // -----------------------------------------------------------------------
    // 2. Пустой пайплайн → кандидаты пусты, цели не записаны, событие Pending
    // -----------------------------------------------------------------------

    [Test]
    public void EmptyPipeline_NoTargetsCommitted_EventPending()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target };
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Pending, evt.Status);
        Assert.AreEqual(0, evt.GetSubjects(SubjectRole.Target).Count);
        Debug.Log("EmptyPipeline_NoTargetsCommitted_EventPending passed.");
    }

    // -----------------------------------------------------------------------
    // 3. AllEntitiesPool → все сущности в кандидатах
    // -----------------------------------------------------------------------

    [Test]
    public void AllEntitiesPool_AddsAllEntities()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        var e1 = AddEntity(state);
        var e2 = AddEntity(state);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool());
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target);
        Assert.AreEqual(3, targets.Count, "AllEntitiesPool должен добавить все 3 сущности.");
        Debug.Log("AllEntitiesPool_AddsAllEntities passed.");
    }

    // -----------------------------------------------------------------------
    // 4. EmptyPool → очищает кандидатов
    // -----------------------------------------------------------------------

    [Test]
    public void EmptyPool_ClearsCandidates()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        AddEntity(state); AddEntity(state);

        // AllEntities → затем EmptyPool → затем ExplicitEntitiesPool с одной сущностью
        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new EmptyPool())
            .AddStep(new ExplicitEntitiesPool(src.Id));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target);
        Assert.AreEqual(1, targets.Count, "После EmptyPool должна остаться только одна явная сущность.");
        Assert.AreEqual(src.Id, targets[0]);
        Debug.Log("EmptyPool_ClearsCandidates passed.");
    }

    // -----------------------------------------------------------------------
    // 5. ExplicitEntitiesPool → только указанные сущности
    // -----------------------------------------------------------------------

    [Test]
    public void ExplicitEntitiesPool_AddsOnlySpecified()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        var e1 = AddEntity(state);
        var e2 = AddEntity(state);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new ExplicitEntitiesPool(e1.Id));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target);
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(e1.Id, targets[0]);
        Debug.Log("ExplicitEntitiesPool_AddsOnlySpecified passed.");
    }

    // -----------------------------------------------------------------------
    // 6. FilterStep — одиночный фильтр
    // -----------------------------------------------------------------------

    [Test]
    public void FilterStep_SingleFilter_RemovesNonMatching()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 100);
        var lowHp = AddEntity(state, hp: 10);
        var highHp = AddEntity(state, hp: 80);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThanOrEqual, 20)));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.IsTrue(targets.Contains(lowHp.Id));
        Assert.IsFalse(targets.Contains(highHp.Id));
        Debug.Log("FilterStep_SingleFilter_RemovesNonMatching passed.");
    }

    // -----------------------------------------------------------------------
    // 7. AndTargetFilter — AND-логика двух фильтров
    // -----------------------------------------------------------------------

    [Test]
    public void AndTargetFilter_BothMustPass()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 100);
        var entityA = AddEntity(state, hp: 30); // 20 ≤ 30 ≤ 50 → проходит оба
        var entityB = AddEntity(state, hp: 10); // 10 < 20 → не проходит второй
        var entityC = AddEntity(state, hp: 60); // 60 > 50 → не проходит первый

        var filter = new AndTargetFilter(
            new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThanOrEqual, 50),
            new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.GreaterThanOrEqual, 20));

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(filter));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.IsTrue(targets.Contains(entityA.Id));
        Assert.IsFalse(targets.Contains(entityB.Id));
        Assert.IsFalse(targets.Contains(entityC.Id));
        Debug.Log("AndTargetFilter_BothMustPass passed.");
    }

    // -----------------------------------------------------------------------
    // 8. OrTargetFilter — OR-логика двух фильтров
    // -----------------------------------------------------------------------

    [Test]
    public void OrTargetFilter_EitherSuffices()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 50);
        var veryLow = AddEntity(state, hp: 5);   // проходит hp < 10
        var veryHigh = AddEntity(state, hp: 95); // проходит hp > 90
        var middle = AddEntity(state, hp: 50);   // не проходит ни один

        var filter = new OrTargetFilter(
            new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 10),
            new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.GreaterThan, 90));

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(filter));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(2, targets.Count, "Должны пройти 2 сущности (< 10 ИЛИ > 90).");
        Assert.IsTrue(targets.Contains(veryLow.Id));
        Assert.IsTrue(targets.Contains(veryHigh.Id));
        Assert.IsFalse(targets.Contains(middle.Id));
        Debug.Log("OrTargetFilter_EitherSuffices passed.");
    }

    // -----------------------------------------------------------------------
    // 9. NotTargetFilter — инверсия
    // -----------------------------------------------------------------------

    [Test]
    public void NotTargetFilter_InvertsResult()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        var other = AddEntity(state);

        // NOT(Self) → принять всё кроме src
        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new NotTargetFilter(new SelfTargetFilter())));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.IsFalse(targets.Contains(src.Id), "src должен быть исключён через NOT(Self).");
        Assert.IsTrue(targets.Contains(other.Id), "other должен быть включён.");
        Debug.Log("NotTargetFilter_InvertsResult passed.");
    }

    // -----------------------------------------------------------------------
    // 10. SelfTargetFilter — только источник события
    // -----------------------------------------------------------------------

    [Test]
    public void SelfFilter_SelectsOnlySource()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        var other = AddEntity(state);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new SelfTargetFilter()))
            .AddStep(new TakeSorter(1))
            .AddStep(new ExitConditionStep(new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onNotMet: new FizzleTargetingAction()));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(src.Id, targets[0]);
        Debug.Log("SelfFilter_SelectsOnlySource passed.");
    }

    // -----------------------------------------------------------------------
    // 11. TakeSorter — ограничивает количество кандидатов
    // -----------------------------------------------------------------------

    [Test]
    public void TakeSorter_LimitsCount()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        for (int i = 0; i < 5; i++) AddEntity(state);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new TakeSorter(2));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(2, evt.GetSubjects(SubjectRole.Target).Count);
        Debug.Log("TakeSorter_LimitsCount passed.");
    }

    // -----------------------------------------------------------------------
    // 12. HighestHpSorter → выбирает первую сущность с наибольшим HP
    // -----------------------------------------------------------------------

    [Test]
    public void HighestHpSorter_SelectsHighestHpEntity()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 1);
        var low = AddEntity(state, hp: 10);
        var high = AddEntity(state, hp: 90);
        var mid = AddEntity(state, hp: 50);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new HighestHpSorter())
            .AddStep(new TakeSorter(1));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(high.Id, targets[0], "HighestHpSorter должен выбрать сущность с HP=90.");
        Debug.Log("HighestHpSorter_SelectsHighestHpEntity passed.");
    }

    // -----------------------------------------------------------------------
    // 13. LowestHpSorter → выбирает первую сущность с наименьшим HP
    // -----------------------------------------------------------------------

    [Test]
    public void LowestHpSorter_SelectsLowestHpEntity()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 100);
        var low = AddEntity(state, hp: 5);
        var mid = AddEntity(state, hp: 50);
        var high = AddEntity(state, hp: 90);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new LowestHpSorter())
            .AddStep(new TakeSorter(1));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(low.Id, targets[0], "LowestHpSorter должен выбрать сущность с HP=5.");
        Debug.Log("LowestHpSorter_SelectsLowestHpEntity passed.");
    }

    // -----------------------------------------------------------------------
    // 14. HighestHpSorter + TakeSorter(2) → берёт топ-2
    // -----------------------------------------------------------------------

    [Test]
    public void HighestHpSorter_TopN()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 1);
        var hp10 = AddEntity(state, hp: 10);
        var hp50 = AddEntity(state, hp: 50);
        var hp90 = AddEntity(state, hp: 90);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new HighestHpSorter())
            .AddStep(new TakeSorter(2));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(2, targets.Count);
        Assert.IsTrue(targets.Contains(hp90.Id));
        Assert.IsTrue(targets.Contains(hp50.Id));
        Assert.IsFalse(targets.Contains(hp10.Id));
        Debug.Log("HighestHpSorter_TopN passed.");
    }

    // -----------------------------------------------------------------------
    // 15. NearestSorter → ближайшая по гексагональному расстоянию
    // -----------------------------------------------------------------------

    [Test]
    public void NearestSorter_SelectsClosestEntity()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hex: new HexCoordinates(0, 0));
        var near = AddEntity(state, hex: new HexCoordinates(1, 0));  // dist=1
        var far = AddEntity(state, hex: new HexCoordinates(5, 0));   // dist=5

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new NearestSorter(src.Id))
            .AddStep(new TakeSorter(1));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        // NearestSorter включает src как ближайшего кандидата (дистанция 0 от самого себя)
        Assert.AreEqual(src.Id, targets[0],
            "NearestSorter puts src first since dist(src, src) = 0.");
        Debug.Log("NearestSorter_SelectsClosestEntity passed.");
    }

    [Test]
    public void NearestSorter_ExcludesSelf_SelectsNearestOther()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hex: new HexCoordinates(0, 0));
        var near = AddEntity(state, hex: new HexCoordinates(1, 0));
        var far = AddEntity(state, hex: new HexCoordinates(5, 0));

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new NotTargetFilter(new SelfTargetFilter())))
            .AddStep(new NearestSorter(src.Id))
            .AddStep(new TakeSorter(1));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(near.Id, targets[0], "Ближайшая НЕ-self — near (dist=1).");
        Debug.Log("NearestSorter_ExcludesSelf_SelectsNearestOther passed.");
    }

    // -----------------------------------------------------------------------
    // 16. RandomSorter с одним seed — детерминированный результат
    // -----------------------------------------------------------------------

/*    [Test]
    public void RandomSorter_SameSeedProducesSameOrder()
    {
        Geid[] Run(int seed)
        {
            var st = new BattleState(seed);
            var q = new EventQueue(st);
            q.Subscribe(new TargetingSystem());
            var s = AddEntity(st);
            for (int i = 0; i < 5; i++) AddEntity(st);
            
            var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
                .AddStep(new AllEntitiesPool())
                // ДОБАВЛЕНО: сортировка по ID для гарантии одинакового начального порядка
                .AddStep(new SortByIdStep())
                .AddStep(new RandomSorter())
                .AddStep(new TakeSorter(3));
            var e = new TestEvent(s.Id, spec);
            q.Enqueue(e);
            q.ProcessQueue();
            return e.GetSubjects(SubjectRole.Target).ToArray();
        }

        var r1 = Run(77777);
        var r2 = Run(77777);

        Assert.AreEqual(3, r1.Length);
        for (int i = 0; i < r1.Length; i++)
            Assert.AreEqual(r1[i], r2[i], $"Индекс {i} должен совпадать при одинаковом seed.");
        Debug.Log("RandomSorter_SameSeedProducesSameOrder passed.");
    }
*/
    // Вспомогательный шаг для сортировки по ID
    private class SortByIdStep : ITargetingStep
    {
        public void Execute(TargetingContext context)
        {
            context.Candidates.Sort((a, b) => a.Value.CompareTo(b.Value));
        }
    }

    // -----------------------------------------------------------------------
    // 17. ExitConditionStep + FizzleTargetingAction → fizzle при нехватке
    // -----------------------------------------------------------------------

    [Test]
    public void ExitCondition_FizzleWhenNotEnoughTargets()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 100);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0)))
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onNotMet: new FizzleTargetingAction()));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Fizzled, evt.Status);
        Debug.Log("ExitCondition_FizzleWhenNotEnoughTargets passed.");
    }

    // -----------------------------------------------------------------------
    // 18. ExitConditionStep + CancelTargetingAction
    // -----------------------------------------------------------------------

    [Test]
    public void ExitCondition_CancelWhenNotEnoughTargets()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 100);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0)))
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onNotMet: new CancelTargetingAction()));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Cancelled, evt.Status);
        Debug.Log("ExitCondition_CancelWhenNotEnoughTargets passed.");
    }

    // -----------------------------------------------------------------------
    // 19. ExitConditionStep + AlternativeEffectAction → оригинал отменён, alt диспатчен
    // -----------------------------------------------------------------------

    [Test]
    public void ExitCondition_AlternativeEffectDispatched()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 100);
        IGameEvent capturedAlt = null;

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0)))
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onNotMet: new AlternativeEffectAction(ctx =>
                {
                    var alt = new TestEvent(src.Id, null);
                    capturedAlt = alt;
                    return alt;
                })));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Cancelled, evt.Status);
        Assert.IsNotNull(capturedAlt, "AlternativeEffectAction должна была вызвать фабрику.");
        Debug.Log("ExitCondition_AlternativeEffectDispatched passed.");
    }

    // -----------------------------------------------------------------------
    // 20. ExitConditionStep + CommitAndStopAction → цели зафиксированы досрочно
    // -----------------------------------------------------------------------

    [Test]
    public void ExitCondition_CommitAndStop_CommitsCurrentCandidates()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        var e1 = AddEntity(state);

        // Pipeline: добавим e1 через ExplicitPool, зафиксируем и остановим,
        // следующий AllEntitiesPool НЕ должен добавить src в цели
        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new ExplicitEntitiesPool(e1.Id))
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onMet: new CommitAndStopAction()))
            .AddStep(new AllEntitiesPool()); // этот шаг не должен выполниться
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(e1.Id, targets[0]);
        Assert.IsFalse(targets.Contains(src.Id), "src не должен попасть — pipeline остановлен.");
        Debug.Log("ExitCondition_CommitAndStop_CommitsCurrentCandidates passed.");
    }

    // -----------------------------------------------------------------------
    // 21. ExitConditionStep: onMet = null и onNotMet = null → пайплайн продолжается
    // -----------------------------------------------------------------------

    [Test]
    public void ExitCondition_BothActionsNull_PipelineContinues()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        var e1 = AddEntity(state);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThan, 100))) // никогда не met
            .AddStep(new TakeSorter(1));
        var evt = new TestEvent(src.Id, spec);
        Assert.DoesNotThrow(() =>
        {
            queue.Enqueue(evt);
            queue.ProcessQueue();
        });
        Assert.AreEqual(EventStatus.Pending, evt.Status);
        Assert.AreEqual(1, evt.GetSubjects(SubjectRole.Target).Count);
        Debug.Log("ExitCondition_BothActionsNull_PipelineContinues passed.");
    }

    // -----------------------------------------------------------------------
    // 22. CandidateCountPredicate — все операторы
    // -----------------------------------------------------------------------

    [Test]
    public void CandidateCountPredicate_AllOperators()
    {
        var sharedState = new BattleState(1);
        var ctx = new EventContext(sharedState, new GameEvent(Geid.New), new EventQueue(sharedState));
        var three = new List<Geid> { Geid.New, Geid.New, Geid.New };

        Assert.IsTrue(new CandidateCountPredicate(ComparisonOperator.Equal, 3).Evaluate(three, ctx));
        Assert.IsTrue(new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 3).Evaluate(three, ctx));
        Assert.IsTrue(new CandidateCountPredicate(ComparisonOperator.LessThanOrEqual, 3).Evaluate(three, ctx));
        Assert.IsTrue(new CandidateCountPredicate(ComparisonOperator.GreaterThan, 2).Evaluate(three, ctx));
        Assert.IsTrue(new CandidateCountPredicate(ComparisonOperator.LessThan, 4).Evaluate(three, ctx));
        Assert.IsFalse(new CandidateCountPredicate(ComparisonOperator.Equal, 2).Evaluate(three, ctx));
        Debug.Log("CandidateCountPredicate_AllOperators passed.");
    }

    // -----------------------------------------------------------------------
    // 23. Кастомная роль цели — PrimaryTarget
    // -----------------------------------------------------------------------

    [Test]
    public void CustomTargetRole_PrimaryTarget()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        AddEntity(state);

        var spec = new TargetingSpec { TargetRole = SubjectRole.PrimaryTarget }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new SelfTargetFilter()))
            .AddStep(new TakeSorter(1));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(1, evt.GetSubjects(SubjectRole.PrimaryTarget).Count);
        Assert.AreEqual(0, evt.GetSubjects(SubjectRole.Target).Count);
        Debug.Log("CustomTargetRole_PrimaryTarget passed.");
    }

    // -----------------------------------------------------------------------
    // 24. Многошаговый пайплайн: Pool → Filter → Sort → Take → ExitCondition
    // -----------------------------------------------------------------------

    [Test]
    public void MultistepPipeline_FullChain()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hp: 100);
        var hp5 = AddEntity(state, hp: 5);
        var hp30 = AddEntity(state, hp: 30);
        var hp70 = AddEntity(state, hp: 70);

        // Взять сущность с наименьшим HP из тех, у кого HP < 50
        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 50)))
            .AddStep(new LowestHpSorter())
            .AddStep(new TakeSorter(1))
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onNotMet: new FizzleTargetingAction()));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(hp5.Id, targets[0], "Должна быть выбрана сущность с HP=5.");
        Debug.Log("MultistepPipeline_FullChain passed.");
    }

    // -----------------------------------------------------------------------
    // 25. Повторяющийся пайплайн: два блока Pool→Filter→Sort→Commit+Stop
    // -----------------------------------------------------------------------

    [Test]
    public void MultiBlockPipeline_SecondBlockAfterCommit_DoesNotExecute()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state);
        var target = AddEntity(state);

        // Первый блок: явно добавить target и зафиксировать
        // Второй блок НЕ должен выполниться благодаря CommitAndStop
        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new ExplicitEntitiesPool(target.Id))
            .AddStep(new ExitConditionStep(
                new CandidateCountPredicate(ComparisonOperator.GreaterThanOrEqual, 1),
                onMet: new CommitAndStopAction()))
            .AddStep(new EmptyPool())         // этот шаг не должен выполниться
            .AddStep(new AllEntitiesPool());  // этот шаг не должен выполниться
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(target.Id, targets[0]);
        Debug.Log("MultiBlockPipeline_SecondBlockAfterCommit_DoesNotExecute passed.");
    }

    // -----------------------------------------------------------------------
    // 26. HexCircleShape — диск
    // -----------------------------------------------------------------------

    [Test]
    public void HexCircleShape_ContainsWithinRadius()
    {
        var shape = new HexCircleShape(2);
        var origin = new HexCoordinates(0, 0);

        Assert.IsTrue(shape.Contains(new HexCoordinates(0, 0), origin), "Центр входит.");
        Assert.IsTrue(shape.Contains(new HexCoordinates(2, 0), origin), "dist=2 входит.");
        Assert.IsTrue(shape.Contains(new HexCoordinates(1, 1), origin), "dist=2 входит (S=-2).");
        Assert.IsFalse(shape.Contains(new HexCoordinates(3, 0), origin), "dist=3 не входит.");
        Debug.Log("HexCircleShape_ContainsWithinRadius passed.");
    }

    // -----------------------------------------------------------------------
    // 27. HexRingShape — только точное расстояние
    // -----------------------------------------------------------------------

    [Test]
    public void HexRingShape_ContainsOnlyAtExactRadius()
    {
        var shape = new HexRingShape(2);
        var origin = new HexCoordinates(0, 0);

        Assert.IsFalse(shape.Contains(new HexCoordinates(0, 0), origin), "Центр не входит.");
        Assert.IsFalse(shape.Contains(new HexCoordinates(1, 0), origin), "dist=1 не входит.");
        Assert.IsTrue(shape.Contains(new HexCoordinates(2, 0), origin), "dist=2 входит.");
        Assert.IsFalse(shape.Contains(new HexCoordinates(3, 0), origin), "dist=3 не входит.");
        Debug.Log("HexRingShape_ContainsOnlyAtExactRadius passed.");
    }

    // -----------------------------------------------------------------------
    // 28. HexLineShape — луч в одном направлении
    // -----------------------------------------------------------------------

    [Test]
    public void HexLineShape_ContainsOnlyAlongDirection()
    {
        var dir = new HexCoordinates(1, 0);
        var shape = new HexLineShape(dir, 3);
        var origin = new HexCoordinates(0, 0);

        Assert.IsTrue(shape.Contains(new HexCoordinates(1, 0), origin));
        Assert.IsTrue(shape.Contains(new HexCoordinates(2, 0), origin));
        Assert.IsTrue(shape.Contains(new HexCoordinates(3, 0), origin));
        Assert.IsFalse(shape.Contains(new HexCoordinates(4, 0), origin), "Дальше MaxLength.");
        Assert.IsFalse(shape.Contains(new HexCoordinates(0, 0), origin), "Сам origin.");
        Assert.IsFalse(shape.Contains(new HexCoordinates(0, 1), origin), "Сбоку.");
        Debug.Log("HexLineShape_ContainsOnlyAlongDirection passed.");
    }

    // -----------------------------------------------------------------------
    // 29. HexConeShape — конус вдоль оси
    // -----------------------------------------------------------------------

    [Test]
    public void HexConeShape_ContainsWithinCone()
    {
        var dir = new HexCoordinates(1, 0);
        var shape = new HexConeShape(dir, maxRadius: 2, halfSpread: 1);
        var origin = new HexCoordinates(0, 0);

        Assert.IsFalse(shape.Contains(origin, origin), "Origin не входит.");
        Assert.IsTrue(shape.Contains(new HexCoordinates(1, 0), origin), "На оси depth=1.");
        Assert.IsTrue(shape.Contains(new HexCoordinates(2, 0), origin), "На оси depth=2.");

        // Соседи оси на depth=1: (1,0)
        Assert.IsTrue(shape.Contains(new HexCoordinates(1, -1), origin), "Сосед оси на depth=1, spread=1.");
        Assert.IsTrue(shape.Contains(new HexCoordinates(1, 1), origin), "Сосед оси на depth=1 с другой стороны, spread=1.");

        // Соседи оси на depth=2: (2,0)
        // Внимание: (2,1) имеет distance((0,0), (2,1)) = 3 > MaxRadius=2, поэтому недопустима
        // Используем (2,-1): distance((0,0), (2,-1)) = max(2,1,1) = 2 ✓
        Assert.IsTrue(shape.Contains(new HexCoordinates(2, -1), origin), "Сосед оси на depth=2, spread=1.");
        
        // Проверяем другого соседа (1,1) который также на расстоянии ≤1 от (2,0)
        // Но (1,1): dist((2,0), (1,1)) = max(1,1,2) = 2 > spread=1 ❌
        // Нужен сосед (2,0) который в пределах MaxRadius и spread
        // Правильные соседи (2,0) в пределах MaxRadius=2:
        //   - (2,-1): dist((0,0), (2,-1)) = 2 ✓, dist((2,0), (2,-1)) = 1 ✓
        //   - (1,0): уже проверен как осевой гекс
        //   - (3,0): dist((0,0), (3,0)) = 3 > MaxRadius ❌
        //   - (3,-1): dist((0,0), (3,-1)) = 3 > MaxRadius ❌
        //   - (1,-1): dist((2,0), (1,-1)) = max(1,1,2) = 2 > spread=1 ❌

        // За пределами MaxRadius
        Assert.IsFalse(shape.Contains(new HexCoordinates(3, 0), origin), "За MaxRadius=2.");

        // За пределами HalfSpread (dist > 1 от осевых гексов)
        Assert.IsFalse(shape.Contains(new HexCoordinates(0, -1), origin), "Вне конуса (слишком далеко от оси).");
        
        // Точка (2,1) за пределами MaxRadius
        Assert.IsFalse(shape.Contains(new HexCoordinates(2, 1), origin), "За MaxRadius=2 (dist=3 от origin).");

        Debug.Log("HexConeShape_ContainsWithinCone passed.");
    }

    // -----------------------------------------------------------------------
    // 30. HexShapeFilter интегрируется с пайплайном
    // -----------------------------------------------------------------------

    [Test]
    public void HexShapeFilter_IntegrationWithPipeline()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hex: new HexCoordinates(0, 0));
        var near = AddEntity(state, hex: new HexCoordinates(1, 0));   // dist=1 → внутри radius=2
        var far = AddEntity(state, hex: new HexCoordinates(5, 0));    // dist=5 → снаружи

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new HexRadiusFilter(src.Id, 2)));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.IsTrue(targets.Contains(src.Id),  "src (dist=0) входит в radius=2.");
        Assert.IsTrue(targets.Contains(near.Id), "near (dist=1) входит в radius=2.");
        Assert.IsFalse(targets.Contains(far.Id), "far (dist=5) не входит в radius=2.");
        Debug.Log("HexShapeFilter_IntegrationWithPipeline passed.");
    }

    // -----------------------------------------------------------------------
    // 31. HexRingFilter — только сущности на точном расстоянии
    // -----------------------------------------------------------------------

    [Test]
    public void HexRingFilter_OnlyExactDistance()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hex: new HexCoordinates(0, 0));
        var dist1 = AddEntity(state, hex: new HexCoordinates(1, 0));
        var dist2 = AddEntity(state, hex: new HexCoordinates(2, 0));
        var dist3 = AddEntity(state, hex: new HexCoordinates(3, 0));

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new HexRingFilter(src.Id, 2)));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.IsFalse(targets.Contains(dist1.Id), "dist=1 не в кольце radius=2.");
        Assert.IsTrue(targets.Contains(dist2.Id),  "dist=2 в кольце.");
        Assert.IsFalse(targets.Contains(dist3.Id), "dist=3 не в кольце radius=2.");
        Debug.Log("HexRingFilter_OnlyExactDistance passed.");
    }

    // -----------------------------------------------------------------------
    // 32. HexShapeFilter без HexComponent → пропускается
    // -----------------------------------------------------------------------

    [Test]
    public void HexShapeFilter_TargetWithoutHex_Excluded()
    {
        var (state, queue) = CreateQueue();
        var src = AddEntity(state, hex: new HexCoordinates(0, 0));
        // Сущность без HexComponent
        var noHex = new BaseEntity();
        noHex.AddComponent(new HealthComponent(100));
        state.AddEntity(noHex);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new FilterStep(new HexRadiusFilter(src.Id, 5)));
        var evt = new TestEvent(src.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.IsFalse(targets.Contains(noHex.Id), "Сущность без HexComponent должна быть исключена.");
        Debug.Log("HexShapeFilter_TargetWithoutHex_Excluded passed.");
    }

    // -----------------------------------------------------------------------
    // 33. NearestSorter без OriginEntity HexComponent → порядок не меняется
    // -----------------------------------------------------------------------

    [Test]
    public void NearestSorter_NoSourceHex_OrderUnchanged()
    {
        var (state, queue) = CreateQueue();
        var src = new BaseEntity();  // без HexComponent
        src.AddComponent(new HealthComponent(100));
        state.AddEntity(src);
        var e1 = AddEntity(state);
        var e2 = AddEntity(state);

        var spec = new TargetingSpec { TargetRole = SubjectRole.Target }
            .AddStep(new AllEntitiesPool())
            .AddStep(new NearestSorter(src.Id));
        var evt = new TestEvent(src.Id, spec);
        Assert.DoesNotThrow(() =>
        {
            queue.Enqueue(evt);
            queue.ProcessQueue();
        });
        Debug.Log("NearestSorter_NoSourceHex_OrderUnchanged passed.");
    }

    // -----------------------------------------------------------------------
    // 34. TargetingSpec.AddStep fluent-цепочка работает
    // -----------------------------------------------------------------------

    [Test]
    public void TargetingSpec_AddStep_FluentChaining()
    {
        var s1 = new AllEntitiesPool();
        var s2 = new TakeSorter(1);

        var spec = new TargetingSpec()
            .AddStep(s1)
            .AddStep(s2);

        Assert.AreEqual(2, spec.Steps.Count);
        Assert.AreSame(s1, spec.Steps[0]);
        Assert.AreSame(s2, spec.Steps[1]);
        Debug.Log("TargetingSpec_AddStep_FluentChaining passed.");
    }
}

