using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Countdown : MonoBehaviour
{
    private Camera cam;

    public float time;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private LODTrigger LODListener;

    private void Start()
    {
        cam = GameManager.instance.mainCamera;
    }

    private void Update()
    {
        if (time > 0f)
        {
            time -= Time.deltaTime;
        }
        else 
        {
            time = 0f;
            Detonate();
        }
    }


    void LateUpdate()
    {
        if (LODListener.withinLOD)
        {
            transform.LookAt(cam.transform.position);

            int m = (int)time / 60;
            int s = (int)time % 60;
            int cs = ((int)(time * 100)) % 100;

            text.text = string.Format("{0:00}:{1:00}:{2:00}", m, s, cs);
        }
    }

    private void Detonate()
    {
        transform.root.GetComponent<ICanExplode>()?.Explode();
    }
}
