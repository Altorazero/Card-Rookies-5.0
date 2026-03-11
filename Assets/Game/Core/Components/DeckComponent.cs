using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент колоды карт сущности.
/// Хранит карты до начала боя и является источником для добора в руку.
/// </summary>
public class DeckComponent : CardStorageBase
{
    public DeckComponent() : base() { }

    public DeckComponent(IEnumerable<IPlayingCard> initialCards) : base(initialCards) { }

    // ─── Добор карт из колоды ─────────────────────────────────────────────────

    /// <summary>Берёт карту с вершины колоды (первую в списке). Null, если колода пуста.</summary>
    public IPlayingCard DrawTop() => RemoveFromTop();

    /// <summary>Берёт карту со дна колоды (последнюю в списке). Null, если колода пуста.</summary>
    public IPlayingCard DrawBottom() => RemoveFromBottom();

    /// <summary>Берёт случайную карту из колоды. Null, если колода пуста.</summary>
    public IPlayingCard DrawRandom(BattleRng rng)
    {
        if (_cards.Count == 0) return null;
        int index = rng.NextInt(_cards.Count);
        return RemoveAt(index);
    }

    /// <summary>Берёт конкретную карту из колоды по идентификатору. Null, если не найдена.</summary>
    public IPlayingCard DrawSpecific(Geid cardId)
    {
        var card = FindById(cardId);
        if (card == null) return null;
        _cards.Remove(card);
        return card;
    }

    /// <summary>Берёт карту из колоды по позиции (0 = вершина). Null, если индекс некорректен.</summary>
    public IPlayingCard DrawAt(int index)
    {
        if (index < 0 || index >= _cards.Count) return null;
        return RemoveAt(index);
    }

    // ─── Выброс карт из колоды ────────────────────────────────────────────────

    /// <summary>
    /// Выбрасывает карту из колоды в сброс навсегда (переносит в DiscardComponent).
    /// </summary>
    public void DiscardCard(IPlayingCard card, DiscardComponent discard)
    {
        if (!Remove(card))
        {
            Debug.LogWarning($"[DeckComponent] Card {card?.Id} not found in deck.");
            return;
        }
        discard.AddToTop(card);
    }

    /// <summary>
    /// Выбрасывает карту из колоды по индексу в сброс.
    /// </summary>
    public void DiscardAt(int index, DiscardComponent discard)
    {
        if (index < 0 || index >= _cards.Count)
        {
            Debug.LogWarning($"[DeckComponent] Index {index} out of range.");
            return;
        }
        var card = RemoveAt(index);
        discard.AddToTop(card);
    }

    /// <summary>
    /// Полностью уничтожает карту из колоды (не перемещает в сброс).
    /// Возвращает true, если карта была найдена и удалена.
    /// </summary>
    public bool DestroyCard(IPlayingCard card) => Remove(card);

    /// <summary>
    /// Полностью уничтожает карту из колоды по идентификатору.
    /// Возвращает true, если карта была найдена и удалена.
    /// </summary>
    public bool DestroyById(Geid cardId) => RemoveById(cardId);
}
