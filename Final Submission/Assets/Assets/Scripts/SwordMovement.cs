using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// I put it on the right controller to track it. No need to put it the whole teleport
/// script on the right controller as well as the left.
/// </summary>
public class SwordMovement : MonoBehaviour {

    public SteamVR_TrackedObject trackedObj;
    public GameObject rig;
    public GameObject fireEffect;
    public GameObject sparkEffect;
    public GameObject laserPrefab;

    private Transform laserTransform;
    private Vector3 hitPoint;
    private GameObject laser;
    private _GameManager gameManagerScript;
    private RaycastHit hit;

    public SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        gameManagerScript = rig.GetComponent<_GameManager>();
    }

    private void Start()
    {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }
    // Update is called once per frame
    void Update () {
        //Debug.Log(Controller.velocity);
        if (!gameManagerScript.gamePlaying)
        {
            if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out hit))
            {
                if (Controller.GetHairTriggerDown())
                {
                    if (hit.transform.tag == "Play")
                    {
                        gameManagerScript.Play();
                        Debug.Log("Playing Game");
                    }
                    else if (hit.transform.tag == "Restart")
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                        gameManagerScript.Play();
                        Debug.Log("Resarting Game");
                    }
                    else if (hit.transform.tag == "Quit")
                    {
                        Application.Quit();
                        Debug.Log("Quitting Game");
                    }
                }
                //Instead of a laser it creates a fire particle effect where you point the controller
                //GameObject fire = Instantiate(fireEffect, hit.point, Quaternion.identity);
                //Destroy(fire, .5f);
                //GameObject sparks = Instantiate(sparkEffect, hit.point, Quaternion.LookRotation(hit.normal));
                //Destroy(sparks, .5f);
                hitPoint = hit.point;
                ShowLaser(hit);
            }  
        }
        else
        {
            laser.SetActive(false);
        }
    }

    /// <summary>
    /// This Method comes from a htc tutorial
    /// </summary>
    /// <param name="hit"></param>
    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true);
        // move the laser accordingly
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        // make the laser rotate accordingly
        laserTransform.LookAt(hitPoint);
        // size the laser accordingly
        // done by creating a new vector3 that gives the local scale of the x and y and sizes the z to fit exactly between the
        // hit.distance and the controller
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }
}
