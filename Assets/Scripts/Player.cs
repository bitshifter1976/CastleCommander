using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        // get playing pieces for ai player
        var playingPieces = GameTiles.Instance.PlayingPieceTiles.Where(t => t.Value.Player.PlayerId == PlayerId).ToList();
        // if no found, select castle to spawn playing piece
        if (playingPieces.Count == 0)
        {
            var castle = GameTiles.Instance.CastleTiles.Values.First(c => c.Player.PlayerId == PlayerId);
            board.DoLeftClick(castle);
        }
        else
        {
            var position = GameTiles.Instance.LandscapeTiles.Values.First(t => t.Movable).BoardPosition;
            var tile = GameTiles.Instance.Get<LandscapeTile>(position);
            if (tile != null)
            {
                board.DoLeftClick(tile);
            }
        }

        Thinking = false;
    }
}
