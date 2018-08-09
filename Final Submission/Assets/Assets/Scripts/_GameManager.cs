using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _GameManager : MonoBehaviour
{
    NinjaAI ninjaScript;
    public bool gamePlaying = false;
    public GameObject ninjaObject;
    public GameObject sword;
    public GameObject rightController;
    public GameObject leftControllerCanvas;
    public GameObject ninja;
    public GameObject menu;
    public GameObject restartText;
    public GameObject playText;
    public GameObject winText;
    public GameObject lossText;
    public GameObject playerHealth;
    public GameObject laser;
    public Transform cameraRigTransform;
    private Vector3 origin;

    private string activeButton;

    private void Start()
    {
        ninjaScript = ninjaObject.GetComponent<NinjaAI>();
        origin = cameraRigTransform.position;
    }

    /// <summary>
    /// When play button is pressed
    /// </summary>
    public void Play()
    {
        ninja.SetActive(true);
        sword.SetActive(true);
        leftControllerCanvas.SetActive(true);
        rightController.SetActive(false);
        menu.SetActive(false);
        gamePlaying = true;
        playerHealth.SetActive(true);
        ninjaScript.playerHealth = ninjaScript.playerStartingHealth;
        ninjaScript.aiHealth = ninjaScript.aiStartingHealth;
    }

    /// <summary>
    /// called when ai health drops to 0
    /// </summary>
    public void Win()
    {
        GameOver();
        winText.SetActive(true);
    }

    /// <summary>
    /// called when player health drops to 0
    /// </summary>
    public void Loss()
    {
        GameOver();
        lossText.SetActive(true);
    }

    /// <summary>
    /// sets stuff active or not after the game
    /// </summary>
    private void GameOver()
    {
        ninja.SetActive(false);
        sword.SetActive(false);
        playText.SetActive(false);
        leftControllerCanvas.SetActive(false);
        rightController.SetActive(true);
        menu.SetActive(true);
        restartText.SetActive(true);
        gamePlaying = false;
        playerHealth.SetActive(false);
        cameraRigTransform.position = origin;
    }
}
