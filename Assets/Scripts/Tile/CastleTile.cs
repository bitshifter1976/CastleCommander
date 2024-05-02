using MagicPigGames;
using UnityEngine;
using UnityEngine.Tilemaps;
using static PlayingPieceTile;

public class CastleTile : GameTile
{
    public Player Player;
    public CastleTileInfo Info;
    private readonly GameObject Healthbar;
    private int MaxEnergy = 20;

    public int Energy
    {
        get => Info.Energy;
        set
        {
            HorizontalHealthbar.SetProgress(value/MaxEnergy);
            Info.Energy = value;
        }
    }

    private HorizontalProgressBar HorizontalHealthbar
    {
        get => Healthbar?.GetComponentInChildren<HorizontalProgressBar>();
    }

    public override Vector3Int BoardPosition
    {
        get => base.BoardPosition;
        set
        {
            base.BoardPosition = value;
            if (Healthbar != null)
            {
                var newPos = Tilemap.CellToWorld(value);
                Healthbar.transform.position = newPos;
                Healthbar.transform.position = newPos + new Vector3(0f, 3f, 0f);
            }
        }
    }

    public Vector3 Position
    {
        get => Healthbar != null ? Healthbar.transform.position : Vector3.zero;
        set
        {
            if (Healthbar != null)
            {
                var newPos = value;
                Healthbar.transform.position = newPos;
                Healthbar.transform.position = newPos + new Vector3(0f, 3f, 0f);
            }
        }
    }

    public CastleTile(Vector3Int boardPosition, Tile tile, CastleTileInfo info, GameObject healthbar, Tilemap tilemap, int id, Color color, Player player, bool movable, int movementCost) : base(boardPosition, tile, tilemap, id, color, TileType.PlayingPiece, movable, movementCost)
    {
        Player = player;
        Info = info;
        Healthbar = healthbar;
        Info.Energy = MaxEnergy;

    }
}
