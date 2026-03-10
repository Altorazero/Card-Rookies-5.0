using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewTestScript
{
    [Test]
    public void EventTest()
    {
        GameEvent @event = new(Geid.New);
        Debug.Log("Event id: " + @event.Id);
        Debug.Log("Event source id: " + @event.SystemSourceId);

        var targetingSpec = new TargetingSpec
        {
            Description = "Single target damage",
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target
        }.AddFilter(new AlwaysValidFilter());

        MassDamageEvent damageEvent = new(Geid.New, Geid.New, Geid.New, 10, targetingSpec);
        Debug.Log("DamageEvent id: " + damageEvent.Id);

        var target = damageEvent.GetFirstSubject(SubjectRole.Target);
        var source = damageEvent.GetFirstSubject(SubjectRole.Source);
        Debug.Log("Damage target id: " + (target == Geid.Empty ? "none" : target.ToString()));
        Debug.Log("Damage source id: " + (source == Geid.Empty ? "none" : source.ToString()));

        Debug.Log("Damage sys source id: " + damageEvent.SystemSourceId);
        Debug.Log("Damage amount: " + damageEvent.DamageAmount);
        Debug.Log("Damage type: " + damageEvent.DamageType);
    }

    [Test]
    public void EventEngineTest()
    {
        DamageSystem damageSystem = new();
        HealthSystem healthSystem = new();
        TargetingSystem targetingSystem = new();
        BattleState battleState = new BattleState(12);

        var sourceEntity = new BaseEntity();
        Debug.Log("Source entity id: " + sourceEntity.Id);
        battleState.AddEntity(sourceEntity);
        var targetEntity = new BaseEntity();
        Debug.Log("Target entity id: " + targetEntity.Id);
        battleState.AddEntity(targetEntity);

        var sourceHealth = new HealthComponent(100);
        sourceEntity.AddComponent(sourceHealth);
        var targetHealth = new HealthComponent(100);
        targetEntity.AddComponent(targetHealth);

        var sourcePower = new PowerComponent(50);
        sourceEntity.AddComponent(sourcePower);

        var singleTargetSpec = new TargetingSpec
        {
            Description = "Single target",
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        }.AddFilter(new AlwaysValidFilter());

        SingleDamageEvent damageEvent = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 80);
        SingleDamageEvent damageEvent1 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 30);
        SingleDamageEvent damageEvent2 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 20);

        EventQueue dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);
        dispatcher.Subscribe(damageSystem);
        dispatcher.Subscribe(healthSystem);

        dispatcher.Enqueue(damageEvent);
        dispatcher.Enqueue(damageEvent1);
        dispatcher.EnqueueWithBarrier(damageEvent2);
        dispatcher.ProcessQueue();
        Debug.Log("Processed damage event id: " + damageEvent.Id);
    }

    [Test]
    public void AdvancedEventEngineTest()
    {
        DamageSystem damageSystem = new();
        HealthSystem healthSystem = new();
        VampSystem vampSystem = new();
        ShieldSystem shieldSystem = new();
        TargetingSystem targetingSystem = new();
        BattleState battleState = new BattleState(12);

        var sourceEntity = new BaseEntity();
        Debug.Log("Source entity id: " + sourceEntity.Id);
        battleState.AddEntity(sourceEntity);
        var targetEntity = new BaseEntity();
        Debug.Log("Target entity id: " + targetEntity.Id);
        battleState.AddEntity(targetEntity);

        var sourceHealth = new HealthComponent(100);
        sourceEntity.AddComponent(sourceHealth);
        var targetHealth = new HealthComponent(100);
        targetEntity.AddComponent(targetHealth);

        var sourcePower = new PowerComponent(50);
        sourceEntity.AddComponent(sourcePower);

        var targetShield = new ShieldComponent(50);
        targetEntity.AddComponent(targetShield);

        var sourceVamp = new VampComponent(0.5f);
        sourceEntity.AddComponent(sourceVamp);

        var massTargetSpec = new TargetingSpec
        {
            Description = "Mass damage - single target",
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        }.AddFilter(new AlwaysValidFilter());

        MassDamageEvent damageEvent = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 80, massTargetSpec);
        MassDamageEvent damageEvent1 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 30, massTargetSpec);
        MassDamageEvent damageEvent2 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 20, massTargetSpec);

        EventQueue dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);
        dispatcher.Subscribe(damageSystem);
        dispatcher.Subscribe(shieldSystem);
        dispatcher.Subscribe(healthSystem);
        dispatcher.Subscribe(vampSystem);

        dispatcher.Enqueue(damageEvent);
        dispatcher.Enqueue(damageEvent1);
        dispatcher.EnqueueWithBarrier(damageEvent2);
        dispatcher.ProcessQueue();
        Debug.Log("Processed damage event id: " + damageEvent.Id);
    }

    [Test]
    public void CardGraphTest()
    {
        DamageSystem damageSystem = new();
        HealthSystem healthSystem = new();
        VampSystem vampSystem = new();
        ShieldSystem shieldSystem = new();
        ExecuteCardGraphSystem cardGraphSystem = new();
        TargetingSystem targetingSystem = new();
        BattleState battleState = new BattleState(12);

        EventQueue dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);
        dispatcher.Subscribe(damageSystem);
        dispatcher.Subscribe(shieldSystem);
        dispatcher.Subscribe(healthSystem);
        dispatcher.Subscribe(vampSystem);
        dispatcher.Subscribe(cardGraphSystem);

        var sourceEntity = new BaseEntity();
        Debug.Log("Source entity id: " + sourceEntity.Id);
        battleState.AddEntity(sourceEntity);
        var targetEntity = new BaseEntity();
        Debug.Log("Target entity id: " + targetEntity.Id);
        battleState.AddEntity(targetEntity);

        var sourceHealth = new HealthComponent(100);
        sourceEntity.AddComponent(sourceHealth);
        var targetHealth = new HealthComponent(100);
        targetEntity.AddComponent(targetHealth);
        var sourceMana = new ManaComponent(100);
        sourceEntity.AddComponent(sourceMana);

        var sourceHex = new HexComponent(new HexCoordinates(0, 0));
        sourceEntity.AddComponent(sourceHex);
        var targetHex = new HexComponent(new HexCoordinates(1, 0));
        targetEntity.AddComponent(targetHex);

        var damageTargetSpec = new TargetingSpec
        {
            Description = "Damage target",
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        }.AddFilter(new AlwaysValidFilter());

        var healTargetSpec = new TargetingSpec
        {
            Description = "Heal target",
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        }.AddFilter(new AlwaysValidFilter());

        var selfHealSpec = new TargetingSpec
        {
            Description = "Self heal",
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        }.AddFilter(new SelfTargetFilter());

        MassDamageEvent damageEvent = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 18, damageTargetSpec);
        MassDamageEvent damageEvent1 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 30, damageTargetSpec);
        MassDamageEvent damageEvent2 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 20, damageTargetSpec);

        var node1 = new CardGraphNode(new List<IGameEvent> { damageEvent });
        var node2 = new CardGraphNode(new List<IGameEvent> { new HealEventWithTargeting(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 50, healTargetSpec), damageEvent1, damageEvent2 });
        var node3 = new CardGraphNode(new List<IGameEvent> { new HealEventWithTargeting(sourceEntity.Id, sourceEntity.Id, sourceEntity.Id, 30, selfHealSpec) });
        var node4 = new CardGraphNode(new List<IGameEvent> { new HealEventWithTargeting(sourceEntity.Id, sourceEntity.Id, sourceEntity.Id, 20, selfHealSpec) });

        var predicate = new ManaLevelPredicate(ComparisonOperator.GreaterThanOrEqual, 30, sourceEntity.Id);
        var predicate2 = new HealthLevelPredicate(ComparisonOperator.LessThan, 900, sourceEntity.Id);

        node1.TieNode(node2, predicate);
        node1.TieNode(node3, predicate2);
        node3.TieNode(node1, predicate2);
        node3.TieNode(node4, predicate2);

        var playCardEvent = new ExecuteCardGraphEvent(sourceEntity.Id, node1);
        dispatcher.Enqueue(playCardEvent);
        dispatcher.ProcessQueue();
        Debug.Log("Processed PlayCardEvent id: " + playCardEvent.Id);
        Debug.Log("Source entity coordinates after card play: " + sourceEntity.GetComponent<HexComponent>()?.Coordinates.ToString());
    }

    [Test]
    public void RandomAndCoordinatesTest()
    {
        DamageSystem damageSystem = new();
        HealthSystem healthSystem = new();
        VampSystem vampSystem = new();
        ShieldSystem shieldSystem = new();
        ExecuteCardGraphSystem cardGraphSystem = new();
        MoveSystem moveSystem = new();
        RandomMovementSystem randomMovementSystem = new();
        RandomDamageSystem randomDamageSystem = new();
        TargetingSystem targetingSystem = new();
        int seed = Environment.TickCount;
        BattleState battleState = new BattleState(seed);
        Debug.Log("Battle RNG seed: " + seed);

        EventQueue dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);
        dispatcher.Subscribe(damageSystem);
        dispatcher.Subscribe(shieldSystem);
        dispatcher.Subscribe(healthSystem);
        dispatcher.Subscribe(vampSystem);
        dispatcher.Subscribe(cardGraphSystem);
        dispatcher.Subscribe(moveSystem);
        dispatcher.Subscribe(randomMovementSystem);
        dispatcher.Subscribe(randomDamageSystem);

        var sourceEntity = new BaseEntity();
        Debug.Log("Source entity id: " + sourceEntity.Id);
        battleState.AddEntity(sourceEntity);
        var targetEntity = new BaseEntity();
        Debug.Log("Target entity id: " + targetEntity.Id);
        battleState.AddEntity(targetEntity);

        var sourceHealth = new HealthComponent(100);
        sourceEntity.AddComponent(sourceHealth);
        var targetHealth = new HealthComponent(100);
        targetEntity.AddComponent(targetHealth);
        var sourceMana = new ManaComponent(100);
        sourceEntity.AddComponent(sourceMana);

        var sourceHex = new HexComponent(new HexCoordinates(0, 0));
        sourceEntity.AddComponent(sourceHex);
        var targetHex = new HexComponent(new HexCoordinates(1, 0));
        targetEntity.AddComponent(targetHex);

        RandomMovementEvent randomMovementEvent = new(sourceEntity.Id, sourceEntity.Id, 2);
        RandomMovementEvent randomMovementEvent1 = new(targetEntity.Id, targetEntity.Id, 2);
        RandomDamageEvent randomDamageEvent = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 1, 200);

        dispatcher.Enqueue(randomDamageEvent);
        dispatcher.Enqueue(randomMovementEvent);
        dispatcher.Enqueue(randomMovementEvent1);
        dispatcher.Enqueue(randomDamageEvent);

        dispatcher.ProcessQueue();

        Debug.Log("Source entity coordinates after move: " + sourceEntity.GetComponent<HexComponent>()?.Coordinates.ToString());
        Debug.Log("Target entity coordinates after move: " + targetEntity.GetComponent<HexComponent>()?.Coordinates.ToString());
    }

    [Test]
    public void TargetingTest()
    {
        int seed = 12345;
        BattleState battleState = new BattleState(seed);
        TargetingSystem targetingSystem = new();

        var source = new BaseEntity();
        battleState.AddEntity(source);

        var lowHp = new BaseEntity();
        var highHp = new BaseEntity();
        battleState.AddEntity(lowHp);
        battleState.AddEntity(highHp);

        lowHp.AddComponent(new HealthComponent(10));
        highHp.AddComponent(new HealthComponent(50));

        var healthFilter = new HealthThresholdFilter(20);

        var targetingSpec = new TargetingSpec
        {
            Description = "Target low health entities",
            Type = TargetingType.Entity,
            Priority = TargetPriority.First,
            MinTargets = 0,
            MaxTargets = TargetCount.All,
            TargetRole = SubjectRole.Target,
            SourceEntity = source.Id
        }.AddFilter(healthFilter);

        var damageEvent = new MassDamageEvent(source.Id, source.Id, Geid.Empty, 10, targetingSpec);

        EventQueue dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);

        dispatcher.Enqueue(damageEvent);
        dispatcher.ProcessQueue();

        var targets = damageEvent.GetSubjects(SubjectRole.Target).ToList();

        Assert.IsNotNull(targets, "Targets should not be null");
        Assert.AreEqual(1, targets.Count, "Expected one target with low health");
        Assert.IsTrue(targets.Contains(lowHp.Id), "Expected targets to contain lowHp.Id");
        Assert.IsFalse(targets.Contains(highHp.Id), "Expected targets NOT to contain highHp.Id");

        Debug.Log("TargetingPredicateTest passed.");
    }

    // ===== NEW TESTS FOR EventQueue FEATURES =====

    [Test]
    public void StateHistoryTest()
    {
        // Проверяем что снимки состояния создаются перед обработкой внешних событий
        BattleState battleState = new BattleState(42);
        var entity = new BaseEntity();
        entity.AddComponent(new HealthComponent(100));
        battleState.AddEntity(entity);

        EventQueue queue = new(battleState);
        HealthSystem healthSystem = new();
        DamageSystem damageSystem = new();
        queue.Subscribe(damageSystem);
        queue.Subscribe(healthSystem);

        var dmg1 = new SingleDamageEvent(entity.Id, entity.Id, entity.Id, 10);
        var dmg2 = new SingleDamageEvent(entity.Id, entity.Id, entity.Id, 15);

        // Каждое внешнее событие должно создавать снимок
        queue.Enqueue(dmg1);
        queue.Enqueue(dmg2);
        queue.ProcessQueue();

        Assert.AreEqual(2, queue.StateHistory.Count, "Expected 2 state snapshots (one per external event)");
        Assert.IsTrue(queue.StateHistory[0].Timestamp <= queue.StateHistory[1].Timestamp,
            "Snapshots should be ordered by time");
        Debug.Log($"StateHistoryTest passed. Snapshots: {queue.StateHistory.Count}");
    }

    [Test]
    public void BarrierWithPredicateTest()
    {
        // Проверяем что барьерная очередь с предикатом работает корректно
        BattleState battleState = new BattleState(42);
        var entity = new BaseEntity();
        var health = new HealthComponent(100);
        entity.AddComponent(health);
        battleState.AddEntity(entity);

        EventQueue queue = new(battleState);
        HealthSystem healthSystem = new();
        DamageSystem damageSystem = new();
        queue.Subscribe(damageSystem);
        queue.Subscribe(healthSystem);

        // Событие в барьерной очереди с предикатом: здоровье < 80 (изначально не выполнен)
        var barrierDmg = new SingleDamageEvent(entity.Id, entity.Id, entity.Id, 5);
        var predicate = new HealthLevelPredicate(ComparisonOperator.LessThan, 80, entity.Id);

        queue.EnqueueWithBarrier(barrierDmg, predicate);

        // Первое событие: наносит 30 урона (здоровье = 70 < 80 → предикат выполнится)
        var mainDmg = new SingleDamageEvent(entity.Id, entity.Id, entity.Id, 30);
        queue.Enqueue(mainDmg);
        queue.ProcessQueue();

        // После обработки: здоровье должно быть 70 - 5 = 65 (предикат выполнился, barrier-событие тоже обработано)
        Assert.AreEqual(65, health.CurrentHealth,
            "Expected health=65 after barrier event released when health < 80");
        Debug.Log($"BarrierWithPredicateTest passed. Health: {health.CurrentHealth}");
    }

    [Test]
    public void BarrierPredicateNotMetTest()
    {
        // Проверяем что барьерное событие НЕ обрабатывается если предикат не выполнен
        BattleState battleState = new BattleState(42);
        var entity = new BaseEntity();
        var health = new HealthComponent(100);
        entity.AddComponent(health);
        battleState.AddEntity(entity);

        EventQueue queue = new(battleState);
        HealthSystem healthSystem = new();
        DamageSystem damageSystem = new();
        queue.Subscribe(damageSystem);
        queue.Subscribe(healthSystem);

        // Барьерное событие: только если здоровье < 10 (никогда не выполнится при 100 hp)
        var barrierDmg = new SingleDamageEvent(entity.Id, entity.Id, entity.Id, 5);
        var predicate = new HealthLevelPredicate(ComparisonOperator.LessThan, 10, entity.Id);

        queue.EnqueueWithBarrier(barrierDmg, predicate);

        // Основное событие: 20 урона
        var mainDmg = new SingleDamageEvent(entity.Id, entity.Id, entity.Id, 20);
        queue.Enqueue(mainDmg);
        queue.ProcessQueue();

        // Здоровье должно быть 80 (только mainDmg обработан, barrierDmg заблокирован)
        Assert.AreEqual(80, health.CurrentHealth,
            "Expected health=80 since barrier predicate was never satisfied");
        Debug.Log($"BarrierPredicateNotMetTest passed. Health: {health.CurrentHealth}");
    }

    [Test]
    public void SubjectsStructureTest()
    {
        // Проверяем новую структуру Subjects
        var sourceId = Geid.New;
        var targetId = Geid.New;

        var evt = new SingleDamageEvent(sourceId, sourceId, targetId, 50);

        // Проверяем что роли правильно инициализированы
        Assert.AreEqual(sourceId, evt.GetFirstSubject(SubjectRole.Source),
            "Source should match");
        Assert.AreEqual(targetId, evt.GetFirstSubject(SubjectRole.Target),
            "Target should match");
        Assert.AreEqual(0, evt.GetSubjects(SubjectRole.Owner).Count,
            "Owner list should be empty");

        // Проверяем что список сортирован по индексам SubjectRole
        Assert.AreEqual((int)SubjectRole.Source, 0);
        Assert.AreEqual((int)SubjectRole.Target, 1);

        var sources = evt.GetSubjects(SubjectRole.Source);
        Assert.AreEqual(1, sources.Count, "Should have exactly 1 source");
        Assert.AreEqual(sourceId, sources[0]);

        Debug.Log("SubjectsStructureTest passed.");
    }

    [Test]
    public void PhaseLogSystemsTest()
    {
        // Проверяем что системы логирования подписываются и не вызывают ошибок
        BattleState battleState = new BattleState(42);
        var entity = new BaseEntity();
        entity.AddComponent(new HealthComponent(100));
        battleState.AddEntity(entity);

        EventQueue queue = new(battleState);
        queue.Subscribe(new PhaseStartLogSystem());
        queue.Subscribe(new PhaseEndLogSystem());
        queue.Subscribe(new DamageSystem());
        queue.Subscribe(new HealthSystem());

        var dmg = new SingleDamageEvent(entity.Id, entity.Id, entity.Id, 10);
        queue.Enqueue(dmg);

        // Должно выполниться без исключений
        Assert.DoesNotThrow(() => queue.ProcessQueue(), "PhaseLog systems should not throw");
        Debug.Log("PhaseLogSystemsTest passed.");
    }

    [Test]
    public void StateSnapshotContentsTest()
    {
        // Проверяем что снимок корректно содержит данные сущности
        BattleState battleState = new BattleState(42);
        var entity = new BaseEntity();
        var health = new HealthComponent(100);
        entity.AddComponent(health);
        battleState.AddEntity(entity);

        EventQueue queue = new(battleState);
        queue.Subscribe(new DamageSystem());
        queue.Subscribe(new HealthSystem());

        // Перед обработкой: здоровье = 100
        var dmg = new SingleDamageEvent(entity.Id, entity.Id, entity.Id, 30);
        queue.Enqueue(dmg);
        queue.ProcessQueue();

        // Снимок должен был содержать здоровье = 100 (до обработки)
        Assert.AreEqual(1, queue.StateHistory.Count, "Expected 1 snapshot");
        var snapshot = queue.StateHistory[0];
        Assert.IsTrue(snapshot.EntitySnapshots.ContainsKey(entity.Id),
            "Snapshot should contain the entity");

        var entitySnap = snapshot.EntitySnapshots[entity.Id];
        Assert.IsTrue(entitySnap.ContainsKey(typeof(HealthComponent)),
            "Entity snapshot should contain HealthComponent");

        var healthSnap = entitySnap[typeof(HealthComponent)] as HealthComponent;
        Assert.IsNotNull(healthSnap, "HealthComponent clone should not be null");
        Assert.AreEqual(100, healthSnap.CurrentHealth,
            "Snapshot health should be 100 (before damage was applied)");

        // После обработки: реальное здоровье = 70
        Assert.AreEqual(70, health.CurrentHealth, "Live health should be 70 after 30 damage");

        Debug.Log($"StateSnapshotContentsTest passed. Snapshot HP: {healthSnap.CurrentHealth}, Live HP: {health.CurrentHealth}");
    }
}

// Вспомогательный класс фильтра для теста
public class HealthThresholdFilter : ITargetFilter
{
    private int threshold;

    public HealthThresholdFilter(int threshold)
    {
        this.threshold = threshold;
    }

    public bool IsTargetValid(Geid target, EventContext context)
    {
        var entity = context.BattleState.GetEntity(target);
        if (entity == null) return false;

        var healthComponent = entity.GetComponent<HealthComponent>();
        if (healthComponent == null) return false;

        return healthComponent.Current <= threshold;
    }
}
