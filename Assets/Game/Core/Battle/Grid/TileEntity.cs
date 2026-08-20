public class TileEntity : BaseEntity
{
    public TileEntity(int q, int r, int e, TerrainType t) : base()
    {
        AddComponent(new HexComponent(new HexCoordinates(q, r)));
        AddComponent(new ElevationComponent(e));
        AddComponent(new TileComponent(t));
    }
}
