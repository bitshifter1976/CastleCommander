using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayingPieceTile : GameTile
{
    public enum PlayingPieceTileType
    {
        None = -1,
        Infantry = 0,
        Artillery = 1,
        Medic = 2
    }

    public PlayingPieceTileType PlayingPieceType;

    public Player Player;

    public PlayingPieceTile(Vector3Int boardPosition, Tile tile, Tilemap tilemap, int id, Color color, PlayingPieceTileType type, Player player, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, color, TileType.PlayingPiece, movable, movementCost)
    {
        PlayingPieceType = type;
        Player = player;
    }
}
