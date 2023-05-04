using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour, IDependOnLOD
{
    [SerializeField] private Scr_NpcSpawner spawner;
    private bool onList = false;

    public void OnLODTriggered(bool enter)
    {
        if (enter && !onList)
        {
            
            spawner.elligibleSpawnPoints.Add(this.transform);
            onList = true;
        }
        else
        {
            spawner.elligibleSpawnPoints.Remove(this.transform);
            onList = false;
        }
    }
}
