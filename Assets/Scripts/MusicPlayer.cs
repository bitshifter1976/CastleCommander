using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public bool IsActive;
    public AudioClip[] Music;
    public Slider SliderVolume;
    public float InitialVolume = 0.5f;
    private AudioSource Audio;

    private void Start()
    {
        Audio = GetComponentInParent<AudioSource>();
        SliderVolume.value = InitialVolume;
        Audio.volume = InitialVolume;
        SliderVolume.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float volume)
    {
        Audio.volume = volume;
    }

    private void Update()
    {
        if (IsActive && !Audio.isPlaying)
            PlayRandomMusic();
    }

    public void PlayRandomMusic()
    {
        SliderVolume.value = Audio.volume;
        Audio.clip = Music[Random.Range(0, Music.Length)];
        Audio.Play();
    }

    public void Stop()
    {
        Audio.Stop();
    }
}
