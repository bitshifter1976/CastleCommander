using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameTile
{
    public enum TileType
    {
        Castle,
        Landscape,
        PlayingPiece
    }

    public Vector3Int BoardPosition;

    public Tile Tile;

    public Tilemap Tilemap;

    public int Id;

    public TileType Type;

    public bool Movable;

    public int MovementCost;

    public GameTile(Vector3Int boardPosition, Tile tile, Tilemap tilemap, int id, Color color, TileType type, bool movable, int movementCost)
    {
        BoardPosition = boardPosition;
        Tile = tile;
        Tile.color = color;
        Tilemap = tilemap;
        Id = id;
        Type = type;
        Movable = movable;
        MovementCost = movementCost;
    }
}
