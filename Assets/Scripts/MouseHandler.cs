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

    public Tile MouseOverLandscapeTile;
    public Tile MouseOverPlayingPiece;
    public Tile LeftSelectedLandscapeTile;
    public Tile LeftSelectedPlayingPiece;
    public Tile RightSelectedLandscapeTile;
    public Tile RightSelectedPlayingPiece;

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
        MouseOverPlayingPiece = TilemapPlayingPieces.GetTile<Tile>(MouseOverPlayingPiecePosition);
        MouseOverLandscapeTilePosition = TilemapLandscape.WorldToCell(mousePos);
        MouseOverLandscapeTile = TilemapLandscape.GetTile<Tile>(MouseOverLandscapeTilePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (MouseOverPlayingPiece != null)
            {
                LeftSelectedPlayingPiecePosition = MouseOverPlayingPiecePosition;
                LeftSelectedPlayingPiece = TilemapPlayingPieces.GetTile<Tile>(LeftSelectedPlayingPiecePosition);
            }
            if (MouseOverLandscapeTile != null)
            {
                LeftSelectedLandscapeTilePosition = MouseOverLandscapeTilePosition;
                LeftSelectedLandscapeTile = TilemapLandscape.GetTile<Tile>(LeftSelectedLandscapeTilePosition);
            }
            if (OnLeftClick != null)
                OnLeftClick(this, new EventArgs());
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (MouseOverPlayingPiece != null)
            {
                RightSelectedLandscapeTilePosition = MouseOverPlayingPiecePosition;
                RightSelectedPlayingPiece = TilemapPlayingPieces.GetTile<Tile>(RightSelectedPlayingPiecePosition);
            }
            if (MouseOverLandscapeTile != null)
            {
                RightSelectedLandscapeTilePosition = MouseOverLandscapeTilePosition;
                RightSelectedLandscapeTile = TilemapLandscape.GetTile<Tile>(RightSelectedLandscapeTilePosition);
            }
            if (OnRightClick != null)
                OnRightClick(this, new EventArgs());
        }
    }
}
