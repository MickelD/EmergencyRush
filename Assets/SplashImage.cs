using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashImage : MonoBehaviour
{
    private RawImage img;

    private void Start()
    {
        img = gameObject.GetComponent<RawImage>();

        if (img != null) { img.enabled = true; }
    }

    public void SplashFinished()
    {
        if (img != null)
        {
            img.enabled = false;
        }
    }
}
