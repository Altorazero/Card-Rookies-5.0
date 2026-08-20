using System;

[Serializable]
public struct HexTileData
{
    public HexCoordinates Coordinates;
    public TerrainType Terrain;
    public int Elevation; // Высота гекса

    public HexTileData(HexCoordinates coordinates, TerrainType terrain, int elevation)
    {
        Coordinates = coordinates;
        Terrain = terrain;
        Elevation = elevation;
    }
}
