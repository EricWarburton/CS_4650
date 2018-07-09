using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Teleport : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;
    private Vector3 lastBounceTransform;
    private NinjaAI ninjaAIScript;
    private float recharge;
    private bool wasGoingDown = true;

    public GameObject teleportBall;
    public GameObject ninja;
    public Transform defaultBallTransform;
    public Transform cameraRigTransform;
    public Transform headTransform;
    public LayerMask teleportMask;
    public bool shouldTeleport;
    public float rechargeTime;
    public Slider slider;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    private void Start()
    {
        ninjaAIScript = ninja.GetComponent<NinjaAI>();
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (recharge <= rechargeTime)
        {
            recharge += Time.deltaTime;
            slider.value = recharge / rechargeTime;
        }
        if (Controller.GetHairTriggerDown())
        {
            if(recharge > rechargeTime)
            {
                teleportBall.SetActive(true);
                ReturnToController();
                // don't want to teleport while holding the ball, that would be OP!
                shouldTeleport = false;
                recharge = 0f;
            }
        }

        if(Controller.GetHairTriggerUp())
        {
            ReleaseObject();
        }
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            if (lastBounceTransform != defaultBallTransform.transform.position && shouldTeleport)
            {
                TeleportPlayer();
                teleportBall.SetActive(false);
                ninjaAIScript.PlayerTeleported();
            }
        }
        
        // if going up and was going down then it just bounced
        if (teleportBall.GetComponent<Rigidbody>().velocity.y > 0 && wasGoingDown)
        {
            if (!GetComponent<FixedJoint>())
            {
                lastBounceTransform.x = teleportBall.transform.position.x;
                lastBounceTransform.z = teleportBall.transform.position.z;
                lastBounceTransform.y = 0f;
                //Debug.Log("LastBounce" + lastBounceTransform);
                wasGoingDown = false;
                shouldTeleport = true;
            }
        }
        // else if it is going down and it was going up set wasGoingDown back to true
        else if(teleportBall.GetComponent<Rigidbody>().velocity.y < 0)
        {
            wasGoingDown = true;
        }
    }


    // I assume that this is not working because of some error with the scene.
    // I most likely have trigger box checked when it shouldn't be or vice versa
    // In the mean time I am using the simple velocity checker above.
    /*    /// <summary>
    /// Sets lastBounceTransform to where the ball last bounced
    /// </summary>
    /// <param name="other"></param>
    public void OnTriggerEnter(Collider other)
    {
        // Not modifying the y because that would move the player 
        // in the y direction the ball's radius every time it teleported.
        lastBounceTransform.x = teleportBall.transform.position.x;
        lastBounceTransform.z = teleportBall.transform.position.z;
        lastBounceTransform.y = 0f;
        shouldTeleport = true;
        Debug.Log("LastBounce" + lastBounceTransform);
    }*/

    /// <summary>
    /// Returns the teleport object to the controller.
    /// </summary>
    private void ReturnToController()
    {
        teleportBall.transform.position = defaultBallTransform.position;
        var joint = AddFixedJoint();
        joint.connectedBody = teleportBall.GetComponent<Rigidbody>();
    }

    // Make a new fixed joint, add it to the controller, and then set it up so it doesn’t break easily.
    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    private void ReleaseObject()
    {
        // Make sure there’s a fixed joint attached to the controller.
        if (GetComponent<FixedJoint>())
        {
            // Remove the connection to the object held by the joint and destroy the joint.
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            // Add the speed and rotation of the controller when the player releases the object, so the result is a realistic arc.
            teleportBall.GetComponent<Rigidbody>().velocity = Controller.velocity;
            teleportBall.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
        }
    }

    private void TeleportPlayer()
    {
        //make sure you don't teleport again while you are already teleporting.
        shouldTeleport = false;
        // Calculates diference between cameraRigs center and the position of the head to ensure
        // accurate teleporting if the player is not standing in the center of the cameraRig
        Vector3 difference = cameraRigTransform.position - headTransform.position;
        // Makes sure that the camera height isn't messed with.
        difference.y = 0f;
        cameraRigTransform.position = lastBounceTransform + difference;
    }
}
