using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CastleTile : GameTile
{
    public Player Player;

    public CastleTile(Vector3Int boardPosition, Tile tile, Tilemap tilemap, int id, Player player, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, TileType.PlayingPiece, movable, movementCost)
    {
        Player = player;
    }
}
