using UnityEngine;
using System.Collections;
using System;

public class SoundRequest
{
    public static event Action<string> ActionRequestSound = delegate { };
    public static event Action<string> ActionRequestMusic = delegate { };

    public static void RequestSound(string tag)
    {
        ActionRequestSound(tag);
    }

    public static void RequestMusic(string tag)
    {
        ActionRequestMusic(tag);
    }
}
