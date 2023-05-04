using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Cinemachine;

public class Player : MonoBehaviour, ICanExplode
{
    [Header("Input"), Space(1)]
    [SerializeField] private KeyCode brakeKey;
    
    [Space(3), Header("Motor"), Space(1)]
    [SerializeField] private float acceleration;
    public float maxSpeed;
    [SerializeField] private float brakingStrenght;
    [SerializeField, Range(0f, 1f)] private float _fuel;
    public float fuelArrowThreshold;
    public System.Action<float> OnUpdateFuel;

    public float Fuel 
    { 
        get 
        { 
            return _fuel; 
        } 

        set 
        {
            _fuel = Mathf.Clamp01(value);

            OnUpdateFuel?.Invoke(_fuel);

            if (_fuel <= 0f)
            {
                GameManager.instance.PlayerFuckingLoses();
            }
            else
            {
                SetActiveAllNavigationArrows(_fuel <= fuelArrowThreshold, new Transform[] { gasArrow});
            }
        } 
    }

    [SerializeField] private AnimationCurve fuelConsumptionRate;

    [Space(3), Header("Steering"), Space(1)]
    [SerializeField] private float minSpeedToSteer;
    [SerializeField] private float traction;
    [SerializeField] private float steerFriction;
    [SerializeField] private float turnStrenght;
    [SerializeField] private float steerAcc;
    [SerializeField] private float steerDecc;
    [Space(2), SerializeField] private float steerSpeed;
    [SerializeField] private float brakeSteer;

    [Space(3), Header("Physics"), Space(1)]
    public Rigidbody rb;
    [SerializeField] private Vector3 centerOfMass;
    [SerializeField] private Vector2 runOverForceMultiplier;
    [SerializeField] private float minSpeedToRunOver;
    [SerializeField] private float minSpeedToCrash;

    [Space(3), Header("Animation"), Space(1)]
    [SerializeField] private Animator chasisAnimator;
    [SerializeField] private float pitchSpeed;
    public Animator doorAnimator;
    [SerializeField] private ParticleSystem exhaustParticles;
    private ParticleSystem.EmissionModule mainParticleSettings;
    [SerializeField] private MeshRenderer chasisMesh;
    [SerializeField] private Vector2Int sirenMaterialIndexFromLeftToRight;
    [SerializeField] private Material lightsOffMaterial;
    [SerializeField] private Material lightsOnMaterial;
    [SerializeField] private int lightMaterialIndex;
    [SerializeField] private Light[] headLights;

    [Space(3), Header("Interaction"), Space(1)]
    [SerializeField] private float minSpeedToInteract;
    public Transform timerSocket;
    public bool isHoldingPatient;

    [Space(3), Header("Death"), Space(1)]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin cameraNoise;
    private float noiseAmplitudeGain;
    [SerializeField] private CanExplode canExplode;
    public bool exploded;

    [Space(3), Header("Navigation"), Space(1)]
    public Transform patientArrows;
    public Transform hospitalArrow;
    public Transform gasArrow;

    [Space(3), Header("AudioSource"), Space(1)]
    [SerializeField] private Vector2 minMaxEnginePitch;
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private AudioSource collisionAudioSource;
    [SerializeField] private Vector2 minMaxCollisionVolumeGain;
    [SerializeField] private AudioSource sirenAudioSource;
    [SerializeField] private AudioSource doorsAudioSource;
    [SerializeField] private AudioSource brakeAudioSource;


    private int PosAudio;
    private float speed;
    private Vector3 desiredDir;
    private float rot;

    public System.Action<float> OnUpdateSpeed;

    //input
    private float yInput;
    private float xInput;
    bool pressingBrake;
    float desiredBrake;

    private void Start()
    {
        desiredDir = transform.forward;

        isHoldingPatient = false;

        exploded = false;

        Fuel = 1;

        //particles preparation
        mainParticleSettings = exhaustParticles.emission;

        //disable all navigation arrows & lights
        SetActiveAllNavigationArrows(false, new Transform[] { patientArrows, hospitalArrow, gasArrow});
        SetSiren(false);

        //camera noise toggle
        cameraNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noiseAmplitudeGain = cameraNoise.m_AmplitudeGain;
    }

    private void Update()
    {
        yInput = Input.GetAxisRaw("Vertical") * Mathf.Ceil(Fuel);
        xInput = Input.GetAxisRaw("Horizontal");
        pressingBrake = Input.GetKey(brakeKey);
        desiredBrake = xInput == 0f ? brakingStrenght : steerFriction;

        //set chasis pitching
        chasisAnimator.SetFloat("Turn", Mathf.Lerp(chasisAnimator.GetFloat("Turn"), rot / brakeSteer, Time.deltaTime * pitchSpeed));

        //diseable camera noise when moving (makes movement feel janky), renable when car is stopped
        cameraNoise.m_AmplitudeGain = Mathf.Lerp(noiseAmplitudeGain, 0, xInput + yInput);

        //set exhaust particles
        mainParticleSettings.enabled = yInput != 0f;

        //fuel drainage
        Fuel -= fuelConsumptionRate.Evaluate(Fuel) * Time.deltaTime * Mathf.Abs(yInput);

        //engines make higher pitched sounds when moving than when idling
        engineAudioSource.pitch = Mathf.Lerp(minMaxEnginePitch.x, minMaxEnginePitch.y, Input.GetAxis("Vertical"));

        brakeAudioSource.mute = !(pressingBrake && speed > minSpeedToSteer);

        //cheaty cheats
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameManager.instance.PlayerFuckingWins();
        }
    }

    private void FixedUpdate()
    {
        //set rb velocity lerping based on axis and brake input [TURNARY OPERATOR]
        rb.velocity = pressingBrake || yInput == 0f ? Vector3.Lerp(rb.velocity, Vector3.zero, desiredBrake * Time.fixedDeltaTime) : Vector3.Lerp(rb.velocity, yInput * maxSpeed * desiredDir, acceleration * Time.fixedDeltaTime);
        speed = rb.velocity.magnitude;
        OnUpdateSpeed(speed);
        /*
         PROBLEMS TO SOLVE
        - Compress current code to reduce number of If statements
         */

        //set steering based on input 
        if (speed > minSpeedToSteer && yInput != 0f && xInput != 0f)
        {
            float targetSteerSpeed = !pressingBrake ? steerSpeed : brakeSteer;
            //rb.angularVelocity = Vector3.Lerp(rb.a
            //
            //ngularVelocity, targetSteerSpeed * xInput * yInput * transform.up, steerAcc * Time.deltaTime);
            rot = Mathf.Lerp(rot, xInput * yInput * targetSteerSpeed, steerAcc * Time.fixedDeltaTime);
            //rb.angularVelocity = !pressingBrake? Vector3.Lerp(rb.angularVelocity, steerSpeed * xInput * yInput * transform.up, steerAcc * Time.deltaTime) : Vector3.Lerp(rb.angularVelocity, brakeSteer * xInput * yInput * transform.up, steerAcc * Time.deltaTime);

        }
        else
        {
            //rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, steerDecc * Time.deltaTime);
            rot = Mathf.Lerp(rot, 0f, steerDecc * Time.fixedDeltaTime);

        }
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rot, 0f));

        //desired direction interpolation
        if (xInput == 0f)
        {
            desiredDir = Vector3.Lerp(desiredDir, transform.forward, traction * Time.fixedDeltaTime);
        }
        else
        {
            if (yInput != 0f && !pressingBrake)
            {
                desiredDir = Vector3.Lerp(desiredDir, Vector3.Normalize(transform.forward + turnStrenght * xInput * yInput * transform.right), steerAcc * Time.fixedDeltaTime);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        IAmbulanceTarget target = other.gameObject.GetComponentInParent<IAmbulanceTarget>();

        if (target != null && rb.velocity.sqrMagnitude <= minSpeedToInteract * minSpeedToInteract)
        {
            target.AmbulanceArrived(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        float _collisionRelativeSpeed = collision.relativeVelocity.magnitude;

        collisionAudioSource.volume = Mathf.Lerp(minMaxCollisionVolumeGain.x, minMaxCollisionVolumeGain.y, _collisionRelativeSpeed / minSpeedToCrash);
        collisionAudioSource.Play();

        ICanBeRunOver runOver = collision.gameObject.GetComponent<ICanBeRunOver>();
        if (runOver != null)
        {
            if (_collisionRelativeSpeed >= minSpeedToRunOver)
            {
                Vector3 force = collision.transform.position - transform.position;
                force.y = 0f;
                force.Normalize();

                force = _collisionRelativeSpeed * runOverForceMultiplier.x * force + Vector3.up * runOverForceMultiplier.y;

                runOver.RunOver(transform, force);
            }
        }
        else
        {
            if (_collisionRelativeSpeed >= minSpeedToCrash)
            {
                //Crashing into an undestructible object explodes the player
                Explode();
            }
        }
    }

    public void PatientDeposited()
    {
        doorAnimator.SetTrigger("Open");
        doorsAudioSource.Play();

        GameManager.instance.patientsDelivered++;

        isHoldingPatient = false;

        SetSiren(false);

        //disable hospital direction Arrow
        SetActiveAllNavigationArrows(false, new Transform[] { hospitalArrow });

        GameObject counter = timerSocket.GetChild(0).gameObject;
        if (counter != null)
        {
            Destroy(counter);
        }
    }
    public void Explode()
    {
        if (!exploded)
        { 
            //failsafe
            exploded = true;

            gameObject.SetActive(false);

            //spawn explosion 
            canExplode.CreateExplosion();

            //notify game manager and have it do its thing
            GameManager.instance.PlayerFuckingLoses();
        }
    }

    private void SetActiveAllNavigationArrows(bool set, Transform[] categories)
    {
        if (gameObject != null)
        {
            foreach (Transform arrowCategory in categories)
            {
                foreach (Transform arrowPivot in arrowCategory)
                {
                    arrowPivot.gameObject.SetActive(set);
                }
            }
        }
    }

    public void PickepUpPatient(NPC patient)
    {
        doorAnimator.SetTrigger("Open");
        doorsAudioSource.Play();

        isHoldingPatient = true;

        //enable hospital arrows
        SetActiveAllNavigationArrows(true, new Transform[] { hospitalArrow });

        //disable  arrow that was pointing at this patient
        TryRemovePatientArrow(patient);

        //engage lights
        SetSiren(true);
    }

    public void TryRemovePatientArrow(NPC patient)
    {
        if (patientArrows != null)
        {
            foreach (Transform pivotArrow in patientArrows)
            {
                if (pivotArrow != null && pivotArrow.gameObject.activeInHierarchy && pivotArrow.GetComponent<Arrow>().target == patient.transform)
                {
                    pivotArrow.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    private void SetSiren(bool set)
    {
        chasisMesh.materials[sirenMaterialIndexFromLeftToRight.x].SetFloat("_Blink", set.GetHashCode());
        chasisMesh.materials[sirenMaterialIndexFromLeftToRight.y].SetFloat("_Blink", set.GetHashCode());

        if (set)
        {
            sirenAudioSource.Play();
        }
        else
        {
            sirenAudioSource.Stop();
        }
    }

   public void SetLights(bool set)
   {
        chasisMesh.materials[lightMaterialIndex] = set? lightsOnMaterial : lightsOffMaterial;

        foreach (Light light in headLights)
        {
            light.enabled = set;
        }
   }
}
