using System;
using System.Threading;

public readonly struct GEID : IEquatable<GEID>, IComparable<GEID>
{
    private static int _currentId;

    public int Value { get; }

    private GEID(int value)
    {
        Value = value;
    }

    // Генератор нового ID
    public static GEID New => new GEID(Interlocked.Increment(ref _currentId));

    public static GEID Empty { get; } = new GEID(0);

    /// <summary>
    /// Создаёт GEID из конкретного значения (используется при клонировании).
    /// </summary>
    public static GEID FromValue(int value) => new GEID(value);

    public override string ToString() => Value.ToString();

    // Реализация IEquatable
    public bool Equals(GEID other) => Value == other.Value;
    public override bool Equals(object obj) => obj is GEID other && Equals(other);
    public override int GetHashCode() => Value;

    // Реализация IComparable
    public int CompareTo(GEID other) => Value.CompareTo(other.Value);

    // Операторы сравнения
    public static bool operator ==(GEID left, GEID right) => left.Equals(right);
    public static bool operator !=(GEID left, GEID right) => !left.Equals(right);
    public static bool operator <(GEID left, GEID right) => left.Value < right.Value;
    public static bool operator <=(GEID left, GEID right) => left.Value <= right.Value;
    public static bool operator >(GEID left, GEID right) => left.Value > right.Value;
    public static bool operator >=(GEID left, GEID right) => left.Value >= right.Value;
}
