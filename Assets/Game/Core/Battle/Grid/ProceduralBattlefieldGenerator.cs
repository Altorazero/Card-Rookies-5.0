using System;
using System.Collections.Generic;

public enum BattlefieldShape
{
    Hexagon,
    Rectangle
}

[Serializable]
public class BattlefieldGeneratorSettings
{
    public BattlefieldShape Shape = BattlefieldShape.Hexagon;
    public int Size = 5; 
    public int MaxElevation = 3;
    public float SteepDropProbability = 0.05f; 
    public int Seed = -1; 
}

public class ProceduralBattlefieldGenerator
{
    /// <summary>
    /// Генерирует арену и напрямую заполняет BattleState ECS-сущностями тайлов.
    /// Возвращает Battlefield, который хранит только быстрые координаты для навигации.
    /// </summary>
    public void Generate(BattlefieldGeneratorSettings settings, BattleState state, int globalSeed = -1)
    {
        
        int seed = settings.Seed != -1 ? settings.Seed : (globalSeed != -1 ? globalSeed : UnityEngine.Random.Range(0, 1000000));
        var rng = new BattleRng(seed);

        var coordinates = GenerateShape(settings.Shape, settings.Size);
        var tileDataMap = new Dictionary<HexCoordinates, HexTileData>();

        // 1. Просчитываем данные (Тип поверхности, Высота 0)
        foreach (var coord in coordinates)
        {
            var terrain = GetRandomTerrain(rng);
            tileDataMap[coord] = new HexTileData(coord, terrain, 0);
        }

        // 2. Считаем плавные перепады высот
        GenerateElevations(tileDataMap, rng, settings);

        // 3. Создаем ECS Сущности (Тайлы) и добавляем в BattleState
        foreach (var kvp in tileDataMap)
        {
            var coord = kvp.Key;
            var data = kvp.Value;

            // Записываем геометрию в быстрый лукап
            

            // Создаем полноправную ECS-сущность тайла
            var tileEntity = new TileEntity(coord.Q, coord.R, data.Elevation, data.Terrain);
            
            // Добавляем в общий котел сущностей
            state.AddEntity(tileEntity);
        }

        // Подключаем поле к стейту
        // (Свойство Field в BattleState нужно будет сделать устанавливаемым или передавать данные)
        
        
    }

    private List<HexCoordinates> GenerateShape(BattlefieldShape shape, int size)
    {
        var hexes = new List<HexCoordinates>();
        if (shape == BattlefieldShape.Hexagon)
        {
            for (int q = -size; q <= size; q++)
            {
                int r1 = Math.Max(-size, -q - size);
                int r2 = Math.Min(size, -q + size);
                for (int r = r1; r <= r2; r++) hexes.Add(new HexCoordinates(q, r));
            }
        }
        else if (shape == BattlefieldShape.Rectangle)
        {
            int width = size;
            int height = size;
            for (int r = 0; r < height; r++)
            {
                int rOffset = r >> 1;
                for (int q = -rOffset; q < width - rOffset; q++) hexes.Add(new HexCoordinates(q, r));
            }
        }
        return hexes;
    }

    private TerrainType GetRandomTerrain(BattleRng rng)
    {
        int val = rng.NextInt(100);
        if (val < 40) return TerrainType.Grass;
        if (val < 70) return TerrainType.Sand;
        if (val < 90) return TerrainType.Stone;
        return TerrainType.Water;
    }

    private void GenerateElevations(Dictionary<HexCoordinates, HexTileData> tileDataMap, BattleRng rng, BattlefieldGeneratorSettings settings)
    {
        var coords = new List<HexCoordinates>(tileDataMap.Keys);
        if (coords.Count == 0) return;

        int numHills = settings.Size;
        var hillCenters = new List<KeyValuePair<HexCoordinates, int>>();
        
        for(int i = 0; i < numHills; i++)
        {
            var center = coords[rng.NextInt(coords.Count)];
            int peakHeight = rng.NextInt(1, settings.MaxElevation + 1);
            hillCenters.Add(new KeyValuePair<HexCoordinates, int>(center, peakHeight));
        }

        foreach (var hex in coords)
        {
            int maxCalculatedHeight = 0;
            foreach (var hill in hillCenters)
            {
                int dist = HexDistance(hex, hill.Key);
                int h = hill.Value - dist;
                if (h > maxCalculatedHeight) maxCalculatedHeight = h;
            }

            if (maxCalculatedHeight < 0) maxCalculatedHeight = 0;

            if (rng.NextInt(100) < (settings.SteepDropProbability * 100))
            {
                maxCalculatedHeight += (rng.NextInt(2) == 0 ? 2 : -2);
                if (maxCalculatedHeight < 0) maxCalculatedHeight = 0;
                if (maxCalculatedHeight > settings.MaxElevation) maxCalculatedHeight = settings.MaxElevation;
            }

            var data = tileDataMap[hex];
            data.Elevation = maxCalculatedHeight;
            tileDataMap[hex] = data;
        }
    }

    private int HexDistance(HexCoordinates a, HexCoordinates b)
    {
        return (Math.Abs(a.Q - b.Q) + Math.Abs(a.R - b.R) + Math.Abs(a.S - b.S)) / 2;
    }
}
