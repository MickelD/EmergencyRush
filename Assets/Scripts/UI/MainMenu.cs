using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Awake()
    {
        Time.timeScale = 1f;

        int id = levelManager.currentLevel.ID;
        levelText.SetText("Level " + id);

        if (id <= 1)
        {
            continueButton.SetActive(false);
        }
    }

    public void PlayGame()
    {
        levelManager.currentLevel = levelManager.levelArray[1];
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
