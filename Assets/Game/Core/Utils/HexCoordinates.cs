using System;
using UnityEngine;

/// <summary>
/// Кубические координаты гекса (axial q,r + производная s = -q-r).
/// Полностью неизменяема — любое "изменение" координаты создаёт новый экземпляр.
/// </summary>
public readonly struct HexCoordinates : IEquatable<HexCoordinates>
{
    public int Q { get; }
    public int R { get; }
    public int S => -Q - R;

    public HexCoordinates(int q, int r)
    {
        Q = q;
        R = r;
    }

    /// <summary>
    /// Преобразование из offset-координат (even-q layout) в кубические.
    /// </summary>
    public static HexCoordinates FromOffset(int col, int row)
    {
        int x = col;
        int z = row - (col + (col & 1)) / 2;
        int y = -x - z;
        return new HexCoordinates(x, y);
    }

    public Vector3 ToWorld(float size)
    {
        float x = size * (Mathf.Sqrt(3f) * Q + Mathf.Sqrt(3f) / 2f * R);
        float z = size * (3f / 2f * R);
        return new Vector3(x, 0f, z);
    }

    public static int Distance(HexCoordinates a, HexCoordinates b)
    {
        return Mathf.Max(
            Mathf.Abs(a.Q - b.Q),
            Mathf.Abs(a.R - b.R),
            Mathf.Abs(a.S - b.S));
    }

    public int DistanceTo(HexCoordinates other) => Distance(this, other);

    public HexCoordinates Neighbor(int direction) => this + Directions[direction];

    public static readonly HexCoordinates[] Directions =
    {
        new(+1, 0),
        new(+1, -1),
        new(0, -1),
        new(-1, 0),
        new(-1, +1),
        new(0, +1),
    };

    public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b) =>
        new(a.Q + b.Q, a.R + b.R);

    public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b) =>
        new(a.Q - b.Q, a.R - b.R);

    public static bool operator ==(HexCoordinates a, HexCoordinates b) => a.Equals(b);
    public static bool operator !=(HexCoordinates a, HexCoordinates b) => !a.Equals(b);

    public bool Equals(HexCoordinates other) => Q == other.Q && R == other.R;
    public override bool Equals(object obj) => obj is HexCoordinates other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Q, R);
    public override string ToString() => $"Hex({Q}, {R}, {S})";
}