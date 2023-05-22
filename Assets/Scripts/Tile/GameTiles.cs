using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static BoardController;

public class GameTiles : MonoBehaviour
{
    public static GameTiles Instance;

    public Dictionary<Vector3, CastleTile> CastleTiles = new();
    public Dictionary<Vector3, LandscapeTile> LandscapeTiles = new();
    public Dictionary<Vector3, PlayingPieceTile> PlayingPieceTiles = new();
    public int LastId = 0;
    public List<TileInfo> TileInfos;
    public List<Tile> TilesForMovement;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public PlayingPieceTile Move(PlayingPieceTile tile, Vector3Int newPosition)
    {
        if (tile != null && PlayingPieceTiles.ContainsKey(tile.BoardPosition))
        {
            PlayingPieceTiles.Remove(tile.BoardPosition);
            var newTile = new PlayingPieceTile(newPosition, tile.Tile, tile.Tilemap, tile.Id, tile.Tile.color, tile.PlayingPieceType, tile.Player, false, tile.MovementCost);
            PlayingPieceTiles.TryAdd(newPosition, newTile);
            return newTile;
        }
        return null;
    }

    public GameTile Add(
        GameTile.TileType type, 
        Vector3Int boardPosition, 
        TileInfo tile, 
        Tilemap tilemap,
        Player player, 
        PlayingPieceTile.PlayingPieceTileType playingPieceType = PlayingPieceTile.PlayingPieceTileType.None, 
        LandscapeTile.LandscapeTileType landscapeType = LandscapeTile.LandscapeTileType.None)
    {
        switch (type)
        {
            case GameTile.TileType.Castle:
                if (CastleTiles.ContainsKey(boardPosition))
                    CastleTiles[boardPosition] = new CastleTile(boardPosition, tile.Tile, tilemap, CastleTiles[boardPosition].Id, tile.Tile.color, player, true, tile.MovementCosts);
                else
                    CastleTiles.Add(boardPosition, new CastleTile(boardPosition, tile.Tile, tilemap, NewId(), tile.Tile.color, player, true, tile.MovementCosts));
                return CastleTiles[boardPosition];
            case GameTile.TileType.Landscape:
                if (LandscapeTiles.ContainsKey(boardPosition))
                    LandscapeTiles[boardPosition] = new LandscapeTile(boardPosition, tile.Tile, tilemap, LandscapeTiles[boardPosition].Id, tile.Tile.color, landscapeType, TilesForMovement.Contains(tile.Tile), tile.MovementCosts);
                else
                    LandscapeTiles.Add(boardPosition, new LandscapeTile(boardPosition, tile.Tile, tilemap, NewId(), tile.Tile.color, landscapeType, TilesForMovement.Contains(tile.Tile), tile.MovementCosts));
                return LandscapeTiles[boardPosition];
            case GameTile.TileType.PlayingPiece:
                if (PlayingPieceTiles.ContainsKey(boardPosition))
                    PlayingPieceTiles[boardPosition] = new PlayingPieceTile(boardPosition, tile.Tile, tilemap, PlayingPieceTiles[boardPosition].Id, player.Color, playingPieceType, player, false, 0);
                else
                    PlayingPieceTiles.Add(boardPosition, new PlayingPieceTile(boardPosition, tile.Tile, tilemap, NewId(), player.Color, playingPieceType, player, false, 0));
                return PlayingPieceTiles[boardPosition];
            default: 
                return null;
        }
    }

    public T Get<T>(Vector3Int boardPosition) where T : GameTile
    {
        GameTile tile = null;

        switch (typeof(T).ToString())
        {
            case "PlayingPieceTile":
                if (PlayingPieceTiles.TryGetValue(boardPosition, out var t1))
                    tile = t1;
                break;
            case "CastleTile":
                if (CastleTiles.TryGetValue(boardPosition, out var t2))
                    tile = t2;
                break;
            case "LandscapeTile":
                if (LandscapeTiles.TryGetValue(boardPosition, out var t3))
                    tile = t3;
                break;
        }
        
        return (T)tile;
    }

    public CastleTile GetCastle(uint playerId)
    {
        return CastleTiles.Select(tile => tile.Value).FirstOrDefault(tile => tile.Player.PlayerId == playerId);
    }

    public int NewId()
    {
        return ++LastId;
    }
}