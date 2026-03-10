using System;
using UnityEngine;

public struct HexCoordinates : IEquatable<HexCoordinates>
{
    private int q;
    private int r;
    public int Q => q;
    public int R => r;
    public int S => -Q - R;
    public HexCoordinates(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public static HexCoordinates FromOffset(int q, int r)
    {
        // преобразование из even-q в кубическую
        int x = q;
        int z = r - (q + (q & 1)) / 2;
        int y = -x - z;
        return new HexCoordinates(x, y);
    }

    public Vector3 ToWorld(float size)
    {
        float x = size * (Mathf.Sqrt(3) * Q + Mathf.Sqrt(3) / 2 * R);
        float z = size * (3f / 2 * R);
        return new Vector3(x, 0, z);
    }

    public static int Distance(HexCoordinates a, HexCoordinates b)
    {
        return Mathf.Max(
            Mathf.Abs(a.Q - b.Q),
            Mathf.Abs(a.R - b.R),
            Mathf.Abs(a.S - b.S)
        );
    }

    public bool Equals(HexCoordinates other)
    {
        return Q == other.Q && R == other.R;
    }
    public static readonly HexCoordinates[] Directions =
    {
            new HexCoordinates(+1, 0),
            new HexCoordinates(+1, -1),
            new HexCoordinates(0, -1),
            new HexCoordinates(-1, 0),
            new HexCoordinates(-1, +1),
            new HexCoordinates(0, +1)
        };

    public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b)
    {
        return new HexCoordinates(a.Q + b.Q, a.R + b.R);
    }

    public override bool Equals(object obj) => obj is HexCoordinates other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Q, R);
    public override string ToString() => $"Hex({Q}, {R}, {S})";
}

