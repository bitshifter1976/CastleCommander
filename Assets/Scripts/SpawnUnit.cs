using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnUnit : MonoBehaviour
{
    public List<TextMeshProUGUI> Heading;
    public List<TextMeshProUGUI> Text1;
    public List<TextMeshProUGUI> Text2;
    public List<TextMeshProUGUI> Text3;
    public List<TextMeshProUGUI> Text4;
    public List<TextMeshProUGUI> Text5;

    public Button ButtonSpawnArtillery;
    public Button ButtonSpawnCavalry;
    public Button ButtonSpawnInfantry;
    public Button ButtonSpawnMedic;
    public Board Board;
    public Vector3Int Position;

    private void OnEnable()
    {
        Heading[0].text = $"unit{Environment.NewLine}type";
        Heading[1].text = $"spawn";
        Heading[2].text = $"spawn{Environment.NewLine}costs";
        Heading[3].text = $"energy";
        Heading[4].text = $"attack";
        Heading[5].text = $"defense";
        Heading[6].text = $"speed";
        Heading[7].text = $"distance{Environment.NewLine}for attack";
        Heading[8].text = $"points{Environment.NewLine}for attack";

        CreateRow(Text1, GameTiles.Instance.PlayingPieceTileInfos.First(t => t.PlayingPieceType == PlayingPieceTile.PlayingPieceTileType.Artillery));
        CreateRow(Text2, GameTiles.Instance.CastleTileInfo);
        CreateRow(Text3, GameTiles.Instance.PlayingPieceTileInfos.First(t => t.PlayingPieceType == PlayingPieceTile.PlayingPieceTileType.Cavalry));
        CreateRow(Text4, GameTiles.Instance.PlayingPieceTileInfos.First(t => t.PlayingPieceType == PlayingPieceTile.PlayingPieceTileType.Infantry));
        CreateRow(Text5, GameTiles.Instance.PlayingPieceTileInfos.First(t => t.PlayingPieceType == PlayingPieceTile.PlayingPieceTileType.Medic));

        ButtonSpawnArtillery.onClick.AddListener(OnSpawnArtillery);
        ButtonSpawnCavalry.onClick.AddListener(OnSpawnCavalry);
        ButtonSpawnInfantry.onClick.AddListener(OnSpawnInfantry);
        ButtonSpawnMedic.onClick.AddListener(OnSpawnMedic);
        ChangeHighlightedColor(ButtonSpawnArtillery, Board.ActivePlayer.Color);
        ChangeHighlightedColor(ButtonSpawnCavalry, Board.ActivePlayer.Color);
        ChangeHighlightedColor(ButtonSpawnInfantry, Board.ActivePlayer.Color);
        ChangeHighlightedColor(ButtonSpawnMedic, Board.ActivePlayer.Color);
    }

    private void ChangeHighlightedColor(Button button, Color color)
    {
        var colors = button.colors;
        colors.highlightedColor = color;
        button.colors = colors;
    }

    private void OnSpawnMedic()
    {
        Board.CreatePlayingPiece(Position, PlayingPieceTile.PlayingPieceTileType.Medic);
        gameObject.SetActive(false);
    }

    private void OnSpawnInfantry()
    {
        Board.CreatePlayingPiece(Position, PlayingPieceTile.PlayingPieceTileType.Infantry);
        gameObject.SetActive(false);
    }

    private void OnSpawnCavalry()
    {
        Board.CreatePlayingPiece(Position, PlayingPieceTile.PlayingPieceTileType.Cavalry);
        gameObject.SetActive(false);
    }

    private void OnSpawnArtillery()
    {
        Board.CreatePlayingPiece(Position, PlayingPieceTile.PlayingPieceTileType.Artillery);
        gameObject.SetActive(false);
    }

    private static void CreateRow(List<TextMeshProUGUI> text, PlayingPieceTileInfo tileInfo)
    {
        text[0].text = tileInfo.PlayingPieceType.ToString().ToLower();
        text[1].text = tileInfo.SpawnCosts.ToString();
        text[2].text = tileInfo.Energy.ToString();
        text[3].text = tileInfo.Attack.ToString();
        text[4].text = tileInfo.Defense.ToString();
        text[5].text = tileInfo.Speed.ToString();
        text[6].text = tileInfo.DistanceForAttack.ToString();
        text[7].text = tileInfo.PointsForAttack.ToString();
    }

    private void CreateRow(List<TextMeshProUGUI> text, CastleTileInfo tileInfo)
    {
        text[0].text = "castle";
        text[1].text = "-";
        text[2].text = tileInfo.Energy.ToString();
        text[3].text = "-";
        text[4].text = tileInfo.Defense.ToString();
        text[5].text = "-";
        text[6].text = "-";
        text[7].text = "-";
    }
}
