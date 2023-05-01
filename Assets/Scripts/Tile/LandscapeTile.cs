using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandscapeTile : GameTile
{
    public enum LandscapeTileType
    {
        None,
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

    public LandscapeTile(Vector3Int boardPosition, Tile tile, Tilemap tilemap, int id, LandscapeTileType type, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, TileType.Landscape, movable, movementCost)
    {
        LandscapeType = type;
    }
}
