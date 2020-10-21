using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using System;

[RequireComponent(typeof(Attack))]
public class PlayerController : MonoBehaviour
{
    public static event Action<Vector3, float> ActionCreateNoise = delegate { };
    public static event Action<PlayerController, int, int> ActionAllocateHealthBar = delegate { };
    public static event Action<PlayerController, int, int> ActionUpdateHealthBar = delegate { };
    public static event Action<PlayerController, int> ActionReleaseHealthBar = delegate { };
    public static event Action<Vector3, int, bool> ActionShowInteraction = delegate { };
    public static event Action<GameObject, int, bool, bool> ActionDamagePop = delegate { };

    //Movement Variables
    public float moveSpeed;
    public float crouchSpeed;
    public float attackMoveSpeed;
    public float dodgeRollSpeed;
    public int[] layerInts;
    public float stunTime;
    private Rigidbody rb;
    private bool canMove = true;
    private bool canDodgeRoll = true;

    //Attack Variables
    public GameObject slashPrefab;  //Temporary
    public GameObject attackHitBox; //Temporary
    public float forwardAttackMovement;
    public float comboCooldownTime;
    public int critMultiplier;
    public int critChance;
    private bool canAttack = true;
    //private bool isAttacking = false;
    private int attackNum;
    private Attack attackScript;
    private int[,] damageAmounts;
    private int attackPresses;
    private int lastAttackPresses;
    private Animator animator;
    private bool canKnockout;
    private float timeWhenAttacked;

    //Stealth Variables
    private int currentAlertLevel;

    //Input Variables
    private InputDevice controller;
    private Vector2 lastValue;

    // Interactable variable
    public IInteractable currentInteractable;
    bool isShowingInteractable = false;

    private GameObject knockoutChecker;

    private bool stunned = false;

    private Camera cam;

    public GameObject sparks;

    // Health
    private Health health;
    [HideInInspector]
    public int healthBarReferenceID;

    private void Start()
    {
        health = GetComponent<Health>();
        health.Init();
        healthBarReferenceID = -1;

        lastValue = Vector3.zero;
        rb = GetComponent<Rigidbody>();
        animator = transform.GetChild(2).GetComponent<Animator>();

        attackScript = GetComponent<Attack>();
        attackNum = 0;

        attackPresses = 0;
        lastAttackPresses = 0;

        knockoutChecker = transform.GetChild(0).gameObject;

        currentInteractable = null;

        AlertChanged(0);

        //Set up the damage amounts for each alert mode
        damageAmounts = new int[1, 3];  //4 for the number of alert modes and 3 for the number of attacks in a combo
        //Damage for stealth mode
        damageAmounts[0, 0] = 6;
        damageAmounts[0, 1] = 12;
        damageAmounts[0, 2] = 24;

        cam = Camera.main;

        Physics.IgnoreLayerCollision(10, 17);
    }

    private void OnEnable()
    {
        ControllerInputSystem.ActionGameInput += GetControllerInput;
        KnockoutChecker.ActionEnemyInRange += EnemyWithinRange;
        AlertSystem.ActionAlertChanged += AlertChanged;

        // Interactables
        Healthpack.ActionPlayerClose += UpdateInteractable;
        SideObjective.ActionPlayerClose += UpdateInteractable;
        EndGameTrigger.ActionPlayerWithinRadius += UpdateInteractable;
        MainObjective.ActionPlayerClose += UpdateInteractable;
    }

    private void OnDisable()
    {
        ControllerInputSystem.ActionGameInput -= GetControllerInput;
        KnockoutChecker.ActionEnemyInRange -= EnemyWithinRange;
        AlertSystem.ActionAlertChanged -= AlertChanged;

        // Interactables
        Healthpack.ActionPlayerClose -= UpdateInteractable;
        SideObjective.ActionPlayerClose -= UpdateInteractable;
        EndGameTrigger.ActionPlayerWithinRadius -= UpdateInteractable;
        MainObjective.ActionPlayerClose -= UpdateInteractable;
    }

    public void SetDoorPry(bool was)
    {
        animator.SetBool("DoorPry", was);
        canMove = !was;
    }

    public void SetDoorPrySuccess(bool was)
    {
        animator.SetBool("DoorPrySuccess", was);
    }

    private void Update()
    {
        ExecutePlayerActions();

        CheckDisplayUI();

        CheckAnimStuck();
    }

    private void CheckAnimStuck()
    {
        if(Time.time - timeWhenAttacked > 0.4f)
        {
            ResetCombo();
            canDodgeRoll = true;
        }
    }

    private void CheckDisplayUI()
    {
        // Should show healthbar
        if (health.GetCurrHealth() < health.maxHealth && health.GetCurrHealth() > 0)
        {
            // Already has health bar reference
            if (healthBarReferenceID >= 0)
            {
                ActionUpdateHealthBar(this, health.GetCurrHealth(), healthBarReferenceID);
            }
            // Needs to allocate healthbar reference
            else
            {
                ActionAllocateHealthBar(this, health.GetCurrHealth(), health.maxHealth);
            }
        }
        // Should not show healthbar
        else
        {
            // Already has health bar reference
            if (healthBarReferenceID >= 0)
            {
                ActionReleaseHealthBar(this, healthBarReferenceID);
            }
        }

        // Should show interaction
        if (currentInteractable != null)
        {
            ActionShowInteraction(currentInteractable.GetPosition(), 0, true);
            isShowingInteractable = true;
        }
        else
        {
            if (isShowingInteractable)
            {
                ActionShowInteraction(Vector3.one, 0, false);
                isShowingInteractable = false;
            }
        }
    }

    private void ExecutePlayerActions()
    {
        if (controller != null && !stunned)
        {

            //Check movement
            if (controller.LeftStick && canMove)
            {
                DoMove();
            }
            else
            {
                animator.SetBool("Running", false);
            }

            //Check dodgeroll
            if (controller.RightBumper.WasPressed && canDodgeRoll)
            {
                DoDodgeRoll();
            }

            //Check attack
            if(controller.Action4.WasPressed && canAttack && canKnockout)
            {
                DoKnockout();
            }
            else if ( controller.Action3.WasPressed && canAttack)
            {
                DoAttack();
            } 

            // Check interaction
            if (controller.Action1.WasPressed && currentInteractable != null)
            {
                currentInteractable.InteractWith(this);
            }

            controller = null;
        }
    }

    #region Subscription Methods

    private void EnemyWithinRange(GameObject enemy, bool isWithinRange)
    {
        // Can use enemy here :

        // Set canKnockout
        canKnockout = isWithinRange;
    }

    private void UpdateInteractable(IInteractable interactable, bool isWithinRange)
    {
        if (isWithinRange)
        {
            currentInteractable = interactable;
        }
        else
        {
            if (interactable == currentInteractable)
            {
                currentInteractable = null;
            }
        }
    }

    private void AlertChanged(int newAlertLevel)
    {
        currentAlertLevel = newAlertLevel;

        if (currentAlertLevel > 0)
        {
            //knockoutChecker.SetActive(false);
            canKnockout = false;
        }
        else
        {
            //knockoutChecker.SetActive(true);
            canKnockout = true;
        }
    }

    private void GetControllerInput(InputDevice device)
    {
        controller = device;
    }

    #endregion

    public void CompleteStep()
    {
        SoundRequest.RequestSound("Walk");
    }

    private void DoKnockout()
    {
        animator.SetBool("Knockout", true);
        SoundRequest.RequestSound("Swing_Heavy");
        canMove = false;
        canAttack = false;
        canDodgeRoll = false;
        SpawnSlash();

        StartCoroutine(FinishKnockout());
    }

    private IEnumerator FinishKnockout()
    {
        yield return new WaitForSeconds(0.15f);
        canMove = true;
        canAttack = true;
        canDodgeRoll = true;
        animator.SetBool("Knockout", false);
        knockoutChecker.GetComponent<KnockoutChecker>().Knockout();
    }

    public void SetKnockout(bool was)
    {
        canKnockout = was;
    }

    public void Stun()
    {
        canMove = false;
        canAttack = false;
        canDodgeRoll = false;
        stunned = true;
        animator.SetBool("Running", false);

        cam.GetComponent<ScreenShake>().Shake(0.05f);

        //Show sparks vfx
        GameObject zapped = Instantiate(sparks, transform);
        zapped.transform.position = transform.position;

        StartCoroutine(EndStun(zapped));
    }

    private IEnumerator EndStun(GameObject zapped)
    {
        yield return new WaitForSeconds(stunTime);
        Destroy(zapped);
        canMove = true;
        canAttack = true;
        canDodgeRoll = true;
        stunned = false;
    }

    private void DoMove()
    {
        if (controller.LeftStick.Value != lastValue)
        {
            //if this is true we want to rotate the player
            Vector3 newRotation = new Vector3(0f, controller.LeftStick.Angle, 0f);
            transform.rotation = Quaternion.Euler(newRotation);

            lastValue = controller.LeftStick.Value;
        }

        //if(isAttacking)
        //{
        //    transform.Translate(Vector3.forward * attackMoveSpeed * Time.deltaTime);
        //}
        //else
        {
            animator.SetBool("Running", true);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }
    
    private void DoAttack()
    {
        if (canAttack)
        {
            attackPresses++;
        }

        if (attackPresses == 1)
        {
            NextAttackAnimation(0);
            lastAttackPresses = attackPresses;
        }
    }

    private void SpawnSlash()
    {
        //Temporary to better show attack
        GameObject slash = Instantiate(slashPrefab);
        slash.transform.position = attackHitBox.transform.position;
        slash.transform.rotation = attackHitBox.transform.rotation;
        //--------------------------------------------------------
    }

    private IEnumerator ComboCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(comboCooldownTime);
        canAttack = true;
    }

    private void DoDodgeRoll()
    {
        if (rb.velocity.magnitude < 5)
        {
            canDodgeRoll = false;
            canAttack = false;
            canMove = false;


            foreach (int i in layerInts)
            {
                Physics.IgnoreLayerCollision(i, 10, true);
            }

            animator.SetBool("DodgeRoll", true);
            SoundRequest.RequestSound("Roll");
            rb.AddForce(transform.forward * dodgeRollSpeed);

            StartCoroutine(FinishDodgeRoll());
        }
    }

    private IEnumerator FinishDodgeRoll()
    {
        yield return new WaitForSeconds(0.4f);

        canAttack = true;
        canMove = true;

        animator.SetBool("DodgeRoll", false);
        rb.velocity = Vector3.zero;

        foreach (int i in layerInts)
        {
            Physics.IgnoreLayerCollision(i, 10, false);
        }

        yield return new WaitForSeconds(1f);
        canDodgeRoll = true;
    }

    public void SwingFinish()
    {
        canMove = true;
        canDodgeRoll = true;
        canAttack = true;
        //rb.velocity = Vector3.zero;


        if (attackNum == 0 && attackPresses == lastAttackPresses)
        {
            ResetCombo();
        }
        else if (attackNum == 0 && attackPresses > lastAttackPresses)
        {
            lastAttackPresses = attackPresses;
            NextAttackAnimation(1);
        }
        else if (attackNum == 1 && attackPresses == lastAttackPresses)
        {
            ResetCombo();
        }
        else if (attackNum == 1 && attackPresses > lastAttackPresses)
        {
            NextAttackAnimation(2);
        }
        else if (attackNum == 2)
        {
            ResetCombo();
            StartCoroutine(ComboCooldown());
        }
        else
        {
            ResetCombo();
        }
    }

    private void ResetCombo()
    {
        animator.SetInteger("Animation", -1);
        canAttack = true;
        //isAttacking = false;
        attackPresses = 0;
        lastAttackPresses = 0;
        timeWhenAttacked = 0;
    }

    private void NextAttackAnimation(int animNum)
    {
        rb.AddForce(transform.forward * forwardAttackMovement);
        animator.SetInteger("Animation", animNum);
        attackNum = animNum;

        timeWhenAttacked = Time.time;

        if (attackNum == 2)
        {
            SoundRequest.RequestSound("Swing_Heavy");
        }
        else
        {
            SoundRequest.RequestSound("Swing_Light");
        }

        //Check Crit
        if(UnityEngine.Random.Range(0, critChance) == 0)
        {
            int damage = damageAmounts[0, attackNum] * critMultiplier;

            GameObject enemy = attackScript.DoAttack(damage);

            //MAKE DAMAGE POPOFF
            if (enemy != null)
            {
                ActionDamagePop(enemy, damage, false, true);
            }
        }
        else
        {
            GameObject enemy = attackScript.DoAttack(damageAmounts[0, attackNum]);

            if (enemy != null)
            {
                ActionDamagePop(enemy, damageAmounts[0, attackNum], false, false);
            }
        }

        SpawnSlash();
        canAttack = true;
        //isAttacking = true;
        canDodgeRoll = false;
    }
}
