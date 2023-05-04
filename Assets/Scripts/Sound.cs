using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound 
{
	public string Name;

	public AudioClip Clip;

	[Range(0f, 1f)]
	public float Volume = 1;

	public bool Loop = false;

	[HideInInspector]
	public AudioSource Source;

}
