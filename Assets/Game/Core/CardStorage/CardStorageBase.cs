using System;
using System.Collections.Generic;

/// <summary>
/// Абстрактное хранилище карт — базовый класс для колоды, руки и сброса.
/// Реализует общие операции добавления, удаления, поиска и перемешивания карт.
/// </summary>
public abstract class CardStorageBase
{
    protected readonly List<IPlayingCard> _cards = new();

    /// <summary>Все карты в хранилище (только для чтения).</summary>
    public IReadOnlyList<IPlayingCard> Cards => _cards;

    /// <summary>Количество карт в хранилище.</summary>
    public int Count => _cards.Count;

    /// <summary>True, если хранилище пустое.</summary>
    public bool IsEmpty => _cards.Count == 0;

    /// <summary>
    /// Создаёт пустое хранилище.
    /// </summary>
    protected CardStorageBase() { }

    /// <summary>
    /// Создаёт хранилище с начальным набором карт.
    /// </summary>
    protected CardStorageBase(IEnumerable<IPlayingCard> initialCards)
    {
        if (initialCards != null)
            _cards.AddRange(initialCards);
    }

    // ─── Добавление карт ──────────────────────────────────────────────────────

    /// <summary>Добавляет карту в начало хранилища (вершина стопки).</summary>
    public void AddToTop(IPlayingCard card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        _cards.Insert(0, card);
    }

    /// <summary>Добавляет карту в конец хранилища (дно стопки).</summary>
    public void AddToBottom(IPlayingCard card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        _cards.Add(card);
    }

    /// <summary>Добавляет карту на указанную позицию. index=0 — вершина.</summary>
    public void AddAt(IPlayingCard card, int index)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        if (index < 0 || index > _cards.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _cards.Insert(index, card);
    }

    // ─── Извлечение карт ──────────────────────────────────────────────────────

    /// <summary>Извлекает и возвращает карту с вершины. Null, если пусто.</summary>
    public IPlayingCard RemoveFromTop()
    {
        if (_cards.Count == 0) return null;
        var card = _cards[0];
        _cards.RemoveAt(0);
        return card;
    }

    /// <summary>Извлекает и возвращает карту со дна. Null, если пусто.</summary>
    public IPlayingCard RemoveFromBottom()
    {
        if (_cards.Count == 0) return null;
        var card = _cards[_cards.Count - 1];
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }

    /// <summary>Извлекает и возвращает карту по индексу. index=0 — вершина.</summary>
    public IPlayingCard RemoveAt(int index)
    {
        if (index < 0 || index >= _cards.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        var card = _cards[index];
        _cards.RemoveAt(index);
        return card;
    }

    /// <summary>
    /// Удаляет конкретную карту из хранилища.
    /// Возвращает true, если карта была найдена и удалена.
    /// </summary>
    public bool Remove(IPlayingCard card)
    {
        if (card == null) return false;
        return _cards.Remove(card);
    }

    /// <summary>
    /// Удаляет карту по её идентификатору.
    /// Возвращает true, если карта была найдена и удалена.
    /// </summary>
    public bool RemoveById(Geid cardId)
    {
        var card = FindById(cardId);
        if (card == null) return false;
        return _cards.Remove(card);
    }

    // ─── Просмотр (без удаления) ─────────────────────────────────────────────

    /// <summary>Возвращает карту с вершины без извлечения. Null, если пусто.</summary>
    public IPlayingCard PeekTop() => _cards.Count > 0 ? _cards[0] : null;

    /// <summary>Возвращает карту со дна без извлечения. Null, если пусто.</summary>
    public IPlayingCard PeekBottom() => _cards.Count > 0 ? _cards[_cards.Count - 1] : null;

    /// <summary>Возвращает карту по индексу без извлечения.</summary>
    public IPlayingCard GetAt(int index)
    {
        if (index < 0 || index >= _cards.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _cards[index];
    }

    /// <summary>Ищет карту по идентификатору. Null, если не найдена.</summary>
    public IPlayingCard FindById(Geid cardId)
    {
        foreach (var card in _cards)
            if (card.Id == cardId) return card;
        return null;
    }

    /// <summary>Проверяет, содержит ли хранилище карту с данным идентификатором.</summary>
    public bool Contains(Geid cardId) => FindById(cardId) != null;

    // ─── Утилиты ─────────────────────────────────────────────────────────────

    /// <summary>Перемешивает карты в хранилище (алгоритм Фишера–Йейтса).</summary>
    public void Shuffle(BattleRng rng)
    {
        for (int i = _cards.Count - 1; i > 0; i--)
        {
            int j = rng.NextInt(i + 1);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }

    /// <summary>Очищает хранилище от всех карт.</summary>
    public void Clear() => _cards.Clear();

    /// <summary>Перемещает карту с одной позиции на другую.</summary>
    public void MoveCard(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _cards.Count)
            throw new ArgumentOutOfRangeException(nameof(fromIndex));
        if (toIndex < 0 || toIndex >= _cards.Count)
            throw new ArgumentOutOfRangeException(nameof(toIndex));
        var card = _cards[fromIndex];
        _cards.RemoveAt(fromIndex);
        _cards.Insert(toIndex, card);
    }

    /// <summary>Возвращает индекс карты по её идентификатору. -1, если не найдена.</summary>
    public int IndexOf(Geid cardId)
    {
        for (int i = 0; i < _cards.Count; i++)
            if (_cards[i].Id == cardId) return i;
        return -1;
    }
}
