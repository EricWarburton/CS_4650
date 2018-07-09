using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTrigger : MonoBehaviour {

    NinjaAI ninjaScript;
    public GameObject ninjaObject;
    private float multiplier = 1.2f;

    // Use this for initialization
    void Start() {
        ninjaScript = ninjaObject.GetComponent<NinjaAI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hit player
        if (other.tag == "MainCamera")
        {
            //player health go down by however much the damage is.
            if (!ninjaScript.isBlocking && !ninjaScript.isDashing)
            {
                ninjaScript.playerHealth -= ninjaScript.currentDamage;
                Debug.Log("Player Health: " + ninjaScript.playerHealth);
                //value of that attack goes up
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
            }
        }
        else if (other.tag == "Sword")
        {
            //ninjaScript.StopAllCoroutines();
            
            ninjaScript.isStunned = false;
            ninjaScript.attack = 0;
            ninjaScript.animator.SetInteger("Attack", 0);
            Debug.Log("blocked");
            //value of that attack goes down
            switch (ninjaScript.currentAttack)
            {
                case 1: // Attack 1
                    ninjaScript.attackDamage1 /= multiplier;
                    break;
                case 2: // Attack 2
                    ninjaScript.attackDamage2 /= multiplier;
                    break;
                case 3: // Attack 3
                    ninjaScript.attackDamage3 /= multiplier;
                    break;
                case 4: // Combo 1
                    ninjaScript.comboAttack1Landed = false;
                    ninjaScript.comboDamage1 /= multiplier;
                    break;
                case 5: // Combo 2
                    ninjaScript.comboAttack2Landed = false;
                    ninjaScript.comboDamage2 /= multiplier;
                    break;
                case 6: // Combo 3
                    ninjaScript.comboDamage3 /= multiplier;
                    break;
                case 7: // Ranged 1
                    ninjaScript.rangedDamage1 /= multiplier;
                    break;
                case 8: // Ranged 2
                    ninjaScript.rangedDamage2 /= multiplier;
                    break;
                default:
                    Debug.Log("No valid currentAttack.");
                    break;
            }
        }
    }
}
