using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound 
{
    public string tag;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 3f)]
    public float pitch;

    [Range(0f, 1f)]
    public float spatialBlend;

    public bool loop;

    private AudioSource source;

    public void SetSource(AudioSource source)
    {
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.spatialBlend = spatialBlend;
        source.loop = loop;

        this.source = source;
    }

    public void Play()
    {
        if (!source.isPlaying)
        {
            source.Play();
        }
    }
}
