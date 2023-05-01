using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayingPieceTile : GameTile
{
    public enum PlayingPieceTileType
    {
        None,
        Infantry,
        Artillery,
        Medic
    }

    public PlayingPieceTileType PlayingPieceType;

    public Player Player;

    public PlayingPieceTile(Vector3Int boardPosition, Tile tile, Tilemap tilemap, int id, PlayingPieceTileType type, Player player, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, TileType.PlayingPiece, movable, movementCost)
    {
        PlayingPieceType = type;
        Player = player;
    }
}
