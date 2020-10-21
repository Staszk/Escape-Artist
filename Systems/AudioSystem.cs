using System;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    public static AudioSystem instance;

    private readonly int AUDIO_SOURCE_NUM = 20;

    public Sound[] sounds;
    public Sound[] backgroundSongs;
    private AudioSource[] sources;
    private AudioSource backgroundSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSounds();
    }

    private void OnEnable()
    {
        SoundRequest.ActionRequestSound += Play;
        SoundRequest.ActionRequestMusic += PlayBackgroundMusic;
    }

    private void OnDisable()
    {
        SoundRequest.ActionRequestSound -= Play;
        SoundRequest.ActionRequestMusic -= PlayBackgroundMusic;
    }

    private void LoadSounds()
    {
        sources = new AudioSource[AUDIO_SOURCE_NUM];

        for (int i = 0; i < AUDIO_SOURCE_NUM; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
        }

        backgroundSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayBackgroundMusic(string tag)
    {
        Sound snd = Array.Find(backgroundSongs, sound => sound.tag == tag);

        if (snd != null)
        {
            backgroundSource.Stop();

            backgroundSource.clip = snd.clip;
            backgroundSource.clip = snd.clip;
            backgroundSource.volume = snd.volume;
            backgroundSource.pitch = snd.pitch;
            backgroundSource.spatialBlend = snd.spatialBlend;
            backgroundSource.loop = snd.loop;

            backgroundSource.Play();
        }
        else
        {
            Debug.LogError("MUSIC " + tag + " DOES NOT EXIST");
        }
    }

    public void Play(string tag)
    {
        Sound snd = Array.Find(sounds, sound => sound.tag == tag);

        if (snd != null)
        {
            for (int i = 0; i < AUDIO_SOURCE_NUM; i++)
            {
                if (!sources[i].isPlaying)
                {
                    sources[i].clip = snd.clip;
                    sources[i].volume = snd.volume;
                    sources[i].pitch = snd.pitch;
                    sources[i].spatialBlend = snd.spatialBlend;
                    sources[i].loop = snd.loop;

                    sources[i].Play();

                    return;
                }
            }
        }
        else
        {
            Debug.LogError("SOUND " + tag + " does not exist");
            return;
        }

        Debug.LogWarning("No Audio Source to Play Sound");
    }
}
