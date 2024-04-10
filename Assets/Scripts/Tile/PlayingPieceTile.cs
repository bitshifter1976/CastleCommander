using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static GameTile;

public class PlayingPieceTile : GameTile
{
    public enum PlayingPieceTileType
    {
        Artillery = 0,
        Cavalry = 1,
        Infantry = 2,
        Medic = 3
    }

    public enum AnimationType
    {
        Attack,
        Death,
        GetHit,
        Idle,
        Run,
        Victory,
        Walk
    }

    public PlayingPieceTileInfo Info;
    public PlayingPieceTileType PlayingPieceType;
    public Player Player;
    public GameObject PlayingPiece;
    private Animator animator;
    private AnimationType animationType;

    public override Vector3Int BoardPosition
    {
        get => base.BoardPosition;
        set
        {
            base.BoardPosition = value;
            if (PlayingPiece != null)
                PlayingPiece.transform.position = Tilemap.CellToWorld(value);
        }
    }

    public AnimationType Animation
    {
        get => animationType;
        set
        {
            animationType = value;
            animator.Play(animationType.ToString());
        }
    }

    public PlayingPieceTile(Vector3Int boardPosition, GameObject playingPiece, Tile tile, PlayingPieceTileInfo tileInfo, Tilemap tilemap, int id, Color color, PlayingPieceTileType type, Player player, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, color, TileType.PlayingPiece, movable, movementCost)
    {
        Info = tileInfo;
        PlayingPieceType = type;
        Player = player;
        PlayingPiece = playingPiece;
        BoardPosition = boardPosition;
        animator = PlayingPiece.GetComponent<Animator>();
        animationType = AnimationType.Idle;
        PlayingPiece.transform.Rotate(0, player.PlayerId == 1 ? 90 : 270, 0);
    }
}
