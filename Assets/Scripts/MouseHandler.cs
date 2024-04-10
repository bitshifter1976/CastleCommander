using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseHandler : MonoBehaviour
{
    public Tilemap TilemapLandscape; 
    public Tilemap TilemapPlayingPieces;

    public GameTile MouseOverLandscapeTile;
    public GameTile MouseOverPlayingPiece;
    public GameTile LeftSelectedLandscapeTile;
    public GameTile LeftSelectedPlayingPiece;
    public GameTile RightSelectedLandscapeTile;
    public GameTile RightSelectedPlayingPiece;

    public Vector3Int MouseOverLandscapeTilePosition;
    public Vector3Int MouseOverPlayingPiecePosition;
    public Vector3Int LeftSelectedLandscapeTilePosition;
    public Vector3Int LeftSelectedPlayingPiecePosition;
    public Vector3Int RightSelectedLandscapeTilePosition;
    public Vector3Int RightSelectedPlayingPiecePosition;

    public EventHandler OnLeftClick;
    public EventHandler OnRightClick;

    private void Update()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        MouseOverPlayingPiecePosition = TilemapPlayingPieces.WorldToCell(mousePos);
        MouseOverPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(MouseOverPlayingPiecePosition);
        MouseOverLandscapeTilePosition = TilemapLandscape.WorldToCell(mousePos);
        MouseOverLandscapeTile = GameTiles.Instance.Get<LandscapeTile>(MouseOverLandscapeTilePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (MouseOverPlayingPiece != null)
            {
                LeftSelectedPlayingPiecePosition = MouseOverPlayingPiecePosition;
                LeftSelectedPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(LeftSelectedPlayingPiecePosition);
            }
            if (MouseOverLandscapeTile != null)
            {
                LeftSelectedLandscapeTilePosition = MouseOverLandscapeTilePosition;
                LeftSelectedLandscapeTile = GameTiles.Instance.Get<LandscapeTile>(LeftSelectedLandscapeTilePosition);
            }
            if (OnLeftClick != null)
                OnLeftClick(this, new EventArgs());
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (MouseOverPlayingPiece != null)
            {
                RightSelectedPlayingPiecePosition = MouseOverPlayingPiecePosition;
                RightSelectedPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(RightSelectedPlayingPiecePosition);
            }
            if (MouseOverLandscapeTile != null)
            {
                RightSelectedLandscapeTilePosition = MouseOverLandscapeTilePosition;
                RightSelectedLandscapeTile = GameTiles.Instance.Get<LandscapeTile>(RightSelectedLandscapeTilePosition);
            }
            if (OnRightClick != null)
                OnRightClick(this, new EventArgs());
        }
    }
}
