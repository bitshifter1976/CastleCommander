using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightBoard : MonoBehaviour
{
    public enum FightBoardState
    {
        Hidden,
        DiceRollStart,
        DicesRolling,
        ShowResult,
        WaitForClose,
        Close
    }

    public GameTile Tile1;
    public GameTile Tile2;
    public Dice Dice1;
    public Dice Dice2;

    public TextMeshProUGUI Heading1;
    public TextMeshProUGUI Text11;
    public TextMeshProUGUI Text12;
    public TextMeshProUGUI Text13;
    public TextMeshProUGUI Text14;
    public TextMeshProUGUI Text15;
    public TextMeshProUGUI Text16;
    public TextMeshProUGUI Text17;

    public TextMeshProUGUI Heading2;
    public TextMeshProUGUI Text21;
    public TextMeshProUGUI Text22;
    public TextMeshProUGUI Text23;
    public TextMeshProUGUI Text24;
    public TextMeshProUGUI Text25;
    public TextMeshProUGUI Text26;
    public TextMeshProUGUI Text27;

    public TextMeshProUGUI Result;
    public FightBoardState State;
    public Button CloseButton;

    public void Show()
    {
        SoundPlayer.Instance.Play("Battle");
        CloseButton.gameObject.SetActive(false);
        gameObject.SetActive(true);
        State = FightBoardState.DiceRollStart;
        Invoke("CloseBoard", 5f);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        State = FightBoardState.Hidden;
    }

    private void CloseBoard()
    {
        SoundPlayer.Instance.Stop("Battle");
        State = FightBoardState.Close;
    }

    private void Update()
    {
        if (State == FightBoardState.Hidden)
            return;

        switch (State)
        {
            case FightBoardState.DiceRollStart:
                {
                    UpdateInfo();
                    Result.text = string.Empty;
                    Dice1.Roll();
                    Dice2.Roll();
                    SoundPlayer.Instance.Play("DicesRolling");
                    State = FightBoardState.DicesRolling;
                    break;
                }
            case FightBoardState.DicesRolling:
                {
                    if (Dice1.RollingFinished() && Dice2.RollingFinished())
                        State = FightBoardState.ShowResult;
                    break;
                }
            case FightBoardState.ShowResult:
                {
                    if (Tile1 is PlayingPieceTile t1 && t1.Info.IsAttacker && Tile2 is PlayingPieceTile t2)
                    {
                        var attack = Dice1.Result + t1.Info.Attack;
                        var defense = Dice2.Result + t2.Info.Defense;
                        Result.text =  $"attack: {Dice1.Result} + {t1.Info.Attack} = {attack}{Environment.NewLine}{Environment.NewLine}";
                        Result.text += $"defense: {Dice2.Result} + {t2.Info.Defense} = {defense}{Environment.NewLine}{Environment.NewLine}";
                        var damage = attack - defense;
                        if (damage < 0)
                        {
                            t1.Info.Energy += damage;
                            Result.text += $"damage to player {t1.Player.PlayerId} : {-damage}";
                        }
                        else
                        {
                            t2.Info.Energy -= damage;
                            Result.text += $"damage to player {t2.Player.PlayerId}: {damage}";
                        }
                    }
                    if (Tile1 is PlayingPieceTile t3 && !t3.Info.IsAttacker && Tile2 is PlayingPieceTile t4)
                    {
                        var attack = Dice2.Result + t4.Info.Attack;
                        var defense = Dice1.Result + t3.Info.Defense;
                        Result.text = $"attack: {Dice2.Result} + {t4.Info.Attack} = {attack}{Environment.NewLine}{Environment.NewLine}";
                        Result.text += $"defense: {Dice1.Result} + {t3.Info.Defense} = {defense}{Environment.NewLine}{Environment.NewLine}";
                        var damage = attack - defense;
                        if (damage < 0)
                        {
                            t4.Info.Energy += damage;
                            Result.text += $"damage to player {t4.Player.PlayerId} : {-damage}";
                        }
                        else
                        {
                            t3.Info.Energy -= damage;
                            Result.text += $"damage to player {t3.Player.PlayerId}: {damage}";
                        }
                    }
                    else if (Tile1 is PlayingPieceTile t5 && t5.Info.IsAttacker && Tile2 is CastleTile t6)
                    {
                        var attack = Dice1.Result + t5.Info.Attack;
                        var defense = Dice2.Result + t6.Info.Attack;
                        Result.text = $"attack: {Dice1.Result} + {t5.Info.Attack} = {attack}{Environment.NewLine}{Environment.NewLine}";
                        Result.text += $"defense: {Dice2.Result} + {t6.Info.Defense} = {defense}{Environment.NewLine}{Environment.NewLine}";
                        var damage = attack - defense;
                        if (damage < 0)
                        {
                            t5.Info.Energy += damage;
                            Result.text += $"damage to player {t5.Player.PlayerId} : {-damage}";
                        }
                        else
                        {
                            t6.Info.Energy -= damage;
                            Result.text += $"damage to castle {t6.Player.PlayerId}: {damage}";
                        }
                    }
                    UpdateInfo();
                    State = FightBoardState.WaitForClose;
                    break;
                }
            case FightBoardState.WaitForClose:
                {
                    // wait for button close click
                    break;
                }
        }
    }

    private void UpdateInfo()
    {
        if (Tile1 != null && Tile1 is PlayingPieceTile t && t.Info != null)
        {
            Heading1.text = $"player {t.Player.PlayerId} " + (t.Info.IsAttacker ? " (attack)" : " (defend)");
            Text11.text = t.Info.PlayingPieceType.ToString().ToLower();
            Text12.text = t.Info.Energy.ToString();
            Text13.text = t.Info.Attack.ToString();
            Text14.text = t.Info.Defense.ToString();
            Text15.text = t.Info.Speed.ToString();
            Text16.text = t.Info.DistanceForAttack.ToString();
            Text17.text = t.Info.PointsForAttack.ToString();
        }
        if (Tile2 != null && Tile2 is PlayingPieceTile t2 && t2.Info != null)
        {
            Heading2.text = $"player {t2.Player.PlayerId} " + (t2.Info.IsAttacker ? " (attack)" : " (defend)");
            Text21.text = t2.Info.PlayingPieceType.ToString().ToLower();
            Text22.text = t2.Info.Energy.ToString();
            Text23.text = t2.Info.Attack.ToString();
            Text24.text = t2.Info.Defense.ToString();
            Text25.text = t2.Info.Speed.ToString();
            Text26.text = t2.Info.DistanceForAttack.ToString();
            Text27.text = t2.Info.PointsForAttack.ToString();
        }
        else if (Tile2 != null && Tile2 is CastleTile c && c.Info != null)
        {
            Heading2.text = $"player {c.Player.PlayerId} (defend)";
            Text21.text = "castle";
            Text22.text = c.Info.Energy.ToString();
            Text23.text = c.Info.Attack.ToString();
            Text24.text = c.Info.Defense.ToString();
            Text25.text = c.Info.Speed.ToString();
            Text26.text = "-";
            Text27.text = "-";
        }
    }
}
