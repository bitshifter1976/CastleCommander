using UnityEngine.Tilemaps;
using static PlayingPieceTile;

public class LandscapeTileInfo
{
    public Tile Tile;
    public float Probability; // value between 1 and 0 
    public int MovementCosts;

    public LandscapeTileInfo(Tile tile, float probability, int movementCosts)
    {
        Tile = tile;
        Probability = probability;
        MovementCosts = movementCosts;
    }
}

public class PlayingPieceTileInfo
{
    public Tile Tile;
    public PlayingPieceTileType PlayingPieceType;
    public int Attack;
    public int Defense;
    public int Speed;

    public PlayingPieceTileInfo(Tile tile, PlayingPieceTileType tileType, int attack, int defense, int speed)
    {
        Tile = tile;
        PlayingPieceType = tileType;
        Attack = attack;
        Defense = defense;
        Speed = speed;
    }
}