using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        LandscapeTileInfos = GetLandscapeTileInfos();
        PlayingPieceTileInfos = GetPlayingPieceTileInfos();
        CastleTileInfo = GetCastleTileInfo();
        TilesForMovement = GetLandscapeTilesForMovement();
        Instance = this;
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
            new LandscapeTileInfo(Mountain,      0.4f,  100),
            new LandscapeTileInfo(Ocean,         0.4f,  100),
            new LandscapeTileInfo(Base,          0.2f,   10),
            new LandscapeTileInfo(PineForest,    0.2f,   20),
            new LandscapeTileInfo(Volcano,       0.1f, 100)
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

        return LandscapeTile.LandscapeTileType.Base;
    }

    public Tile GetRandomLandscapeTile()
    {
        var probability = Random.Range(0f, 1f);
        var tiles = GetLandscapeTileInfos();
        var possibleTiles = tiles.Where(t => t.Probability >= probability).Select(s => s.Tile).ToList();
        if (possibleTiles.Count == 0)
            possibleTiles.Add(tiles[Random.Range(0, tiles.Count)].Tile);
        return possibleTiles[Random.Range(0, possibleTiles.Count)];
    }

    public void Move(PlayingPieceTile tile, Vector3Int newPosition)
    {
        if (tile != null && PlayingPieceTiles.ContainsKey(tile.BoardPosition))
        {
            LandscapeTiles[tile.BoardPosition].Movable = true;
            LandscapeTiles[newPosition].Movable = false;
            PlayingPieceTiles.Remove(tile.BoardPosition);
            PlayingPieceTiles.Add(newPosition, tile);
            tile.BoardPosition = newPosition;
        }
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
        PlayingPieceTile.PlayingPieceTileType? playingPieceType, 
        LandscapeTile.LandscapeTileType? landscapeType)
    {
        switch (type)
        {
            case GameTile.TileType.Castle:
                CastleTiles.Add(boardPosition, new CastleTile(boardPosition, tile, tileInfo3, tilemap, NewId(), tileInfo.Tile.color, player, true, tileInfo.MovementCosts));
                return CastleTiles[boardPosition];
            case GameTile.TileType.Landscape:
                LandscapeTiles.Add(boardPosition, new LandscapeTile(boardPosition, tile, tilemap, NewId(), tileInfo.Tile.color, landscapeType.Value, TilesForMovement.Contains(tileInfo.Tile), tileInfo.MovementCosts));
                return LandscapeTiles[boardPosition];
            case GameTile.TileType.PlayingPiece:
                var newPlayingPiece = CreatePlayingPiece(boardPosition, player, playingPieceType.Value, tilemap);
                PlayingPieceTiles.Add(boardPosition, new PlayingPieceTile(boardPosition, newPlayingPiece, tile, tileInfo2, tilemap, NewId(), player.Color, playingPieceType.Value, player, false, 0));
                return PlayingPieceTiles[boardPosition];
            default: 
                return null;
        }
    }

    private static GameObject CreatePlayingPiece(Vector3Int boardPosition, Player player, PlayingPieceTileType playingPieceType, Tilemap tilemap)
    {
        var prefabName = string.Empty;
        switch (playingPieceType)
        {
            case PlayingPieceTileType.Artillery:
                prefabName = (player.PlayerId == 1) ? "Archer_MaskTint_PBR" : "Archer_Standard_PBR";
                break;
            case PlayingPieceTileType.Cavalry:
                prefabName = (player.PlayerId == 1) ? "Horseman_MaskTint_PBR" : "Horseman_Standard_PBR";
                break;
            case PlayingPieceTileType.Infantry:
                prefabName = (player.PlayerId == 1) ? "Footman_MaskTint_PBR" : "Footman_Standard_PBR";
                break;
            case PlayingPieceTileType.Medic:
                prefabName = (player.PlayerId == 1) ? "Mage_MaskTint_PBR" : "Mage_Standard_PBR";
                break;
        }
        var prefab = AssetDatabase.LoadAssetAtPath($"Assets/Prefabs/{prefabName}.prefab", typeof(GameObject));
        var pos = tilemap.CellToWorld(boardPosition);
        var clone = Instantiate(prefab, pos, Quaternion.identity) as GameObject;

        return clone;
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
                Destroy(((PlayingPieceTile)tile).PlayingPiece);
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

    public void Clear()
    {
        CastleTiles.Clear();
        PlayingPieceTiles.Clear();
        LandscapeTiles.Clear();
    }
}