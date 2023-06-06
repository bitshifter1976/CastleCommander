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
    public PlayingPieceTileType PlayingPieceType;
    public int Attack;
    public int Defense;
    public int Speed;
    public int DistanceForAttack;
    public int Energy;
    public bool IsAttacker;
    public static readonly int MaxEnergy = 10;

    public PlayingPieceTileInfo(PlayingPieceTileType tileType, int attack, int defense, int speed, int distanceForAttack)
    {
        PlayingPieceType = tileType;
        Attack = attack;
        Defense = defense;
        Speed = speed;
        DistanceForAttack = distanceForAttack;
        Energy = MaxEnergy;
    }
}

public class CastleTileInfo
{
    public int Attack;
    public int Defense;
    public int Speed;
    public int Energy;
    public static readonly int MaxEnergy = 20;

    public CastleTileInfo(int attack, int defense, int speed)
    {
        Attack = attack;
        Defense = defense;
        Speed = speed;
        Energy = MaxEnergy;
    }
}