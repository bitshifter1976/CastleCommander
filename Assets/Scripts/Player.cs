using System;
using System.Collections;
using System.Collections.Generic;
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
        
        StartCoroutine(SimulatePlayer(board));
    }

    private IEnumerator SimulatePlayer(Board board)
    {
        Thinking = true;

        // 1. start round by pressing ok
        yield return new WaitForSeconds(1f);
        board.MessageBoxButtonOk.onClick.Invoke();
        yield return new WaitForSeconds(1f);

        // 2. TODO: show 'thinking...' above castle

        // 3. get playing pieces and castle for ai player
        var units = GameTiles.Instance.PlayingPieceTiles.Values.Where(t => t.Player.PlayerId == PlayerId).ToList();
        var castle = GameTiles.Instance.CastleTiles.Values.First(c => c.Player.PlayerId == PlayerId);
        var enemyUnits = GameTiles.Instance.PlayingPieceTiles.Values.Where(t => t.Player.PlayerId != PlayerId).ToList();
        var enemyCastle = GameTiles.Instance.CastleTiles.Values.First(c => c.Player.PlayerId != PlayerId);

        // 4. if no playing piece found on board or sometimes, select castle and spawn unit
        if (units.Count == 0 || Random.Range(0, 5) == 0)
        {
            yield return StartCoroutine(SpawnUnit(board, castle, units, enemyCastle, enemyUnits));
            units = GameTiles.Instance.PlayingPieceTiles.Values.Where(t => t.Player.PlayerId == PlayerId).ToList();
        }

        // 5. attack enemy castle if possible, then try to attack some other enemy unit
        yield return StartCoroutine(TryToAttack(board, castle, units, enemyCastle, enemyUnits));

        // 6. move any unit
        yield return StartCoroutine(MoveAnyUnit(board, castle, units, enemyCastle, enemyUnits));

        // 7. attack enemy castle if possible, then try to attack some other enemy unit
        yield return StartCoroutine(TryToAttack(board, castle, units, enemyCastle, enemyUnits));

        // 8. end round if not already happend
        if (board.ActivePlayer.IsAi)
            board.ButtonEndTurn.onClick.Invoke();

        Thinking = false;
    }

    private IEnumerator MoveAnyUnit(Board board, CastleTile castle, List<PlayingPieceTile> units, CastleTile enemyCastle, List<PlayingPieceTile> enemyUnits)
    {
        if (units.Count == 0)
            yield break;

        // select random unit
        var anyUnit = units[Random.Range(0, units.Count-1)];
        board.DoLeftClick(anyUnit);
        yield return new WaitForSeconds(1f);

        // try to move unit to random tile
        GameTile tile = null;
        var maxIterations = 100;
        var iteration = 0;
        while (tile == null && iteration < maxIterations)
        {
            iteration++;
            var possibleTiles = GameTiles.Instance.LandscapeTiles.Values.Where(t => t.Movable).ToList();
            if (possibleTiles.Count > 0)
            {
                tile = possibleTiles[Random.Range(0, possibleTiles.Count - 1)];
                if (board.ShowPath(tile.BoardPosition))
                {
                    var oldDistance = board.GetDistance(enemyCastle.BoardPosition, anyUnit.BoardPosition);
                    var newDistance = board.GetDistance(enemyCastle.BoardPosition, tile.BoardPosition);
                    if (newDistance > oldDistance)
                    {
                        tile = null;
                    }
                    else
                    {
                        // if new position is closer to enemy castle then go but wait a little
                        board.DoLeftClick(tile);
                        // wait until movement finished
                        while (board.AnimationRunning)
                            yield return new WaitForSeconds(1f);
                    }
                }
                else
                {
                    tile = null;
                }
            }
        }
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator SpawnUnit(Board board, CastleTile castle, List<PlayingPieceTile> units, CastleTile enemyCastle, List<PlayingPieceTile> enemyUnits)
    {
        // select castle
        board.DoLeftClick(castle);
        yield return new WaitForSeconds(1f);
        // select unit type in popup
        var randomUnitType = (PlayingPieceTileType)Random.Range(0, Enum.GetValues(typeof(PlayingPieceTileType)).Cast<int>().Max());
        board.DoSelectUnitType(randomUnitType);
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator TryToAttack(Board board, CastleTile castle, List<PlayingPieceTile> units, CastleTile enemyCastle, List<PlayingPieceTile> enemyUnits)
    {
        var enemyInAttackRangeFound = false;
        var maxIterations = 100;
        var iteration = 0;
        while (board.ActivePlayer.PointsLeft > 0 && !enemyInAttackRangeFound && iteration < maxIterations)
        {
            iteration++;
            foreach (var unit in units)
            {
                // select unit
                board.DoLeftClick(unit);
                // if castle in range attack
                if (board.DoRightClick(enemyCastle))
                {
                    enemyInAttackRangeFound = true;
                    while (board.FightBoardShowing)
                        yield return new WaitForSeconds(1f);
                    yield break;
                }
                else
                {
                    foreach (var enemyUnit in enemyUnits)
                    {
                        // if enemy in range attack
                        if (board.DoRightClick(enemyUnit))
                        {
                            enemyInAttackRangeFound = true;
                            while (board.FightBoardShowing)
                                yield return new WaitForSeconds(1f);
                            yield break;
                        }
                    }
                    if (enemyInAttackRangeFound)
                        yield break;
                }
                // deselect unit if not attacked, otherwise we break
                board.DoLeftClick(unit);
            }
        }
    }
}
