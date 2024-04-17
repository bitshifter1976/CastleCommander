using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    public Player Player;

    public TextMeshProUGUI Text1;
    public TextMeshProUGUI Text2;
    public TextMeshProUGUI Text3;
    public TextMeshProUGUI Text4;

    private void Update()
    {
        Text1.text = $"player {Player.PlayerId}";
        Text2.text = $"points: {Player.PointsLeft}";
        Text3.text = $"spawns: {Player.SpawnsPointsLeft}";
        Text4.text = $"distance: {Player.Distance}";
    }
}
