using System;
using System.Threading;

public readonly struct Geid : IEquatable<Geid>, IComparable<Geid>
{
    private static int _currentId;

    public int Value { get; }

    private Geid(int value)
    {
        Value = value;
    }

    // Получение нового ID
    public static Geid New => new Geid(Interlocked.Increment(ref _currentId));

    public static Geid Empty { get; } = new Geid(0);

    public override string ToString() => Value.ToString();

    // Реализация IEquatable
    public bool Equals(Geid other) => Value == other.Value;
    public override bool Equals(object obj) => obj is Geid other && Equals(other);
    public override int GetHashCode() => Value;

    // Реализация IComparable
    public int CompareTo(Geid other) => Value.CompareTo(other.Value);

    // Операторы сравнения
    public static bool operator ==(Geid left, Geid right) => left.Equals(right);
    public static bool operator !=(Geid left, Geid right) => !left.Equals(right);
    public static bool operator <(Geid left, Geid right) => left.Value < right.Value;
    public static bool operator <=(Geid left, Geid right) => left.Value <= right.Value;
    public static bool operator >(Geid left, Geid right) => left.Value > right.Value;
    public static bool operator >=(Geid left, Geid right) => left.Value >= right.Value;
}
