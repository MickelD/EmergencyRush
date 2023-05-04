using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanBeRunOver 
{
    public void RunOver(Transform source, Vector3 force);
}
