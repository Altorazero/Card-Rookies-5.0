using System.Collections.Generic;

/// <summary>
/// Базовый интерфейс игровой карты.
/// Карта содержит название, описание, стоимость ресурсов и список порождаемых событий.
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
    /// <summary>Список событий, порождаемых картой при разыгрывании.</summary>
    IReadOnlyList<IGameEvent> Effects { get; }
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
    public IReadOnlyList<IGameEvent> Effects { get; private set; }

    public BasicPlayingCard(string name, string description, int manaCost, int energyCost, IReadOnlyList<IGameEvent> effects = null)
    {
        Id = Geid.New;
        Name = name;
        Description = description;
        ManaCost = manaCost;
        EnergyCost = energyCost;
        Effects = effects ?? new List<IGameEvent>();
    }

    /// <summary>Создаёт карту без стоимости ресурсов.</summary>
    public BasicPlayingCard(string name, string description, IReadOnlyList<IGameEvent> effects = null)
        : this(name, description, 0, 0, effects)
    {
    }
}