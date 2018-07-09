using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// I put it on the right controller to track it. No need to put it the whole teleport
/// script on the right controller as well as the left.
/// </summary>
public class SwordMovement : MonoBehaviour {

    public SteamVR_TrackedObject trackedObj;

    public SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update () {
        //Debug.Log(Controller.velocity);
    }
}
