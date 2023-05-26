using UnityEngine.Tilemaps;
using static PlayingPieceTile;

public class PlayingPieceTileInfo
{
    public PlayingPieceTileType TileType;
    public int Attack;
    public int Defense;
    public int Speed;

    public PlayingPieceTileInfo(PlayingPieceTileType tileType, int attack, int defense, int speed)
    {
        TileType = tileType;
        Attack = attack;
        Defense = defense;
        Speed = speed;
    }
}