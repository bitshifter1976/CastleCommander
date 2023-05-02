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
using System.IO;

public class BoardController : MonoBehaviour
{
    #region Definitions
    public enum BoardMode 
    { 
        WholeBoard 
    };

    public class TileInfo
    {
        public Tile Tile;
        public float Probability; // value between 1 and 0 
        public int MovementCosts;

        public TileInfo(Tile tile, float probability, int movementCosts)
        {
            Tile = tile;
            Probability = probability;
            MovementCosts = movementCosts;
        }
    }

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
    public Tilemap TilemapLandscape;
    public Tilemap TilemapUnderTiles;
    [Header("Input")]
    public MouseHandler MouseHandler;
    [Header("Hud")]
    public Button ButtonReload;
    public Timer Timer;
    public GameObject MessageBox;
    public Button MessageBoxButtonOk;
    public Button MessageBoxButtonCancel;
    public TextMeshProUGUI MessageBoxText;
    public TextMeshProUGUI MessageBoxButtonOkText;
    public TextMeshProUGUI MessageBoxButtonCancelText;
    [Header("Dice")]
    public Animator Dice1Animation;
    public Animator Dice2Animation;
    public SpriteRenderer Dice1CurrentFrame;
    public SpriteRenderer Dice2CurrentFrame;

    [Header("********** Tiles **********")]
    public Tile Base;
    public Tile Desert;
    public Tile LeafForest;
    public Tile PineForest;
    public Tile Jungle;
    public Tile Mountain;
    public Tile Ocean;
    public Tile Grass;
    public Tile Castle;
    public Tile Volcano;
    public Tile UnderDirt;
    public Tile UnderOcean;

    [Header("********** Info **********")]
    public Player ActivePlayer;
    public BoardState State = BoardState.Load;
    public int Dice1Result;
    public int Dice2Result;
    public int PointsForMovement;
    public int MovementCosts;
    public List<Vector3Int> MovementPath;
    public Vector3Int[] MovementDirectionsOdd  = new Vector3Int[6] { Vector3Int.left, Vector3Int.right, new Vector3Int(0,1,0)/*top-left*/, new Vector3Int(1,1,0)/*top-right*/, new Vector3Int(0, -1, 0)/*bottom-left*/, new Vector3Int(1,-1,0)/*bottom-right*/ };
    public Vector3Int[] MovementDirectionsEven = new Vector3Int[6] { Vector3Int.left, Vector3Int.right, new Vector3Int(-1, 1, 0)/*top-left*/, new Vector3Int(0, 1, 0)/*top-right*/, new Vector3Int(-1, -1, 0)/*bottom-left*/, new Vector3Int(0, -1, 0)/*bottom-right*/ };

    private readonly Dictionary<Vector3Int, Tile> tilesLandscape = new();
    private readonly Dictionary<Vector3Int, Tile> tilesUnderTiles = new();
    private readonly float TimeToAddTileSec = 0.01f;
    private readonly float TimeToEndRoundSec = 30f;
    private readonly float TimeToMovePlayingPieceSec = 1f;
    private readonly float AlphaSelected = 1f;
    private readonly float AlphaUnselected = 100f/256f;
    private float TimeToRollDice1Sec = 0f;
    private float TimeToRollDice2Sec = 0f;
    private float timeElapsed = 0f;
    private LandscapeTile selectedLandscapeTile;
    private CastleTile selectedCastle;
    private PlayingPieceTile selectedPlayingPiece;
    private PlayingPieceTile formerSelectedPlayingPiece;
    private Pathfinder<Vector3Int> pathfinder;

    #endregion

    private List<TileInfo> GetLandscapeTileInfos()
    {
        return new List<TileInfo>
        {
            new TileInfo(Grass,         1.0f,   10),
            new TileInfo(LeafForest,    0.5f,   20),
            new TileInfo(Desert,        0.5f,   30),
            new TileInfo(Jungle,        0.5f,   30),
            new TileInfo(Mountain,      0.4f,   100),
            new TileInfo(Ocean,         0.4f,   100), 
            new TileInfo(Base,          0.2f,   10), 
            new TileInfo(PineForest,    0.2f,   20), 
            new TileInfo(Volcano,       0.05f,  100) 
        };
    }

    public List<Tile> GetLandscapeTilesForMovement()
    {
        return new List<Tile>
        {
            Grass,
            LeafForest,
            Desert,
            Jungle,
            Base,
            PineForest
        };
    }

    private void ShowMessageBox(string message, string buttonTextOk = "ok", string buttonTextCancel = null, BoardState? stateToTrigger = null)
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
                if (stateToTrigger.HasValue)
                    State = stateToTrigger.Value;
            }); 
            // button cancel
            var buttonCancelEnabled = !string.IsNullOrEmpty(buttonTextCancel);
            MessageBoxButtonCancelText.text = buttonTextCancel;
            MessageBoxButtonCancel.gameObject.SetActive(buttonCancelEnabled);
            MessageBoxButtonCancel.onClick.AddListener(() =>
            {
                MessageBoxButtonCancel.onClick.RemoveAllListeners();
                MessageBox.SetActive(false);
            });
            MessageBox.SetActive(true);
        }
    }

    private void Start()
    {
        MessageBox.SetActive(false);
        Dice1Animation.enabled = false;
        Dice2Animation.enabled = false;
        ActivePlayer = Player1;
        GameTiles.Instance.TileInfos = GetLandscapeTileInfos();
        GameTiles.Instance.TilesForMovement = GetLandscapeTilesForMovement();
        ButtonReload.onClick.AddListener(OnReloadClick);
        MouseHandler.OnClick += OnBoardClick;
    }

    private void OnReloadClick()
    {
        State = BoardState.ConfirmLoad;
    }

    private void SwitchPlayer()
    {
        ActivePlayer = ActivePlayer == Player1 ? Player2 : Player1;
    }

    public bool Reload()
    {
        var result = false;
        try
        {
            Timer.StopTimer();
            TilemapPlayingPieces.ClearAllTiles();
            TilemapLandscape.ClearAllTiles();
            TilemapUnderTiles.ClearAllTiles();
            ActivePlayer = Player1;
            CreateBoard();
            result = true;
        }
        catch (Exception e)
        {
            throw e;
        }
        return result;
    }

    private void Update()
    {
        switch (State)
        {
            case BoardState.ConfirmLoad:
                ShowMessageBox("Do you really want to abort\nand reload board?", "Yes", "No", BoardState.Load); 
                break;
            case BoardState.Load:
                if (Reload())
                    State = BoardState.CreateBoard;
                break;
            case BoardState.CreateBoard:
                timeElapsed += Time.deltaTime;
                if (timeElapsed >= TimeToAddTileSec)
                {
                    timeElapsed = 0;
                    if (!AddTile())
                        State = BoardState.PlayerGetReady;
                }
                break;
            case BoardState.PlayerGetReady:
                ShowMessageBox($"Player {ActivePlayer.PlayerId} get ready!", "Go", null, BoardState.RollDiceStart);
                break;
            case BoardState.RollDiceStart:
                TimeToRollDice1Sec = Random.Range(1f, 2f);
                TimeToRollDice2Sec = Random.Range(1f, 2f);
                Dice1Animation.enabled = true;
                Dice2Animation.enabled = true;
                State = BoardState.RollDice;
                break;
            case BoardState.RollDice:
                timeElapsed += Time.deltaTime;
                var dice1RollingFinished = false;
                var dice2RollingFinished = false;
                if (timeElapsed >= TimeToRollDice1Sec)
                {
                    Dice1Animation.enabled = false;
                    Dice1Result = int.Parse(Dice1CurrentFrame.sprite.name.Substring("Dice".Length));
                    dice1RollingFinished = true;
                }
                if (timeElapsed >= TimeToRollDice2Sec)
                {
                    Dice2Animation.enabled = false;
                    Dice2Result = int.Parse(Dice2CurrentFrame.sprite.name.Substring("Dice".Length));
                    dice2RollingFinished = true;
                }
                if (dice1RollingFinished && dice2RollingFinished)
                {
                    timeElapsed = 0;
                    PointsForMovement = Dice1Result*10 + Dice2Result*10;
                    Timer.StartTimer();
                    State = BoardState.PlayRound;
                }
                break;
            case BoardState.PlayRound:
                if (Timer.TimeElapsedSec >= TimeToEndRoundSec || PointsForMovement <= 0)
                    State = BoardState.FinishRound;
                break;
            case BoardState.FinishRound:
                Timer.StopTimer();
                SwitchPlayer();
                State = BoardState.PlayerGetReady;
                break;
            case BoardState.GameEnd:
                break;
        }
    }

    private void CreateBoard()
    {
        if (GeneratorMode == BoardMode.WholeBoard)
        {
            var xMin = -BoardWidth  / 2;
            var xMax = BoardWidth   / 2;
            var yMin = -BoardHeight / 2;
            var yMax = BoardHeight  / 2;
            // add landscape tiles with its' under tiles
            for (var x = xMin; x <= xMax; x++)
            {
                for (var y = yMax; y >= yMin; y--)
                {
                    var landscapeTile = GetRandomLandscapeTile();
                    var position = new Vector3Int(x, y, 0);
                    tilesLandscape.Add(position, landscapeTile);
                    tilesUnderTiles.Add(position, landscapeTile == Ocean ? UnderOcean : UnderDirt);
                    GameTiles.Instance.Add(GameTile.TileType.Landscape, position, landscapeTile, TilemapLandscape, null, PlayingPieceTile.PlayingPieceTileType.None, GetLandscapeType(landscapeTile));
                }
            }
            // add player castle tiles
            var x1 = xMin+1;
            var x2 = xMax-1;
            var y1 = Random.Range(yMin, yMax);
            var y2 = Random.Range(yMin, yMax);
            var pos1 = new Vector3Int(x1, y1, 0);
            var pos2 = new Vector3Int(x2, y2, 0);
            tilesLandscape[pos1] = Castle;
            tilesUnderTiles[pos1] = UnderDirt;
            tilesLandscape[pos2] = Castle;
            tilesUnderTiles[pos2] = UnderDirt;
            GameTiles.Instance.Add(GameTile.TileType.Castle, pos1, Castle, TilemapLandscape, Player1);
            GameTiles.Instance.Add(GameTile.TileType.Castle, pos2, Castle, TilemapLandscape, Player2);
        }
    }

    private LandscapeTile.LandscapeTileType GetLandscapeType(Tile landscapeTile)
    {
        if (landscapeTile == Base)
            return LandscapeTile.LandscapeTileType.Base;
        else if (landscapeTile == Desert)
            return LandscapeTile.LandscapeTileType.Desert;
        else if (landscapeTile == Grass)
            return LandscapeTile.LandscapeTileType.Grass;
        else if (landscapeTile == Jungle)
            return LandscapeTile.LandscapeTileType.Jungle;
        else if (landscapeTile == LeafForest)
            return LandscapeTile.LandscapeTileType.LeafForest;
        else if (landscapeTile == Mountain)
            return LandscapeTile.LandscapeTileType.Mountain;
        else if (landscapeTile == Ocean)
            return LandscapeTile.LandscapeTileType.Ocean;
        else if (landscapeTile == PineForest)
            return LandscapeTile.LandscapeTileType.PineForest;
        else if (landscapeTile == Volcano)
            return LandscapeTile.LandscapeTileType.Volcano;

        return LandscapeTile.LandscapeTileType.None;
    }

    private Tile GetRandomLandscapeTile()
    {
        var probability = Random.Range(0f, 1f);
        var tiles = GetLandscapeTileInfos();
        var possibleTiles = tiles.Where(t => t.Probability >= probability).Select(s => s.Tile).ToList();
        if (possibleTiles.Count == 0)
            possibleTiles.Add(tiles[Random.Range(0, tiles.Count)].Tile);
        return possibleTiles[Random.Range(0, possibleTiles.Count)];       
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

    private void OnBoardClick(object sender, EventArgs e)
    {
        if (State != BoardState.PlayRound)
            return;

        // get selected game tiles
        selectedLandscapeTile = GameTiles.Instance.Get<LandscapeTile>(MouseHandler.SelectedLandscapeTilePosition);
        selectedCastle = GameTiles.Instance.Get<CastleTile>(MouseHandler.SelectedLandscapeTilePosition);
        selectedPlayingPiece = GameTiles.Instance.Get<PlayingPieceTile>(MouseHandler.SelectedPlayingPiecePosition);

        // if not prior selected playing piece, select it for movement
        if (formerSelectedPlayingPiece == null && selectedPlayingPiece != null && selectedPlayingPiece.Player.PlayerId == ActivePlayer.PlayerId)
            SelectPlayingPiece(selectedPlayingPiece);
        // if prior selected playing piece, move it
        else if (formerSelectedPlayingPiece != null && selectedLandscapeTile != null && formerSelectedPlayingPiece.Player.PlayerId == ActivePlayer.PlayerId && GetLandscapeTilesForMovement().Contains(selectedLandscapeTile.Tile))
            MovePlayingPiece(formerSelectedPlayingPiece);
        // if own castle clicked, spawn playing piece
        else if (selectedCastle != null && selectedCastle.Player.PlayerId == ActivePlayer.PlayerId)
            PlacePlayingPiece(MouseHandler.SelectedLandscapeTilePosition);
        
    }

    private void PlacePlayingPiece(Vector3Int position)
    {
        var tile = ActivePlayer.PlayingPiece;
        if (tile != null)
        {
            var tileInfo = GameTiles.Instance.Add(GameTile.TileType.PlayingPiece, position, tile, TilemapPlayingPieces, ActivePlayer, PlayingPieceTile.PlayingPieceTileType.Infantry);
            SelectPlayingPiece(tileInfo as PlayingPieceTile);
        }
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
        MouseHandler.SelectedPlayingPiece = ActivePlayer.PlayingPiece;
    }

    private void DeselectPlayingPiece(PlayingPieceTile playingPiece)
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

    private void MovePlayingPiece(PlayingPieceTile selectedPlayingPiece)
    {
        var position = selectedPlayingPiece.BoardPosition;
        var target = MouseHandler.SelectedLandscapeTilePosition;
        pathfinder = new Pathfinder<Vector3Int>(DistanceFunc, ConnectionsAndCosts);
        pathfinder.GenerateAstarPath(position, target, out MovementPath);
        if (MovementPath.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(Move());
        }
    }

    IEnumerator Move()
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
