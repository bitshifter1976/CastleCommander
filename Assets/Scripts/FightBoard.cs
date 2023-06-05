using System;
using TMPro;
using UnityEngine;

public class FightBoard : MonoBehaviour
{
    public enum FightBoardState
    {
        Hidden,
        DicesRolling,
        ShowResult,
        Close
    }

    public Player Player1;
    public Player Player2;
    public PlayingPieceTile Tile1;
    public PlayingPieceTile Tile2;
    public Dice Dice1;
    public Dice Dice2;

    public TextMeshProUGUI Heading1;
    public TextMeshProUGUI Text11;
    public TextMeshProUGUI Text12;
    public TextMeshProUGUI Text13;
    public TextMeshProUGUI Text14;
    public TextMeshProUGUI Text15;
    public TextMeshProUGUI Text16;

    public TextMeshProUGUI Heading2;
    public TextMeshProUGUI Text21;
    public TextMeshProUGUI Text22;
    public TextMeshProUGUI Text23;
    public TextMeshProUGUI Text24;
    public TextMeshProUGUI Text25;
    public TextMeshProUGUI Text26;

    public FightBoardState State;

    public void Show()
    {
        gameObject.SetActive(true);
        Dice1.Roll(); 
        Dice2.Roll();
        SoundPlayer.Instance.Play("DicesRolling");
        State = FightBoardState.DicesRolling;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        State = FightBoardState.Hidden;
    }

    private void Update()
    {
        if (State == FightBoardState.Hidden)
            return;

        if (Player1 != null && Tile1 != null && Tile1.Info != null)
        {
            Heading1.text = $"player {Player1.PlayerId} " + (Player1.IsAttacker ? " (attacking)" : " (defending)");
            Text11.text = Tile1.Info.PlayingPieceType.ToString().ToLower();
            Text12.text = Tile1.Info.Energy.ToString();
            Text13.text = Tile1.Info.Attack.ToString();
            Text14.text = Tile1.Info.Defense.ToString();
            Text15.text = Tile1.Info.Speed.ToString();
            Text16.text = Tile1.Info.DistanceForAttack.ToString();
        }
        if (Player2 != null && Tile2 != null && Tile2.Info != null)
        {
            Heading2.text = $"player {Player2.PlayerId} " + (Player2.IsAttacker ? " (attacking)" : " (defending)");
            Text21.text = Tile2.Info.PlayingPieceType.ToString().ToLower();
            Text22.text = Tile2.Info.Energy.ToString();
            Text23.text = Tile2.Info.Attack.ToString();
            Text24.text = Tile2.Info.Defense.ToString();
            Text25.text = Tile2.Info.Speed.ToString();
            Text26.text = Tile2.Info.DistanceForAttack.ToString();
        }

        switch (State)
        {
            case FightBoardState.DicesRolling:
                if (Dice1.RollingFinished() && Dice2.RollingFinished())
                    State = FightBoardState.ShowResult;
                break;
            case FightBoardState.ShowResult:
                //ActivePlayer.PointsLeft = activeDices.Sum(d => d.Result) * 10;
                break;
        }
    }
}
