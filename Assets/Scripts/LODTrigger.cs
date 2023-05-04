using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LODTrigger : MonoBehaviour
{
    private enum TargetObject { parent, root, thisGameObject}

    [SerializeField, Tooltip("Default behaviour is to disable Mesh Renderer on this selected target, if present")] 
    private bool implementDefaultBehaviour;

    [SerializeField] private TargetObject targetObject;

    private IDependOnLOD dependOnLOD;
    private MeshRenderer meshRenderer;
    private readonly float LODSphereRadius = 250;
    public bool withinLOD;

    private void Start()
    {
        switch (targetObject)
        {
            case TargetObject.parent:

                meshRenderer = transform.parent.GetComponent<MeshRenderer>();
                dependOnLOD = transform.parent.GetComponent<IDependOnLOD>();
                break;

            case TargetObject.root:

                meshRenderer = transform.root.GetComponent<MeshRenderer>();
                dependOnLOD = transform.root.GetComponent<IDependOnLOD>();
                break;

            case TargetObject.thisGameObject:
            default:

                meshRenderer = gameObject.GetComponent<MeshRenderer>();
                dependOnLOD = gameObject.GetComponent<IDependOnLOD>();
                break;
        }

        SetLOD(Vector3.Distance(transform.position, GameManager.instance.player.transform.position) <= LODSphereRadius);
    }

    private void OnTriggerExit(Collider other)
    {
        SetLOD(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        SetLOD(true);
    }

    private void SetLOD(bool set)
    {
        if (implementDefaultBehaviour && meshRenderer != null)
        {
            meshRenderer.enabled = set;
        }

        withinLOD = set;
        dependOnLOD?.OnLODTriggered(set);
    }
}
