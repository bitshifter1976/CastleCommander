using UnityEngine;
using UnityEngine.Tilemaps;

public class LandscapeTile : GameTile
{
    public enum LandscapeTileType
    {
        Base,
        Desert,
        LeafForest,
        PineForest,
        Jungle,
        Mountain,
        Ocean,
        Grass,
        Castle,
        Volcano,
        UnderDirt,
        UnderOcean
    }

    public LandscapeTileType LandscapeType;

    public LandscapeTile(Vector3Int boardPosition, Tile tile, Tilemap tilemap, int id, Color color, LandscapeTileType type, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, color, TileType.Landscape, movable, movementCost)
    {
        LandscapeType = type;
    }
}
