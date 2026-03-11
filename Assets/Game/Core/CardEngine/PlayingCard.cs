/// <summary>
/// Базовый интерфейс игровой карты.
/// Карта содержит название, описание, стоимость ресурсов и корневой узел графа событий.
/// </summary>
public interface IPlayingCard
{
    Geid Id { get; }
    string Name { get; }
    string Description { get; }
    /// <summary>Стоимость маны для разыгрывания карты.</summary>
    int ManaCost { get; }
    /// <summary>Стоимость энергии для разыгрывания карты.</summary>
    int EnergyCost { get; }
    CardGraphNode CardGraphRootNode { get; }
}

/// <summary>
/// Базовая реализация игровой карты.
/// </summary>
public class BasicPlayingCard : IPlayingCard
{
    public Geid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int ManaCost { get; private set; }
    public int EnergyCost { get; private set; }
    public CardGraphNode CardGraphRootNode { get; private set; }

    public BasicPlayingCard(string name, string description, int manaCost, int energyCost, CardGraphNode cardGraphRootNode)
    {
        Id = Geid.New;
        Name = name;
        Description = description;
        ManaCost = manaCost;
        EnergyCost = energyCost;
        CardGraphRootNode = cardGraphRootNode;
    }

    /// <summary>Создаёт карту без стоимости ресурсов.</summary>
    public BasicPlayingCard(string name, string description, CardGraphNode cardGraphRootNode)
        : this(name, description, 0, 0, cardGraphRootNode)
    {
    }
}