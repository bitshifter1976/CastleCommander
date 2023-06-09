using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionInfoBar : MonoBehaviour
{
    public TextMeshProUGUI Heading1;
    public TextMeshProUGUI Heading2;
    public TextMeshProUGUI Heading3;
    public TextMeshProUGUI Heading4;
    public TextMeshProUGUI Heading5;
    public TextMeshProUGUI Heading6; 
    public TextMeshProUGUI Heading7;
    public TextMeshProUGUI Text1;
    public TextMeshProUGUI Text2;
    public TextMeshProUGUI Text3;
    public TextMeshProUGUI Text4;
    public TextMeshProUGUI Text5;
    public TextMeshProUGUI Text6;
    public TextMeshProUGUI Text7;

    private PlayingPieceTile playingPiece;
    private CastleTile castle;
    private Image background;

    private void Start()
    {
        background = GetComponent<Image>();
        Hide();
    }

    public void Show(GameTile tile)
    {
        playingPiece = null;
        castle = null;
        if (tile is PlayingPieceTile p)
        {
            playingPiece = p;
            background.color = playingPiece.Player.Color;
        }
        else if (tile is CastleTile c)
        {
            castle = c;
            background.color = castle.Player.Color;
        }
        Heading1.text = "type";
        Heading2.text = "energy";
        Heading3.text = "attack";
        Heading4.text = "defense";
        Heading5.text = "speed";
        Heading6.text = $"dist att";
        Heading7.text = $"pts att";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playingPiece != null && playingPiece.Info != null)
        { 
            Text1.text = playingPiece.Info.PlayingPieceType.ToString().ToLower();
            Text2.text = playingPiece.Info.Energy.ToString();
            Text3.text = playingPiece.Info.Attack.ToString();
            Text4.text = playingPiece.Info.Defense.ToString();
            Text5.text = playingPiece.Info.Speed.ToString();
            Text6.text = playingPiece.Info.DistanceForAttack.ToString();
            Text7.text = playingPiece.Info.PointsForAttack.ToString();
        }
        else if (castle != null && castle.Info != null)
        {
            Text1.text = "castle";
            Text2.text = castle.Info.Energy.ToString();
            Text3.text = castle.Info.Attack.ToString();
            Text4.text = castle.Info.Defense.ToString();
            Text5.text = "-";
            Text6.text = "-";
            Text7.text = "-";
        }
    }
}
