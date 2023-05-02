using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource myAudio;
    public AudioClip[] myMusic;
    public Slider sliderVolume;

    private void Start()
    {
        myAudio = GetComponent<AudioSource>();
        myAudio.volume = sliderVolume.value;
        sliderVolume.onValueChanged.AddListener(OnVolumeChanged);
        PlayRandomMusic();
    }

    private void OnVolumeChanged(float volume)
    {
        myAudio.volume = volume;
    }

    private void Update()
    {
        if (!myAudio.isPlaying)
            PlayRandomMusic();
    }

    private void PlayRandomMusic()
    {
        myAudio.clip = myMusic[Random.Range(0, myMusic.Length)];
        myAudio.Play();
    }

    public void OnMusicVolumeChanged(float volume)
    {
        myAudio.volume = volume;
    }
}
