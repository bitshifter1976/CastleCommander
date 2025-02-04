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
    public int SpawnsPointsLeft;
    public bool Active;
    public float Distance;
    public bool Thinking;

    public void StartRound(Board board)
    {
        // start round by pressing ok
        StartCoroutine(SimulateStartRoundClick(board));        
    }

    public void Think(Board board)
    {
        if (Thinking)
            return;
        
        StartCoroutine(SimulatePlayer(board));
    }

    private IEnumerator SimulatePlayer(Board board)
    {
        Thinking = true;        

        // TODO: show 'thinking...' above castle

        // get playing pieces and castle of both players for ai calculation
        GetPlayingPieces(out var units, out var castle, out var enemyUnits, out var enemyCastle);

        // while player has points left and we are the active player
        while (board.ActivePlayer.PointsLeft > 0 && board.ActivePlayer.PlayerId == PlayerId)
        {
            // if no playing piece found on board or sometimes, select castle and spawn unit
            var unitOnCastleTile = units.Any(u => u.BoardPosition == castle.BoardPosition);
            if (!unitOnCastleTile && (units.Count == 0 || Random.Range(0, 5) == 0))
            {
                yield return SpawnUnit(board, castle, units, enemyCastle, enemyUnits);
                GetPlayingPieces(out units, out castle, out enemyUnits, out enemyCastle);
            }

            // attack enemy castle if possible, then try to attack some other enemy unit
            yield return TryToAttack(board, castle, units, enemyCastle, enemyUnits);
            GetPlayingPieces(out units, out castle, out enemyUnits, out enemyCastle);

            // move any unit
            yield return MoveAnyUnit(board, castle, units, enemyCastle, enemyUnits);
            GetPlayingPieces(out units, out castle, out enemyUnits, out enemyCastle);

            // attack enemy castle if possible, then try to attack some other enemy unit
            yield return TryToAttack(board, castle, units, enemyCastle, enemyUnits);
            GetPlayingPieces(out units, out castle, out enemyUnits, out enemyCastle);
        }

        // end round if not already happend
        if (board.ActivePlayer.PlayerId == PlayerId)
            board.ButtonEndTurn.onClick.Invoke();

        Thinking = false;
    }

    private IEnumerator SimulateStartRoundClick(Board board)
    {
        yield return new WaitForSeconds(1f);
        board.MessageBoxButtonOk.onClick.Invoke();
        yield return new WaitForSeconds(1f);
    }

    private void GetPlayingPieces(out List<PlayingPieceTile> units, out CastleTile castle, out List<PlayingPieceTile> enemyUnits, out CastleTile enemyCastle)
    {
        units = GameTiles.Instance.PlayingPieceTiles.Values.Where(t => t.Player.PlayerId == PlayerId).ToList();
        castle = GameTiles.Instance.CastleTiles.Values.First(c => c.Player.PlayerId == PlayerId);
        enemyUnits = GameTiles.Instance.PlayingPieceTiles.Values.Where(t => t.Player.PlayerId != PlayerId).ToList();
        enemyCastle = GameTiles.Instance.CastleTiles.Values.First(c => c.Player.PlayerId != PlayerId);
    }

    private IEnumerator MoveAnyUnit(Board board, CastleTile castle, List<PlayingPieceTile> units, CastleTile enemyCastle, List<PlayingPieceTile> enemyUnits)
    {
        // select random unit
        if (units.Count == 0)
            yield return null;
        var anyUnit = units[Random.Range(0, units.Count)];
        board.SimulateLeftClick(anyUnit);
        yield return new WaitForSeconds(1f);

        // get possible tiles to move to
        var possibleTiles = GameTiles.Instance.LandscapeTiles.Values.Where(t => t.Movable).ToList();
        possibleTiles = possibleTiles.Where(t => board.MovementPossible(t.BoardPosition)).ToList();
        if (possibleTiles.Count == 0)
            yield return null;

        // try to move unit to random tile
        GameTile tile = null;
        var maxIterations = 100;
        var iteration = 0;
        while (tile == null && iteration < maxIterations)
        {
            iteration++;
            tile = possibleTiles[Random.Range(0, possibleTiles.Count)];
            var oldDistance = board.GetDistance(enemyCastle.BoardPosition, anyUnit.BoardPosition);
            var newDistance = board.GetDistance(enemyCastle.BoardPosition, tile.BoardPosition);
            // if we are closer to enemy castle than before, lets go
            if (newDistance < oldDistance)
            {
                // show path on grid
                board.ShowPath(tile.BoardPosition);
                // now move
                board.SimulateLeftClick(tile);
                // wait until movement finished
                while (board.AnimationRunning)
                    yield return new WaitForSeconds(1f);
                // remove path on grid
                board.TilemapPath.ClearAllTiles();
            }
            else
            {
                tile = null;
            }
        }
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator SpawnUnit(Board board, CastleTile castle, List<PlayingPieceTile> units, CastleTile enemyCastle, List<PlayingPieceTile> enemyUnits)
    {
        // select castle
        board.SimulateLeftClick(castle);
        yield return new WaitForSeconds(1f);
        // close dialog
        board.SpawnUnit.ButtonClose.onClick.Invoke();
        // select random unit type and spawn unit
        var randomUnitType = (PlayingPieceTileType)Random.Range(0, Enum.GetValues(typeof(PlayingPieceTileType)).Cast<int>().Max()+1);
        switch (randomUnitType)
        {
            case PlayingPieceTileType.Artillery:
                board.CreatePlayingPiece(castle.BoardPosition, PlayingPieceTileType.Artillery);
                break;
            case PlayingPieceTileType.Cavalry:
                board.CreatePlayingPiece(castle.BoardPosition, PlayingPieceTileType.Cavalry);
                break;
            case PlayingPieceTileType.Infantry:
                board.CreatePlayingPiece(castle.BoardPosition, PlayingPieceTileType.Infantry);
                break;
            case PlayingPieceTileType.Medic:
                board.CreatePlayingPiece(castle.BoardPosition, PlayingPieceTileType.Medic);
                break;
        }
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
                board.SimulateLeftClick(unit);
                // if castle in range attack
                if (board.SimulateRightClick(enemyCastle))
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
                        if (board.SimulateRightClick(enemyUnit))
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
                board.SimulateLeftClick(unit);
            }
        }
    }
}
