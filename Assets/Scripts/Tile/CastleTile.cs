using UnityEngine;
using UnityEngine.Tilemaps;

public class CastleTile : GameTile
{
    public Player Player;

    public CastleTile(Vector3Int boardPosition, Tile tile, Tilemap tilemap, int id, Color color, Player player, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, color, TileType.PlayingPiece, movable, movementCost)
    {
        Player = player;
    }
}
