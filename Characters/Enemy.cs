using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Enemy : StateMachine
{
    public static event Action ActionSawPlayer = delegate { };
    public static event Action<Enemy, int, int> ActionAllocateHealthBar = delegate { };
    public static event Action<Enemy, int, int> ActionUpdateHealthBar = delegate { };
    public static event Action<Enemy, int> ActionReleaseHealthBar = delegate { };
    public static event Action<Enemy> ActionAllocateStealth = delegate { };
    public static event Action<Enemy, int> ActionUpdateStealth = delegate { };
    public static event Action<Enemy, int> ActionReleaseStealth = delegate { };
    public static event Action<Enemy> ActionAllocateStun = delegate { };
    public static event Action<Enemy, int, float> ActionUpdateStun = delegate { };
    public static event Action<Enemy, int> ActionReleaseStun = delegate { };

    // Alert level variables
    public float distanceToChasePlayerInLockDown;
    private int currentAlertLevel;
    public bool inLockDown;
    public SecureRoom parentRoom;

    //Cone of vision variables
    private readonly float[] sightAngles = { 45, 50, 55, 60, 65 };
    private float currentSightAngle;
    private FieldOfView fov;

    //Pathfinding variables
    public float patrolSpeed;
    public float chaseSpeed;
    private Transform[] path;
    private int numberOfNodesInPath;
    private Transform goal;
    private NavMeshAgent agent;
    private int pathIndex;
    private Vector3 lastPosition;
    private List<Transform> allNodes;

    //State variables
    public float timeToWakeUp;
    public float timeToUnStun;
    private State currState;
    private bool patroling;
    private bool noticedPlayer;
    private bool wasKnockedOut;
    private float timeWhenKnockedOut;
    private float timeWhenStunned;
    private bool stun;

    //Player variables
    private GameObject player;
    private bool playerInRoom;

    //Animator
    private Animator lightAnim;
    private Animator modelAnim;
    public GameObject particle;

    // Health
    private Camera mainCam;
    public Health health;
    public int healthBarReferenceID;

    // Pop Up
    private bool showStealthPopUp;
    public int stealthReferenceID;

    // Stun
    public int stunReferenceID;

    //Attack variables
    public int damage;
    public float timeToAttackAgain;
    private float timeSinceAttack;
    private bool attacked;

    private void OnEnable()
    {
        AlertSystem.ActionAlertChanged += AlertChanged;
    }

    private void OnDisable()
    {
        AlertSystem.ActionAlertChanged -= AlertChanged;
    }

    public void InitializeEnemy(int num, List<Transform> possibleNodes, SecureRoom parent)
    {
        mainCam = Camera.main;
        health = GetComponent<Health>();
        health.Init();
        healthBarReferenceID = -1;
        stealthReferenceID = -1;
        stunReferenceID = -1;

        parentRoom = parent;

        allNodes = possibleNodes;

        lightAnim = GetComponent<Animator>();

        modelAnim = transform.GetChild(1).GetComponent<Animator>();

        fov = GetComponent<FieldOfView>();

        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>().gameObject;

        numberOfNodesInPath = num;
        path = new Transform[numberOfNodesInPath];
        GeneratePath(possibleNodes);

        currState = new PatrolState();
        currState.OnEnter(this);

        AlertChanged(0);

        noticedPlayer = false;

        playerInRoom = false;

        attacked = false;

        inLockDown = false;

        stun = false;
        
        Physics.IgnoreLayerCollision(11, 14);
        Physics.IgnoreLayerCollision(11, 16);

        lastPosition = transform.position;

        particle.SetActive(false);

        showStealthPopUp = false;
    }

    public void ExecuteFrame()
    {
        CheckDisplayUI();

        fov.UpdateView();

        for (int i = 0; i < currState.transitions.Count; ++i)
        {
            if (currState.transitions[i].CheckCondition())
            {
                State temp = currState;
                currState = currState.transitions[i].nextState;
                temp.OnExit();
                currState.OnEnter(this);
            }
        }

        currState.OnUpdate();

    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void CheckDisplayUI()
    {
        Vector3 screenPoint = mainCam.WorldToViewportPoint(transform.position);

        if ((screenPoint.x > 0 && screenPoint.x < 1) && (screenPoint.y > 0 && screenPoint.y < 1) && screenPoint.z > 0)
        {
            ShowHealthBar();

            ShowStun();

            ShowStealthPopUp();
        }
        else
        {
            RemoveHealthBar();

            RemoveStealthPopUp();

            RemoveStun();
        }
    }

    private void ShowStun()
    {
        if (health.GetCurrHealth() <= 0)
        {
            if (stunReferenceID == -1)
            {
                // Allocate
                ActionAllocateStun(this);
            }
            else
            {
                // Update
                float percentLeft = ((Time.time - timeWhenKnockedOut)) / (timeToWakeUp);

                ActionUpdateStun(this, stunReferenceID, percentLeft);
            }
        }
        else
        {
            RemoveStun();
        }
    }

    private void RemoveStun()
    {
        if (stunReferenceID != -1)
        {
            // Release
            ActionReleaseStun(this, stunReferenceID);
        }
    }

    private void ShowStealthPopUp()
    {
        if (showStealthPopUp)
        {
            if (stealthReferenceID == -1)
            {
                ActionAllocateStealth(this);
            }
            else
            {
                ActionUpdateStealth(this, stealthReferenceID);
            }
        }
        else
        {
            RemoveStealthPopUp();
        }
    }

    private void RemoveStealthPopUp()
    {
        if (stealthReferenceID != -1)
        {
            ActionReleaseStealth(this, stealthReferenceID);
        }
    }

    private void ShowHealthBar()
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
    }

    private void RemoveHealthBar()
    {
        // Already has health bar reference
        if (healthBarReferenceID >= 0)
        {
            ActionReleaseHealthBar(this, healthBarReferenceID);
        }
    }

    private void GeneratePath(List<Transform> allNodes)
    {
        List<Transform> nodes = allNodes;
        for (int i = 0; i < numberOfNodesInPath; ++i)
        {
            int num = UnityEngine.Random.Range(0, nodes.Count - 1);
            path[i] = nodes[num];
            nodes.RemoveAt(num);
        }

        pathIndex = 0;
        goal = path[pathIndex];
    }

    public void SetKnockedOut(bool was)
    {
        wasKnockedOut = was;

        if(wasKnockedOut)
        {
            modelAnim.SetBool("Knockedout", true);
        }
    }

    public bool GetKnockedOut()
    {
        return wasKnockedOut;
    }

    private void See()
    {
        if (fov.FindVisibleTargets())
        {
            if (patroling)
            {
                lightAnim.SetBool("SeeThePlayer", true);
            }
        }
        else
        {
            if (patroling)
            {
                lightAnim.SetBool("SeeThePlayer", false);
            }
        }
    }

    private void SawThePlayer()
    {
        noticedPlayer = true;

        parentRoom.RoomAlerted();
        parentRoom.NotifyEnemiesOfPlayer();
    }

    public void NotifyOfPlayer()
    {
        noticedPlayer = true;
    }

    public bool GetNoticedPlayer()
    {
        return noticedPlayer;
    }

    private void AlertChanged(int newAlertLevel)
    {
        currentAlertLevel = newAlertLevel;
        currentSightAngle = sightAngles[currentAlertLevel];
        fov.viewAngle = currentSightAngle;

        if(newAlertLevel == 4)
        {
            inLockDown = true;
        }
    }

    public void SetWasAttacked(bool was)
    {
        noticedPlayer = was;
       // StartCoroutine(NotAttacked());
    }

    IEnumerator NotAttacked()
    {
        yield return new WaitForSeconds(5);
        noticedPlayer = false;
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.2f);
        if (!wasKnockedOut)
        {
            GetComponent<Attack>().DoAttack(damage);
            SoundRequest.RequestSound("Guard_Swing");
            modelAnim.SetBool("Attack", false);
        }
        attacked = true;
    }

    public void SetPlayerInRoom(bool was)
    {
        playerInRoom = was;
    }

    public void Stun()
    {
        stun = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        //if (other.gameObject.CompareTag("Player"))
        //{
        //    GetComponent<Rigidbody>().velocity = Vector3.zero;
        //}
    }

    public void PlayerClose()
    {
        if (!wasKnockedOut)
        {
            showStealthPopUp = true;
        }
        else
        {
            showStealthPopUp = false;
        }
    }

    public void PlayerFar()
    {
        showStealthPopUp = false;
    }

    public void InteractWith(PlayerController interactingPlayer)
    {
        
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    //STATES
    private class PatrolState : State
    {
        public override void OnEnter(Enemy theEnemy)
        {
            enemy = theEnemy;

            transitions = new List<Transition>
            {
                new GotKnockedOutTransition(enemy),
                new LockDownTransition(enemy),
                new StunTransition(enemy),
                new SeePlayerTransition(enemy)
            };

            enemy.goal = enemy.path[enemy.pathIndex];

            enemy.patroling = true;

            enemy.agent.speed = enemy.patrolSpeed;

            enemy.lightAnim.SetBool("SeeThePlayer", false);
        }

        public override void OnUpdate()
        {
            enemy.See();

            if (Vector3.Distance(enemy.transform.position, enemy.path[enemy.pathIndex].position) < 0.5f)
            {
                if (enemy.pathIndex < enemy.path.Length - 1)
                {
                    enemy.pathIndex++;
                }
                else
                {
                    enemy.pathIndex = 0;
                }

                enemy.goal = enemy.path[enemy.pathIndex];
            }

            //CheckValidity();

            enemy.agent.destination = enemy.goal.position;
        }

        private void CheckValidity()
        {
            NavMeshPath path = new NavMeshPath();
            enemy.agent.CalculatePath(enemy.goal.position, path);

            //If the the guard cannot reach where the node currently is
            if (path.status == NavMeshPathStatus.PathPartial)
            {
                //Find a different node to walk to
                enemy.path[enemy.pathIndex] = FindNewNode();

                enemy.goal = enemy.path[enemy.pathIndex];
            }
        }

        private Transform FindNewNode()
        {
            int num = UnityEngine.Random.Range(0, enemy.allNodes.Count);

            return enemy.allNodes[num];
        }

        public override void OnExit()
        {
            enemy.patroling = false;

            transitions.Clear();
        }
    }

    private class ChasePlayerState : State
    {
        public override void OnEnter(Enemy theEnemy)
        {
            enemy = theEnemy;

            transitions = new List<Transition>
            {
                new GotKnockedOutTransition(enemy),
                new StunTransition(enemy),
                new CloseToPlayerTransition(enemy),
                new PlayerOutofRoomTransition(enemy)
            };

            ActionSawPlayer();

            enemy.agent.speed = enemy.chaseSpeed;

            enemy.lightAnim.SetBool("SeeThePlayer", true);
        }

        public override void OnUpdate()
        {
            enemy.See();

            enemy.goal = enemy.player.transform;

            NavMeshPath path = new NavMeshPath();
            enemy.agent.CalculatePath(enemy.goal.position, path);

            //If the the guard cannot reach where the player currently is
            if(path.status == NavMeshPathStatus.PathPartial)
            {
                //Find closest guard walk node to the player and go there instead
                enemy.agent.destination = FindClosestNodeToPlayer();
            }
            else
            {
                enemy.agent.destination = enemy.goal.position;
            }
        }

        private Vector3 FindClosestNodeToPlayer()
        {
            Vector3 closestNode = enemy.allNodes[0].transform.position;

            if (NavMesh.SamplePosition(enemy.player.transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                closestNode = hit.position;
            }

            return closestNode;
        }

        public override void OnExit()
        {
            transitions.Clear();
        }
    }

    private class AttackState : State
    {
        public override void OnEnter(Enemy theEnemy)
        {
            enemy = theEnemy;

            transitions = new List<Transition>
            {
                new GotKnockedOutTransition(enemy),
                new HaveAttackedTransition(enemy)
            };


            enemy.GetComponent<NavMeshAgent>().enabled = false;

            enemy.modelAnim.SetBool("Attack", true);

            enemy.StartCoroutine(enemy.Attack());
        }

        public override void OnUpdate()
        {
            enemy.GetComponent<Rigidbody>().velocity = Vector3.zero;

        }

        public override void OnExit()
        {
            enemy.modelAnim.SetBool("Attack", false);
            enemy.GetComponent<NavMeshAgent>().enabled = true;
            enemy.timeSinceAttack = Time.time;
            enemy.attacked = false;
            transitions.Clear();
        }
    }

    private class KnockedoutState : State
    {
        public override void OnEnter(Enemy theEnemy)
        {
            theEnemy.PlayerFar();
            theEnemy.wasKnockedOut = true;

            enemy = theEnemy;

            transitions = new List<Transition>
            {
                new WakeUpAlertTransition(enemy),
                new WakeUpStealthTransition(enemy),
            };

            enemy.particle.SetActive(true);

            enemy.GetComponent<NavMeshAgent>().enabled = false;

            enemy.health.ZeroOutHealth();

            enemy.fov.DisableFOV();

            enemy.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            enemy.timeWhenKnockedOut = Time.time;

            SoundRequest.RequestSound("Power_Down");

            //Make sure the body doesn't get in the way of guards
            enemy.GetComponent<BoxCollider>().enabled = false;
        }

        public override void OnUpdate()
        {
            enemy.GetComponent<Rigidbody>().velocity = Vector3.zero;

            //Account for animation length
            if (Time.time - enemy.timeWhenKnockedOut > enemy.timeToWakeUp - 1.5)
            {
                enemy.modelAnim.SetBool("Knockedout", false);
            }

            enemy.wasKnockedOut = false;

        }

        public override void OnExit()
        {
            enemy.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            enemy.fov.EnableFOV();

            enemy.GetComponent<NavMeshAgent>().enabled = true;

            enemy.GetComponent<BoxCollider>().enabled = true;

            enemy.health.ResetHealth();
            transitions.Clear();

            enemy.particle.SetActive(false);

        }
    }

    private class StunnedState : State
    {
        public override void OnEnter(Enemy theEnemy)
        {
            enemy = theEnemy;
            transitions = new List<Transition>
            {
                new UnStunTransition(enemy),
            };

            enemy.GetComponent<NavMeshAgent>().enabled = false;

            enemy.timeWhenStunned = Time.time;

            enemy.stun = false;
        }

        public override void OnExit()
        {
            enemy.GetComponent<NavMeshAgent>().enabled = true;
            transitions.Clear();
        }
    }

    private class LockDownState : State
    {
        public override void OnEnter(Enemy theEnemy)
        {
            enemy = theEnemy;

            transitions = new List<Transition>
            {
                new GotKnockedOutTransition(enemy),
                new CloseToPlayerTransition(enemy)
            };

            enemy.lightAnim.SetBool("LockDown", true);

            enemy.modelAnim.SetBool("Knockedout", false);

            enemy.agent.speed = enemy.chaseSpeed;

            enemy.noticedPlayer = true;
        }

        public override void OnUpdate()
        {
            enemy.goal = enemy.player.transform;

            NavMeshPath path = new NavMeshPath();
            enemy.agent.CalculatePath(enemy.goal.position, path);

            //If the the guard cannot reach where the player currently is
            if (path.status == NavMeshPathStatus.PathPartial)
            {
                //Find closest guard walk node to the player and go there instead
                enemy.agent.destination = FindClosestNodeToPlayer();
            }
            else
            {
                enemy.agent.destination = enemy.player.transform.position;
            }
        }

        private Vector3 FindClosestNodeToPlayer()
        {
            Vector3 closestNode = enemy.allNodes[0].transform.position;
            for (int i = 1; i < enemy.allNodes.Count - 1; ++i)
            {
                float dist1 = Vector3.Distance(enemy.player.transform.position, closestNode);
                float dist2 = Vector3.Distance(enemy.player.transform.position, enemy.allNodes[i].transform.position);

                if (dist2 < dist1)
                {
                    closestNode = enemy.allNodes[i].transform.position;
                }
            }

            return closestNode;
        }

        public override void OnExit()
        {
            transitions.Clear();
        }
    }

    //TRANSITIONS
    private class SeePlayerTransition : Transition
    {
        private Enemy enemy;

        public SeePlayerTransition(Enemy theEnemy)
        {
            enemy = theEnemy;
            nextState = new ChasePlayerState();
        }

        public override bool CheckCondition()
        {
            if (enemy.noticedPlayer && enemy.playerInRoom)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }

    private class CloseToPlayerTransition : Transition
    {
        private Enemy enemy;

        public CloseToPlayerTransition(Enemy theEnemy)
        {
            enemy = theEnemy;
            nextState = new AttackState();
        }

        public override bool CheckCondition()
        {
            if (Vector3.Distance(enemy.transform.position, enemy.player.transform.position) < enemy.GetComponent<Attack>().radius &&
                Time.time - enemy.timeSinceAttack > enemy.timeToAttackAgain)
            {
                return true;
            }
            return false;
        }
    }

    private class HaveAttackedTransition : Transition
    {

        private Enemy enemy;

        public HaveAttackedTransition(Enemy e)
        {
            nextState = new ChasePlayerState();
            enemy = e;
        }

        public override bool CheckCondition()
        {
            if (enemy.attacked)
            {
                return true;
            }
            return false;
        }
    }

    private class GotKnockedOutTransition : Transition
    {
        private Enemy enemy;

        public GotKnockedOutTransition(Enemy theEnemy)
        {
            enemy = theEnemy;
            nextState = new KnockedoutState();
        }

        public override bool CheckCondition()
        {
            if (enemy.wasKnockedOut)
            {
                return true;
            }
            return false;
        }
    }

    private class WakeUpAlertTransition : Transition
    {
        private Enemy enemy;

        public WakeUpAlertTransition(Enemy theEnemy)
        {
            enemy = theEnemy;
            nextState = new ChasePlayerState();
        }

        public override bool CheckCondition()
        {
            if (Time.time - enemy.timeWhenKnockedOut > enemy.timeToWakeUp && enemy.parentRoom.hasAlerted)
            {
                return true;
            }
            return false;
        }
    }

    private class WakeUpStealthTransition : Transition
    {
        private Enemy enemy;

        public WakeUpStealthTransition(Enemy theEnemy)
        {
            enemy = theEnemy;
            nextState = new PatrolState();
        }

        public override bool CheckCondition()
        {
            if (Time.time - enemy.timeWhenKnockedOut > enemy.timeToWakeUp && !enemy.parentRoom.hasAlerted)
            {
                return true;
            }
            return false;
        }
    }

    private class LockDownTransition : Transition
    {
        private Enemy enemy;

        public LockDownTransition(Enemy theEnemy)
        {
            enemy = theEnemy;
            nextState = new LockDownState();
        }

        public override bool CheckCondition()
        {
            if (enemy.playerInRoom && enemy.inLockDown)
            {
                return true;
            }
            return false;
        }
    }

    private class UnStunTransition : Transition
    {
        private Enemy enemy;

        public UnStunTransition(Enemy en)
        {
            enemy = en;
            nextState = new ChasePlayerState();
        }

        public override bool CheckCondition()
        {
            if(Time.time - enemy.timeWhenStunned > enemy.timeToUnStun)
            {
                return true;
            }
            return false;
        }
    }

    private class StunTransition : Transition
    {
        private Enemy enemy;

        public StunTransition(Enemy en)
        {
            enemy = en;
            nextState = new StunnedState();
        }

        public override bool CheckCondition()
        {
            if(enemy.stun)
            {
                return true;
            }
            return false;
        }
    }

    private class PlayerOutofRoomTransition : Transition
    {
        private Enemy enemy;

        public PlayerOutofRoomTransition(Enemy en)
        {
            enemy = en;
            nextState = new PatrolState();
        }

        public override bool CheckCondition()
        {
            if(enemy.playerInRoom)
            {
                return false;
            }
            return true;
        }
    }
}
