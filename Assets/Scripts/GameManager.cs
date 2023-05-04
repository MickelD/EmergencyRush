using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    [Header("Singleton"), Space(1)]
    public static GameManager instance;
    public LevelManager levelManager;
    public int levelID;

    [Space(3), Header("Game States"), Space(1)]
    public float deathDelay;

    [Space(3), Header("Scene References"), Space(1)]
    [SerializeField] private Scr_NpcSpawner npcSpawner;
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private GameObject sceneLight;
    public Camera mainCamera;
    public Player player;

    [Space(3), Header("Patient Generation"), Space(1)]
    public Vector2Int minMaxNPCsOnScene;
    public float patientFuseTime;
    [SerializeField] private Vector2 minMaxGenerationTime;
    [SerializeField] private float startupDelay;
    [SerializeField] private int maxActivePatients;
    public bool selectingPatients;
    public int patientCount;
    public int patientGoal;

    public System.Action OnPlayerDied;
    public System.Action<int, int> OnUpdatePatients;
    public System.Action OnPlayerWins;

    private int _patientsDelivered;
    public int patientsDelivered
    {
        get { 
            return _patientsDelivered; 
        }

        set { 
            _patientsDelivered = value;

            OnUpdatePatients?.Invoke(_patientsDelivered, patientGoal);

            if (_patientsDelivered >= patientGoal)
            {
                PlayerFuckingWins();
            }
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;

            SetGameRules();
        }

        levelID = levelManager.currentLevel.ID;
    }

    private IEnumerator Start()
    {
        patientCount = 0;
        selectingPatients = true;

        player.SetLights(levelManager.currentLevel.lightsOn);

        yield return null;

        patientsDelivered = 0;

        yield return new WaitForSeconds(startupDelay);

        while (true)
        {
            if (selectingPatients && patientCount < maxActivePatients)
            {
                TryPickPatient();
            }
            yield return new WaitForSeconds(Random.Range(minMaxGenerationTime.x, minMaxGenerationTime.y));
        }

    }

    void TryPickPatient()
    {
        NPC patient = npcSpawner.npcsOnScene[Random.Range(0, npcSpawner.npcsOnScene.Count)];

        if (!patient.isPatient && patient.elligibleAsPatient)
        {
            patientCount++;
            patient.SetAsPatient();

            //set navigation arrow pointing at this patient
            if (player != null)
            {
                foreach (Transform arrowPivot in player.patientArrows)
                {
                    if (!arrowPivot.gameObject.activeInHierarchy)
                    {
                        //set constraint source as patients transform
                        arrowPivot.GetComponent<Arrow>().target = patient.transform;

                        arrowPivot.transform.LookAt(patient.transform);

                        //activate arrow
                        arrowPivot.gameObject.SetActive(true);

                        break;
                    }
                }
            }
        }
        else
        {
            TryPickPatient();
        }
    }

    public void PlayerFuckingLoses()
    {
        OnPlayerDied?.Invoke();
    }

    public void PlayerFuckingWins()
    {
        OnPlayerWins?.Invoke();
    }

    private void SetGameRules()
    {
        Level level = levelManager.currentLevel;

        patientFuseTime = level.patientFuseTime;
        minMaxGenerationTime = level.minMaxGenerationTime;
        startupDelay = level.startupDelay;
        maxActivePatients = level.maxActivePatients;
        patientGoal = level.patientGoal;
        minMaxNPCsOnScene = level.minMaxNPCsOnScene;

        postProcessVolume.profile = level.postProcessProfile;
        Destroy(sceneLight);
        sceneLight = Instantiate(level.light, transform);
    }

}
