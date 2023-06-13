using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class PlaylistSelector : MonoBehaviour
{
    public MusicPlayer PlayerRock;
    public MusicPlayer PlayerMedieval;
    public TMP_Dropdown PlaylistDropdown;

    private void Start()
    {
        PlaylistDropdown.onValueChanged.AddListener((UnityEngine.Events.UnityAction<int>)((choice) =>
        {
            PlayerRock.Stop();
            PlayerMedieval.Stop();

            if (choice == 0)
            {
                PlayerMedieval.IsActive = false;
                PlayerRock.IsActive = true;
                PlayerRock.PlayRandomMusic();
            }
            else
            {
                PlayerRock.IsActive = false;
                PlayerMedieval.IsActive = true;
                PlayerMedieval.PlayRandomMusic();
            }
        }));
    }
}
