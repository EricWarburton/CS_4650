using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwordTrigger : MonoBehaviour
{

    NinjaAI ninjaScript;
    public GameObject playerSword;
    public GameObject ninjaObject;
    public GameObject rig;
    //public GameObject notifyEffect;
    public GameObject currentSword;
    // The reason I have about 2x more value for misses is due to the psychological concept of loss aversion
    private float multiplier = 1.1f;
    private float divider = 1.2f;
    private _GameManager gameManagerScript;
    private PlayerSwordTrigger playerSwordTriggerScript;

    // Use this for initialization
    void Start()
    {
        ninjaScript = ninjaObject.GetComponent<NinjaAI>();
        gameManagerScript = rig.GetComponent<_GameManager>();
        playerSwordTriggerScript = playerSword.GetComponent<PlayerSwordTrigger>();
        //currentSword = this.GetComponent<GameObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hit player
        Debug.Log("PlayerRecentlyHit: " + ninjaScript.playerRecentlyHit + " PlayerHp: " + ninjaScript.playerHealth);
        
        if (other.tag == "MainCamera" && !ninjaScript.playerRecentlyHit)
        {
            ninjaScript.playerRecentlyHit = true;
            //player health go down by however much the damage is.

            ninjaScript.playerHealth -= ninjaScript.currentDamage;
            //playerSwordTriggerScript.slider.value = ninjaScript.playerHealth;
            playerSwordTriggerScript.sliderPlayer.value = ninjaScript.playerHealth;
            Debug.Log("Current Damage: " + ninjaScript.currentDamage);
            Debug.Log("Player Health: " + ninjaScript.playerHealth);
            //value of that attack goes up
            if (ninjaScript.playerHealth < 0)
            {
                gameManagerScript.Loss();
                //Debug.Log("You Died!");
            }
            switch (ninjaScript.currentAttack)
            {
                case 1: // Attack 1
                    ninjaScript.attackDamage1 *= multiplier;
                    break;
                case 2: // Attack 2
                    ninjaScript.attackDamage2 *= multiplier;
                    break;
                case 3: // Attack 3
                    ninjaScript.attackDamage3 *= multiplier;
                    break;
                case 4: // Combo 1
                    ninjaScript.comboAttack1Landed = true;
                    ninjaScript.comboDamage1 *= multiplier;
                    break;
                case 5: // Combo 2
                    ninjaScript.comboAttack2Landed = true;
                    ninjaScript.comboDamage2 *= multiplier;
                    break;
                case 6: // Combo 3
                    ninjaScript.comboDamage3 *= multiplier;
                    break;
                case 7: // Ranged 1
                    ninjaScript.rangedDamage1 *= multiplier;
                    break;
                case 8: // Ranged 2
                    ninjaScript.rangedDamage2 *= multiplier;
                    break;
                default:
                    Debug.Log("No valid currentAttack.");
                    break;
            }
            /* Apparently OnCollisionEnter doesn't work on child objects.
             * I am still going to work on trying to figure out how to do this though if I have time.
            GameObject notifyHit = Instantiate(notifyEffect, contact.point, Quaternion.identity);
            Destroy(notifyHit, .3f);*/
        }
        else if (other.tag == "Sword")
        {
            ninjaScript.CancelAttack();
            //value of that attack goes down because attack was blocked
            switch (ninjaScript.currentAttack)
            {
                case 1: // Attack 1
                    ninjaScript.attackDamage1 /= divider;
                    break;
                case 2: // Attack 2
                    ninjaScript.attackDamage2 /= divider;
                    break;
                case 3: // Attack 3
                    ninjaScript.attackDamage3 /= divider;
                    break;
                case 4: // Combo 1
                    ninjaScript.comboAttack1Landed = false;
                    ninjaScript.comboDamage1 /= divider;
                    break;
                case 5: // Combo 2
                    ninjaScript.comboAttack2Landed = false;
                    ninjaScript.comboDamage2 /= divider;
                    break;
                case 6: // Combo 3
                    ninjaScript.comboDamage3 /= divider;
                    break;
                case 7: // Ranged 1
                    ninjaScript.rangedDamage1 /= divider;
                    break;
                case 8: // Ranged 2
                    ninjaScript.rangedDamage2 /= divider;
                    break;
                default:
                    Debug.Log("No valid currentAttack.");
                    break;
            }
            /* Apparently OnCollisionEnter doesn't work on child objects
            GameObject notifyBlock = Instantiate(notifyEffect, contact.point, Quaternion.identity);
            Destroy(notifyBlock, .3f);*/

        }
    }
}
