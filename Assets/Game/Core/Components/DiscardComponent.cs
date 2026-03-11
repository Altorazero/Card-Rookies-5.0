using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент сброса карт.
/// Разыгранные карты из руки по умолчанию помещаются сюда.
/// </summary>
public class DiscardComponent : CardStorageBase
{
    public DiscardComponent() : base() { }

    public DiscardComponent(IEnumerable<IPlayingCard> initialCards) : base(initialCards) { }

    // ─── Операции со сбросом ──────────────────────────────────────────────────

    /// <summary>
    /// Перемещает все карты из сброса обратно в колоду и перемешивает колоду.
    /// Сброс очищается.
    /// </summary>
    public void ShuffleIntoDeck(DeckComponent deck, BattleRng rng)
    {
        foreach (var card in _cards)
            deck.AddToBottom(card);
        _cards.Clear();
        deck.Shuffle(rng);
        Debug.Log($"[DiscardComponent] Shuffled {deck.Count} cards back into deck.");
    }

    /// <summary>
    /// Берёт конкретную карту из сброса по идентификатору.
    /// Возвращает карту и удаляет её из сброса. Null, если не найдена.
    /// </summary>
    public IPlayingCard TakeCard(Geid cardId)
    {
        var card = FindById(cardId);
        if (card == null) return null;
        _cards.Remove(card);
        return card;
    }

    /// <summary>
    /// Берёт верхнюю карту из сброса. Null, если сброс пуст.
    /// </summary>
    public IPlayingCard TakeTop() => RemoveFromTop();

    /// <summary>
    /// Берёт карту из сброса по индексу. Null, если индекс некорректен.
    /// </summary>
    public IPlayingCard TakeAt(int index)
    {
        if (index < 0 || index >= _cards.Count) return null;
        return RemoveAt(index);
    }
}
