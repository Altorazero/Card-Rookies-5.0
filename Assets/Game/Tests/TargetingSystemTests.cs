using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Полный набор тестов для системы таргетинга (TargetingSystem + TargetingSpec).
/// Покрывает: фильтры, приоритеты, количество целей, поведение при нехватке целей, роли.
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
        var entity = new BaseEntity();
        entity.AddComponent(new HealthComponent(hp));
        if (hex.HasValue)
            entity.AddComponent(new HexComponent(hex.Value));
        state.AddEntity(entity);
        return entity;
    }

    /// <summary>
    /// Минимальное событие-заглушка, реализующее INeedTargeting.
    /// </summary>
    private class TestTargetingEvent :
        IGameEvent, IHaveSubjects,
        ITargetResolvePhaseEvent,
        IGuardPhaseEvent,
        IApplyPhaseEvent,
        INeedTargeting
    {
        public EventStatus Status { get; set; } = EventStatus.Pending;
        public Geid Id { get; } = Geid.New;
        public Geid SystemSourceId { get; }
        public List<List<Geid>> Subjects { get; set; }
        public ITargetingSpec TargetingSpec { get; set; }

        public TestTargetingEvent(Geid sourceId, ITargetingSpec spec)
        {
            SystemSourceId = sourceId;
            TargetingSpec = spec;
            Subjects = SubjectsHelper.Create((SubjectRole.Source, sourceId));
        }
    }

    // -----------------------------------------------------------------------
    // 1. No targeting spec → event is not fizzled
    // -----------------------------------------------------------------------

    [Test]
    public void NoTargetingSpec_EventContinuesNormally()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state);

        var evt = new TestTargetingEvent(source.Id, null);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreNotEqual(EventStatus.Fizzled, evt.Status,
            "Event without targeting spec should not be fizzled.");
        Debug.Log("NoTargetingSpec_EventContinuesNormally passed.");
    }

    // -----------------------------------------------------------------------
    // 2. TargetingType.None → no candidates, event fizzles (MinTargets = 1)
    // -----------------------------------------------------------------------

    [Test]
    public void TargetingTypeNone_NoTargetsFound_Fizzles()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state);

        var spec = new TargetingSpec
        {
            Type = TargetingType.None,
            MinTargets = 1,
            MaxTargets = 1,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Fizzled, evt.Status,
            "TargetingType.None should yield 0 candidates → fizzle when MinTargets = 1.");
        Debug.Log("TargetingTypeNone_NoTargetsFound_Fizzles passed.");
    }

    // -----------------------------------------------------------------------
    // 3. Empty filter list → all candidates accepted
    // -----------------------------------------------------------------------

    [Test]
    public void NoFilters_AllEntitiesSelected()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state);
        var e1 = AddEntity(state);
        var e2 = AddEntity(state);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 0,
            MaxTargets = TargetCount.All,
            TargetRole = SubjectRole.Target,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target);
        Assert.AreEqual(3, targets.Count,
            "Without filters all 3 entities should be selected.");
        Debug.Log("NoFilters_AllEntitiesSelected passed.");
    }

    // -----------------------------------------------------------------------
    // 4. Single filter selects only matching entities
    // -----------------------------------------------------------------------

    [Test]
    public void SingleFilter_OnlyMatchingEntitiesSelected()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);
        var lowHp = AddEntity(state, hp: 10);
        var highHp = AddEntity(state, hp: 80);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 0,
            MaxTargets = TargetCount.All,
            TargetRole = SubjectRole.Target,
        }.AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThanOrEqual, 20));

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count, "Only the low-HP entity should pass the filter.");
        Assert.IsTrue(targets.Contains(lowHp.Id), "Low-HP entity should be in targets.");
        Assert.IsFalse(targets.Contains(highHp.Id), "High-HP entity should NOT be in targets.");
        Debug.Log("SingleFilter_OnlyMatchingEntitiesSelected passed.");
    }

    // -----------------------------------------------------------------------
    // 5. Multiple filters with AND logic
    // -----------------------------------------------------------------------

    [Test]
    public void MultipleFilters_AndLogic_OnlyBothConditionsMet()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);

        // Entity A: HP 30 — passes both (<= 50 AND >= 20)
        var entityA = AddEntity(state, hp: 30);
        // Entity B: HP 10 — passes first but not second (< 20)
        var entityB = AddEntity(state, hp: 10);
        // Entity C: HP 60 — fails first (> 50)
        var entityC = AddEntity(state, hp: 60);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 0,
            MaxTargets = TargetCount.All,
            TargetRole = SubjectRole.Target,
        }
        .AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThanOrEqual, 50))
        .AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.GreaterThanOrEqual, 20));

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count, "Only entityA satisfies both filters.");
        Assert.IsTrue(targets.Contains(entityA.Id), "entityA (HP=30) should be selected.");
        Assert.IsFalse(targets.Contains(entityB.Id), "entityB (HP=10) should NOT be selected.");
        Assert.IsFalse(targets.Contains(entityC.Id), "entityC (HP=60) should NOT be selected.");
        Debug.Log("MultipleFilters_AndLogic_OnlyBothConditionsMet passed.");
    }

    // -----------------------------------------------------------------------
    // 6. SelfTargetFilter selects only the source entity
    // -----------------------------------------------------------------------

    [Test]
    public void SelfFilter_SelectsOnlySourceEntity()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state);
        var other = AddEntity(state);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = source.Id,
        }.AddFilter(new SelfTargetFilter());

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count, "SelfFilter should select exactly 1 target.");
        Assert.AreEqual(source.Id, targets[0], "Selected target should be the source entity.");
        Debug.Log("SelfFilter_SelectsOnlySourceEntity passed.");
    }

    // -----------------------------------------------------------------------
    // 7. Priority: HighestHp selects entity with most HP
    // -----------------------------------------------------------------------

    [Test]
    public void PriorityHighestHp_SelectsEntityWithMostHp()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 1);
        var lowHp = AddEntity(state, hp: 10);
        var midHp = AddEntity(state, hp: 50);
        var highHp = AddEntity(state, hp: 90);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.HighestHp,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(highHp.Id, targets[0],
            "HighestHp priority should select the entity with 90 HP.");
        Debug.Log("PriorityHighestHp_SelectsEntityWithMostHp passed.");
    }

    // -----------------------------------------------------------------------
    // 8. Priority: LowestHp selects entity with least HP
    // -----------------------------------------------------------------------

    [Test]
    public void PriorityLowestHp_SelectsEntityWithLeastHp()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);
        var lowHp = AddEntity(state, hp: 5);
        var midHp = AddEntity(state, hp: 50);
        var highHp = AddEntity(state, hp: 90);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.LowestHp,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(lowHp.Id, targets[0],
            "LowestHp priority should select the entity with 5 HP.");
        Debug.Log("PriorityLowestHp_SelectsEntityWithLeastHp passed.");
    }

    // -----------------------------------------------------------------------
    // 9. Priority: Nearest selects closest entity by hex distance
    // -----------------------------------------------------------------------

    [Test]
    public void PriorityNearest_SelectsClosestEntityByHex()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hex: new HexCoordinates(0, 0));
        var near = AddEntity(state, hex: new HexCoordinates(1, 0));   // distance 1
        var far = AddEntity(state, hex: new HexCoordinates(5, 0));    // distance 5

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.Nearest,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = source.Id,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count);
        Assert.AreEqual(near.Id, targets[0],
            "Nearest priority should select the entity at distance 1, not 5.");
        Debug.Log("PriorityNearest_SelectsClosestEntityByHex passed.");
    }

    // -----------------------------------------------------------------------
    // 10. Priority: Random — same seed produces same result
    // -----------------------------------------------------------------------

    [Test]
    public void PriorityRandom_SameSeedProducesSameResult()
    {
        // Run twice with the same seed; results must match
        Geid[] RunWithSeed(int seed)
        {
            var state = new BattleState(seed);
            var queue = new EventQueue(state);
            queue.Subscribe(new TargetingSystem());

            var source = AddEntity(state);
            for (int i = 0; i < 5; i++)
                AddEntity(state);

            var spec = new TargetingSpec
            {
                Type = TargetingType.Entity,
                Priority = TargetPriority.Random,
                MinTargets = 1,
                MaxTargets = 3,
                TargetRole = SubjectRole.Target,
            };
            var evt = new TestTargetingEvent(source.Id, spec);
            queue.Enqueue(evt);
            queue.ProcessQueue();
            return evt.GetSubjects(SubjectRole.Target).ToArray();
        }

        var run1 = RunWithSeed(9999);
        var run2 = RunWithSeed(9999);

        Assert.AreEqual(3, run1.Length, "Should select 3 targets.");
        Assert.AreEqual(run1.Length, run2.Length, "Both runs should return same count.");
        for (int i = 0; i < run1.Length; i++)
            Assert.AreEqual(run1[i], run2[i], $"Target at index {i} should match across runs.");
        Debug.Log("PriorityRandom_SameSeedProducesSameResult passed.");
    }

    // -----------------------------------------------------------------------
    // 11. MaxTargets limits the selection
    // -----------------------------------------------------------------------

    [Test]
    public void MaxTargets_LimitsSelection()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state);
        for (int i = 0; i < 5; i++)
            AddEntity(state);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 0,
            MaxTargets = 2,
            TargetRole = SubjectRole.Target,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target);
        Assert.AreEqual(2, targets.Count, "MaxTargets = 2 should select exactly 2 entities.");
        Debug.Log("MaxTargets_LimitsSelection passed.");
    }

    // -----------------------------------------------------------------------
    // 12. MaxTargets = TargetCount.All selects every valid candidate
    // -----------------------------------------------------------------------

    [Test]
    public void MaxTargetsAll_SelectsAllValidCandidates()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state);
        AddEntity(state); AddEntity(state); AddEntity(state);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 0,
            MaxTargets = TargetCount.All,
            TargetRole = SubjectRole.Target,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target);
        Assert.AreEqual(4, targets.Count,
            "TargetCount.All should select all 4 entities (source + 3 others).");
        Debug.Log("MaxTargetsAll_SelectsAllValidCandidates passed.");
    }

    // -----------------------------------------------------------------------
    // 13. MinTargets = 0 allows empty target list without fizzle
    // -----------------------------------------------------------------------

    [Test]
    public void MinTargetsZero_EmptyResultAllowed()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);

        // Filter that accepts nothing
        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 0,
            MaxTargets = TargetCount.All,
            TargetRole = SubjectRole.Target,
        }.AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0));

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreNotEqual(EventStatus.Fizzled, evt.Status,
            "MinTargets = 0 should allow 0 results without fizzling.");
        Assert.AreEqual(0, evt.GetSubjects(SubjectRole.Target).Count,
            "Target list should be empty when nothing passes the filter.");
        Debug.Log("MinTargetsZero_EmptyResultAllowed passed.");
    }

    // -----------------------------------------------------------------------
    // 14. InsufficientTargets: Cancel → EventStatus.Fizzled
    // -----------------------------------------------------------------------

    [Test]
    public void InsufficientTargets_Cancel_EventFizzles()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);
        // No entity passes hp < 0

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            MinTargets = 1,
            MaxTargets = 1,
            OnInsufficientTargets = InsufficientTargetsBehavior.Cancel,
        }.AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0));

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Fizzled, evt.Status,
            "Cancel behavior should set status to Fizzled when targets are insufficient.");
        Debug.Log("InsufficientTargets_Cancel_EventFizzles passed.");
    }

    // -----------------------------------------------------------------------
    // 15. InsufficientTargets: UseFound → proceeds with partial target list
    // -----------------------------------------------------------------------

    [Test]
    public void InsufficientTargets_UseFound_ProceedsWithPartialTargets()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);
        var lowHp = AddEntity(state, hp: 5);
        // Only 1 entity passes hp <= 10, but MinTargets = 2

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 2,
            MaxTargets = TargetCount.All,
            TargetRole = SubjectRole.Target,
            OnInsufficientTargets = InsufficientTargetsBehavior.UseFound,
        }.AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThanOrEqual, 10));

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreNotEqual(EventStatus.Fizzled, evt.Status,
            "UseFound should not fizzle the event.");
        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(1, targets.Count, "Should proceed with 1 found target.");
        Assert.IsTrue(targets.Contains(lowHp.Id), "Found target should be the low-HP entity.");
        Debug.Log("InsufficientTargets_UseFound_ProceedsWithPartialTargets passed.");
    }

    // -----------------------------------------------------------------------
    // 16. InsufficientTargets: ShootVoid → event continues, Subjects empty
    // -----------------------------------------------------------------------

    [Test]
    public void InsufficientTargets_ShootVoid_EventContinuesWithNoTargets()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            OnInsufficientTargets = InsufficientTargetsBehavior.ShootVoid,
        }.AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0));

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreNotEqual(EventStatus.Fizzled, evt.Status,
            "ShootVoid should not fizzle the event.");
        Assert.AreNotEqual(EventStatus.Cancelled, evt.Status,
            "ShootVoid should not cancel the event.");
        Assert.AreEqual(0, evt.GetSubjects(SubjectRole.Target).Count,
            "ShootVoid should leave Subjects empty.");
        Debug.Log("InsufficientTargets_ShootVoid_EventContinuesWithNoTargets passed.");
    }

    // -----------------------------------------------------------------------
    // 17. InsufficientTargets: AlternativeEffect → original cancelled, alt dispatched
    // -----------------------------------------------------------------------

    [Test]
    public void InsufficientTargets_AlternativeEffect_AltEventDispatched()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);

        IGameEvent capturedAlt = null;

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            MinTargets = 1,
            MaxTargets = 1,
            OnInsufficientTargets = InsufficientTargetsBehavior.AlternativeEffect,
            AlternativeEffectFactory = ctx =>
            {
                var alt = new TestTargetingEvent(source.Id, null);
                capturedAlt = alt;
                return alt;
            },
        }.AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0));

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreEqual(EventStatus.Cancelled, evt.Status,
            "Original event should be Cancelled when AlternativeEffect is triggered.");
        Assert.IsNotNull(capturedAlt, "AlternativeEffectFactory should have been called.");
        Debug.Log("InsufficientTargets_AlternativeEffect_AltEventDispatched passed.");
    }

    // -----------------------------------------------------------------------
    // 18. AlternativeEffect with null factory → original cancelled, no crash
    // -----------------------------------------------------------------------

    [Test]
    public void InsufficientTargets_AlternativeEffectNullFactory_OriginalCancelled()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 100);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            MinTargets = 1,
            MaxTargets = 1,
            OnInsufficientTargets = InsufficientTargetsBehavior.AlternativeEffect,
            AlternativeEffectFactory = null,
        }.AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0));

        var evt = new TestTargetingEvent(source.Id, spec);
        Assert.DoesNotThrow(() =>
        {
            queue.Enqueue(evt);
            queue.ProcessQueue();
        }, "Null factory should not throw an exception.");
        Assert.AreEqual(EventStatus.Cancelled, evt.Status,
            "Original event should be Cancelled even with null factory.");
        Debug.Log("InsufficientTargets_AlternativeEffectNullFactory_OriginalCancelled passed.");
    }

    // -----------------------------------------------------------------------
    // 19. Targets placed in correct custom role (PrimaryTarget)
    // -----------------------------------------------------------------------

    [Test]
    public void TargetRole_PrimaryTarget_TargetsPlacedCorrectly()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state);
        var target = AddEntity(state);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.PrimaryTarget,
            SourceEntity = source.Id,
        }.AddFilter(new SelfTargetFilter());  // selects source

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var primaryTargets = evt.GetSubjects(SubjectRole.PrimaryTarget).ToList();
        var regularTargets = evt.GetSubjects(SubjectRole.Target).ToList();

        Assert.AreEqual(1, primaryTargets.Count,
            "One entity should be placed in PrimaryTarget role.");
        Assert.AreEqual(0, regularTargets.Count,
            "Regular Target role should remain empty.");
        Assert.AreEqual(source.Id, primaryTargets[0]);
        Debug.Log("TargetRole_PrimaryTarget_TargetsPlacedCorrectly passed.");
    }

    // -----------------------------------------------------------------------
    // 20. Multiple targets selected and all placed in Subjects
    // -----------------------------------------------------------------------

    [Test]
    public void MultipleTargets_AllPlacedInSubjects()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state);
        var e1 = AddEntity(state);
        var e2 = AddEntity(state);
        var e3 = AddEntity(state);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 3,
            MaxTargets = 3,
            TargetRole = SubjectRole.Target,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(3, targets.Count, "Exactly 3 targets should be selected.");
        Debug.Log("MultipleTargets_AllPlacedInSubjects passed.");
    }

    // -----------------------------------------------------------------------
    // 21. HighestHp priority selects top-N when MaxTargets > 1
    // -----------------------------------------------------------------------

    [Test]
    public void PriorityHighestHp_TopN_CorrectOrder()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hp: 1);
        var hp10 = AddEntity(state, hp: 10);
        var hp50 = AddEntity(state, hp: 50);
        var hp90 = AddEntity(state, hp: 90);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.HighestHp,
            MinTargets = 2,
            MaxTargets = 2,
            TargetRole = SubjectRole.Target,
        };
        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        var targets = evt.GetSubjects(SubjectRole.Target).ToList();
        Assert.AreEqual(2, targets.Count, "Should select 2 targets.");
        Assert.IsTrue(targets.Contains(hp90.Id), "HP=90 should be selected.");
        Assert.IsTrue(targets.Contains(hp50.Id), "HP=50 should be selected.");
        Assert.IsFalse(targets.Contains(hp10.Id), "HP=10 should NOT be selected.");
        Debug.Log("PriorityHighestHp_TopN_CorrectOrder passed.");
    }

    // -----------------------------------------------------------------------
    // 22. Nearest priority: no source entity → falls back to First order
    // -----------------------------------------------------------------------

    [Test]
    public void PriorityNearest_NoSourceEntity_FallsBackToFirst()
    {
        var (state, queue) = CreateQueue();
        var source = AddEntity(state, hex: new HexCoordinates(0, 0));
        var near = AddEntity(state, hex: new HexCoordinates(1, 0));
        var far = AddEntity(state, hex: new HexCoordinates(5, 0));

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.Nearest,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = null,  // no source — should not crash
        };
        Assert.DoesNotThrow(() =>
        {
            var evt = new TestTargetingEvent(source.Id, spec);
            queue.Enqueue(evt);
            queue.ProcessQueue();
        }, "Nearest without SourceEntity should not throw.");
        Debug.Log("PriorityNearest_NoSourceEntity_FallsBackToFirst passed.");
    }

    // -----------------------------------------------------------------------
    // 23. Nearest priority: source entity has no HexComponent → falls back
    // -----------------------------------------------------------------------

    [Test]
    public void PriorityNearest_SourceHasNoHex_FallsBackGracefully()
    {
        var (state, queue) = CreateQueue();
        // Source has NO HexComponent
        var source = new BaseEntity();
        source.AddComponent(new HealthComponent(100));
        state.AddEntity(source);

        var near = AddEntity(state, hex: new HexCoordinates(1, 0));

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.Nearest,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = source.Id,
        };
        Assert.DoesNotThrow(() =>
        {
            var evt = new TestTargetingEvent(source.Id, spec);
            queue.Enqueue(evt);
            queue.ProcessQueue();
        }, "Nearest with source having no HexComponent should not throw.");
        Debug.Log("PriorityNearest_SourceHasNoHex_FallsBackGracefully passed.");
    }

    // -----------------------------------------------------------------------
    // 24. TargetCount.All is respected even when entities = 0
    // -----------------------------------------------------------------------

    [Test]
    public void MaxTargetsAll_EmptyBattlefield_ZeroTargets()
    {
        var (state, queue) = CreateQueue();
        // Source is the only entity; filter excludes it
        var source = AddEntity(state, hp: 100);

        var spec = new TargetingSpec
        {
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 0,
            MaxTargets = TargetCount.All,
            TargetRole = SubjectRole.Target,
        }.AddFilter(new IMetricLevelTargetFilter<HealthComponent>(ComparisonOperator.LessThan, 0));

        var evt = new TestTargetingEvent(source.Id, spec);
        queue.Enqueue(evt);
        queue.ProcessQueue();

        Assert.AreNotEqual(EventStatus.Fizzled, evt.Status);
        Assert.AreEqual(0, evt.GetSubjects(SubjectRole.Target).Count);
        Debug.Log("MaxTargetsAll_EmptyBattlefield_ZeroTargets passed.");
    }

    // -----------------------------------------------------------------------
    // 25. Verify TargetingSpec.AddFilter returns this (fluent chain)
    // -----------------------------------------------------------------------

    [Test]
    public void TargetingSpec_AddFilter_FluentChainingWorks()
    {
        var filter1 = new AlwaysValidFilter();
        var filter2 = new SelfTargetFilter();

        var spec = new TargetingSpec()
            .AddFilter(filter1)
            .AddFilter(filter2);

        Assert.AreEqual(2, spec.Filters.Count, "Two filters should be registered.");
        Assert.AreSame(filter1, spec.Filters[0]);
        Assert.AreSame(filter2, spec.Filters[1]);
        Debug.Log("TargetingSpec_AddFilter_FluentChainingWorks passed.");
    }
}
