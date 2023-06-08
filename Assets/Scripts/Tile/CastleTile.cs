using UnityEngine;
using UnityEngine.Tilemaps;

public class CastleTile : GameTile
{
    public Player Player;
    public CastleTileInfo Info;

    private int MaxEnergy = 20;

    public CastleTile(Vector3Int boardPosition, Tile tile, CastleTileInfo info, Tilemap tilemap, int id, Color color, Player player, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, color, TileType.PlayingPiece, movable, movementCost)
    {
        Player = player;
        Info = info;
        Info.Energy = MaxEnergy;
    }
}
