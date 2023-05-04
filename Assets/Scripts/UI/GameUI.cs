using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Audio;

public class GameUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    [Header("Elements"), Space(1)]
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject levelCompleteMenu;
    private Player player;

    [Header("Gas Bar"), Space(1)]
    [SerializeField] private Image gasBar;

    [Header("Speed Bar"), Space(1)]
    [SerializeField] private Image speedBar;
    private float maxSpeed;

    [Header("Patient Counter"), Space(1)]
    [SerializeField] private TextMeshProUGUI patientsCounter;

    [Header("Level Text"), Space(1)]
    [SerializeField] private TextMeshProUGUI levelText;

    [Space(3), Header("Pause Menu"), Space(1)]
    private bool canPause;
    private bool gameIsPaused;

    [Space(3), Header("Level Complete Menu"), Space(1)]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private AudioSource victoryAudioSource;

    [Space(3), Header("Sound groups"), Space(1)]
    [SerializeField] private AudioMixerGroup musicSoundGroup;
    [SerializeField] private AudioMixerGroup gameSoundGroup;

    private void Start()
    {
        GameManager.instance.player.OnUpdateFuel += UpdateFuel;
        GameManager.instance.OnPlayerDied += OnPlayerDeath;
        GameManager.instance.OnUpdatePatients += UpdatePatients;
        GameManager.instance.player.OnUpdateSpeed += UpdateSpeed;
        GameManager.instance.OnPlayerWins += LevelCompleteUI;
        maxSpeed = GameManager.instance.player.maxSpeed;

        Time.timeScale = 1f;

        canPause = true;

        levelText.text = "LEVEL " + GameManager.instance.levelID.ToString();

        gameSoundGroup.audioMixer.SetFloat("GameVolume", 0f);
        musicSoundGroup.audioMixer.SetFloat("MusicVolume", 0f);

        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        levelCompleteMenu.SetActive(false);
        HUD.SetActive(true);
    }

    private void OnDisable()
    {
        GameManager.instance.player.OnUpdateFuel -= UpdateFuel;
        GameManager.instance.OnPlayerDied -= OnPlayerDeath;
        GameManager.instance.OnUpdatePatients -= UpdatePatients;
        GameManager.instance.player.OnUpdateSpeed -= UpdateSpeed;
        GameManager.instance.OnPlayerWins -= LevelCompleteUI;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canPause)
        {
            if (gameIsPaused)
            {
                Resume();
                HUD.SetActive(true);
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;

        gameSoundGroup.audioMixer.SetFloat("GameVolume", 0f);
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
        HUD.SetActive(false);

        gameSoundGroup.audioMixer.SetFloat("GameVolume", -80f);
    }

    public IEnumerator GameOverUI()
    {
        yield return new WaitForSecondsRealtime(GameManager.instance.deathDelay);

        gameOverMenu.SetActive(true);
        gameIsPaused = true;
    }

    public void LevelCompleteUI()
    {
        gameSoundGroup.audioMixer.SetFloat("GameVolume", -80f);
        musicSoundGroup.audioMixer.SetFloat("MusicVolume", -80f);
        victoryAudioSource.Play();

        float _timeSinceLevelLoad = Time.timeSinceLevelLoad;

        int m = (int)_timeSinceLevelLoad / 60;
        int s = (int)_timeSinceLevelLoad % 60;
        int cs = ((int)(_timeSinceLevelLoad * 100)) % 100;

        timeText.text = string.Format("{0:00}'{1:00}.{2:00}\"", m, s, cs);

        levelCompleteMenu.SetActive(true);
        Time.timeScale = 0f;
        canPause = false;
        HUD.SetActive(false);
    }

    private void OnPlayerDeath()
    {
        musicSoundGroup.audioMixer.SetFloat("MusicVolume", -80f);
        HUD.SetActive(false);
        Time.timeScale = 0.8f;
        canPause = false;

        StartCoroutine(GameOverUI());
    }

    public void LoadNextLevel()
    {
        int id = levelManager.currentLevel.ID;
        id++;

        //Due to a visual bug with Serialized Classes, the LVL of index 0 has no attributes, we start counting at 1, so 6 lvls (from 0 to 5) is really just 5 (from 1 to 5)
        if (id >= levelManager.levelArray.Length)
        {
            levelManager.currentLevel = levelManager.levelArray[levelManager.levelArray.Length - 1];
            SceneManager.LoadScene(0);
        }
        else
        {
            levelManager.currentLevel = levelManager.levelArray[levelManager.currentLevel.ID + 1];
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Exit()
    {
        print("See u soon ^^");
        Application.Quit();
    }

    private void UpdateFuel(float f)
    {
        gasBar.fillAmount = f;
    }

    private void UpdateSpeed(float speed)
    {
        speedBar.fillAmount = speed / maxSpeed;
    }

    private void UpdatePatients(int current, int goal)
    {
        patientsCounter.text = current.ToString("00") + "/" + goal.ToString("00");
    }
}
