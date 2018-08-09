using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[System.Serializable]
public class NinjaAI : MonoBehaviour
{
    #region Variables + Start
    public float fightOffset;
    public float fightOffsetInner;
    public float playerHealth;
    public float aiHealth;
    /// <summary>
    /// AVG frames per second when writing this is around 200 meaning that a value of three will happen every 1.66 seconds (3/1000*200 == .6)
    /// higher aggression will make the AI attack more frequently
    /// </summary>
    public float aggression;
    public GameObject weaponModel;
    public GameObject secondaryWeaponModel;
    public GameObject playerSword;
    public BoxCollider bc1, bc2, bc3, bc4, bc5, bc6, bc7, bc8, bc9, bc10;
    public SphereCollider sc1, sc2, sc3;
    public Material ninjaGold;
    public GameObject rig;
    [HideInInspector] public float playerStartingHealth = 100;
    [HideInInspector] public float aiStartingHealth = 100;
    [HideInInspector] public Vector3 swordVelocity;
    [HideInInspector] public bool inAnimation = false;
    [HideInInspector] public bool isBlocking;
    [HideInInspector] public bool iFrame;
    [HideInInspector] public bool firstStage = true;
    [HideInInspector] public Animator animator;
    [HideInInspector] public float currentDamage;
    [HideInInspector] public float comboDamage1;
    [HideInInspector] public float comboDamage2;
    [HideInInspector] public float comboDamage3;
    [HideInInspector] public float attackDamage1;
    [HideInInspector] public float attackDamage2;
    [HideInInspector] public float attackDamage3;
    [HideInInspector] public float rangedDamage1;
    [HideInInspector] public float rangedDamage2;
    [HideInInspector] public float dodgeCost = 5f;
    [HideInInspector] public float cAttack1Cost = 10f;
    [HideInInspector] public float cAttack2Cost = 10f;
    [HideInInspector] public float cAttack3Cost = 10f;
    [HideInInspector] public float attack1Cost = 5f;
    [HideInInspector] public float attack2Cost = 7f;
    [HideInInspector] public float attack3Cost = 50f;
    [HideInInspector] public float rangeAttack1Cost = 10;
    [HideInInspector] public float rangeAttack2Cost = 15;
    [HideInInspector] public float currentEnergy = 100;
    [HideInInspector] public bool comboAttack1Landed;
    [HideInInspector] public bool comboAttack2Landed;
    [HideInInspector] public bool aiRecenlyHit = false;
    [HideInInspector] public bool playerRecentlyHit = false;
    [HideInInspector] public int currentAttack;

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject ninja;
    private _GameManager gameManagerScript;
    private SwordMovement swordMovementScript;
    private Rigidbody rigidBody;
    private Vector3 temp;
    private float lookx;
    private float lookz;
    private float xDistance;
    private float zDistance;
    private float runSpeed;
    private float distance;
    private float distanceToPlayer;
    private bool runMore = true;
    private bool justDodged = false;
    private bool isPaused = false;
    private float totalExtraEnergy = 0;
    private float count = 0;
    private float t1 = 0;
    private float t2 = 0;
    private float t3 = 0;
    private float t4 = 0;
    private float t5 = 0;

    private void Start()
    {
        swordMovementScript = playerSword.GetComponent<SwordMovement>();
        gameManagerScript = rig.GetComponent<_GameManager>();
        animator = this.GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        ResetValues();
        playerHealth = 100;
        aiHealth = 100;
        playerStartingHealth = playerHealth;
        aiStartingHealth = aiHealth;
    }

    private void ResetValues()
    {
        // starting values, they change through the game depending on if the AI hits or misses the player
        attackDamage1 = Random.Range(4, 7); // more likely to use this one since a2 costs more
        attackDamage2 = Random.Range(4, 7); // very likely to use this one
        attackDamage3 = Random.Range(28, 32); // only likely to use this if high energy
        comboDamage1 = Random.Range(4, 7); // less likely to use this one since a1 costs half as much
        comboDamage2 = comboDamage1 * 2;
        comboDamage3 = comboDamage1 * 5;
        rangedDamage1 = Random.Range(9, 11); // more likely to use ranged attack 1
        rangedDamage2 = Random.Range(9, 12); // less likely to use ranged attack 2
    }

    /// <summary>
    /// If the player plays too passively then the AI gets a massive power buff on its damage
    /// </summary>
    private void EnrageAI()
    {
        attackDamage1 *= 4;
        attackDamage2 *= 4;
        attackDamage3 *= 4;
        comboDamage1 *= 4;
        comboDamage2 *= 4;
        comboDamage3 *= 4;
        rangedDamage1 *= 3;
        rangedDamage2 *= 3;
    }
    #endregion

    #region Updates
    private void FixedUpdate()
    {
        rigidBody.AddForce(0, -9.83f, 0, ForceMode.Acceleration);
    }

    /// <summary>
    /// ai brain
    /// </summary>
    private void Update()
    {
        swordVelocity = swordMovementScript.Controller.velocity;//DISABLE WITHOUT CONTROLLERS TO FIX ARRAY OUT OF INDEX----------------------------------------------------
        // The more the  player swings the controllers the more aggressive the AI will be
        float extraEnergy = Mathf.Max(Mathf.Abs(swordVelocity.x), Mathf.Abs(swordVelocity.y), Mathf.Abs(swordVelocity.z)) / 10;
        currentEnergy += Time.deltaTime + extraEnergy;
        totalExtraEnergy += extraEnergy;
        //Debug.Log("extraEnergy: " + totalExtraEnergy);

        if (attackDamage1 < 2 || attackDamage2 < 2 || attackDamage3 < 15 || comboDamage1 < 2 ||
            comboDamage2 < 4 || comboDamage3 < 10 || rangedDamage1 < 5 || rangedDamage2 < 5)
        {
            EnrageAI();
        }
        if (iFrame)
        {
            RemoveHitBoxes();
            ChangeRenderMode("Fade");
        }
        else
        {
            AddHitBoxes();
            ChangeRenderMode("Opaque");
        }
        
        Timer(1);
        Timer(2);
        Timer(3);
        Timer(5);
        //distanceToPlayer = Vector3.Distance(ninja.transform.position, target.transform.position);

        xDistance = Mathf.Abs(transform.position.x - target.transform.position.x);
        zDistance = Mathf.Abs(transform.position.z - target.transform.position.z);
        if (firstStage)
        {
            //In the first stage of the fight the AI will resort to blocking when he is out of energy and the fight will be up close
            if ((xDistance > fightOffset || zDistance > fightOffset || //too far away from the target, so move closer
            xDistance < -fightOffset || zDistance < -fightOffset)
            //distanceToPlayer > fightOffset
            && runMore && !inAnimation && !isBlocking)
            {
                //Debug.Log("xD: " + xDistance + "zD: " + zDistance);
                RunAtPlayer();
            }
            else if ((xDistance <= fightOffset && xDistance >= fightOffsetInner) || (zDistance <= fightOffset && zDistance >= fightOffsetInner) ||
                     (-xDistance >= fightOffset && -xDistance <= fightOffsetInner) || (-zDistance >= fightOffset && -zDistance <= fightOffsetInner)
                     //distanceToPlayer > fightOffsetInner && distanceToPlayer < fightOffset
                     )
            {
                ReachedFightZone();
                Dodge();
                if (!inAnimation)
                {
                    if (comboAttack1Landed || comboAttack2Landed)
                    {
                        playerRecentlyHit = false;
                        Attack();
                    }
                    else
                    {
                        int random = Random.Range(1, 1000);
                        if (random <= aggression && !playerRecentlyHit)
                        {
                            Attack();
                        }
                    }
                }
            }
            else if ((xDistance < fightOffsetInner && zDistance < fightOffsetInner || -xDistance > fightOffsetInner && -zDistance > fightOffsetInner)
                    //distanceToPlayer < fightOffsetInner
                    && !inAnimation && !isBlocking)
            {
                MoveBack();
            }
            else
            {
                //Debug.Log("runMore: " + runMore + " inAnimation: " + inAnimation + " isBlocking: " + isBlocking);
                //Debug.Log("DistanceX: " + xDistance + " DistanceZ: " + zDistance);
                Timer(4);
            }
        }
        else
        {
            //In the second stage the AI will use ranged attacks and focus on the spinning attack that the player can't block
        }
    }

    /// <summary>
    /// Holds several timers for the game
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private void Timer(int number)
    {
        if (justDodged && number == 1)
        {
            t3 += Time.deltaTime;
            if (t3 > 2)
            {
                justDodged = false;
                t3 = 0;
            }
        }

        if (animator.enabled == false && number == 2)
        {
            t1 += Time.deltaTime;
            if (t1 > 1)
            {
                animator.enabled = true;
                t1 = 0;
            }
        }

        if (aiRecenlyHit && number == 3)
        {
            if (aiHealth > 0)
            {
                t2 += Time.deltaTime;
                if (t2 > 1.5)
                {
                    aiRecenlyHit = false;
                    t2 = 0;
                }
            }
            else
            {
                gameManagerScript.Win();
                //Debug.Log("You Win!");
            }
            
        }

        if(number == 4)
        {
            t4 += Time.deltaTime;
            if(t4 > 2.5)
            {
                RunAtPlayer();
                t4 = 0;
            }
        }

        if(playerRecentlyHit && number == 5)
        {
            t5 += Time.deltaTime;
            if(t5 > 1)
            {
                playerRecentlyHit = false;
                t5 = 0;
            }
        }
    }

    #endregion

    #region Render Mode
    // I found this code online. It is used to swap the rendermode in real time
    // I am swapping the rendermode to create a gold glow when the ninja in invulnerable
    // https://sassybot.com/blog/swapping-rendering-mode-in-unity-5-0/
    private void ChangeRenderMode(string blendMode)
    {
        if (blendMode == "Opaque")
        {
            ninjaGold.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            ninjaGold.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            ninjaGold.SetInt("_ZWrite", 1);
            ninjaGold.DisableKeyword("_ALPHATEST_ON");
            ninjaGold.DisableKeyword("_ALPHABLEND_ON");
            ninjaGold.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            ninjaGold.renderQueue = -1;
        }
        else if (blendMode == "Fade")
        {
            ninjaGold.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ninjaGold.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ninjaGold.SetInt("_ZWrite", 0);
            ninjaGold.DisableKeyword("_ALPHATEST_ON");
            ninjaGold.EnableKeyword("_ALPHABLEND_ON");
            ninjaGold.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            ninjaGold.renderQueue = 3000;
        }
        else
        {
            Debug.Log("Blend mode type not in list");
        }
    }
    #endregion

    #region Attack
    /// <summary>
    /// Decides what attack is the most efficient
    /// </summary>
    private void Attack()
    {
        if (!inAnimation)
        {
            //Give higher values to attacks that won't hit the sword
            float a1Value;
            float a2Value;
            float a3Value;
            float c1Value;

            a1Value = (currentEnergy - attack1Cost) * attackDamage1;
            a2Value = (currentEnergy - attack2Cost) * attackDamage2;
            a3Value = (currentEnergy - attack3Cost) * attackDamage3;
            c1Value = (currentEnergy - cAttack1Cost) * comboDamage1;
            

            float[] array = { a1Value, a2Value, a3Value, c1Value };
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
                    //Debug.Log("Attack1");
                    currentDamage = attackDamage1;
                    currentAttack = 1;
                }
                else if (maxValue == a2Value)
                {
                    Attack2();
                    currentEnergy -= attack2Cost;
                    //Debug.Log("Attack2");
                    currentDamage = attackDamage2;
                    currentAttack = 2;
                }
                else if (maxValue == a3Value)
                {
                    Attack3();
                    currentEnergy -= attack3Cost;
                    //Debug.Log("Attack3");
                    currentDamage = attackDamage3;
                    currentAttack = 3;
                }
                else if (maxValue == c1Value)
                {
                    currentEnergy -= cAttack1Cost;
                    //Debug.Log("Combo1");
                    ComboAttack1();
                    currentDamage = comboDamage1;
                    currentAttack = 4;
                }
                else
                {
                    Debug.Log("maxValue does not match any of these options.");
                    Debug.Log("A1 " + a1Value + " A2 " + a2Value + " A3 " + a3Value + " c1 " + c1Value);
                }
            }
            else // A combo attack hit, so figure out which one and if the AI has enough energy then use the next attack in the combo
            {
                if (comboAttack1Landed && !comboAttack2Landed)
                {
                    ComboAttack2();
                    currentEnergy -= cAttack2Cost;
                    //Debug.Log("Combo2");
                    currentDamage = comboDamage2;
                    currentAttack = 5;
                }
                else if (comboAttack2Landed)
                {
                    ComboAttack3();
                    currentEnergy -= cAttack3Cost;
                    //Debug.Log("Combo3");
                    currentDamage = comboDamage3;
                    currentAttack = 6;
                }
            }
        }
        //FacePlayer();
    }

    private void RangedAttack()
    {
        if (!inAnimation)
        {
            float r1Value;
            float r2Value;

            r1Value = (currentEnergy - rangeAttack1Cost) * rangedDamage1;
            r2Value = (currentEnergy - rangeAttack2Cost) * rangedDamage2;

            if (r1Value > r2Value)
            {
                RangedAttack1();
                currentEnergy -= rangeAttack1Cost;
                //Debug.Log("Ranged1");
                currentDamage = rangedDamage1;
                currentAttack = 7;
            }
            else
            {
                RangedAttack2();
                currentEnergy -= rangeAttack2Cost;
                //Debug.Log("Ranged2");
                currentDamage = rangedDamage2;
                currentAttack = 8;
            }
        }
    }
    #endregion

    #region Dodge + Block
    /// <summary>
    /// If the sword is swinging towards the AI it will dodge it in the opposite direction
    /// </summary>
    private void Dodge()
    {
        // make sure that the sword is moving fast enough for a swing
        // to make sure this isn't abused I need to disable hit boxes if it enters a certain range slowly, to prevent it from speednig up suddenly for hits.
        // check distance, no reason to dodge if I am not close enough to hit anything.
        distance = Vector3.Distance(target.transform.position, ninja.transform.position);
        //Debug.Log("Distance: " + distance);
        //After measuring how far away I had to be to hit any of the ninjas hitbox it was ~2.9-3 units away and at a height of 5'11, 3 units should work fine for the average user
        //People with freakishly long arms will be able to abuse game mechanics
        int randomVariable = Random.Range(0, 10);
        
        if (((Mathf.Abs(swordVelocity.x) > 2 || Mathf.Abs(swordVelocity.y) > 2 || Mathf.Abs(swordVelocity.z) > 2) && !iFrame) && !inAnimation && distance < 3)
        {
            justDodged = true;
            //Debug.Log(GetDodgeDirection());
            if ("Right" == GetDodgeDirection())
            {
                StartCoroutine(_Dash(2));
                currentEnergy -= dodgeCost;
                if(randomVariable > 3)
                {
                    runMore = true;
                }
            }
            else if ("Left" == GetDodgeDirection())
            {
                StartCoroutine(_Dash(4));
                currentEnergy -= dodgeCost;
                if(randomVariable > 5)
                {
                    runMore = true;
                }
            }
            else if ("Back" == GetDodgeDirection())
            {
                StartCoroutine(_Dash(3));
                currentEnergy -= dodgeCost;
                if(randomVariable > 8)
                {
                    runMore = true;
                }
            }
            else
            {
                //Debug.Log("Player Swung Back");
            }
        }
    }

    /// <summary>
    /// Returns the direction that the player is swingnig the sword
    /// Possible return values: Unassigned, Left, Front, Right
    /// </summary>
    /// <returns></returns>
    private string GetDodgeDirection()
    {
        //Unassigned / Left / Front / Right
        //The AI will never have to dodge forwards, so a sword swinging backwards will never be a problem
        string dodgeDirection = "Unassigned";

        lookx = transform.forward.x;
        lookz = transform.forward.z;

        if (lookx >= 0 && lookz > 0)// Looking between 0 and 90 degrees
        {
            if (swordVelocity.x <= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Left";
            }
            else if (swordVelocity.x <= 0 && swordVelocity.z <= 0)
            //else if (swordVelocity.x >= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Back";
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
            //else if (swordVelocity.x <= 0 && swordVelocity.z >= 0)
            else if (swordVelocity.x >= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Back";
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
            //else if (swordVelocity.x <= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Back";
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
            //else if (swordVelocity.x >= 0 && swordVelocity.z <= 0)
            else if (swordVelocity.x <= 0 && swordVelocity.z >= 0)
            {
                dodgeDirection = "Back";
            }
            else if (swordVelocity.x <= 0 && swordVelocity.z <= 0)
            {
                dodgeDirection = "Right";
            }
        }
        return dodgeDirection;
    }

    /// <summary>
    /// Starts animation for blocking and sets isBlocking = true
    /// </summary>
    private void Block()
    {
        animator.SetBool("Block", true);
        isBlocking = true;
    }
    #endregion

    #region Movement + Rotation
    /// <summary>
    /// Resets a few values to make the AI follow the player if they teleport
    /// </summary>
    public void PlayerTeleported()
    {
        runMore = true;
        FacePlayer();
        isBlocking = false;
        inAnimation = false;
        animator.SetBool("Block", false);
    }

    /// <summary>
    /// Turns the AI to face the player
    /// </summary>
    private void FacePlayer()
    {
        Vector3 turnPosition = target.transform.position;
        turnPosition.y = 0;
        transform.LookAt(turnPosition, Vector3.up);
    }

    /// <summary>
    /// Makes the AI run towards the player
    /// </summary>
    public void RunAtPlayer()
    {
        iFrame = true;
        animator.SetBool("Moving", true);
        animator.SetBool("Running", true);
        navMeshAgent.SetDestination(target.transform.position);
        if (justDodged) // make the AI run back into fighting position quickly to attack
        {
            runSpeed = .8f;
            //Debug.Log("justDoged was true");
        }
        else
        {
            runSpeed = (xDistance + zDistance) / 10; // distance of ~14 will produce max run speed
        }
        animator.SetFloat("Input Zf", runSpeed); // why the crap was the default animation set type for Input Z an Int???
        navMeshAgent.speed = runSpeed * 8; // 8 seem to match his animation speed decently
    }

    /// <summary>
    /// sets iFrame = false and tells the ai to not run anymore
    /// </summary>
    public void ReachedFightZone()
    {
        iFrame = false;
        animator.SetBool("Moving", false);
        animator.SetBool("Running", false);
        navMeshAgent.SetDestination(transform.position);
        runMore = false;
    }

    /// <summary>
    /// if the AI happens to move too close to the player it will make him roll back once
    /// </summary>
    public void MoveBack()
    {
        iFrame = true;
        FacePlayer();
        StartCoroutine(_Dash(3));
        runMore = true;
    }
    #endregion

    #region Player Interaction
    /// <summary>
    /// If the player blocks the attack I want the animation to cancel instead of continuing the animation.
    /// After you are given 1 second to damage the AI
    /// </summary>
    public void CancelAttack()
    {
        if (inAnimation && currentAttack != 3)
        {
            animator.enabled = false;
            //prevents the attack from hurting the player if it collides after the player blocks the attack
            playerRecentlyHit = true;
            //Debug.Log("BLOCKED!!!!!!!!!!");
        }
        else
        {
            //Debug.Log("Can't block that");
            //Debug.Log("inAnimation: " + inAnimation);
        }
    }

    /// <summary>
    /// Removes all hitboxes from the AI
    /// </summary>
    public void RemoveHitBoxes()
    {
        bc1.enabled = false;
        bc2.enabled = false;
        bc3.enabled = false;
        bc4.enabled = false;
        bc5.enabled = false;
        bc6.enabled = false;
        bc7.enabled = false;
        bc8.enabled = false;
        bc9.enabled = false;
        bc10.enabled = false;
        sc1.enabled = false;
        sc2.enabled = false;
        sc3.enabled = false;
    }

    /// <summary>
    /// Adds the hitboxes back to the AI
    /// </summary>
    public void AddHitBoxes()
    {
        bc1.enabled = true;
        bc2.enabled = true;
        bc3.enabled = true;
        bc4.enabled = true;
        bc5.enabled = true;
        bc6.enabled = true;
        bc7.enabled = true;
        bc8.enabled = true;
        bc9.enabled = true;
        bc10.enabled = true;
        sc1.enabled = true;
        sc2.enabled = true;
        sc3.enabled = true;
    }
    #endregion

    #region Animation Methods
    /// <summary>
    /// Sets up a dash
    /// 1: forward     2: Right     3: Back      4: Left
    /// </summary>
    /// <param name="dashDirection"></param>
    /// <returns></returns>
    public IEnumerator _Dash(int dashDirection)
    {
        iFrame = true;
        RemoveHitBoxes();
        animator.SetInteger("Dash", dashDirection);
        yield return new WaitForEndOfFrame();
        StartCoroutine(UseAttackMove(.65f));
        yield return new WaitForSeconds(.1f);
        animator.SetInteger("Dash", 0);
        AddHitBoxes();
        iFrame = false;
    }

    /// <summary>
    /// Sets up animaitons that don't use root motion
    /// </summary>
    /// <param name="pauseTime"></param>
    /// <returns></returns>
    public IEnumerator UseAttack(float pauseTime)
    {
        if (!inAnimation)
        {
            inAnimation = true;
            animator.applyRootMotion = false;
            yield return new WaitForSeconds(pauseTime + .2f);
            animator.SetInteger("Attack", 0);
            inAnimation = false;
            currentDamage = 0;
        }
    }

    /// <summary>
    /// Sets up moving animations
    /// </summary>
    /// <param name="pauseTime"></param>
    /// <returns></returns>
    public IEnumerator UseAttackMove(float pauseTime)
    {
        if (!inAnimation)
        {
            inAnimation = true;
            animator.applyRootMotion = true;
            yield return new WaitForSeconds(pauseTime + .2f);
            animator.SetInteger("Attack", 0);
            inAnimation = false;
            navMeshAgent.SetDestination(transform.position);
            FacePlayer();
            runMore = true;
            currentDamage = 0;
        }
    }

    /// <summary>
    /// Sets up ranged animations
    /// </summary>
    /// <param name="pauseTime"></param>
    /// <returns></returns>
    public IEnumerator UseAttackRanged(float pauseTime)
    {
        if (!inAnimation)
        {
            inAnimation = true;
            animator.applyRootMotion = false;
            secondaryWeaponModel.SetActive(false);
            yield return new WaitForSeconds(pauseTime);
            inAnimation = false;
            //Debug.Log("2nd weapon active: " + secondaryWeaponModel.activeSelf);
            yield return new WaitForSeconds(.2f);
            secondaryWeaponModel.SetActive(true);
            animator.applyRootMotion = false;
            FacePlayer();
            currentDamage = 0;
        }
    }

    /// <summary>
    /// Right to left
    /// </summary>
    public void Attack1()
    {
        StopAllCoroutines();
        animator.SetTrigger("Attack1RTrigger");
        StartCoroutine(UseAttack(.9f));
    }

    /// <summary>
    /// Bottom to top
    /// </summary>
    public void Attack2()
    {
        StopAllCoroutines();
        animator.SetTrigger("RangeAttack1Trigger");
        StartCoroutine(UseAttack(.9f));
    }

    /// <summary>
    /// Spinning attack (Can't block)
    /// </summary>
    public void Attack3()
    {
        StopAllCoroutines();
        animator.SetTrigger("MoveAttack1Trigger");
        StartCoroutine(UseAttackMove(1.5f));
    }

    /// <summary>
    /// Right to left
    /// </summary>
    public void ComboAttack1()
    {
        float tempHealth = playerHealth;
        StopAllCoroutines();
        animator.SetInteger("Attack", 1);
        StartCoroutine(UseAttack(.6f));
        if(tempHealth > playerHealth)
        {
            //Debug.Log("Combo1 Landed");
            comboAttack1Landed = true;
        }
        else
        {
            //Debug.Log("Combo1 failed");
            comboAttack1Landed = false;
            comboAttack2Landed = false;
        }
    }

    /// <summary>
    /// Top left to bottom right
    /// </summary>
    public void ComboAttack2()
    {
        float tempHealth = playerHealth;
        StopAllCoroutines();
        animator.SetInteger("Attack", 2);
        StartCoroutine(UseAttack(.8f));
        if(tempHealth > playerHealth)
        {
            //Debug.Log("Combo2 Landed");
            comboAttack2Landed = true;
        }
        else
        {
            //Debug.Log("Combo2 failed");
            comboAttack2Landed = false;
            comboAttack1Landed = false;
        }
    }

    /// <summary>
    /// Double swipe from left to right
    /// </summary>
    public void ComboAttack3()
    {
        comboAttack1Landed = false;
        comboAttack2Landed = false;
        StopAllCoroutines();
        animator.SetInteger("Attack", 3);
        StartCoroutine(UseAttack(1.2f));
    }

    /// <summary>
    /// Throws one projectile
    /// </summary>
    public void RangedAttack1()
    {
        StopAllCoroutines();
        animator.SetTrigger("RangeAttack2Trigger");
        StartCoroutine(UseAttack(1f));
    }

    /// <summary>
    /// Throws multiple projectiles
    /// </summary>
    public void RangedAttack2()
    {
        StopAllCoroutines();
        animator.SetTrigger("RangeAttack3Trigger");
        StartCoroutine(UseAttack(1f));
        FacePlayer();
    }

    /// <summary>
    /// Animation for getting hit
    /// </summary>
    public void GetHitReact()
    {
        StartCoroutine(UseAttack(1.4f));
        animator.SetTrigger("LightHitTrigger");
    }

    /// <summary>
    /// Animation for blocking an attack
    /// </summary>
    public void BlockHitReact()
    {
        StartCoroutine(UseAttack(.5f));
        animator.SetTrigger("BlockHitReactTrigger");
    }

    /// <summary>
    /// Animation for running out of blocking energy
    /// </summary>
    public void BlockBreak()
    {
        StartCoroutine(UseAttack(1f));
        animator.SetTrigger("BlockBreakTrigger");
    }
    #endregion
}

