using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WorldSettings : MonoBehaviour
{
    public bool developerMode = false;

    public Text gameOverText;

    private bool isGameOver = false;

    public float AmbientRadiationLevel { get; private set; }

    public float highRadiationDamage = 1f;
    public float RadiationMax { get; set; }

    private TimeAndDate timeController;

    private string currentScene;

    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        gameOverText.text = "";
        foreach (GameObject statBar in GameObject.FindGameObjectsWithTag("StatBar"))
            statBar.SetActive(developerMode);

        timeController = FindObjectOfType<TimeAndDate>();

        RadiationMax = highRadiationDamage;
        AmbientRadiationLevel = 0;

        currentScene = SceneManager.GetActiveScene().name;

        startTime = Time.time;
    }

    void Update()
    {
        if (currentScene.Equals("Island")) // If the player is outside.
        {
            // Equation that controls the level of ambientRadiation the player takes.
            // Currently setup so the player dies after about 2 days if they stay outside
            AmbientRadiationLevel = Mathf.Clamp(1.5f * Mathf.Exp(1.5f * (((Time.time - startTime) / timeController.dayDuration) - 2.3f)) - .047f, 0, RadiationMax);
        }
    }

    public void SetGameOver()
    {
        isGameOver = true;

        // get amount of days player lasted
        int numDays = (int)GetComponent<TimeAndDate>().numberDays;

        gameOverText.text = "Game Over!\n"+ "You lasted " + numDays.ToString() + " days.";

        // Invoke("LoadCurrentLevel", 3);
        Invoke("LoadMainMenu", 3);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    void LoadCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
