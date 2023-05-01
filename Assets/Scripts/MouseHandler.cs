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

    public Tile SelectedLandscapeTile;
    public Tile MouseOverLandscapeTile;
    public Tile SelectedPlayingPiece;
    public Tile MouseOverPlayingPiece;

    public Vector3Int SelectedLandscapeTilePosition;
    public Vector3Int MouseOverLandscapeTilePosition;
    public Vector3Int SelectedPlayingPiecePosition;
    public Vector3Int MouseOverPlayingPiecePosition;

    public EventHandler OnClick;

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
                SelectedPlayingPiecePosition = MouseOverPlayingPiecePosition;
                SelectedPlayingPiece = TilemapPlayingPieces.GetTile<Tile>(SelectedPlayingPiecePosition);
            }
            if (MouseOverLandscapeTile != null)
            {
                SelectedLandscapeTilePosition = MouseOverLandscapeTilePosition;
                SelectedLandscapeTile = TilemapLandscape.GetTile<Tile>(SelectedLandscapeTilePosition);
            }
            if (OnClick != null)
                OnClick(this, new EventArgs());
        }
    }
}
