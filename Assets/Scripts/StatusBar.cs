using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
    public Player Player1;
    public Player Player2;

    public TextMeshProUGUI TextP11;
    public TextMeshProUGUI TextP12;
    public TextMeshProUGUI TextP13;
    public TextMeshProUGUI TextP14;
    public TextMeshProUGUI TextP15;

    public TextMeshProUGUI TextP21;
    public TextMeshProUGUI TextP22;
    public TextMeshProUGUI TextP23;
    public TextMeshProUGUI TextP24;
    public TextMeshProUGUI TextP25;

    private void Update()
    {
        TextP11.text = $"player: {Player1.PlayerId}";
        TextP12.text = $"points left: {Player1.PointsLeft}";
        TextP13.text = $"spawns left: {Player1.SpawnsLeft}";
        TextP14.text = $"active units: {Player1.UnitCount}";
        TextP15.text = $"dead units: {Player1.UnitDeadCount}";

        TextP21.text = $"player: {Player2.PlayerId}";
        TextP22.text = $"points left: {Player2.PointsLeft}";
        TextP23.text = $"spawns left: {Player2.SpawnsLeft}";
        TextP24.text = $"active units: {Player2.UnitCount}";
        TextP25.text = $"dead units: {Player2.UnitDeadCount}";

        if (Player1.Active)
        {
            TextP11.color = Player1.Color;
            TextP12.color = Player1.Color;
            TextP13.color = Player1.Color;
            TextP14.color = Player1.Color;
            TextP15.color = Player1.Color;

            TextP21.color = Player2.ColorInactive;
            TextP22.color = Player2.ColorInactive;
            TextP23.color = Player2.ColorInactive;
            TextP24.color = Player2.ColorInactive;
            TextP25.color = Player2.ColorInactive;
        }
        else
        {
            TextP11.color = Player1.ColorInactive;
            TextP12.color = Player1.ColorInactive;
            TextP13.color = Player1.ColorInactive;
            TextP14.color = Player1.ColorInactive;
            TextP15.color = Player1.ColorInactive;

            TextP21.color = Player2.Color;
            TextP22.color = Player2.Color;
            TextP23.color = Player2.Color;
            TextP24.color = Player2.Color;
            TextP25.color = Player2.Color;
        }
    }
}
