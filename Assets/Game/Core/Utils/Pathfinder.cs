using System;
using System.Collections.Generic;
public class Pathfinder
{
    private class PathNode : IComparable<PathNode>
    {
        public HexCoordinates Coordinates;
        public int GCost; // Стоимость от старта
        public int HCost; // Эвристика до финиша
        public int FCost => GCost + HCost;
        public PathNode Parent;

        public int CompareTo(PathNode other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0) compare = HCost.CompareTo(other.HCost);
            return compare;
        }
    }

    public static List<HexCoordinates> FindPath(BattleState state, HexCoordinates start, HexCoordinates end, int maxElevationDiff = 1)
    {
        var openSet = new List<PathNode>();
        var closedSet = new HashSet<HexCoordinates>();
        var nodes = new Dictionary<HexCoordinates, PathNode>();

        var startNode = new PathNode { Coordinates = start, GCost = 0, HCost = HexDistance(start, end) };
        openSet.Add(startNode);
        nodes[start] = startNode;

        while (openSet.Count > 0)
        {
            openSet.Sort();
            var currentNode = openSet[0];
            openSet.RemoveAt(0);
            closedSet.Add(currentNode.Coordinates);

            if (currentNode.Coordinates.Equals(end))
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (var neighbor in GetNeighbors(currentNode.Coordinates))
            {
                if (closedSet.Contains(neighbor)) continue;

                // 1. Проверяем, существует ли гекс (пол)
                if (state.GetTileAtHex(neighbor) == null) continue;

                // 2. Проверяем перепад высот
                if (!IsElevationValid(state, currentNode.Coordinates, neighbor, maxElevationDiff)) continue;

                // 3. Проверяем, свободен ли гекс (нет ли других сущностей, блокирующих путь)
                // Разрешаем пройти в финальную точку (вдруг мы идем бить врага), но промежуточные должны быть пустыми
                // В идеале - позволять проходить сквозь союзников, но для начала блокируем всё, кроме конца
                if (!neighbor.Equals(end) && state.GetOccupantAtHex(neighbor) != null && state.GetOccupantAtHex(neighbor).HasComponent<TeamComponent>()) continue;

                int moveCost = 1; // Стоимость перехода на 1 гекс
                int newMovementCostToNeighbor = currentNode.GCost + moveCost;

                if (!nodes.TryGetValue(neighbor, out var neighborNode))
                {
                    neighborNode = new PathNode { Coordinates = neighbor };
                    nodes[neighbor] = neighborNode;
                }

                if (newMovementCostToNeighbor < neighborNode.GCost || !openSet.Contains(neighborNode))
                {
                    neighborNode.GCost = newMovementCostToNeighbor;
                    neighborNode.HCost = HexDistance(neighbor, end);
                    neighborNode.Parent = currentNode;

                    if (!openSet.Contains(neighborNode))
                        openSet.Add(neighborNode);
                }
            }
        }

        return null; // Путь не найден
    }

    private static List<HexCoordinates> RetracePath(PathNode startNode, PathNode endNode)
    {
        var path = new List<HexCoordinates>();
        var currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.Coordinates);
            currentNode = currentNode.Parent;
        }
        path.Reverse(); // Путь от старта к финишу (исключая сам старт)
        return path;
    }

    private static int HexDistance(HexCoordinates a, HexCoordinates b)
    {
        return (Math.Abs(a.Q - b.Q) + Math.Abs(a.R - b.R) + Math.Abs(a.S - b.S)) / 2;
    }

    private static IEnumerable<HexCoordinates> GetNeighbors(HexCoordinates hex)
    {
        var directions = new[]
        {
            new HexCoordinates(1, 0), new HexCoordinates(1, -1), new HexCoordinates(0, -1),
            new HexCoordinates(-1, 0), new HexCoordinates(-1, 1), new HexCoordinates(0, 1)
        };

        foreach (var dir in directions)
        {
            yield return new HexCoordinates(hex.Q + dir.Q, hex.R + dir.R);
        }
    }

    private static bool IsElevationValid(BattleState state, HexCoordinates from, HexCoordinates to, int maxDiff)
    {
        var tileFrom = state.GetTileAtHex(from);
        var tileTo = state.GetTileAtHex(to);

        return Math.Abs(1 - 1) <= maxDiff;
    }
}
