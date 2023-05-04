using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class NPC : MonoBehaviour, IAmbulanceTarget, ICanExplode, ICanBeRunOver, IDependOnLOD
{
    [Header("AI"), Space(1)]
    public Scr_NpcSpawner navigationPoints;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Vector2 minMaxWaitingTime;
    private Coroutine updateCoroutine;

    [Space(3), Header("Patient Behaviour"), Space(1)]
    public bool elligibleAsPatient;
    [SerializeField] private GameObject bombHead;
    [SerializeField] private GameObject pickUpTrigger;
    public bool isPatient;
    [SerializeField] private GameObject fuseTimer;
    private float fuseTime;
    [SerializeField] private Transform timerSocket;

    [Space(3), Header("LOD"), Space(1)]
    [SerializeField] private Animator animator;

    [Space(3), Header("Destruction"), Space(1)]
    [SerializeField] private CanExplode canExplode;
    [SerializeField] private Rigidbody head;
    [SerializeField] private float headLifeTime;
    [SerializeField] private GameObject bloodExplosion;
    private bool exploded;
    private VisualEffect headBlood;
    private Collider headHitbox;

    [Space(3), Header("Audio"), Space(1)]
    [SerializeField] private AudioSource sound;
    private SoundManager soundManager;


    public static System.Action OnPatientPickedUp;

    private void Start()
    {
        head.isKinematic = true;

        headBlood = head.gameObject.GetComponent<VisualEffect>();
        headBlood.enabled = false;

        headHitbox = head.gameObject.GetComponent<Collider>();
        headHitbox.enabled = false;

        //Setear la primera localización a la que se moverá el NPC de forma aleatoria
        //Coger un hijo aleatorio del objeto que tiene todos los puntos de destino
        animator.SetTrigger("Walk");
        agent.destination = navigationPoints.transform.GetChild(Random.Range(0, navigationPoints.transform.childCount)).position;
        

        //disable pickUp collider (it is for patients only)
        pickUpTrigger.SetActive(false);

        //decide fusetime based on game rules
        fuseTime = GameManager.instance.patientFuseTime;

        exploded = false;
    }

    private void Update()
    {
        if (updateCoroutine == null && agent.enabled && agent.remainingDistance <= agent.stoppingDistance) 
        {
            
            StopAllCoroutines();

            if (!agent.pathPending)
            {
                updateCoroutine = StartCoroutine(WaitTime());
            }

            

        }
    }

    IEnumerator WaitTime()
    {
       
        animator.ResetTrigger("Walk");
        animator.SetTrigger("Idle");

        yield return new WaitForSeconds(Random.Range(minMaxWaitingTime.x, minMaxWaitingTime.y));

        
        agent.destination = navigationPoints.transform.GetChild(Random.Range(0, navigationPoints.transform.childCount)).position;

        updateCoroutine = null;

        animator.ResetTrigger("Idle");
        animator.SetTrigger("Walk");

    }

    public void SetAsPatient()
    {
        //NPC is set as a patient, disable movement and enable pick up radius
        StopAllCoroutines();
        agent.enabled = false;
        animator.SetTrigger("Idle");
        bombHead.SetActive(true);
        head.gameObject.SetActive(false);
        pickUpTrigger.SetActive(true);
        isPatient = true;

        //PlaySound(sound, 2);

        //create fuseTimer
        Instantiate(fuseTimer, timerSocket).GetComponent<Countdown>().time = fuseTime;
    }

    public void AmbulanceArrived(Player ambulance)
    {
        if (ambulance.isHoldingPatient == false)
        {
            OnPatientPickedUp?.Invoke();
            ambulance.PickepUpPatient(this);

            //transfer timer to player
            Transform counterTrans = timerSocket.GetChild(0);
            timerSocket.GetChild(0).parent = ambulance.timerSocket;
            counterTrans.transform.localPosition = Vector3.zero;
            counterTrans.transform.rotation = Quaternion.identity;
            //PlaySound(sound, 3);

            RemoveNPC();

        }
    }

    public void RemoveNPC()
    {
        if (isPatient)
        {
            GameManager.instance.player.TryRemovePatientArrow(this);
            GameManager.instance.patientCount--;
        }

        navigationPoints.npcsOnScene.Remove(this);

        //replace NPC
        navigationPoints.SpawnNPC();

        Destroy(gameObject);
    }

    public void Explode()
    {
        if (!exploded)
        {
            exploded = true;

            RemoveNPC();
            canExplode.CreateExplosion();
        }
    }

    public void RunOver(Transform source, Vector3 force)
    {
        if (!isPatient)
        {
            head.gameObject.transform.parent = null;

            head.isKinematic = false;
            head.AddForce(force, ForceMode.Impulse);
            head.AddTorque(force * 0.1f, ForceMode.Impulse);

            headBlood.enabled = true;
            headHitbox.enabled = true;

            Destroy(head.gameObject, headLifeTime);
            Destroy(Instantiate(bloodExplosion, transform.position, Quaternion.identity), headLifeTime);

            RemoveNPC();
        }
        else
        {
            Explode();
        }
    }
    private void PlaySound(AudioSource source, int numArray)
    {
        if (!source.isPlaying)
        {
            source.clip = soundManager.clips[numArray].clip;
            source.volume = soundManager.clips[numArray].volume;
            source.Play();
        }
    }

    public void OnLODTriggered(bool enter)
    {
        //NPCS should not suddenly become patients on the player's face, this helps to hide the lack of transition from one state to another
        elligibleAsPatient = !enter;
    }
}
