using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitTypeInfo : MonoBehaviour
{
    public List<TextMeshProUGUI> Heading;
    public List<TextMeshProUGUI> Text1;
    public List<TextMeshProUGUI> Text2;
    public List<TextMeshProUGUI> Text3;
    public List<TextMeshProUGUI> Text4;
    public List<TextMeshProUGUI> Text5;

    public Button CloseButton;

    private void Start()
    {        
        Heading[0].text = "unit type";
        Heading[1].text = "energy";
        Heading[2].text = "attack";
        Heading[3].text = "defense";
        Heading[4].text = "speed";
        Heading[5].text = "dist att";
        Heading[6].text = "pts att";

        foreach (var tileInfo in GameTiles.Instance.PlayingPieceTileInfos)
        {
            CreateRow(Text1, GameTiles.Instance.PlayingPieceTileInfos.First(t => t.PlayingPieceType == PlayingPieceTile.PlayingPieceTileType.Artillery));
            CreateRow(Text2, GameTiles.Instance.CastleTileInfo);
            CreateRow(Text3, GameTiles.Instance.PlayingPieceTileInfos.First(t => t.PlayingPieceType == PlayingPieceTile.PlayingPieceTileType.Cavalry));
            CreateRow(Text4, GameTiles.Instance.PlayingPieceTileInfos.First(t => t.PlayingPieceType == PlayingPieceTile.PlayingPieceTileType.Infantry));
            CreateRow(Text5, GameTiles.Instance.PlayingPieceTileInfos.First(t => t.PlayingPieceType == PlayingPieceTile.PlayingPieceTileType.Medic));
        }

        gameObject.SetActive(true);
    }

    private static void CreateRow(List<TextMeshProUGUI> text, PlayingPieceTileInfo tileInfo)
    {
        text[0].text = tileInfo.PlayingPieceType.ToString().ToLower();
        text[1].text = tileInfo.Energy.ToString();
        text[2].text = tileInfo.Attack.ToString();
        text[3].text = tileInfo.Defense.ToString();
        text[4].text = tileInfo.Speed.ToString();
        text[5].text = tileInfo.DistanceForAttack.ToString();
        text[6].text = tileInfo.PointsForAttack.ToString();
    }

    private void CreateRow(List<TextMeshProUGUI> text, CastleTileInfo tileInfo)
    {
        text[0].text = "castle";
        text[1].text = tileInfo.Energy.ToString();
        text[2].text = "-";
        text[3].text = tileInfo.Defense.ToString();
        text[4].text = "-";
        text[5].text = "-";
        text[6].text = "-";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
