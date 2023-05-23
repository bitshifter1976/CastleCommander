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

public class BoardController : MonoBehaviour
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
        GameEnd
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
    public Tilemap TilemapCastleSelect;
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
    public int PointsForMovement;
    public int MovementCosts;
    public List<Vector3Int> MovementPath;
    public Vector3Int[] MovementDirectionsOdd  = new Vector3Int[6] { Vector3Int.left, Vector3Int.right, new Vector3Int(0,1,0)/*top-left*/, new Vector3Int(1,1,0)/*top-right*/, new Vector3Int(0, -1, 0)/*bottom-left*/, new Vector3Int(1,-1,0)/*bottom-right*/ };
    public Vector3Int[] MovementDirectionsEven = new Vector3Int[6] { Vector3Int.left, Vector3Int.right, new Vector3Int(-1, 1, 0)/*top-left*/, new Vector3Int(0, 1, 0)/*top-right*/, new Vector3Int(-1, -1, 0)/*bottom-left*/, new Vector3Int(0, -1, 0)/*bottom-right*/ };

    private readonly Dictionary<Vector3Int, Tile> tilesLandscape = new();
    private readonly Dictionary<Vector3Int, Tile> tilesUnderTiles = new();
    private float timeElapsed = 0f;
    private LandscapeTile selectedLandscapeTile;
    private CastleTile selectedCastle;
    private PlayingPieceTile selectedPlayingPiece;
    private PlayingPieceTile formerSelectedPlayingPiece;
    private Pathfinder<Vector3Int> pathfinder;
    private GameObject selectUnitTypeBox;
    private TMP_Dropdown selectUnitTypeDropdown;
    #endregion

    private void ShowMessageBox(string message, string buttonTextOk = "ok", string buttonTextCancel = null, BoardState? stateToTriggerOnOk = null, BoardState? stateToTriggerOnCancel = null)
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
    }

    private void Start()
    {
        MessageBox.SetActive(false);
        ActivePlayer = Player1;
        pathfinder = new Pathfinder<Vector3Int>(DistanceFunc, ConnectionsAndCosts);
        ButtonReload.onClick.AddListener(OnReloadClick);
        ButtonShowUnitTypeInfo.onClick.AddListener(OnShowUnitTypeInfo);
        ButtonHideUnitTypeInfo.onClick.AddListener(OnHideUnitTypeInfo);
        ButtonEndTurn.onClick.AddListener(OnEndTurn);
        MouseHandler.OnClick += OnBoardClick;
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
        ActivePlayer = ActivePlayer == Player1 ? Player2 : Player1;
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
                        PointsForMovement = activeDices.Sum(d => d.Result) * 10;
                        Timer.StartTimer(TimeToEndRoundSec);
                        State = BoardState.PlayRound;
                    }
                    break;
                }
            case BoardState.PlayRound:
                {
                    ShowPath();
                    // round over?
                    if (Timer.IsOver() || PointsForMovement <= 0)
                        State = BoardState.FinishRound;
                    break;
                }
            case BoardState.FinishRound:
                {
                    HideSelectUnitTypeBox();
                    DeselectPlayingPiece(formerSelectedPlayingPiece);
                    TilemapPath.ClearAllTiles();
                    TilemapCastleSelect.ClearAllTiles();
                    Timer.StopTimer();
                    SwitchPlayer();
                    State = BoardState.PlayerGetReady;
                    break;
                }
            case BoardState.GameEnd:
                {
                    break;
                }
        }
    }

    public void LoadBoard()
    {
        Timer.StopTimer();
        TilemapPlayingPieces.ClearAllTiles();
        TilemapPath.ClearAllTiles();
        TilemapCastleSelect.ClearAllTiles();
        TilemapLandscape.ClearAllTiles();
        TilemapUnderTiles.ClearAllTiles();
        ActivePlayer = Player1;
        CreateBoard();
    }

    private void CreateBoard()
    {
        if (GeneratorMode == BoardMode.WholeBoard)
        {
            var xMin = -BoardWidth / 2;
            var xMax = BoardWidth / 2;
            var yMin = -BoardHeight / 2;
            var yMax = BoardHeight / 2;
            // add landscape tiles with its' under tiles
            for (var x = xMin; x <= xMax; x++)
            {
                for (var y = yMax; y >= yMin; y--)
                {
                    var landscapeTile = GameTiles.Instance.GetRandomLandscapeTile();
                    var position = new Vector3Int(x, y, 0);
                    tilesLandscape.Add(position, landscapeTile);
                    tilesUnderTiles.Add(position, landscapeTile == GameTiles.Instance.Ocean ? GameTiles.Instance.UnderOcean : GameTiles.Instance.UnderDirt);
                    GameTiles.Instance.Add(GameTile.TileType.Landscape, position, GameTiles.Instance.GetLandscapeTileInfos().First(ti => ti.Tile == landscapeTile), TilemapLandscape, ActivePlayer, PlayingPieceTile.PlayingPieceTileType.None, GameTiles.Instance.GetLandscapeType(landscapeTile));
                }
            }
            // add player castle tiles
            var x1 = xMin + 1;
            var x2 = xMax - 1;
            var y1 = Random.Range(yMin, yMax);
            var y2 = Random.Range(yMin, yMax);
            var pos1 = new Vector3Int(x1, y1, 0);
            var pos2 = new Vector3Int(x2, y2, 0);
            tilesLandscape[pos1] = GameTiles.Instance.Castle1;
            tilesUnderTiles[pos1] = GameTiles.Instance.UnderDirt;
            tilesLandscape[pos2] = GameTiles.Instance.Castle2;
            tilesUnderTiles[pos2] = GameTiles.Instance.UnderDirt;
            GameTiles.Instance.Add(GameTile.TileType.Castle, pos1, GameTiles.Instance.GetLandscapeTileInfos().First(ti => ti.Tile == GameTiles.Instance.Base), TilemapLandscape, Player1);
            GameTiles.Instance.Add(GameTile.TileType.Castle, pos2, GameTiles.Instance.GetLandscapeTileInfos().First(ti => ti.Tile == GameTiles.Instance.Base), TilemapLandscape, Player2);
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

    private void HideSelectUnitTypeBox()
    {
        if (selectUnitTypeBox != null)
            Destroy(selectUnitTypeBox);
    }

    private void SelectActiveCastle()
    {
        var selectedPlayingField = GameTiles.Instance.GetCastle(ActivePlayer.PlayerId);
        var selectedCastle = GameTiles.Instance.Select;
        selectedCastle.color = new Color(ActivePlayer.Color.r, ActivePlayer.Color.g, ActivePlayer.Color.b, AlphaSelected);
        TilemapCastleSelect.SetTile(selectedPlayingField.BoardPosition, selectedCastle);
    }

    private void ShowPath()
    {
        TilemapPath.ClearAllTiles();
        if (formerSelectedPlayingPiece != null && formerSelectedPlayingPiece.Player.PlayerId == ActivePlayer.PlayerId)
        {
            pathfinder.GenerateAstarPath(formerSelectedPlayingPiece.BoardPosition, MouseHandler.MouseOverLandscapeTilePosition, out var path);
            var costs = 0;
            var movementPossible = true;
            foreach (var position in path)
            {
                var tile = GameTiles.Instance.Get<LandscapeTile>(position);
                var playingPiece = GameTiles.Instance.Get<PlayingPieceTile>(position);
                costs += tile.MovementCost;
                var pathTile = GameTiles.Instance.Path;
                if (tile.Movable && costs <= PointsForMovement && playingPiece == null && movementPossible)
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
    }

    private void OnBoardClick(object sender, EventArgs e)
    {
        if (State != BoardState.PlayRound)
            return;

        // get selected game tiles
        selectedLandscapeTile = GameTiles.Instance.Get<LandscapeTile>(MouseHandler.SelectedLandscapeTilePosition);
        selectedCastle = GameTiles.Instance.Get<CastleTile>(MouseHandler.SelectedLandscapeTilePosition);
        selectedPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(MouseHandler.SelectedPlayingPiecePosition);
        var tileToMoveToHasPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(MouseHandler.SelectedLandscapeTilePosition) != null;

        // if not prior selected playing piece, select it for movement
        if (formerSelectedPlayingPiece == null && selectedPlayingPiece != null && selectedPlayingPiece.Player.PlayerId == ActivePlayer.PlayerId)
            SelectPlayingPiece(selectedPlayingPiece);
        // if prior selected playing piece, move it
        else if (formerSelectedPlayingPiece != null && selectedLandscapeTile != null && formerSelectedPlayingPiece.Player.PlayerId == ActivePlayer.PlayerId && MovementPath != null && !tileToMoveToHasPlayingPiece)
            MovePlayingPiece(formerSelectedPlayingPiece);
        // if prior selected playing piece selected again, deselect it
        else if (formerSelectedPlayingPiece != null && selectedPlayingPiece != null && formerSelectedPlayingPiece == selectedPlayingPiece)
            DeselectPlayingPiece(selectedPlayingPiece);
        // if own castle clicked, spawn playing piece
        else if (selectedCastle != null && selectedCastle.Player.PlayerId == ActivePlayer.PlayerId)
            PlacePlayingPiece(MouseHandler.SelectedLandscapeTilePosition);
    }

    private void PlacePlayingPiece(Vector3Int position)
    {
        if (selectUnitTypeBox != null)
            return;
        
        selectUnitTypeBox = Instantiate(SelectUnitTypePrefab, Vector3.zero, Quaternion.identity);
        selectUnitTypeBox.transform.SetParent(Hud.transform, false);
        selectUnitTypeDropdown = selectUnitTypeBox.GetComponentInChildren<TMP_Dropdown>(true);
        selectUnitTypeDropdown.onValueChanged.AddListener((choice) =>
        {
            var playingPieceType = (PlayingPieceTile.PlayingPieceTileType)choice;
            Tile tile = null;
            switch (playingPieceType)
            {
                case PlayingPieceTile.PlayingPieceTileType.Infantry:
                    tile = ActivePlayer.InfantryTile;
                    break;
                case PlayingPieceTile.PlayingPieceTileType.Artillery:
                    tile = ActivePlayer.ArtilleryTile;
                    break;
                case PlayingPieceTile.PlayingPieceTileType.Medic:
                    tile = ActivePlayer.MedicTile;
                    break;
            }
            if (tile != null)
            {
                var tileInfo = GameTiles.Instance.Add(GameTile.TileType.PlayingPiece, position, new TileInfo(tile, 1, 0), TilemapPlayingPieces, ActivePlayer, playingPieceType);
                SelectPlayingPiece(tileInfo as PlayingPieceTile);
            }
            Destroy(selectUnitTypeBox);
        });
    }

    private void SelectPlayingPiece(PlayingPieceTile playingPiece)
    {
        var origColor = playingPiece.Tile.color;
        var color = new Color(origColor.r, origColor.g, origColor.b, AlphaSelected);
        playingPiece.Tile.color = color;
        TilemapPlayingPieces.SetTile(playingPiece.BoardPosition, null);
        TilemapPlayingPieces.SetTile(playingPiece.BoardPosition, playingPiece.Tile);
        formerSelectedPlayingPiece = playingPiece;
        MouseHandler.SelectedPlayingPiecePosition = playingPiece.BoardPosition;
        MouseHandler.SelectedPlayingPiece = playingPiece.Tile;
    }

    private void DeselectPlayingPiece(PlayingPieceTile playingPiece)
    {
        if (playingPiece != null)
        {
            var origColor = playingPiece.Tile.color;
            var color = new Color(origColor.r, origColor.g, origColor.b, AlphaUnselected);
            playingPiece.Tile.color = color;
            TilemapPlayingPieces.SetTile(playingPiece.BoardPosition, null);
            TilemapPlayingPieces.SetTile(playingPiece.BoardPosition, playingPiece.Tile);
            formerSelectedPlayingPiece = null;
            MouseHandler.SelectedPlayingPiecePosition = Vector3Int.zero;
            MouseHandler.SelectedPlayingPiece = null;
        }
    }

    private void MovePlayingPiece(PlayingPieceTile selectedPlayingPiece)
    {
        if (MovementPath.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(MoveFormerSelectedPlayingPiece());
        }
    }

    IEnumerator MoveFormerSelectedPlayingPiece()
    {
        MovementCosts = GameTiles.Instance.LandscapeTiles.Values.Where(t => MovementPath.Contains(t.BoardPosition)).Sum(t => t.MovementCost);
        if (MovementCosts <= PointsForMovement)
        {
            while (MovementPath.Count > 0)
            {
                var position = MovementPath[0];
                MovementPath.RemoveAt(0);
                // update tile info
                var newTile = GameTiles.Instance.Move(formerSelectedPlayingPiece, position);
                // remove old tile
                formerSelectedPlayingPiece.Tilemap.SetTile(formerSelectedPlayingPiece.BoardPosition, null);
                // add new tile
                if (MovementPath.Count > 0)
                    SelectPlayingPiece(newTile);
                else
                    DeselectPlayingPiece(newTile);
                newTile.Tilemap.SetTile(position, newTile.Tile);
                // wait
                yield return new WaitForSeconds(TimeToMovePlayingPieceSec);
            }
            // calculate points for movement
            PointsForMovement -= MovementCosts;
        }
    }

    private float DistanceFunc(Vector3Int a, Vector3Int b)
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
