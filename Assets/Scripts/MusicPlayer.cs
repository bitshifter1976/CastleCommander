using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] Music;
    public Slider SliderVolume;
    private AudioSource audio;

    private void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.volume = SliderVolume.value;
        SliderVolume.onValueChanged.AddListener(OnVolumeChanged);
        PlayRandomMusic();
    }

    private void OnVolumeChanged(float volume)
    {
        audio.volume = volume;
    }

    private void Update()
    {
        if (!audio.isPlaying)
            PlayRandomMusic();
    }

    private void PlayRandomMusic()
    {
        audio.clip = Music[Random.Range(0, Music.Length)];
        audio.Play();
    }

    public void OnMusicVolumeChanged(float volume)
    {
        audio.volume = volume;
    }
}
