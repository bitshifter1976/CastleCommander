using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    public bool IsAi;
    public uint PlayerId;
    public Color Color;
    public Color ColorInactive;
    public Tile InfantryTile; 
    public Tile ArtilleryTile;
    public Tile MedicTile;
    public int PointsLeft;
    public int SpawnsLeft;
    public int UnitCount;
    public int UnitDeadCount;
    internal bool Active;
}
