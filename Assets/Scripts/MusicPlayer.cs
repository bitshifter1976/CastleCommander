using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource Audio;
    public AudioClip[] Music;
    public Slider SliderVolume;

    private void Start()
    {
        Audio = GetComponent<AudioSource>();
        Audio.volume = SliderVolume.value;
        SliderVolume.onValueChanged.AddListener(OnVolumeChanged);
        PlayRandomMusic();
    }

    private void OnVolumeChanged(float volume)
    {
        Audio.volume = volume;
    }

    private void Update()
    {
        if (!Audio.isPlaying)
            PlayRandomMusic();
    }

    private void PlayRandomMusic()
    {
        Audio.clip = Music[Random.Range(0, Music.Length)];
        Audio.Play();
    }

    public void OnMusicVolumeChanged(float volume)
    {
        Audio.volume = volume;
    }
}
