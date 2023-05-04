using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


[System.Serializable]
public class SoundManager : MonoBehaviour
{
    public ClipInfo[] clips;
}


[System.Serializable]
public struct ClipInfo
{
    public AudioClip clip;

    public float volume;
}
