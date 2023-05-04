using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Transform target;
    private enum UrgencySource { patientBomb, playerBomb, playerGas}

    [SerializeField] private Vector2 minMaxUrgency;
    [SerializeField] private UrgencySource urgencySource;
    [SerializeField, GradientUsage(hdr: true)] private Gradient colGrad;
    private float urgency;
    [SerializeField] private float amp;
    [SerializeField] private Transform arrow;
    [SerializeField] private float distanceThreshold;

    private Countdown activeTimer;
    private Player player;
    private Vector3 restPosition;
    bool restPositionStored = false;
    float t;
    private Material mat;

    private void OnEnable()
    {
        if (!restPositionStored)
        {
            restPosition = arrow.localPosition;
            restPositionStored = true;
        }

        //reset Urgency and position
        arrow.localPosition = restPosition;
        t = 0f;

        //set material properties
        mat = arrow.GetComponent<MeshRenderer>().material;
        mat.SetColor("_ColorA", colGrad.colorKeys[0].color);
        mat.SetColor("_ColorB", colGrad.colorKeys[^1].color);       
    }

    private void Start()
    {
        player = GameManager.instance.player;
    }

    void FixedUpdate()
    {
        transform.LookAt(target);

        bool withinDistanceThreshold = (target.position - transform.position).sqrMagnitude < distanceThreshold;

        float ang =  withinDistanceThreshold ? 90f : 0f;
        arrow.localRotation = Quaternion.Euler(0f, 90f, ang);

        urgency = UpdateUrgency();

        t += Time.fixedDeltaTime;

        float freq = Mathf.Sin(Mathf.Lerp(minMaxUrgency.x, minMaxUrgency.y, urgency) * t);

        mat.SetFloat("_Freq", freq);

        arrow.localPosition = restPosition + amp * freq * (withinDistanceThreshold? Vector3.up : Vector3.forward);
    }

    private float UpdateUrgency()
    {
        switch (urgencySource)
        {
            case UrgencySource.patientBomb:

                activeTimer = target.GetComponentInChildren<Countdown>();
                break;

            case UrgencySource.playerBomb:

                activeTimer = transform.root.GetComponentInChildren<Countdown>();
                break;

            case UrgencySource.playerGas:
            default:
                break;
        }         

        if (activeTimer != null)
        {
            return Mathf.Lerp(1f, 0f, activeTimer.time / GameManager.instance.patientFuseTime);
        }
        else
        {
            return (1 - player.Fuel / player.fuelArrowThreshold);
        }
    }
}
