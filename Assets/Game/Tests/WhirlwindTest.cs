using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture, Timeout(5000) ]
public class WhirlwindTest
{
    private static readonly BindingKey<IEnumerable<IEntity>> TestTargetsKey = new("TestTargets");
    private static readonly BindingKey<IEnumerable<IEntity>> SpendersKey = new("Spenders");

    private class MockInteractionService : IInteractionService
    {
        public IEntity SelectedTarget { get; set; }

        public void RequestTargetSelection<T>(ISelectionExecution<T> execution)
        {
            var candidate = execution.Candidates.FirstOrDefault(c => (object)c == (object)SelectedTarget);
            if (candidate != null)
            {
                execution.Complete(new[] { candidate });
            }
            else
            {
                execution.Cancel();
            }
        }
    }

    [Test, Timeout(5000)]
    public void TestPlayCardPipelineWithInteractiveTargeting()
    {
        // 1. Инициализация сцены и сущностей (передаем seed для BattleRng)
        var state = new BattleState(42);

        var caster = new BaseEntity();
        var casterHealth = new HealthComponent(30, 30);
        var casterMana = new ManaComponent(10, 10);
        var casterHand = new HandComponent();
        var casterDiscard = new DiscardComponent();
        caster.AddComponent(casterHealth);
        caster.AddComponent(casterMana);
        caster.AddComponent(casterHand);
        caster.AddComponent(casterDiscard);

        var target = new BaseEntity();
        var targetHealth = new HealthComponent(20, 20);
        target.AddComponent(targetHealth);

        state.AddEntity(caster);
        state.AddEntity(target);

        // 2. Инициализация EventQueue и подписка систем
        var queue = new EventQueue(state);

        var playCheckSystem = new PlayCardCheckSystem();
        var playEventSystem = new PlayCardEventSystem();
        var resourceCostSystem = new ResourceSystem();
        var damageSystem = new DamageSystem();
        var healthSystem = new HealthSystem();

        queue.Subscribe(playCheckSystem);
        queue.Subscribe(playEventSystem);
        queue.Subscribe(resourceCostSystem);
        queue.Subscribe(damageSystem);
        queue.Subscribe(healthSystem);

        // Настраиваем Mock-интерактивность
        var mockInteraction = new MockInteractionService { SelectedTarget = target };
        queue.Interaction = mockInteraction;

        // 3. Создаем граф для карты
        // Стоимость: 3 маны. Эффект: 12 урона по выбранной цели.
        var startNode = new StartNode();

        var costNode = new SpendResourceNode()
        {
            Amount = new ConstantSpec<int>(3),
            ResourceType = new ConstantSpec<MetricResourceType>(MetricResourceType.Mana),
            Source = new CasterSpec(),
            Spenders = new BindingSpec<IEnumerable<IEntity>>(BuiltInBindings.Spenders),
        };

        var resolveBindingNode = new ResolveBindingNode<IEntity>()
        {
            Provider = new AllEntitiesProvider<IEntity>(),
            Transforms = new List<ICandidateTransform<IEntity>>(),
            Selector = new HumanChoiceSelector<IEntity>(),
            Output = TestTargetsKey
        };

        var damageNode = new DamageNode()
        {
            Amount = new ConstantSpec<int>(12),
            DamageType = new ConstantSpec<DamageType>(DamageType.Physical),
            Source = new CasterSpec(),
            Targets = new BindingSpec<IEnumerable<IEntity>>(TestTargetsKey)
        };

        var endNode = new EndNode();

        // Сборка графа через конструктор, принимающий входную точку
        var graph = new CardGraph(startNode);
        graph.AddNode(costNode);
        graph.AddNode(resolveBindingNode);
        graph.AddNode(damageNode);
        graph.AddNode(endNode);

        // Соединяем узлы через Connect метод графа
        graph.Connect(startNode.Next, costNode);
        graph.Connect(costNode.Success, resolveBindingNode);
        graph.Connect(resolveBindingNode.Success, damageNode);
        graph.Connect(damageNode.Success, endNode);

        // Создаем определение карты и ее экземпляр
        var cardDef = ScriptableObject.CreateInstance<CardDefinition>();
        cardDef.CardGraph = graph;

        var cardInstance = new CardInstance(cardDef);

        // Кладем карту в руку кастера
        casterHand.Add(cardInstance);

        Assert.Contains(cardInstance, casterHand.Cards.ToList());
        Assert.AreEqual(0, casterDiscard.Count);
        Assert.AreEqual(10, casterMana.Current);
        Assert.AreEqual(20, targetHealth.Current);

        // 4. Запуск разыгрывания карты через событие
        var playCheckEvent = new PlayCardCheckEvent(GEID.New, cardInstance, caster);
        queue.Enqueue(playCheckEvent);
        queue.ProcessQueue();

        // 5. Проверка результатов
        // Карта должна быть извлечена из руки и положена в сброс
        Assert.IsFalse(casterHand.Cards.Contains(cardInstance));
        Assert.Contains(cardInstance, casterDiscard.Cards.ToList());

        // Стоимость (3 маны) должна быть списана
        Assert.AreEqual(7, casterMana.Current);

        // Враг должен получить 12 единиц урона (20 - 12 = 8 здоровья)
        Assert.AreEqual(8, targetHealth.Current);

        Debug.Log("Integration test completed successfully!");
    }
}
