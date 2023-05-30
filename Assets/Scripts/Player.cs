using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    public bool IsAi;
    public uint PlayerId;
    public Color Color;
    public Color ColorInactive;
    public Tile ArtilleryTile;
    public Tile CavalryTile;
    public Tile InfantryTile; 
    public Tile MedicTile;
    public int PointsLeft;
    public int SpawnsLeft;
    public bool Active;
    public int Distance;
}
