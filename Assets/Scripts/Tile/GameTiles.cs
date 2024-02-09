using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static PlayingPieceTile;
using Random = UnityEngine.Random;

public class GameTiles : MonoBehaviour
{
    public static GameTiles Instance;

    public Dictionary<Vector3, CastleTile> CastleTiles = new();
    public Dictionary<Vector3, LandscapeTile> LandscapeTiles = new();
    public Dictionary<Vector3, PlayingPieceTile> PlayingPieceTiles = new();
    public int LastId = 0;
    public List<LandscapeTileInfo> LandscapeTileInfos;
    public List<PlayingPieceTileInfo> PlayingPieceTileInfos;
    public CastleTileInfo CastleTileInfo;
    public List<Tile> TilesForMovement;

    public Tile Base;
    public Tile Desert;
    public Tile LeafForest;
    public Tile PineForest;
    public Tile Jungle;
    public Tile Mountain;
    public Tile Ocean;
    public Tile Grass;
    public Tile Castle1;
    public Tile Castle2;
    public Tile Volcano;
    public Tile UnderDirt;
    public Tile UnderOcean;
    public Tile Path;
    public Tile Select;

    private void Start()
    {
        Instance = this;
        Instance.LandscapeTileInfos = GetLandscapeTileInfos();
        Instance.PlayingPieceTileInfos = GetPlayingPieceTileInfos();
        Instance.CastleTileInfo = GetCastleTileInfo();
        Instance.TilesForMovement = GetLandscapeTilesForMovement();
    }

    private List<PlayingPieceTileInfo> GetPlayingPieceTileInfos()
    {
        return new List<PlayingPieceTileInfo>
        {
            //                                                       attack defense speed   distAtt ptsAtt
            new PlayingPieceTileInfo(PlayingPieceTileType.Artillery, 5,     0,      1,      30,     25),
            new PlayingPieceTileInfo(PlayingPieceTileType.Cavalry,   3,     1,      4,      7,      25),
            new PlayingPieceTileInfo(PlayingPieceTileType.Infantry,  2,     3,      2,      2,      25),
            new PlayingPieceTileInfo(PlayingPieceTileType.Medic,     1,     1,      3,      2,      25),
        };
    }

    private CastleTileInfo GetCastleTileInfo()
    {
        return new CastleTileInfo(1);
    }

    public List<LandscapeTileInfo> GetLandscapeTileInfos()
    {
        return new List<LandscapeTileInfo>
        {
            new LandscapeTileInfo(Grass,         1.0f,   10),
            new LandscapeTileInfo(LeafForest,    0.5f,   20),
            new LandscapeTileInfo(Desert,        0.5f,   30),
            new LandscapeTileInfo(Jungle,        0.5f,   30),
            new LandscapeTileInfo(Mountain,      0.4f,   100),
            new LandscapeTileInfo(Ocean,         0.4f,   100),
            new LandscapeTileInfo(Base,          0.2f,   10),
            new LandscapeTileInfo(PineForest,    0.2f,   20),
            new LandscapeTileInfo(Volcano,       0.05f,  100)
        };
    }

    private List<Tile> GetLandscapeTilesForMovement()
    {
        return new List<Tile>
        {
            Grass,
            LeafForest,
            Desert,
            Jungle,
            Base,
            PineForest
        };
    }

    public LandscapeTile.LandscapeTileType GetLandscapeType(Tile landscapeTile)
    {
        if (landscapeTile == Base)
            return LandscapeTile.LandscapeTileType.Base;
        else if (landscapeTile == Desert)
            return LandscapeTile.LandscapeTileType.Desert;
        else if (landscapeTile == Grass)
            return LandscapeTile.LandscapeTileType.Grass;
        else if (landscapeTile == Jungle)
            return LandscapeTile.LandscapeTileType.Jungle;
        else if (landscapeTile == LeafForest)
            return LandscapeTile.LandscapeTileType.LeafForest;
        else if (landscapeTile == Mountain)
            return LandscapeTile.LandscapeTileType.Mountain;
        else if (landscapeTile == Ocean)
            return LandscapeTile.LandscapeTileType.Ocean;
        else if (landscapeTile == PineForest)
            return LandscapeTile.LandscapeTileType.PineForest;
        else if (landscapeTile == Volcano)
            return LandscapeTile.LandscapeTileType.Volcano;

        return LandscapeTile.LandscapeTileType.None;
    }

    public Tile GetRandomLandscapeTile()
    {
        var probability = Random.Range(0f, 1f);
        var tiles = GetLandscapeTileInfos();
        var possibleTiles = tiles.Where(t => t.Probability >= probability).Select(s => s.Tile).ToList();
        if (possibleTiles.Count == 0)
            possibleTiles.Add(tiles[Random.Range(0, tiles.Count - 1)].Tile);
        return possibleTiles[Random.Range(0, possibleTiles.Count - 1)];
    }

    public PlayingPieceTile Move(PlayingPieceTile tile, Vector3Int newPosition)
    {
        if (tile != null && PlayingPieceTiles.ContainsKey(tile.BoardPosition))
        {
            PlayingPieceTiles.Remove(tile.BoardPosition);
            LandscapeTiles[tile.BoardPosition].Movable = true;
            var newTile = new PlayingPieceTile(newPosition, tile.Tile, tile.Info, tile.Tilemap, tile.Id, tile.Tile.color, tile.PlayingPieceType, tile.Player, false, tile.MovementCost);
            PlayingPieceTiles.TryAdd(newPosition, newTile);
            LandscapeTiles[newPosition].Movable = false;
            return newTile;
        }
        return null;
    }

    public GameTile Add(
        GameTile.TileType type, 
        Vector3Int boardPosition,
        Tile tile,
        LandscapeTileInfo tileInfo,
        PlayingPieceTileInfo tileInfo2,
        CastleTileInfo tileInfo3,
        Tilemap tilemap,
        Player player, 
        PlayingPieceTile.PlayingPieceTileType playingPieceType = PlayingPieceTile.PlayingPieceTileType.None, 
        LandscapeTile.LandscapeTileType landscapeType = LandscapeTile.LandscapeTileType.None)
    {
        switch (type)
        {
            case GameTile.TileType.Castle:
                if (CastleTiles.ContainsKey(boardPosition))
                    CastleTiles[boardPosition] = new CastleTile(boardPosition, tile, tileInfo3, tilemap, CastleTiles[boardPosition].Id, tileInfo.Tile.color, player, true, tileInfo.MovementCosts);
                else
                    CastleTiles.Add(boardPosition, new CastleTile(boardPosition, tile, tileInfo3, tilemap, NewId(), tileInfo.Tile.color, player, true, tileInfo.MovementCosts));
                return CastleTiles[boardPosition];
            case GameTile.TileType.Landscape:
                if (LandscapeTiles.ContainsKey(boardPosition))
                    LandscapeTiles[boardPosition] = new LandscapeTile(boardPosition, tile, tilemap, LandscapeTiles[boardPosition].Id, tileInfo.Tile.color, landscapeType, TilesForMovement.Contains(tileInfo.Tile), tileInfo.MovementCosts);
                else
                    LandscapeTiles.Add(boardPosition, new LandscapeTile(boardPosition, tile, tilemap, NewId(), tileInfo.Tile.color, landscapeType, TilesForMovement.Contains(tileInfo.Tile), tileInfo.MovementCosts));
                return LandscapeTiles[boardPosition];
            case GameTile.TileType.PlayingPiece:
                if (PlayingPieceTiles.ContainsKey(boardPosition))
                    PlayingPieceTiles[boardPosition] = new PlayingPieceTile(boardPosition, tile, tileInfo2, tilemap, PlayingPieceTiles[boardPosition].Id, player.Color, playingPieceType, player, false, 0);
                else
                    PlayingPieceTiles.Add(boardPosition, new PlayingPieceTile(boardPosition, tile, tileInfo2, tilemap, NewId(), player.Color, playingPieceType, player, false, 0));
                return PlayingPieceTiles[boardPosition];
            default: 
                return null;
        }
    }

    public void Delete(GameTile tile)
    {
        tile.Tilemap.SetTile(tile.BoardPosition, null);
        switch (tile.Type)
        {
            case GameTile.TileType.Castle:
                CastleTiles.Remove(tile.BoardPosition);
                break;
            case GameTile.TileType.Landscape:
                LandscapeTiles.Remove(tile.BoardPosition);
                break;
            case GameTile.TileType.PlayingPiece:
                PlayingPieceTiles.Remove(tile.BoardPosition);
                break;
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