using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayingPieceTile : GameTile
{
    public enum PlayingPieceTileType
    {
        None = -1,
        Artillery = 0,
        Cavalry = 1,
        Infantry = 2,
        Medic = 3
    }

    public PlayingPieceTileInfo Info;
    public PlayingPieceTileType PlayingPieceType;
    public Player Player;

    public PlayingPieceTile(Vector3Int boardPosition, PlayingPieceTileInfo tile, Tilemap tilemap, int id, Color color, PlayingPieceTileType type, Player player, bool movable, int movementCost) : base(boardPosition, tile.Tile, tilemap, id, color, TileType.PlayingPiece, movable, movementCost)
    {
        Info = tile;
        PlayingPieceType = type;
        Player = player;
    }
}
