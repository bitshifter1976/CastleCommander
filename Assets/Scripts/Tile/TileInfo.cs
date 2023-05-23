using UnityEngine.Tilemaps;

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