using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SoundPlayer : MonoBehaviour 
{
	public static SoundPlayer Instance;

	public List<Sound> Sounds;

	private void Start()
	{
		Instance = this;
        foreach (var s in Sounds)
        {
            s.Source = gameObject.AddComponent<AudioSource>();
            s.Source.clip = s.Clip;
            s.Source.volume = s.Volume;
            s.Source.loop = s.Loop;
        }
	}

	public void Play(string sound)
	{
		var s = Sounds.FirstOrDefault(item => item.Name == sound);
        s?.Source.Play();
	}

    public void Play(string sound, float volume)
    {
        var s = Sounds.FirstOrDefault(item => item.Name == sound);
		if (s != null)
		{
			s.Volume = volume;
			s.Source.Play();
		}
    }

    public void Stop(string sound)
	{
		var s = Sounds.FirstOrDefault(item => item.Name == sound);
        s?.Source.Stop();
	}

}
