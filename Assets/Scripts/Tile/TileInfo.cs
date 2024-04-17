using UnityEditor;
using UnityEngine;
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

    public LandscapeTileInfo Clone()
    {
        return new LandscapeTileInfo(Tile, Probability, MovementCosts);
    }
}

public class PlayingPieceTileInfo
{
    public PlayingPieceTileType PlayingPieceType;
    public GameObject PlayingPiece;
    public int SpawnCosts;
    public int Attack;
    public int Defense;
    public int Speed;
    public int Energy;
    public int DistanceForAttack;
    public int PointsForAttack;
    public bool IsAttacker;
    public static readonly int MaxEnergy = 10;

    public PlayingPieceTileInfo(PlayingPieceTileType tileType, int spawnCosts, int attack, int defense, int speed, int distanceForAttack, int pointsForAttack)
    {
        PlayingPieceType = tileType;
        SpawnCosts = spawnCosts;
        Attack = attack;
        Defense = defense;
        Speed = speed;
        DistanceForAttack = distanceForAttack;
        PointsForAttack = pointsForAttack;
        Energy = MaxEnergy;
    }

    public PlayingPieceTileInfo Clone()
    {
        return new PlayingPieceTileInfo(PlayingPieceType, SpawnCosts, Attack, Defense, Speed, DistanceForAttack, PointsForAttack);
    }
}

public class CastleTileInfo
{
    public int Attack;
    public int Defense;
    public int Speed;
    public int Energy;
    public static readonly int MaxEnergy = 20;

    public CastleTileInfo(int defense)
    {
        Defense = defense;
        Energy = MaxEnergy;
    }

    public CastleTileInfo Clone()
    {
        return new CastleTileInfo(Defense);
    }
}