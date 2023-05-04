using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Scr_NpcSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] npcPrefab;

    public List<Transform> elligibleSpawnPoints;
    public List<NPC> npcsOnScene;

    private int npcCount;
    private int maxNPCs; 

    void Start()
    {
        maxNPCs = Random.Range(GameManager.instance.minMaxNPCsOnScene.x, GameManager.instance.minMaxNPCsOnScene.y + 1);

        while (npcsOnScene.Count < maxNPCs)
        {
            SpawnNPC();
        }
    }

    public void SpawnNPC()
    {
        Vector3 randomOffset = new(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        //Vector3 spawnPoint = elligibleSpawnPoints[Random.Range(0, elligibleSpawnPoints.Count)].position + randomOffset;
        Vector3 spawnPoint = transform.GetChild(Random.Range(0, transform.childCount)).position + randomOffset;

        NPC thisNPC = Instantiate(npcPrefab[Random.Range(0, npcPrefab.Length)], spawnPoint, Quaternion.identity).GetComponent<NPC>();

        thisNPC.navigationPoints = this;
        npcsOnScene.Add(thisNPC);

        npcCount++;
        thisNPC.gameObject.name = npcCount.ToString();
    }


}
