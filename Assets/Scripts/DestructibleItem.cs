using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleItem : MonoBehaviour, ICanBeRunOver, IDependOnLOD
{
    private Rigidbody[] parts;
    [SerializeField] private float debrisLifetime;
    [SerializeField] private AudioSource sound;
    [SerializeField] private Collider hitBox;

    void Awake()
    {
        parts = new Rigidbody[transform.childCount];

        for (int i = 0; i < parts.Length; i++)
        {
            //store and array with the fragments
            parts[i] = transform.GetChild(i).gameObject.GetComponent<Rigidbody>();

            if(parts[i] == null)
            {
                throw new System.Exception("DestructibleItem script requires children with Rigidbodies, but a child of an object with tihs script was found without one");
            }
        }

        SetAsDynamicDebris(false);
    }

    public void RunOver(Transform source, Vector3 force)
    {
        SetAsDynamicDebris(true);

        foreach (Rigidbody rb in parts)
        {
            rb.transform.parent = null;

            rb.AddForce(force, ForceMode.Impulse);
            rb.AddTorque(transform.right * force.z, ForceMode.Impulse);

            Destroy(rb.gameObject, debrisLifetime);
        }

        //Aqui es donde el parent de todas las piezas deber�a ser destruido o inhabilitadoç
        sound.Play();
        hitBox.enabled = false;
        Destroy(gameObject, debrisLifetime);
    }

    private void SetAsDynamicDebris(bool set)
    {
        foreach (Rigidbody part in parts)
        {
            part.isKinematic = !set;

            Collider col = part.GetComponent<Collider>();
            
            if (col != null)
            {
                col.enabled = set;
            }
        }
    }

    public void OnLODTriggered(bool enter)
    {
        foreach (Rigidbody rb in parts)
        {
            if (rb != null)
            {
                MeshRenderer mesh = rb.GetComponent<MeshRenderer>();

                if (mesh != null)
                {
                    mesh.enabled = enter;
                }
            }
        }
    }

}
