using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public Sound[] sounds;
    // Start is called before the first frame update
    void Awake()
    {
        foreach(Sound sound in sounds)
        {
        	sound.source = gameObject.AddComponent<AudioSource>();
        	sound.source.clip = sound.clip;

        	sound.source.volume = sound.volume;
        	sound.source.pitch = sound.pitch;
        }
    }

    public void Play(string name)
    {
    	Sound s = Array.Find(sounds, sound => sound.name == name);
    	s.source.Play();
    }

}

[System.Serializable]
public class Sound
{
	public string name;

	public AudioClip clip;

	[Range(0F, 1F)]
	public float volume;
	[Range(0.1F, 3F)]
	public float pitch;

	[HideInInspector]
	public AudioSource source;
}