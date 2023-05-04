using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseMaterialData : MonoBehaviour
{
    [SerializeField] MeshRenderer mesh;

    [Space(3), Header("Material Properties")]
    [SerializeField, ColorUsage(true, true)] private Color col;
    [SerializeField] private float freq;
    [SerializeField] private float size;
    [SerializeField] private float tiling;

    private Material material;

    void Start()
    {
        material = mesh.material;


        material.SetColor("_tint", col);
        material.SetFloat("_Freq", freq);
        material.SetFloat("_size", size);
        material.SetFloat("_alpha", col.a);
        material.SetFloat("_tile", tiling);
    }
}
