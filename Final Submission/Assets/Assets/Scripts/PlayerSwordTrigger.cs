using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSwordTrigger : MonoBehaviour
{

    NinjaAI ninjaScript;
    public GameObject ninjaObject;
    public GameObject rig;
    public float damageToHead = 15;
    public float damageToChest = 10;
    public float damageToWaist = 8;
    public float damageToUpperArm = 6;
    public float damageToLowerArm = 4;
    public float damageToUpperLeg = 6;
    public float damageToLowerLeg = 4;
    public Slider slider;
    public Slider sliderPlayer;

    private _GameManager gameManagerScript;

    // Use this for initialization
    void Start()
    {
        ninjaScript = ninjaObject.GetComponent<NinjaAI>();
        gameManagerScript = rig.GetComponent<_GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ninjaScript.aiRecenlyHit)
        {
            if (other.tag == "Head")
            {
                ninjaScript.aiHealth -= damageToHead;
                ninjaScript.aiRecenlyHit = true;
            }
            else if (other.tag == "Chest")
            {
                ninjaScript.aiHealth -= damageToChest;
                ninjaScript.aiRecenlyHit = true;
            }
            else if (other.tag == "Waist")
            {
                ninjaScript.aiHealth -= damageToWaist;
                ninjaScript.aiRecenlyHit = true;
            }
            else if (other.tag == "UpperArm")
            {
                ninjaScript.aiHealth -= damageToUpperArm;
                ninjaScript.aiRecenlyHit = true;
            }
            else if (other.tag == "LowerArm")
            {
                ninjaScript.aiHealth -= damageToLowerArm;
                ninjaScript.aiRecenlyHit = true;
            }
            else if (other.tag == "UpperLeg")
            {
                ninjaScript.aiHealth -= damageToUpperLeg;
                ninjaScript.aiRecenlyHit = true;
            }
            else if (other.tag == "LowerLeg")
            {
                ninjaScript.aiHealth -= damageToLowerLeg;
                ninjaScript.aiRecenlyHit = true;
            }
            else
            {
                Debug.Log("Player Sword connected with " + other.name);
            }
            //Debug.Log("aiHealth: " + ninjaScript.aiHealth);
            slider.value = ninjaScript.aiHealth;
        }
    }
}
