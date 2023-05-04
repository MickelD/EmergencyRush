using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundCrush : MonoBehaviour
{

    [SerializeField] public AudioSource sound;

    private void Awake()
    {
    }


    public void OnCollisionEnter(Collision collision)
    {
        sound.Play();
    }

}
