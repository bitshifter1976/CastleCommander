using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlaylistSelector : MonoBehaviour
{
    public MusicPlayer PlayerRock;
    public MusicPlayer PlayerMedieval;
    public TMP_Dropdown PlaylistDropdown;

    private void Start()
    {
        var choice = PlayerRock.IsActive ? 0 : 1;
        SelectPlaylist(choice);
        PlaylistDropdown.value = choice;

        PlaylistDropdown.onValueChanged.AddListener((choice) =>
        {
            SelectPlaylist(choice);
        });
    }

    private void SelectPlaylist(int choice)
    {
        switch (choice)
        {
            case 0:
                PlayerRock.IsActive = true;
                PlayerMedieval.IsActive = false;
                break;
            case 1:
                PlayerMedieval.IsActive = true;
                PlayerRock.IsActive = false;
                break;
            default:
                break;
        }
    }
}
