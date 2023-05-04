using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hospital : MonoBehaviour, IAmbulanceTarget
{
    [SerializeField] private GameObject dropTrigger;

    private void OnEnable()
    {
        NPC.OnPatientPickedUp += OpenHospital;
    }

    private void OnDisable()
    {
        NPC.OnPatientPickedUp -= OpenHospital;
    }

    private void Start()
    {
        dropTrigger.SetActive(false);
    }

    public void AmbulanceArrived(Player ambulance)
    {
        if (ambulance.isHoldingPatient)
        {
            dropTrigger.SetActive(false);

            ambulance.PatientDeposited();
        }
    }

    private void OpenHospital()
    {
        dropTrigger.SetActive(true);
    }
}
