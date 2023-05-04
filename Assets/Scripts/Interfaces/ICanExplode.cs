using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public interface ICanExplode
{
    public void Explode();
}

[System.Serializable]
public class CanExplode
{
    [SerializeField] private GameObject[] explosionPrefabs;
    [SerializeField] private Transform sourceTrans;
    [SerializeField] public Vector2 minMaxRadius;
    [SerializeField] private float force;
    [SerializeField] private float randomForceDeviation;
    [SerializeField] private float upperImpulse;
    [SerializeField] private float prefabLifetime;

    public void CreateExplosion()
    {
        foreach (GameObject prefab in explosionPrefabs)
        {
            GameObject instantiatedPrefab = Object.Instantiate(prefab, sourceTrans.position, sourceTrans.rotation);

            CinemachineImpulseSource impulse = instantiatedPrefab.GetComponent<CinemachineImpulseSource>();

            if (impulse != null)
            {
                impulse.GenerateImpulse();
            }

            Object.Destroy(instantiatedPrefab, prefabLifetime);
        }

        Collider[] cols = Physics.OverlapSphere(sourceTrans.position, minMaxRadius.y);
        foreach (Collider col in cols)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(force + Random.Range(-randomForceDeviation, randomForceDeviation), sourceTrans.position, minMaxRadius.y);
                rb.AddForce(Vector3.up * upperImpulse);
            }

            ICanExplode iCanExplode = col.GetComponent<ICanExplode>();

            if (iCanExplode != null && Vector3.Distance(col.transform.position, sourceTrans.position) < minMaxRadius.x)
            {
                iCanExplode.Explode();
            }
        }
    }
}
