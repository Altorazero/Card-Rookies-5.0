using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;

public class NewTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void EventTest()
    {
        GameEvent @event = new(Geid.New);
        Debug.Log("Event id: " + @event.Id);
        Debug.Log("Event source id: " + @event.SystemSourceId);

        // Создаем спецификацию таргетинга для MassDamageEvent
        var targetingSpec = new BasicTargetingSpec
        {
            Description = "Single target damage",
            Type = TargetingType.Entity,
            TargetFilter = new AlwaysValidFilter(),
            Selector = new FirstTargetSelector(1),
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target
        };

        MassDamageEvent damageEvent = new(Geid.New, Geid.New, Geid.New, 10, targetingSpec);
        Debug.Log("DamageEvent id: " + damageEvent.Id);

        var target = damageEvent.SubjectsList?.SingleOrDefault(t => t.Role == SubjectRole.Target)?.Entity;
        var source = damageEvent.SubjectsList?.SingleOrDefault(t => t.Role == SubjectRole.Source)?.Entity;
        Debug.Log("Damage target id: " + (target.Equals(default(Geid)) ? "none" : target.ToString()));
        Debug.Log("Damage source id: " + (source.Equals(default(Geid)) ? "none" : source.ToString()));

        Debug.Log("Damage sys source id: " + damageEvent.SystemSourceId);
        Debug.Log("Damage amount: " + damageEvent.DamageAmount);
        Debug.Log("Damage type: " + damageEvent.DamageType);
    }

    [Test]
    public void EventEngineTest()
    {
        // Setup
        DamageSystem damageSystem = new();
        HealthSystem healthSystem = new();
        TargetingSystem targetingSystem = new();
        BattleState battleState = new BattleState(12);


        // Create entities
        var sourceEntity = new BaseEntity();
        Debug.Log("Source entity id: " + sourceEntity.Id);
        battleState.AddEntity(sourceEntity);
        var targetEntity = new BaseEntity();
        Debug.Log("Target entity id: " + targetEntity.Id);
        battleState.AddEntity(targetEntity);

        // Add HealthComponent to entities
        var sourceHealth = new HealthComponent(100);
        sourceEntity.AddComponent(sourceHealth);
        var targetHealth = new HealthComponent(100);
        targetEntity.AddComponent(targetHealth);

        //Add PowerComponent to source entity
        var sourcePower = new PowerComponent(50);
        sourceEntity.AddComponent(sourcePower);

        // Создаем спецификации таргетинга
        var singleTargetSpec = new BasicTargetingSpec
        {
            Description = "Single target",
            Type = TargetingType.Entity,
            TargetFilter = new AlwaysValidFilter(),
            Selector = new FirstTargetSelector(1),
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        };

        // Create events (first arg = systemSourceId)
        SingleDamageEvent damageEvent = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 80);
        SingleDamageEvent damageEvent1 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 30);
        SingleDamageEvent damageEvent2 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 20);

        // Create dispatcher 
        EventDispatcher dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);
        dispatcher.Subscribe(damageSystem);
        dispatcher.Subscribe(healthSystem);

        // Enqueue and process event
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

        // Create entities
        var sourceEntity = new BaseEntity();
        Debug.Log("Source entity id: " + sourceEntity.Id);
        battleState.AddEntity(sourceEntity);
        var targetEntity = new BaseEntity();
        Debug.Log("Target entity id: " + targetEntity.Id);
        battleState.AddEntity(targetEntity);


        // Add HealthComponent to entities
        var sourceHealth = new HealthComponent(100);
        sourceEntity.AddComponent(sourceHealth);
        var targetHealth = new HealthComponent(100);
        targetEntity.AddComponent(targetHealth);


        //Add PowerComponent to source entity
        var sourcePower = new PowerComponent(50);
        sourceEntity.AddComponent(sourcePower);

        //Add ShieldComponent to target entity
        var targetShield = new ShieldComponent(50);
        targetEntity.AddComponent(targetShield);

        //Add VampComponent to source entity
        var sourceVamp = new VampComponent(0.5f);
        sourceEntity.AddComponent(sourceVamp);

        // Спецификация таргетинга для массового урона
        var massTargetSpec = new BasicTargetingSpec
        {
            Description = "Mass damage - single target",
            Type = TargetingType.Entity,
            TargetFilter = new AlwaysValidFilter(),
            Selector = new FirstTargetSelector(1),
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        };

        // Create events (first arg = systemSourceId)
        MassDamageEvent damageEvent = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 80, massTargetSpec);
        MassDamageEvent damageEvent1 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 30, massTargetSpec);
        MassDamageEvent damageEvent2 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 20, massTargetSpec);

        // Create dispatcher
        EventDispatcher dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);
        dispatcher.Subscribe(damageSystem);
        dispatcher.Subscribe(shieldSystem);
        dispatcher.Subscribe(healthSystem);
        dispatcher.Subscribe(vampSystem);

        // Enqueue and process event
        dispatcher.Enqueue(damageEvent);
        dispatcher.Enqueue(damageEvent1);
        dispatcher.EnqueueWithBarrier(damageEvent2);
        dispatcher.ProcessQueue();
        Debug.Log("Processed damage event id: " + damageEvent.Id);
    }

    [Test]
    public void CardGraphTest()
    {
        // Setup
        DamageSystem damageSystem = new();
        HealthSystem healthSystem = new();
        VampSystem vampSystem = new();
        ShieldSystem shieldSystem = new();
        ExecuteCardGraphSystem cardGraphSystem = new();
        TargetingSystem targetingSystem = new();
        BattleState battleState = new BattleState(12);

        // Create dispatcher
        EventDispatcher dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);
        dispatcher.Subscribe(damageSystem);
        dispatcher.Subscribe(shieldSystem);
        dispatcher.Subscribe(healthSystem);
        dispatcher.Subscribe(vampSystem);
        dispatcher.Subscribe(cardGraphSystem);

        // Create entities
        var sourceEntity = new BaseEntity();
        Debug.Log("Source entity id: " + sourceEntity.Id);
        battleState.AddEntity(sourceEntity);
        var targetEntity = new BaseEntity();
        Debug.Log("Target entity id: " + targetEntity.Id);
        battleState.AddEntity(targetEntity);


        // Add HealthComponent to entities
        var sourceHealth = new HealthComponent(100);
        sourceEntity.AddComponent(sourceHealth);
        var targetHealth = new HealthComponent(100);
        targetEntity.AddComponent(targetHealth);
        var sourceMana = new ManaComponent(100);
        sourceEntity.AddComponent(sourceMana);
        // Add HexComponent to entities
        var sourceHex = new HexComponent(new HexCoordinates(0, 0));
        sourceEntity.AddComponent(sourceHex);
        var targetHex = new HexComponent(new HexCoordinates(1, 0));
        targetEntity.AddComponent(targetHex);

        // Спецификации таргетинга
        var damageTargetSpec = new BasicTargetingSpec
        {
            Description = "Damage target",
            Type = TargetingType.Entity,
            TargetFilter = new AlwaysValidFilter(),
            Selector = new FirstTargetSelector(1),
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        };

        var healTargetSpec = new BasicTargetingSpec
        {
            Description = "Heal target",
            Type = TargetingType.Entity,
            TargetFilter = new AlwaysValidFilter(),
            Selector = new FirstTargetSelector(1),
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        };

        var selfHealSpec = new BasicTargetingSpec
        {
            Description = "Self heal",
            Type = TargetingType.Entity,
            TargetFilter = new SelfTargetFilter(),
            Selector = new FirstTargetSelector(1),
            MinTargets = 1,
            MaxTargets = 1,
            TargetRole = SubjectRole.Target,
            SourceEntity = sourceEntity.Id
        };

        // Create events (first arg = systemSourceId)
        MassDamageEvent damageEvent = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 18, damageTargetSpec);
        MassDamageEvent damageEvent1 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 30, damageTargetSpec);
        MassDamageEvent damageEvent2 = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 20, damageTargetSpec);

        var node1 = new CardGraphNode(new List<IGameEvent> { damageEvent }); //deals 18 damage
        var node2 = new CardGraphNode(new List<IGameEvent> { new HealEventWithTargeting(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 50, healTargetSpec), damageEvent1, damageEvent2 });  //heals 50, takes 30 damage, takes 20 damage
        var node3 = new CardGraphNode(new List<IGameEvent> { new HealEventWithTargeting(sourceEntity.Id, sourceEntity.Id, sourceEntity.Id, 30, selfHealSpec) }); //heals 30
        var node4 = new CardGraphNode(new List<IGameEvent> { new HealEventWithTargeting(sourceEntity.Id, sourceEntity.Id, sourceEntity.Id, 20, selfHealSpec) }); //heals 20

        var predicate = new ManaLevelPredicate(ComparisonOperator.GreaterThanOrEqual, 30, sourceEntity.Id);
        var predicate2 = new HealthLevelPredicate(ComparisonOperator.LessThan, 900, sourceEntity.Id);

        node1.TieNode(node2, predicate);
        node1.TieNode(node3, predicate2);
        node3.TieNode(node1, predicate2); //loop back to node1
        node3.TieNode(node4, predicate2); //unconditional tie to node4
        // Enqueue and process event
        var playCardEvent = new ExecuteCardGraphEvent(sourceEntity.Id, node1);
        dispatcher.Enqueue(playCardEvent);
        dispatcher.ProcessQueue();
        Debug.Log("Processed PlayCardEvent id: " + playCardEvent.Id);
        Debug.Log("Source entity coordinates after card play: " + sourceEntity.GetComponent<HexComponent>()?.Coordinates.ToString());
    }

    [Test]
    public void RandomAndCoordinatesTest()
    {
        // Setup
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

        // Create dispatcher
        EventDispatcher dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);
        dispatcher.Subscribe(damageSystem);
        dispatcher.Subscribe(shieldSystem);
        dispatcher.Subscribe(healthSystem);
        dispatcher.Subscribe(vampSystem);
        dispatcher.Subscribe(cardGraphSystem);
        dispatcher.Subscribe(moveSystem);
        dispatcher.Subscribe(randomMovementSystem);
        dispatcher.Subscribe(randomDamageSystem);
        // Create entities
        var sourceEntity = new BaseEntity();
        Debug.Log("Source entity id: " + sourceEntity.Id);
        battleState.AddEntity(sourceEntity);
        var targetEntity = new BaseEntity();
        Debug.Log("Target entity id: " + targetEntity.Id);
        battleState.AddEntity(targetEntity);


        // Add HealthComponent to entities
        var sourceHealth = new HealthComponent(100);
        sourceEntity.AddComponent(sourceHealth);
        var targetHealth = new HealthComponent(100);
        targetEntity.AddComponent(targetHealth);
        var sourceMana = new ManaComponent(100);
        sourceEntity.AddComponent(sourceMana);

        // Add HexComponent to entities
        var sourceHex = new HexComponent(new HexCoordinates(0, 0));
        sourceEntity.AddComponent(sourceHex);
        var targetHex = new HexComponent(new HexCoordinates(1, 0));
        targetEntity.AddComponent(targetHex);
        // Create MoveEntityEvent
        RandomMovementEvent randomMovementEvent = new(sourceEntity.Id, sourceEntity.Id, 2);
        RandomMovementEvent randomMovementEvent1 = new(targetEntity.Id, targetEntity.Id, 2);
        // RandomDamageEvent: first arg = systemSourceId
        RandomDamageEvent randomDamageEvent = new(sourceEntity.Id, sourceEntity.Id, targetEntity.Id, 1, 200);

        // Enqueue and process event
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
        // Инициализируем фиксированный сид
        int seed = 12345;
        BattleState battleState = new BattleState(seed);
        TargetingSystem targetingSystem = new();

        // Создаём сущности и их здоровье
        var source = new BaseEntity();
        battleState.AddEntity(source);

        var lowHp = new BaseEntity();
        var highHp = new BaseEntity();
        battleState.AddEntity(lowHp);
        battleState.AddEntity(highHp);

        // Добавляем HealthComponent (MetricComponent) с разными значениями
        lowHp.AddComponent(new HealthComponent(10));   // Низкое здоровье
        highHp.AddComponent(new HealthComponent(50));  // Высокое здоровье

        // Создаем фильтр для таргетинга: только сущности с Health <= 20
        var healthFilter = new HealthThresholdFilter(20);

        // Создаем спецификацию таргетинга
        var targetingSpec = new BasicTargetingSpec
        {
            Description = "Target low health entities",
            Type = TargetingType.Entity,
            TargetFilter = healthFilter,
            Selector = new AllTargetsSelector(),
            MinTargets = 0,
            MaxTargets = 10,
            TargetRole = SubjectRole.Target,
            SourceEntity = source.Id
        };

        // Создаем событие с таргетингом
        var damageEvent = new MassDamageEvent(source.Id, source.Id, Geid.Empty, 10, targetingSpec);

        // Создаем диспетчер и обрабатываем событие
        EventDispatcher dispatcher = new(battleState);
        dispatcher.Subscribe(targetingSystem);

        dispatcher.Enqueue(damageEvent);
        dispatcher.ProcessQueue();

        // Проверяем результаты
        var targets = damageEvent.SubjectsList?.Where(s => s.Role == SubjectRole.Target).Select(s => s.Entity).ToList();

        Assert.IsNotNull(targets, "Targets should not be null");
        Assert.AreEqual(1, targets.Count, "Expected one target with low health");
        Assert.IsTrue(targets.Contains(lowHp.Id), "Expected targets to contain lowHp.Id");
        Assert.IsFalse(targets.Contains(highHp.Id), "Expected targets NOT to contain highHp.Id");

        Debug.Log("TargetingPredicateTest passed.");
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