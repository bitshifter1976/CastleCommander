using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static PlayingPieceTile;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    public bool IsAi;
    public uint PlayerId;
    public Color Color;
    public Color ColorInactive;
    public Tile ArtilleryTile;
    public Tile CavalryTile;
    public Tile InfantryTile; 
    public Tile MedicTile;
    public int PointsLeft;
    public int SpawnsLeft;
    public bool Active;
    public float Distance;

    private bool Thinking;

    public void Think(Board board)
    {
        if (Thinking)
            return;

        Thinking = true;

        // 1. get playing pieces and castle for ai player
        var playingPiecesOnBoard = GameTiles.Instance.PlayingPieceTiles.Where(t => t.Value.Player.PlayerId == PlayerId).ToList();
        var castle = GameTiles.Instance.CastleTiles.Values.First(c => c.Player.PlayerId == PlayerId);
        var enemyCastle = GameTiles.Instance.CastleTiles.Values.First(c => c.Player.PlayerId != PlayerId);

        // 2. if no playing piece found on board, select castle and spawn unit
        if (playingPiecesOnBoard.Count == 0 || Random.Range(0, 5) == 0)
        {
            board.DoLeftClick(castle);
            var randomUnitType = (PlayingPieceTileType)Random.Range(Enum.GetValues(typeof(PlayingPieceTileType)).Cast<int>().Min(), Enum.GetValues(typeof(PlayingPieceTileType)).Cast<int>().Max() + 1);
            board.DoSelectUnitType(randomUnitType);
            var newUnit = GameTiles.Instance.Get<PlayingPieceTile>(board.MouseHandler.LeftSelectedPlayingPiecePosition);
            board.DoLeftClick(newUnit);
        }

        // 3. attack enemy castle if possible, then try to attack some other enemy unit
        var enemyInAttackRangeFound = false;
        while (board.ActivePlayer.PointsLeft > 0 && !enemyInAttackRangeFound)
        {
            if (board.DoRightClick(enemyCastle))
            {
                enemyInAttackRangeFound |= true;
            }
            else
            {
                var possibleUnitsToAttack = GameTiles.Instance.PlayingPieceTiles.Values.Where(t => t.Player.PlayerId != PlayerId).ToList();
                foreach (var possibleUnit in possibleUnitsToAttack)
                {
                    if (board.DoRightClick(possibleUnit))
                    {
                        enemyInAttackRangeFound = true;
                        break;
                    }
                }
            }
        }

        // 4. move any unit
        var possibleUnits = GameTiles.Instance.PlayingPieceTiles.Values.Where(t => t.Player.PlayerId == PlayerId).ToList();
        var anyUnit = possibleUnits[Random.Range(0, possibleUnits.Count)];
        board.DoLeftClick(anyUnit);
        GameTile tile = null;
        var maxIterations = 100;
        var iteration = 0;
        while (tile == null && iteration < maxIterations)
        {
            iteration++;
            var possibleTiles = GameTiles.Instance.LandscapeTiles.Values.Where(t => t.Movable).ToList();
            if (possibleTiles.Count == 0)
                break;
            tile = possibleTiles[Random.Range(0, possibleTiles.Count)];
            if (board.ShowPath(tile.BoardPosition))
            {
                // if new position is closer to enemy castle then go
                var oldDistance = board.GetDistance(enemyCastle.BoardPosition, anyUnit.BoardPosition);
                var newDistance = board.GetDistance(enemyCastle.BoardPosition, tile.BoardPosition);
                if (newDistance > oldDistance)
                {
                    tile = null;
                }
            }
            else
            {
                tile = null;
            }
        }
        board.DoLeftClick(tile);

        // 5. attack enemy castle if possible, then try to attack some other enemy unit
        enemyInAttackRangeFound = false;
        while (board.ActivePlayer.PointsLeft > 0 && !enemyInAttackRangeFound)
        {
            if (board.DoRightClick(enemyCastle))
            {
                enemyInAttackRangeFound |= true;
            }
            else
            {
                var possibleUnitsToAttack = GameTiles.Instance.PlayingPieceTiles.Values.Where(t => t.Player.PlayerId != PlayerId).ToList();
                foreach (var possibleUnit in possibleUnitsToAttack)
                {
                    if (board.DoRightClick(possibleUnit))
                    {
                        enemyInAttackRangeFound = true;
                        break;
                    }
                }
            }
        }

        Thinking = false;
    }
}
