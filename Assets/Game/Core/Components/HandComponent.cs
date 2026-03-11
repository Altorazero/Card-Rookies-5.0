using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент руки карт сущности.
/// Рука связана с конкретной колодой и отдельным сбросом.
/// Автоматически добирает карты в начале боя и в начале каждого хода (кроме первого).
/// </summary>
public class HandComponent : CardStorageBase
{
    /// <summary>Колода, из которой добираются карты в руку.</summary>
    public DeckComponent LinkedDeck { get; }

    /// <summary>Количество карт, автоматически добираемых в начале хода.</summary>
    public int AutoDrawCount { get; set; } = 1;

    /// <summary>
    /// True, если начальный добор (в начале боя) уже был выполнен.
    /// Используется для пропуска добора в начале первого хода.
    /// </summary>
    public bool HasDrawnInitial { get; set; } = false;

    public HandComponent(DeckComponent linkedDeck) : base()
    {
        LinkedDeck = linkedDeck ?? throw new System.ArgumentNullException(nameof(linkedDeck));
    }

    public HandComponent(DeckComponent linkedDeck, IEnumerable<IPlayingCard> initialCards) : base(initialCards)
    {
        LinkedDeck = linkedDeck ?? throw new System.ArgumentNullException(nameof(linkedDeck));
    }

    // ─── Добор карт ───────────────────────────────────────────────────────────

    /// <summary>
    /// Добирает одну карту с вершины связанной колоды в руку.
    /// Возвращает добранную карту или null, если колода пуста.
    /// </summary>
    public IPlayingCard DrawFromDeck()
    {
        var card = LinkedDeck.DrawTop();
        if (card == null)
        {
            Debug.LogWarning("[HandComponent] Cannot draw: deck is empty.");
            return null;
        }
        AddToBottom(card);
        return card;
    }

    /// <summary>
    /// Добирает указанное количество карт из связанной колоды в руку.
    /// </summary>
    public void DrawMultiple(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (LinkedDeck.IsEmpty) break;
            DrawFromDeck();
        }
    }

    /// <summary>
    /// Добирает конкретную карту из связанной колоды в руку по идентификатору.
    /// Возвращает true, если карта была найдена и перенесена.
    /// </summary>
    public bool DrawSpecificFromDeck(Geid cardId)
    {
        var card = LinkedDeck.DrawSpecific(cardId);
        if (card == null) return false;
        AddToBottom(card);
        return true;
    }

    /// <summary>
    /// Добирает случайную карту из связанной колоды в руку.
    /// Возвращает добранную карту или null, если колода пуста.
    /// </summary>
    public IPlayingCard DrawRandomFromDeck(BattleRng rng)
    {
        var card = LinkedDeck.DrawRandom(rng);
        if (card == null) return null;
        AddToBottom(card);
        return card;
    }

    // ─── Разыгрывание карт ───────────────────────────────────────────────────

    /// <summary>
    /// Разыгрывает карту из руки: извлекает её и возвращает для дальнейшей обработки.
    /// Карту нужно поместить в сброс вручную или через PlayCardFromHand.
    /// Возвращает null, если карта не найдена.
    /// </summary>
    public IPlayingCard PlayCard(IPlayingCard card)
    {
        if (!Remove(card))
        {
            Debug.LogWarning($"[HandComponent] Card {card?.Id} not found in hand.");
            return null;
        }
        return card;
    }

    /// <summary>
    /// Разыгрывает карту из руки по идентификатору.
    /// Возвращает null, если карта не найдена.
    /// </summary>
    public IPlayingCard PlayCardById(Geid cardId)
    {
        var card = FindById(cardId);
        if (card == null) return null;
        return PlayCard(card);
    }

    // ─── Сброс карт из руки ──────────────────────────────────────────────────

    /// <summary>
    /// Сбрасывает карту из руки в сброс.
    /// </summary>
    public void DiscardCard(IPlayingCard card, DiscardComponent discard)
    {
        if (!Remove(card))
        {
            Debug.LogWarning($"[HandComponent] Card {card?.Id} not found in hand.");
            return;
        }
        discard.AddToTop(card);
    }

    /// <summary>
    /// Сбрасывает карту из руки в сброс по идентификатору.
    /// </summary>
    public void DiscardCardById(Geid cardId, DiscardComponent discard)
    {
        var card = FindById(cardId);
        if (card == null)
        {
            Debug.LogWarning($"[HandComponent] Card {cardId} not found in hand.");
            return;
        }
        DiscardCard(card, discard);
    }

    /// <summary>
    /// Выбрасывает карту из руки навсегда (не в сброс).
    /// Возвращает true, если карта была найдена и удалена.
    /// </summary>
    public bool ExileCard(IPlayingCard card) => Remove(card);

    /// <summary>
    /// Выбрасывает карту из руки навсегда по идентификатору.
    /// </summary>
    public bool ExileCardById(Geid cardId) => RemoveById(cardId);

    /// <summary>
    /// Сбрасывает все карты из руки в сброс.
    /// </summary>
    public void DiscardAll(DiscardComponent discard)
    {
        foreach (var card in _cards)
            discard.AddToTop(card);
        _cards.Clear();
    }

    // ─── Автодобор ───────────────────────────────────────────────────────────

    /// <summary>
    /// Выполняет автоматический добор карт в количестве AutoDrawCount.
    /// Вызывается системой в начале боя и в начале каждого хода (кроме первого).
    /// </summary>
    public void AutoDraw()
    {
        DrawMultiple(AutoDrawCount);
    }
}
