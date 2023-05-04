using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasStation : MonoBehaviour, IAmbulanceTarget
{
    [SerializeField] private AnimationCurve refuelRate;

    public void AmbulanceArrived(Player ambulance)
    {
        ambulance.Fuel += refuelRate.Evaluate(ambulance.Fuel) * Time.deltaTime;
    }
}
