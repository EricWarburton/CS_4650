using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NinjaAI : MonoBehaviour
{
    #region Variables + Start
    [HideInInspector] public Animator animator;
    public float fightOffset;
    public float gravity = -9.83f;
    public float runSpeed = 8f;
    public float walkSpeed = 3f;
    public float playerHealth = 100;
    public float aiHealth = 100;
    [HideInInspector] public float currentDamage;
    public GameObject headset;
    [HideInInspector] public float comboDamage1;
    [HideInInspector] public float comboDamage2;
    [HideInInspector] public float comboDamage3;
    [HideInInspector] public float attackDamage1;
    [HideInInspector] public float attackDamage2;
    [HideInInspector] public float attackDamage3;
    [HideInInspector] public float rangedDamage1;
    [HideInInspector] public float rangedDamage2;
    [HideInInspector] public float cAttack1Cost = 10f;
    [HideInInspector] public float cAttack2Cost = 10f;
    [HideInInspector] public float cAttack3Cost = 10f;
    [HideInInspector] public float attack1Cost = 5f;
    [HideInInspector] public float attack2Cost = 7f;
    [HideInInspector] public float attack3Cost = 50f;
    [HideInInspector] public float rangeAttack1Cost = 10;
    [HideInInspector] public float rangeAttack2Cost = 15;
    [HideInInspector] public float currentEnergy = 100;
    [HideInInspector] public bool isStunned = false;
    [HideInInspector] public bool isBlocking = false;
    [HideInInspector] public bool isDashing = false;
    [HideInInspector] public bool comboAttack1Landed;
    [HideInInspector] public bool comboAttack2Landed;
    [HideInInspector] public int attack = 0;
    [HideInInspector] public int currentAttack;
    //states include run, fight, 
    public string aiState;
    //states include fight, defend,
    public string playerState;
    
    bool canChain;

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject weaponModel;
    [SerializeField] private GameObject secondaryWeaponModel;
    [SerializeField] private GameObject playerSword;
    [SerializeField] private GameObject ninja;
    //energy costs
    [SerializeField] private float dodgeCost = 5f;

    private CapsuleCollider hitBox;
    private Vector3 fightingDistance;
    private Vector3 headsetOffset;
    private Vector3 temp;
    private Vector3 swordVelocity;
    private SwordMovement swordMovementScript;
    private float distanceFromSword;
    private float xDistance;
    private float zDistance;
    private float totalOffset;
    private float runPercent;
    private float blockEnergy = 50;
    private float dodgeValue;
    private float specialDamage;
    private float lookx;
    private float lookz;
    private bool runMore = true;
    
    Rigidbody rigidBody;

    // Use this for initialization
    void Start()
    {
        swordMovementScript = playerSword.GetComponent<SwordMovement>();
        animator = this.GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        ResetValues();
        hitBox = ninja.GetComponent<CapsuleCollider>();
    }

    /// <summary>
    /// sets the values of each attack (higher value means a higher chance of the AI using it)
    /// This makes each fight feel consistent, but different fights feel different.
    /// </summary>
    private void ResetValues()
    {
        // starting values, they change through the game
        attackDamage1 = Random.Range(4, 7); // more likely to use this one since a2 costs more
        attackDamage2 = Random.Range(4, 7); // very likely to use this one
        attackDamage3 = Random.Range(28, 32); // only likely to use this if high energy
        comboDamage1 = Random.Range(4, 7); // less likely to use this one since a1 costs half as much
        comboDamage2 = comboDamage1 * 2;
        comboDamage3 = comboDamage1 * 3;
        rangedDamage1 = Random.Range(9, 11); // more likely to use ranged attack 1
        rangedDamage2 = Random.Range(9, 12); // less likely to use ranged attack 2
    }

    #endregion

    #region Updates
    private void FixedUpdate()
    {
        rigidBody.AddForce(0, gravity, 0, ForceMode.Acceleration);
    }

    // Update is called once per frame
    void Update()
    {
        //1 unit == 1 meter real world space
        swordVelocity = swordMovementScript.Controller.velocity;//RE-ENABLE AT HOME//////////////////////////////////////////////////////////////////////////////
        //Debug.Log("Looking angle" + transform.forward);
        // This should in theory make the AI more aggressive if the player is more aggressive
        currentEnergy += Time.deltaTime + (Mathf.Abs(swordVelocity.x) +
                                   Mathf.Abs(swordVelocity.y) +
                                   Mathf.Abs(swordVelocity.z)) / 20f;

        SetDestination();
        SetState();

    }

    #endregion

    #region Set State
    /// <summary>
    /// Makes sure the ninja isn't running towards the player, and if he isn't then he will decide how to fight
    /// </summary>
    private void SetState()
    {
        //Debug.Log("energy " + currentEnergy);
        //Still need to do blocking

        if (aiState == "fight") // if the AI is not moving
        {
            // if player swinging sword

            // is it better for the AI to trade hits or dodge?
            if (playerHealth > aiHealth)
            {
                Debug.Log("Dodge");
                // Trading hits is more beneficial for the AI than to dodge
                int randomNum = Random.Range(0, 10);
                if (randomNum < 7)// higher chance for the AI to try to trade hits than to dodge
                {
                    Attack();
                    
                }
                else
                {
                    Dodge();

                }
            }
            else if (currentEnergy < 0)
            {
                
                Block();
                FacePlayer();
                //Debug.Log("currentEnergy: " + currentEnergy);
            }
            else // Player is not swinging the sword at AI
            {
                if (currentEnergy > 0)
                {
                    if(isBlocking)
                    {
                        isStunned = false;
                    }
                    isBlocking = false;
                    //isDashing = false;
                    animator.SetBool("Block", false);
                }
                Debug.Log("attack");
                Attack();
            }
        }
        else
        {
            Debug.Log("AI is running");
        }
    }
    #endregion

    #region Attacking
    /// <summary>
    /// Decides what hit is going to be the most efficient for the AI to pull off.
    /// </summary>
    private void Attack()
    {
        Debug.Log(isBlocking + " " + isDashing + " " + isStunned);
        //StopAllCoroutines();
        if (!isBlocking && !isDashing && !isStunned)
        {
            // deciding this based on value vs cost. 
            // Each attack will have a different cost/value based on the situation.
            // Each attack has a base value that will change depending on different variables
            // such as how many times that move has already been used.
            // if an attack hits, then make the damage go up otherwise make the damage go down.
            float a1Value;
            float a2Value;
            float a3Value;
            float c1Value;
            float r1Value;
            float r2Value;

            a1Value = (currentEnergy - attack1Cost) * attackDamage1;
            a2Value = (currentEnergy - attack2Cost) * attackDamage2;
            a3Value = (currentEnergy - attack3Cost) * attackDamage3;
            c1Value = (currentEnergy - cAttack1Cost) * comboDamage1;
            r1Value = (currentEnergy - rangeAttack1Cost) * rangedDamage1;
            r2Value = (currentEnergy - rangeAttack2Cost) * rangedDamage2;
            /*
            // The following if else statements are an attempt to fix a NaN error that was being returned after a bit.
            // Will be removed as soon as I know that my current fix actually worked.
            if (currentEnergy - attack1Cost > 0)
            {
                a1Value = (currentEnergy - attack1Cost) * attackDamage1;
                //Debug.Log("Attack1 value: " + a1Value);
                //Debug.Log("currentEnergy: " + currentEnergy + ", attack1Cost: " + attack1Cost + ", attackDamage1: " + attackDamage1);
            }
            else
            {
                a1Value = 2f;
            }
            if(currentEnergy - attack2Cost > 0)
            {
                a2Value = (currentEnergy - attack2Cost) * attackDamage2;
                //Debug.Log("Attack2 value: " + a2Value);
                //Debug.Log("currentEnergy: " + currentEnergy + ", attack2Cost: " + attack2Cost + ", attackDamage2: " + attackDamage2);
            }
            else
            {
                a2Value = .1f;
            }
            if(currentEnergy - attack3Cost > 0)
            {
                a3Value = (currentEnergy - attack3Cost) * attackDamage3;
            }
            else
            {
                a3Value = .1f;
            }
            if(currentEnergy - cAttack1Cost > 0)
            {
                c1Value = (currentEnergy - cAttack1Cost) * comboDamage1;
            }
            else
            {
                c1Value = .1f;
            }
            if(currentEnergy - rangeAttack1Cost > 0)
            {
                r1Value = (currentEnergy - rangeAttack1Cost) * rangedDamage1;
            }
            else
            {
                r1Value = .1f;
            }
            if(currentEnergy - rangeAttack2Cost > 0)
            {
                r2Value = (currentEnergy - rangeAttack2Cost) * rangedDamage2;
            }
            else
            {
                r2Value = .1f;
            }*/
            
            // find highest value
            
            float[] array = { a1Value, a2Value, a3Value, c1Value, r1Value, r2Value };
            float maxValue = 0;

            maxValue = Mathf.Max(array[0], array[1]);
            int i = 2;
            while (i < array.Length)
            {
                maxValue = Mathf.Max(array[i], maxValue);
                i++;
            }

            // play the animation with the highest value
            if (!comboAttack1Landed && !comboAttack2Landed)
            {
                if (maxValue == a1Value)
                {
                    Attack1();
                    currentEnergy -= attack1Cost;
                    Debug.Log("Attack1");
                    currentDamage = attackDamage1;
                    currentAttack = 1;
                }
                else if (maxValue == a2Value)
                {
                    Attack2();
                    currentEnergy -= attack2Cost;
                    Debug.Log("Attack2");
                    currentDamage = attackDamage2;
                    currentAttack = 2;
                }
                else if (maxValue == a3Value)
                {
                    Attack3();
                    currentEnergy -= attack3Cost;
                    Debug.Log("Attack3");
                    currentDamage = attackDamage3;
                    currentAttack = 3;
                }
                else if (maxValue == c1Value)
                {
                    currentEnergy -= cAttack1Cost;
                    Debug.Log("Combo1");
                    StartCoroutine(_Attack1());
                    currentDamage = comboDamage1;
                    currentAttack = 4;
                }
                else if (maxValue == r1Value)
                {
                    RangedAttack1();
                    currentEnergy -= rangeAttack1Cost;
                    Debug.Log("Ranged1");
                    currentDamage = rangedDamage1;
                    currentAttack = 7;
                }
                else if (maxValue == r2Value)
                {
                    RangedAttack2();
                    currentEnergy -= rangeAttack2Cost;
                    Debug.Log("Ranged2");
                    currentDamage = rangedDamage2;
                    currentAttack = 8;
                }
                else
                {
                    Debug.Log("maxValue does not match any of these options.");
                    Debug.Log("A1 " + a1Value + " A2 " + a2Value + " A3 " + a3Value + " c1 " + c1Value + " r1 " + r1Value + " r2 " + r2Value);
                }
            }
            else // A combo attack hit, so figure out which one and if the AI has enough energy then use the next attack in the combo
            {
                if (comboAttack1Landed && !comboAttack2Landed)
                {
                    StartCoroutine(_Attack2());
                    currentEnergy -= cAttack2Cost;
                    Debug.Log("Combo2");
                    currentDamage = comboDamage2;
                    currentAttack = 5;
                }
                else if (comboAttack2Landed)
                {
                    StartCoroutine(_Attack3());
                    currentEnergy -= cAttack3Cost;
                    Debug.Log("Combo3");
                    currentDamage = comboDamage3;
                    currentAttack = 6;
                }
            }
        }
        FacePlayer();
    }

    #endregion

    #region Dodging + Blocking
    /// <summary>
    /// Returns the direction that the player is swingnig the sword
    /// Possible return values: Unassigned, Left, Front, Right
    /// </summary>
    /// <returns></returns>
    private string PlayerSwing()
    {
        //Unassigned / Left / Front / Right
        //The AI will never have to dodge forwards, so a sword swinging backwards will never be a problem
        currentDamage = 0; 
        string dodgeDirection = "Unassigned";

        lookx = transform.forward.x;
        lookz = transform.forward.z;

        //Debug.Log(swordVelocity);
        if (lookx >= 0 && lookz > 0)// Looking between 0 and 90 degrees
        {
            if (swordVelocity.x <= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Left";
            }
            else if (swordVelocity.x >= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Forward";
            }
            else if (swordVelocity.x >= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Right";
            }
        }
        else if (lookx <= 0 && lookz > 0)// Looking between 90 and 180 degrees
        {
            if (swordVelocity.x <= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Left";
            }
            else if (swordVelocity.x <= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Forward";
            }
            else if (swordVelocity.x >= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Right";
            }
        }
        else if (lookx <= 0 && lookz < 0)// Looking between 180 and 270 degrees
        {
            if (swordVelocity.x >= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Left";
            }
            else if (swordVelocity.x <= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Forward";
            }
            else if (swordVelocity.x <= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Right";
            }
        }
        else if (lookx >= 0 && lookz < 0)// Looking between 270 and 360 degrees
        {
            if (swordVelocity.x >= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Left";
            }
            else if (swordVelocity.x >= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Forward";
            }
            else if (swordVelocity.x <= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Right";
            }
        }
        return dodgeDirection;
    }

    /// <summary>
    /// dodges the players sword.
    /// </summary>
    private void Dodge()
    {
        // make sure that the sword is moving fast enough for a swing
        // to make sure this isn't abused I need to disable hit boxes if it enters a certain range slowly, to prevent it from speednig up suddenly for hits.
        // calculate distance between the sword and the ninja to make sure he isn't overly jumpy
        // distance from the axis of the sword to the tip is .245 units
        distanceFromSword = .245f + Mathf.Sqrt((transform.position.x - playerSword.transform.position.x) * (transform.position.x - playerSword.transform.position.x)
            + (transform.position.z - playerSword.transform.position.z) * (transform.position.z - playerSword.transform.position.z)); // ^2 operator was not working
        Debug.Log(distanceFromSword);
        if (!isDashing && !isStunned)// don't need to dodge if ai is already in a dodge or attacking
        {
            if (Mathf.Abs(swordVelocity.x) > 2 || Mathf.Abs(swordVelocity.y) > 2 || Mathf.Abs(swordVelocity.z) > 1
                && distanceFromSword < 1)
            {
                Debug.Log(PlayerSwing());
                if ("Right" == PlayerSwing())
                {
                    StartCoroutine(_Dash(2));
                    currentEnergy -= dodgeCost;
                    Debug.Log("Dash Right");
                }
                if ("Left" == PlayerSwing())
                {
                    StartCoroutine(_Dash(4));
                    currentEnergy -= dodgeCost;
                    Debug.Log("Dash Left");
                }
                if ("Forward" == PlayerSwing())
                {
                    StartCoroutine(_Dash(3));
                    currentEnergy -= dodgeCost;
                    Debug.Log("Dash Back");
                }
                if(PlayerSwing() == "Undefined")
                {
                    Debug.Log("Player Swung Back");
                }
            }
        }
    }

    /// <summary>
    /// Sets the ninja to block
    /// Will implement the blocking mechanics soon.
    /// </summary>
    private void Block()
    {
        //Debug.Log("Energy " + currentEnergy);
        animator.SetBool("Block", true);
        isBlocking = true;
    }
    #endregion

    #region Rotation & Movement
    /// <summary>
    /// Call if the player just teleported to rotate the player accordingly
    /// </summary>
    public void PlayerTeleported()
    {
        runMore = true;
        //make sure the ninja isn't in the middle of a swing
        FacePlayer();
        isBlocking = false;
        animator.SetBool("Block", false);
    }

    /// <summary>
    /// Faces the AI to look at the player.
    /// </summary>
    private void FacePlayer()
    {
        Vector3 turnPosition = target.transform.position;
        turnPosition.y = 0;
        transform.LookAt(turnPosition, Vector3.up);
    }

    /// <summary>
    /// Makes the AI follow the player at an offset and makes the animation
    /// of the running speed appropriate for different distances
    /// Also, rotates the player in certain circumstances
    /// </summary>
    private void SetDestination()
    {
        xDistance = Mathf.Abs(transform.position.x - target.transform.position.x);
        zDistance = Mathf.Abs(transform.position.z - target.transform.position.z);

        if ((xDistance > fightOffset || zDistance > fightOffset || //too far away from the target, so move closer
            xDistance < -fightOffset || zDistance < -fightOffset) 
            && runMore && !isDashing && !isBlocking)
        {
            aiState = "run";
            animator.SetBool("Moving", true);
            animator.SetBool("Running", true);
            navMeshAgent.SetDestination(target.transform.position);
            runPercent = (xDistance + zDistance) / 10; // distance of ~14 will produce max run speed
            animator.SetFloat("Input Zf", runPercent); // why the crap was the default animation set type for Input Z an Int???
            navMeshAgent.speed = runPercent * 8; // 8 seem to match his animation speed decently
        }
        else if((xDistance < fightOffset &&  xDistance > fightOffset / 2)||( zDistance < fightOffset  && zDistance > fightOffset / 2) ||
                 (xDistance > -fightOffset && xDistance < -fightOffset / 2)||( zDistance > -fightOffset && zDistance < -fightOffset / 2) &&
                 !isDashing && !isBlocking)
        {
            aiState = "fight";
            Debug.Log("Medium distance");
            animator.SetBool("Moving", false);
            animator.SetBool("Running", false);
            navMeshAgent.SetDestination(transform.position);
            temp = target.transform.position;
            runMore = false; // prevents the character from being super jumpy if the player moves back slightly inside the play area.Debug.Log("Move back a few steps");
        }
        else if(!isDashing)
        {
            aiState = "run";
            Debug.Log("Move back a few steps");
            currentDamage = 0;
            FacePlayer();
            Debug.Log(xDistance + " " + zDistance);
            if(xDistance < fightOffset / 2 && zDistance < fightOffset / 2 || xDistance > -fightOffset / 2 && zDistance > -fightOffset / 2)
            {
                //StopAllCoroutines();
                StartCoroutine(_Dash(3));
            }
            isStunned = false;
            runMore = true;
        }
    }
    #endregion

    #region Animaitons
    //For some reason there were several animations included in the pack without methods

    /// <summary>
    /// Attack 1 
    /// </summary>
    public void Attack1()
    {
        StopAllCoroutines();
        attack = 1;
        animator.SetTrigger("Attack1RTrigger");
        StartCoroutine(_LockMovementAndAttack(.9f));
    }

    /// <summary>
    /// Ranged Attack 1
    /// </summary>
    public void RangedAttack1()
    {
        StopAllCoroutines();
        secondaryWeaponModel.SetActive(false);
        animator.SetTrigger("RangeAttack2Trigger");
        StartCoroutine(_LockMovementAndAttack(.9f));
        secondaryWeaponModel.SetActive(true);
    }

    /// <summary>
    /// Ranged Attack 2
    /// </summary>
    public void RangedAttack2()
    {
        StopAllCoroutines();
        secondaryWeaponModel.SetActive(false);
        animator.SetTrigger("RangeAttack3Trigger");
        StartCoroutine(_LockMovementAndAttack(.9f));
        secondaryWeaponModel.SetActive(true);
    }
    #endregion

    #region Methods that are NOT mine
    // They came from the ninja warrior animation pack.
    // I modified a few things to fit an AI rather than controller for a few of them though.

    /// <summary>
    /// Was ranged attack 1, upward slash
    /// </summary>
    public void Attack2()
    {
        StopAllCoroutines();
        animator.SetTrigger("RangeAttack1Trigger");
        attack = 4;
        StartCoroutine(_LockMovementAndAttack(0.9f));
    }

    /// <summary>
    /// Was MoveAttack, spinning attack
    /// </summary>
    public void Attack3()
    {
        StopAllCoroutines();
        attack = 5;
        animator.SetTrigger("MoveAttack1Trigger");
        StartCoroutine(_LockMovementAndAttack(0.9f));
    }

    /// <summary>
    /// comboAttack1
    /// </summary>
    /// <returns></returns>
    IEnumerator _Attack1()
    {
        StopAllCoroutines();
        float tempHealth = playerHealth;
        canChain = false;
        animator.SetInteger("Attack", 1);
        attack = 1;
        StartCoroutine(_ChainWindow(0.2f, 0.7f));
        StartCoroutine(_LockMovementAndAttack(0.6f));
        if(tempHealth > playerHealth)//used to allow the AI to continue the combo
        {
            Debug.Log("Combo1 Landed");
            comboAttack1Landed = true;
        }
        else
        {
            Debug.Log("Combo1 failed");
            comboAttack1Landed = false;
        }
        yield return null;
    }

    /// <summary>
    /// comboAttack2
    /// </summary>
    /// <returns></returns>
    IEnumerator _Attack2()
    {
        StopAllCoroutines();
        float tempHealth = playerHealth;
        canChain = false;
        animator.SetInteger("Attack", 2);
        attack = 2;
        StartCoroutine(_ChainWindow(0.2f, 0.8f));
        StartCoroutine(_LockMovementAndAttack(0.8f));
        if(tempHealth > playerHealth)//used to allow the AI to continue the combo
        {
            Debug.Log("Combo2 Landed");
            comboAttack2Landed = true;
        }
        else
        {
            Debug.Log("Combo2 Failed");
            comboAttack2Landed = false;
        }
        yield return null;
    }

    /// <summary>
    /// comboAttack3
    /// </summary>
    /// <returns></returns>
    IEnumerator _Attack3()
    {
        comboAttack1Landed = false;
        comboAttack2Landed = false;
        StopAllCoroutines();
        animator.SetInteger("Attack", 3);
        attack = 3;
        StartCoroutine(_LockMovementAndAttack(1.2f));
        canChain = false;
        yield return null;
    }

    /// <summary>
    /// dash(number) (1: forward, 2: right, 3: back, 4: left)
    /// </summary>
    /// <param name="dashDirection"></param>
    /// <returns></returns>
    public IEnumerator _Dash(int dashDirection)
    {
        isDashing = true;
        hitBox.enabled = false;
        animator.SetInteger("Dash", dashDirection);
        StartCoroutine(_LockMovementAndAttack(0.65f));
        yield return new WaitForSeconds(.1f);
        animator.SetInteger("Dash", 0);
        hitBox.enabled = true;
        isDashing = false;
    }

    /// <summary>
    /// Animation used when the AI is blocking and the attack was blocked
    /// </summary>
    /// <returns></returns>
    public IEnumerator _BlockHitReact()
    {
        StartCoroutine(_LockMovementAndAttack(0.5f));
        animator.SetTrigger("BlockHitReactTrigger");
        yield return null;
    }

    /// <summary>
    /// Animation used when the AIs block was broken through
    /// </summary>
    /// <returns></returns>
    public IEnumerator _BlockBreak()
    {
        StartCoroutine(_LockMovementAndAttack(1f));
        animator.SetTrigger("BlockBreakTrigger");
        yield return null;
    }

    /// <summary>
    /// defines how long it will allow the combo attack to continue
    /// </summary>
    /// <param name="timeToWindow"></param>
    /// <param name="chainLength"></param>
    /// <returns></returns>
    public IEnumerator _ChainWindow(float timeToWindow, float chainLength)
    {
        yield return new WaitForSeconds(timeToWindow);
        canChain = true;
        animator.SetInteger("Attack", 0);
        yield return new WaitForSeconds(chainLength);
        canChain = false;
    }

    /// <summary>
    /// resets a lot of values to allow the animation to work correctly
    /// </summary>
    /// <param name="pauseTime"></param>
    /// <returns></returns>
    public IEnumerator _LockMovementAndAttack(float pauseTime)
    {
        isStunned = true;
        animator.applyRootMotion = true;
        animator.SetFloat("Input X", 0);
        //animator.SetFloat("Input Z", 0);
        animator.SetBool("Moving", false);
        yield return new WaitForSeconds(pauseTime);
        animator.SetInteger("Attack", 0);
        canChain = false;
        isStunned = false;
        animator.applyRootMotion = false;
        //small pause to let blending finish
        yield return new WaitForSeconds(0.2f);
        attack = 0;
    }
    #endregion
}
