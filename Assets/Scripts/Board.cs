using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;
using Aoiti.Pathfinding;
using static PlayingPieceTile;

public class Board : MonoBehaviour
{
    #region Definitions
    public enum BoardMode 
    { 
        WholeBoard 
    };

    public enum BoardState
    {
        Load,
        ConfirmLoad,
        CreateBoard,
        PlayerGetReady,
        RollDiceStart,
        RollDice,
        PlayRound,
        FinishRound,
        GameEnd,
        GameReload,
        GameClose
    }

    #endregion

    #region Properties
    [Header("********** Configuration **********")]
    [Header("General")]
    public BoardMode GeneratorMode = BoardMode.WholeBoard;
    public int BoardWidth = 24;
    public int BoardHeight = 14;
    [Header("Player")]
    public Player Player1;
    public Player Player2;
    [Header("Tilemaps")]
    public Tilemap TilemapPlayingPieces;
    public Tilemap TilemapPath;
    public Tilemap TilemapSelect;
    public Tilemap TilemapLandscape;
    public Tilemap TilemapUnderTiles;
    [Header("Input")]
    public MouseHandler MouseHandler;
    [Header("Hud")]
    public GameObject Hud;
    public Button ButtonReload;
    public Button ButtonEndTurn;
    public Button ButtonShowUnitTypeInfo;
    public Button ButtonHideUnitTypeInfo;
    public Timer Timer;
    public GameObject MessageBox;
    public Button MessageBoxButtonOk;
    public Button MessageBoxButtonCancel;
    public TextMeshProUGUI MessageBoxText;
    public TextMeshProUGUI MessageBoxButtonOkText;
    public TextMeshProUGUI MessageBoxButtonCancelText;
    public GameObject SelectUnitTypePrefab;
    public GameObject UnitTypeInfo;
    public SelectionInfoBar UnitTypeInfoBar;
    public FightBoard FightBoard;
    [Header("Dices")]
    public List<Dice> Dices;
    [Header("Timeouts")]
    public float TimeToAddTileSec = 0.01f;
    public float TimeToEndRoundSec = 30f;
    public float TimeToMovePlayingPieceSec = 0.1f;
    [Header("Playing piece")]
    public float AlphaSelected = 1f;
    public float AlphaUnselected = 100f/256f;
    [Header("********** Info **********")]
    [Header("General")]
    public Player ActivePlayer;
    public BoardState State = BoardState.Load;
    [Header("Playing piece movement")]
    public List<Vector3Int> MovementPath;
    public Vector3Int[] MovementDirectionsOdd  = new Vector3Int[6] { Vector3Int.left, Vector3Int.right, new Vector3Int(0,1,0)/*top-left*/, new Vector3Int(1,1,0)/*top-right*/, new Vector3Int(0, -1, 0)/*bottom-left*/, new Vector3Int(1,-1,0)/*bottom-right*/ };
    public Vector3Int[] MovementDirectionsEven = new Vector3Int[6] { Vector3Int.left, Vector3Int.right, new Vector3Int(-1, 1, 0)/*top-left*/, new Vector3Int(0, 1, 0)/*top-right*/, new Vector3Int(-1, -1, 0)/*bottom-left*/, new Vector3Int(0, -1, 0)/*bottom-right*/ };
    public bool AnimationRunning;
    public bool FightBoardShowing;

    private readonly Dictionary<Vector3Int, Tile> tilesLandscape = new();
    private readonly Dictionary<Vector3Int, Tile> tilesUnderTiles = new();
    private float timeElapsed = 0f;
    private PlayingPieceTile formerLeftSelectedPlayingPiece;
    private Pathfinder<Vector3Int> pathfinder;
    private GameObject selectUnitTypeBox;
    private TMP_Dropdown selectUnitTypeDropdown;
    private CastleTile leftSelectedCastle;
    private PlayingPieceTile leftSelectedPlayingPiece;
    #endregion

    private bool ShowMessageBox(string message, string buttonTextOk = "ok", string buttonTextCancel = null, BoardState? stateToTriggerOnOk = null, BoardState? stateToTriggerOnCancel = null)
    {
        if (!MessageBox.activeSelf)
        {
            MessageBoxText.text = message;
            // button ok
            var buttonOkEnabled = !string.IsNullOrEmpty(buttonTextOk);
            MessageBoxButtonOkText.text = buttonTextOk;
            MessageBoxButtonCancel.gameObject.SetActive(buttonOkEnabled);
            MessageBoxButtonOk.onClick.AddListener(() =>
            {
                MessageBoxButtonOk.onClick.RemoveAllListeners();
                MessageBox.SetActive(false);
                if (stateToTriggerOnOk.HasValue)
                    State = stateToTriggerOnOk.Value;
            }); 
            // button cancel
            var buttonCancelEnabled = !string.IsNullOrEmpty(buttonTextCancel);
            MessageBoxButtonCancelText.text = buttonTextCancel;
            MessageBoxButtonCancel.gameObject.SetActive(buttonCancelEnabled);
            MessageBoxButtonCancel.onClick.AddListener(() =>
            {
                MessageBoxButtonCancel.onClick.RemoveAllListeners();
                MessageBox.SetActive(false);
                if (stateToTriggerOnCancel.HasValue)
                    State = stateToTriggerOnCancel.Value;
            });
            MessageBox.SetActive(true);
        }
        return MessageBox.activeSelf;
    }

    private void Start()
    {
        MessageBox.SetActive(false);
        FightBoardShowing = false;
        ActivePlayer = Player1;
        Player1.Active = true;
        Player2.Active = false;
        pathfinder = new Pathfinder<Vector3Int>(GetDistance, ConnectionsAndCosts);
        ButtonReload.onClick.AddListener(OnReloadClick);
        ButtonShowUnitTypeInfo.onClick.AddListener(OnShowUnitTypeInfo);
        ButtonHideUnitTypeInfo.onClick.AddListener(OnHideUnitTypeInfo);
        ButtonEndTurn.onClick.AddListener(OnEndTurn);
        MouseHandler.OnLeftClick += OnBoardLeftClick;
        MouseHandler.OnRightClick += OnBoardRightClick;
    }

    private void OnHideUnitTypeInfo()
    {
        UnitTypeInfo.SetActive(false);
    }

    private void OnShowUnitTypeInfo()
    {
        UnitTypeInfo.SetActive(true);
    }

    private void OnEndTurn()
    {
        if (State == BoardState.PlayRound)
            State = BoardState.FinishRound;
    }

    private void OnReloadClick()
    {
        if (State == BoardState.PlayRound)
            State = BoardState.ConfirmLoad;
    }

    private void SwitchPlayer()
    {
        if (ActivePlayer == Player1)
        {
            ActivePlayer = Player2;
            Player1.Active = false;
            Player2.Active = true;
        }
        else
        {
            ActivePlayer = Player1;
            Player1.Active = true;
            Player2.Active = false;
        }
    }

    private void Update()
    {
        switch (State)
        {
            case BoardState.ConfirmLoad:
                {
                    ShowMessageBox("do you want to abort the game\nand load a new board?", "yes", "no", BoardState.Load, BoardState.PlayRound);
                    break;
                }
            case BoardState.Load:
                {
                    LoadBoard();
                    State = BoardState.CreateBoard;
                    break;
                }
            case BoardState.CreateBoard:
                {
                    timeElapsed += Time.deltaTime;
                    if (timeElapsed >= TimeToAddTileSec)
                    {
                        timeElapsed = 0;
                        if (!AddTile())
                            State = BoardState.PlayerGetReady;
                    }
                    break;
                }
            case BoardState.PlayerGetReady:
                {
                    ShowMessageBox($"player {ActivePlayer.PlayerId} get ready!", "go", null, BoardState.RollDiceStart);
                    SelectActiveCastle();
                    if (ActivePlayer.IsAi)
                        ActivePlayer.Think(this);
                    break;
                }
            case BoardState.RollDiceStart:
                {
                    var activeDices = Dices.Where(d => d.Player.PlayerId == ActivePlayer.PlayerId).ToList();
                    activeDices.ForEach(d => d.Roll());
                    SoundPlayer.Instance.Play("DicesRolling");
                    State = BoardState.RollDice;
                    break;
                }
            case BoardState.RollDice:
                {
                    var activeDices = Dices.Where(d => d.Player.PlayerId == ActivePlayer.PlayerId).ToList();
                    if (activeDices.All(d => d.RollingFinished()))
                    {
                        ActivePlayer.PointsLeft = activeDices.Sum(d => d.Result) * 10;
                        Timer.StartTimer(TimeToEndRoundSec);
                        State = BoardState.PlayRound;
                    }
                    break;
                }
            case BoardState.PlayRound:
                {
                    if (FightBoardShowing)
                    {
                        if (FightBoard.State == FightBoard.FightBoardState.Hidden)
                        {
                            FightBoard.Show();
                        }
                        else if (FightBoard.State == FightBoard.FightBoardState.Close)
                        {
                            FightBoardShowing = false;
                            FightBoard.Hide();
                            if (FightBoard.Tile1 is PlayingPieceTile playingPiece)
                                playingPiece.Animation = AnimationType.Idle;
                            if (FightBoard.Tile2 is PlayingPieceTile playingPiece2)
                                playingPiece2.Animation = AnimationType.Idle;
                            if (FightBoard.Tile1 is PlayingPieceTile t && t.Info.Energy <= 0)
                            {
                                t.Animation = AnimationType.Death;
                                StartCoroutine(DoDeleteAfterTime(FightBoard.Tile1, 5f));
                            }
                            else if (FightBoard.Tile2 is PlayingPieceTile t2 && t2.Info.Energy <= 0)
                            {
                                t2.Animation = AnimationType.Death;
                                StartCoroutine(DoDeleteAfterTime(FightBoard.Tile2, 5f));
                            }
                            else if (FightBoard.Tile2 is CastleTile c && c.Info.Energy <= 0)
                            {
                                GameTiles.Instance.Delete(FightBoard.Tile2);
                                var winningUnits = GameTiles.Instance.PlayingPieceTiles.Where(p => p.Value.Player.PlayerId != c.Player.PlayerId).ToList();
                                winningUnits.ForEach(p => p.Value.Animation = AnimationType.Victory);
                                State = BoardState.GameEnd;
                            }
                        }
                    }
                    else if (Timer.IsOver() || ActivePlayer.PointsLeft <= 0)
                    {
                        if (!AnimationRunning)
                        {
                            leftSelectedCastle = null;
                            leftSelectedPlayingPiece = null;
                            formerLeftSelectedPlayingPiece = null;
                            State = BoardState.FinishRound;
                        }
                    }
                    else
                    {
                        if (!AnimationRunning)
                        {
                            if (!ActivePlayer.IsAi)
                                ShowPath(MouseHandler.MouseOverLandscapeTilePosition);

                            if (leftSelectedPlayingPiece != null)
                                UnitTypeInfoBar.Show(leftSelectedPlayingPiece);
                            else if (leftSelectedCastle != null)
                                UnitTypeInfoBar.Show(leftSelectedCastle);
                            else
                                UnitTypeInfoBar.Hide();
                        }
                    }
                    break;
                }
            case BoardState.FinishRound:
                {
                    HideSelectUnitTypeBox();
                    DeselectPlayingPiece(formerLeftSelectedPlayingPiece);
                    TilemapPath.ClearAllTiles();
                    TilemapSelect.ClearAllTiles();
                    Timer.StopTimer();
                    SwitchPlayer();
                    State = BoardState.PlayerGetReady;
                    break;
                }
            case BoardState.GameEnd:
                {
                    ShowMessageBox($"player {ActivePlayer.PlayerId} won the game!", "ok", null, BoardState.GameReload);
                    break;
                }
            case BoardState.GameReload:
                {
                    ShowMessageBox($"do you want to load a new game?", "yes", "no", BoardState.Load, BoardState.GameClose);
                    break;
                }
            case BoardState.GameClose:
                {
                    Application.Quit();
                    break;
                }
        }
    }

    private IEnumerator DoDeleteAfterTime(GameTile tile, float timeSec)
    {
        yield return new WaitForSeconds(timeSec);
        GameTiles.Instance.Delete(tile);
    }

    public void LoadBoard()
    {
        Timer.StopTimer();
        TilemapPlayingPieces.ClearAllTiles();
        TilemapPath.ClearAllTiles();
        TilemapSelect.ClearAllTiles();
        TilemapLandscape.ClearAllTiles();
        TilemapUnderTiles.ClearAllTiles();
        tilesLandscape.Clear();
        tilesUnderTiles.Clear();
        GameTiles.Instance.Clear();
        Player1.PointsLeft = 0;
        Player1.SpawnsLeft = 8;
        Player1.Distance = 0;
        Player2.PointsLeft = 0;
        Player2.SpawnsLeft = 8;
        Player2.Distance = 0;
        CreateBoard();
    }

    private void CreateBoard()
    {
        if (GeneratorMode == BoardMode.WholeBoard)
        {
            var borderX = 2f;
            var borderY = 3f;
            var xMin = -BoardWidth / 2 - borderX;
            var xMax = BoardWidth / 2 + borderX;
            var yMin = -BoardHeight / 2 - borderY;
            var yMax = BoardHeight / 2 + borderY;
            // add landscape tiles with its' under tiles
            for (var x = xMin; x <= xMax; x++)
            {
                for (var y = yMax; y >= yMin; y--)
                {
                    var landscapeTile = (x >= -BoardWidth / 2 && x <= BoardWidth / 2) && (y >= -BoardHeight / 2 && y <= BoardHeight / 2) ? GameTiles.Instance.GetRandomLandscapeTile() : GameTiles.Instance.Ocean;
                    var position = new Vector3Int((int)Math.Round(x), (int)Math.Round(y), 0);
                    tilesLandscape.Add(position, landscapeTile);
                    tilesUnderTiles.Add(position, landscapeTile == GameTiles.Instance.Ocean ? GameTiles.Instance.UnderOcean : GameTiles.Instance.UnderDirt);
                    GameTiles.Instance.Add(GameTile.TileType.Landscape, position, landscapeTile, GameTiles.Instance.LandscapeTileInfos.First(ti => ti.Tile == landscapeTile).Clone(), null, null, TilemapLandscape, ActivePlayer, null, GameTiles.Instance.GetLandscapeType(landscapeTile));
                }
            }
            // add player castle tiles
            var x1 = -BoardWidth / 2 + 1;
            var x2 = BoardWidth / 2 - 1;
            var y1 = Random.Range(-BoardHeight / 2 + 1, BoardHeight / 2);
            var y2 = Random.Range(-BoardHeight / 2 + 1, BoardHeight / 2);
            var pos1 = new Vector3Int(x1, y1, 0);
            var pos2 = new Vector3Int(x2, y2, 0);
            tilesLandscape[pos1] = GameTiles.Instance.Castle1;
            tilesUnderTiles[pos1] = GameTiles.Instance.UnderDirt;
            tilesLandscape[pos2] = GameTiles.Instance.Castle2;
            tilesUnderTiles[pos2] = GameTiles.Instance.UnderDirt;
            GameTiles.Instance.Add(GameTile.TileType.Castle, pos1, GameTiles.Instance.Castle1, GameTiles.Instance.LandscapeTileInfos.First(ti => ti.Tile == GameTiles.Instance.Base).Clone(), null, GameTiles.Instance.CastleTileInfo.Clone(), TilemapLandscape, Player1, null, null);
            GameTiles.Instance.Add(GameTile.TileType.Castle, pos2, GameTiles.Instance.Castle2, GameTiles.Instance.LandscapeTileInfos.First(ti => ti.Tile == GameTiles.Instance.Base).Clone(), null, GameTiles.Instance.CastleTileInfo.Clone(), TilemapLandscape, Player2, null, null);
        }
    }

    private bool AddTile()
    {
        var tileCreated = false;

        if (GeneratorMode == BoardMode.WholeBoard)
        {
            if (tilesLandscape.Count > 0)
            {
                var tileInfo = tilesLandscape.OrderByDescending(t => t.Key.y).OrderBy(t => t.Key.x).First();
                tilesLandscape.Remove(tileInfo.Key);
                TilemapLandscape.SetTile(tileInfo.Key, tileInfo.Value);
                tileCreated = true;
            }
            if (tilesUnderTiles.Count > 0)
            {
                var tileInfo = tilesUnderTiles.OrderByDescending(t => t.Key.y).OrderBy(t => t.Key.x).First();
                tilesUnderTiles.Remove(tileInfo.Key);
                TilemapUnderTiles.SetTile(tileInfo.Key, tileInfo.Value);
                tileCreated = true;
            }
        }

        return tileCreated;
    }

    private void SelectActiveCastle()
    {
        var selectedPlayingField = GameTiles.Instance.GetCastle(ActivePlayer.PlayerId);
        var selectedCastle = GameTiles.Instance.Select;
        selectedCastle.color = new Color(ActivePlayer.Color.r, ActivePlayer.Color.g, ActivePlayer.Color.b, AlphaSelected);
        TilemapSelect.SetTile(selectedPlayingField.BoardPosition, selectedCastle);
    }

    public bool ShowPath(Vector3Int toPosition)
    {
        var movementPossible = false;
        TilemapPath.ClearAllTiles();
        if (formerLeftSelectedPlayingPiece != null && formerLeftSelectedPlayingPiece.Player.PlayerId == ActivePlayer.PlayerId)
        {
            ActivePlayer.Distance = GetDistance(formerLeftSelectedPlayingPiece.BoardPosition, toPosition);
            pathfinder.GenerateAstarPath(formerLeftSelectedPlayingPiece.BoardPosition, toPosition, out var path);
            var costs = 0f;
            movementPossible = true;
            foreach (var position in path)
            {
                var castle = GameTiles.Instance.Get<CastleTile>(position);
                var tile = GameTiles.Instance.Get<LandscapeTile>(position);
                costs += CalcualteMovementCosts(tile.MovementCost, formerLeftSelectedPlayingPiece.Info.Speed);
                var pathTile = GameTiles.Instance.Path;
                if (tile.Movable && costs <= ActivePlayer.PointsLeft && movementPossible && castle == null)
                {
                    pathTile.color = new Color(Color.green.r, Color.green.g, Color.green.b, AlphaUnselected);
                }
                else
                {
                    movementPossible = false;
                    pathTile.color = new Color(Color.red.r, Color.red.g, Color.red.b, AlphaUnselected);
                }
                TilemapPath.SetTile(position, pathTile);
            }
            MovementPath = movementPossible ? path : null;
        }
        return movementPossible;
    }

    private float CalcualteMovementCosts(int costs, int speed)
    {
        var maxSpeed = (float)GameTiles.Instance.PlayingPieceTileInfos.Max(p => p.Speed);
        return costs / (speed / maxSpeed * 2f);
    }

    public void SimulateLeftClick(GameTile tile)
    {
        if (tile == null)
            return;
        MouseHandler.LeftSelectedLandscapeTilePosition = tile.BoardPosition;
        MouseHandler.LeftSelectedPlayingPiecePosition = tile.BoardPosition;
        StartCoroutine(DoLeftClick());
    }

    private IEnumerator DoLeftClick()
    {
        OnBoardLeftClick(null, null);
        yield return null;
    }

    private void OnBoardLeftClick(object sender, EventArgs e)
    {
        if ((State != BoardState.PlayRound && !ActivePlayer.IsAi) || AnimationRunning)
            return;

        // get selected game tiles
        var leftSelectedLandscapeTile = GameTiles.Instance.Get<LandscapeTile>(MouseHandler.LeftSelectedLandscapeTilePosition);
        leftSelectedCastle = GameTiles.Instance.Get<CastleTile>(MouseHandler.LeftSelectedLandscapeTilePosition);
        leftSelectedPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(MouseHandler.LeftSelectedPlayingPiecePosition);
        var tileToMoveToHasPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(MouseHandler.LeftSelectedLandscapeTilePosition) != null;
        // if not prior selected playing piece, select it for movement
        if (formerLeftSelectedPlayingPiece == null && leftSelectedPlayingPiece != null && leftSelectedPlayingPiece.Player.PlayerId == ActivePlayer.PlayerId)
            SelectPlayingPiece(leftSelectedPlayingPiece);
        // if prior selected playing piece, move it
        else if (formerLeftSelectedPlayingPiece != null && leftSelectedLandscapeTile != null && formerLeftSelectedPlayingPiece.Player.PlayerId == ActivePlayer.PlayerId && MovementPath != null && !tileToMoveToHasPlayingPiece)
            MovePlayingPiece();
        // if prior selected playing piece selected again, deselect it
        else if (formerLeftSelectedPlayingPiece != null && leftSelectedPlayingPiece != null && formerLeftSelectedPlayingPiece == leftSelectedPlayingPiece)
            DeselectPlayingPiece(leftSelectedPlayingPiece);
        // if own castle clicked, spawn playing piece
        else if (leftSelectedCastle != null && leftSelectedCastle.Player.PlayerId == ActivePlayer.PlayerId)
            PlacePlayingPiece(MouseHandler.LeftSelectedLandscapeTilePosition);
    }

    public bool SimulateRightClick(GameTile tile)
    {
        if (tile == null)
            return false;
        MouseHandler.RightSelectedLandscapeTilePosition = tile.BoardPosition;
        MouseHandler.RightSelectedPlayingPiecePosition = tile.BoardPosition;
        if (AttackPossible())
        {
            StartCoroutine(DoAttack());
            return true;
        }
        return false;
    }

    private IEnumerator DoAttack()
    {
        Attack();
        yield return null;
    }

    private void OnBoardRightClick(object sender, EventArgs e)
    {
        if (State != BoardState.PlayRound || AnimationRunning)
            return;

        Attack();
    }

    private bool AttackPossible()
    {
        // get selected game tiles
        var rightSelectedPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(MouseHandler.RightSelectedPlayingPiecePosition);
        var rightSelectedCaste = GameTiles.Instance.Get<CastleTile>(MouseHandler.RightSelectedLandscapeTilePosition);
        // if left selected playing piece and right selected playing piece
        if (leftSelectedPlayingPiece != null && rightSelectedPlayingPiece != null && (rightSelectedCaste == null || rightSelectedCaste.BoardPosition == rightSelectedPlayingPiece.BoardPosition) && ActivePlayer.PointsLeft >= leftSelectedPlayingPiece.Info.PointsForAttack)
        {
            var attacker = leftSelectedPlayingPiece;
            var defender = rightSelectedPlayingPiece;
            ActivePlayer.Distance = GetDistance(attacker.BoardPosition, defender.BoardPosition);
            switch (attacker.PlayingPieceType)
            {
                case PlayingPieceTile.PlayingPieceTileType.Artillery:
                case PlayingPieceTile.PlayingPieceTileType.Cavalry:
                case PlayingPieceTile.PlayingPieceTileType.Infantry:
                    // if defender in attack range, do attack
                    if (attacker.Player.PlayerId == ActivePlayer.PlayerId && defender.Player.PlayerId != ActivePlayer.PlayerId && ActivePlayer.Distance <= attacker.Info.DistanceForAttack)
                    {
                        return true;
                    }
                    break;
                case PlayingPieceTile.PlayingPieceTileType.Medic:
                    // if attacker next to defender, do healing
                    if (attacker.Player.PlayerId == ActivePlayer.PlayerId && defender.Player.PlayerId == ActivePlayer.PlayerId && ActivePlayer.Distance <= attacker.Info.DistanceForAttack)
                    {
                        return true;
                    }
                    break;
            }
        }
        // if prior left selected playing piece and right selected castle
        else if (leftSelectedPlayingPiece != null && rightSelectedCaste != null && ActivePlayer.PointsLeft >= leftSelectedPlayingPiece.Info.PointsForAttack)
        {
            var attacker = leftSelectedPlayingPiece;
            var defender = rightSelectedCaste;
            ActivePlayer.Distance = GetDistance(attacker.BoardPosition, defender.BoardPosition);
            if (attacker.Player.PlayerId == ActivePlayer.PlayerId && defender.Player.PlayerId != ActivePlayer.PlayerId && ActivePlayer.Distance <= attacker.Info.DistanceForAttack)
            {
                return true;
            }
        }
        return false;
    }

    private bool Attack()
    {
        var enemyAttacked = false;
        // get selected game tiles
        var rightSelectedPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(MouseHandler.RightSelectedPlayingPiecePosition);
        var rightSelectedCaste = GameTiles.Instance.Get<CastleTile>(MouseHandler.RightSelectedLandscapeTilePosition);
        // if left selected playing piece and right selected playing piece
        if (leftSelectedPlayingPiece != null && rightSelectedPlayingPiece != null && (rightSelectedCaste == null || rightSelectedCaste.BoardPosition == rightSelectedPlayingPiece.BoardPosition) && ActivePlayer.PointsLeft >= leftSelectedPlayingPiece.Info.PointsForAttack)
        {
            var attacker = leftSelectedPlayingPiece;
            var defender = rightSelectedPlayingPiece;
            ActivePlayer.Distance = GetDistance(attacker.BoardPosition, defender.BoardPosition);
            switch (attacker.PlayingPieceType)
            {
                case PlayingPieceTile.PlayingPieceTileType.Artillery:
                case PlayingPieceTile.PlayingPieceTileType.Cavalry:
                case PlayingPieceTile.PlayingPieceTileType.Infantry:
                    // if defender in attack range, do attack
                    if (attacker.Player.PlayerId == ActivePlayer.PlayerId && defender.Player.PlayerId != ActivePlayer.PlayerId && ActivePlayer.Distance <= attacker.Info.DistanceForAttack)
                    {
                        ActivePlayer.PointsLeft -= leftSelectedPlayingPiece.Info.PointsForAttack;
                        attacker.Animation = PlayingPieceTile.AnimationType.Attack;
                        defender.Animation = PlayingPieceTile.AnimationType.GetHit;
                        ShowFightBoard(attacker, defender, ActivePlayer.Distance > 2);
                        enemyAttacked = true;
                    }
                    break;
                case PlayingPieceTile.PlayingPieceTileType.Medic:
                    // if attacker next to defender, do healing
                    if (attacker.Player.PlayerId == ActivePlayer.PlayerId && defender.Player.PlayerId == ActivePlayer.PlayerId && ActivePlayer.Distance <= attacker.Info.DistanceForAttack)
                    {
                        ActivePlayer.PointsLeft -= leftSelectedPlayingPiece.Info.PointsForAttack;
                        HealPlayingPiece(attacker, defender);
                        enemyAttacked = true;
                    }
                    break;
            }
        }
        // if prior left selected playing piece and right selected castle
        else if (leftSelectedPlayingPiece != null && rightSelectedCaste != null && ActivePlayer.PointsLeft >= leftSelectedPlayingPiece.Info.PointsForAttack)
        {
            var attacker = leftSelectedPlayingPiece;
            var defender = rightSelectedCaste;
            ActivePlayer.Distance = GetDistance(attacker.BoardPosition, defender.BoardPosition);
            if (attacker.Player.PlayerId == ActivePlayer.PlayerId && defender.Player.PlayerId != ActivePlayer.PlayerId && ActivePlayer.Distance <= attacker.Info.DistanceForAttack)
            {
                ActivePlayer.PointsLeft -= leftSelectedPlayingPiece.Info.PointsForAttack;
                ShowFightBoard(attacker, defender, ActivePlayer.Distance > 2);
                enemyAttacked = true;
            }
        }
        
        return enemyAttacked;
    }

    private void HealPlayingPiece(PlayingPieceTile attacker, PlayingPieceTile defender)
    {
        SoundPlayer.Instance.Play("Heal");
        defender.Info.Energy = PlayingPieceTileInfo.MaxEnergy;
    }

    private void ShowFightBoard(GameTile attacker, GameTile defender, bool rangedAttack)
    {
        SoundPlayer.Instance.Play(rangedAttack ? "RangedAttack" : "Attack");
        if (attacker is PlayingPieceTile a && defender is PlayingPieceTile d)
        {
            a.Info.IsAttacker = true;
            d.Info.IsAttacker = false;
            FightBoard.Tile1 = a.Player.PlayerId == 1 ? a : d;
            FightBoard.Tile2 = a.Player.PlayerId == 2 ? a : d;
        }
        else if (attacker is PlayingPieceTile a2 && defender is CastleTile c)
        {
            a2.Info.IsAttacker = true;
            FightBoard.Tile1 = a2;
            FightBoard.Tile2 = c;
        }
        FightBoardShowing = true;
    }

    private void PlacePlayingPiece(Vector3Int position)
    {
        if (selectUnitTypeBox != null || ActivePlayer.SpawnsLeft < 1)
            return;

        ShowSelectUnitType(position);
    }

    private void ShowSelectUnitType(Vector3Int position)
    {
        selectUnitTypeBox = Instantiate(SelectUnitTypePrefab, Vector3.zero, Quaternion.identity);
        selectUnitTypeBox.transform.SetParent(Hud.transform, false);
        selectUnitTypeDropdown = selectUnitTypeBox.GetComponentInChildren<TMP_Dropdown>(true);
        selectUnitTypeDropdown.onValueChanged.AddListener((choice) =>
        {
            var maxEnumValue = Enum.GetValues(typeof(PlayingPieceTileType)).Cast<int>().Max();
            if (choice <= maxEnumValue)
            {
                var playingPieceType = (PlayingPieceTileType)choice;
                Tile tile = null;
                switch (playingPieceType)
                {
                    case PlayingPieceTileType.Artillery:
                        tile = ActivePlayer.ArtilleryTile;
                        break;
                    case PlayingPieceTileType.Cavalry:
                        tile = ActivePlayer.CavalryTile;
                        break;
                    case PlayingPieceTileType.Infantry:
                        tile = ActivePlayer.InfantryTile;
                        break;
                    case PlayingPieceTileType.Medic:
                        tile = ActivePlayer.MedicTile;
                        break;
                }
                ActivePlayer.SpawnsLeft--;
                var tileInfo = GameTiles.Instance.PlayingPieceTileInfos.First(i => i.PlayingPieceType == playingPieceType).Clone();
                var tile2 = GameTiles.Instance.Add(GameTile.TileType.PlayingPiece, position, tile, null, tileInfo, null, TilemapPlayingPieces, ActivePlayer, playingPieceType, null);
                SelectPlayingPiece(tile2 as PlayingPieceTile);
            }

            Destroy(selectUnitTypeBox);
        });
    }

    private void HideSelectUnitTypeBox()
    {
        if (selectUnitTypeBox != null)
            Destroy(selectUnitTypeBox);
    }

    public void DoSelectUnitType(PlayingPieceTileType type)
    {
        selectUnitTypeDropdown.value = (int)type;
    }

    private void SelectPlayingPiece(PlayingPieceTile playingPiece)
    {
        SoundPlayer.Instance.Play("Select");
        var origColor = playingPiece.Tile.color;
        var color = new Color(origColor.r, origColor.g, origColor.b, AlphaSelected);
        playingPiece.Tile.color = color;
        formerLeftSelectedPlayingPiece = playingPiece;
        MouseHandler.LeftSelectedPlayingPiecePosition = playingPiece.BoardPosition;
        MouseHandler.LeftSelectedPlayingPiece = playingPiece;
        var selectedPlayingField = GameTiles.Instance.GetCastle(ActivePlayer.PlayerId);
        var selectedCastle = GameTiles.Instance.Select;
        selectedCastle.color = new Color(ActivePlayer.Color.r, ActivePlayer.Color.g, ActivePlayer.Color.b, AlphaSelected);
        TilemapSelect.SetTile(selectedPlayingField.BoardPosition, selectedCastle);
    }

    private void DeselectPlayingPiece(PlayingPieceTile playingPiece)
    {
        if (playingPiece != null)
        {
            SoundPlayer.Instance.Play("Select");
            var origColor = playingPiece.Tile.color;
            var color = new Color(origColor.r, origColor.g, origColor.b, AlphaUnselected);
            playingPiece.Tile.color = color;
            formerLeftSelectedPlayingPiece = null;
            leftSelectedPlayingPiece = null;
            MouseHandler.LeftSelectedPlayingPiecePosition = Vector3Int.zero;
            MouseHandler.LeftSelectedPlayingPiece = null;
        }
    }

    private void MovePlayingPiece()
    {
        if (MovementPath.Count > 0 && formerLeftSelectedPlayingPiece != null && formerLeftSelectedPlayingPiece.BoardPosition != null)
        {
            SoundPlayer.Instance.Play("Marching");
            AnimationRunning = true;
            StartCoroutine(MoveFormerSelectedPlayingPiece());
        }
    }

    IEnumerator MoveFormerSelectedPlayingPiece()
    {
        var movementCosts = GameTiles.Instance.LandscapeTiles.Values.Where(t => MovementPath.Contains(t.BoardPosition)).Sum(t => t.MovementCost);
        var costs = CalcualteMovementCosts(movementCosts, formerLeftSelectedPlayingPiece.Info.Speed);
        if (costs <= ActivePlayer.PointsLeft)
        {
            // animate playing piece
            formerLeftSelectedPlayingPiece.Animation = PlayingPieceTile.AnimationType.Walk;
            while (MovementPath.Count > 0)
            {
                // get next position
                var position = MovementPath[0];
                MovementPath.RemoveAt(0);
                // update tile info
                GameTiles.Instance.Move(formerLeftSelectedPlayingPiece, position);
                // select tile
                if (MovementPath.Count > 0)
                    SelectPlayingPiece(formerLeftSelectedPlayingPiece);
                else
                {
                    formerLeftSelectedPlayingPiece.Animation = PlayingPieceTile.AnimationType.Idle;
                    DeselectPlayingPiece(formerLeftSelectedPlayingPiece);
                }
                // wait
                yield return new WaitForSeconds(TimeToMovePlayingPieceSec);
            }
            // calculate points for movement
            ActivePlayer.PointsLeft -= (int)Math.Round(costs);
            SoundPlayer.Instance.Stop("Marching");
        }
        AnimationRunning = false;
    }

    public float GetDistance(Vector3Int a, Vector3Int b)
    {
        return (a - b).sqrMagnitude;
    }

    private Dictionary<Vector3Int, float> ConnectionsAndCosts(Vector3Int position)
    {
        var result = new Dictionary<Vector3Int, float>();
        var directions = position.y % 2 == 0 ? MovementDirectionsEven : MovementDirectionsOdd;
        foreach (var dir in directions)
        {
            var tile = GameTiles.Instance.Get<LandscapeTile>(position + dir);
            if (tile != null && tile.Movable)
                result.Add(position + dir, tile.MovementCost);
        }
        return result;
    }
}
