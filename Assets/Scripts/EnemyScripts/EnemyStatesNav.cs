using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.TerrainAPI;

public class EnemyStatesNav : MonoBehaviour
{
    public GameObject player;
    public enum FSMStates
    {
        Idle, // animation state 0
        Patrol, // animation state 1
        Chase, // anim state 1
        Attack, // anim state 2
        Dead, // anim state 3
        Flee // anim state 1
    }

    // state that enemy is in
    public FSMStates currentState;
    // how fast the enemy moves
    public float patrolSpeed;
    [SerializeField]
    private float chaseSpeed = 0;
    // how quickly the enemy rotates to face player
    // slow rotation speed means the enemy cannot attack a player that can outstrafe them
    public float rotationSpeed;
    // radius of how close the player can get before becoming hostile
    public float detectionRange;
    // how close enemy must be to initiate attack
    public float attackRange;
    // how often the enemy attacks
    public float attackSpeed;
    // damage dealt in one attack
    public float attackDamage;
    // plays sound on enemy attack
    public AudioClip attackSound;
    // toggles whether enemy will detect or attack player
    public bool isPassive = true;
    // toggles whether enemy will flee when player is detected
    public bool willFlee = false;
    // will only detect player at full range when in line of sight
    // if player sneaks up from behind, detection range is halved
    public bool lineOfSight = true;
    public bool hasRunAnim = false;

    // instead of patrolling, stays idle if no patrol points set
    public bool noPatrol = false;

    // Time until player takes damage after attack animation begins
    [SerializeField]
    private float timeToDamage = .7f;
    // How long it takes to attack
    [SerializeField]
    private float attackDuration = .7f;

    // Helps create patrol points
    [SerializeField]
    private int numPatrolPoints = 3;
    [SerializeField]
    private int patrolRadius = 15;
    [SerializeField]
    private float ySpawnMin = 7, ySpawnMax = 30;

    [SerializeField]
    private float patrolDistance = 100;

    public NavMeshAgent nav;

    // animation controller of enemy
    Animator anim;
    // next place the enemy walks to
    Vector3 nextDestination;
    int currentDestinationIndex = 0;
    float distanceToPlayer;
    // next time to attack
    float elapsedTime;
    PlayerStatistics playerStats;
    // script for dropped loot on death
    ItemSource dropScript;


    private float attackTime = 0f;
    private bool hasSwung = false;

    private WorldSettings worldSettings;

    private Vector3[] patrolPoints;

    private SkinnedMeshRenderer meshy;

    // Start is called before the first frame update
    void Start()
    {
        // get player object with not already set
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        nav = GetComponent<NavMeshAgent>();
        playerStats = player.GetComponent<PlayerStatistics>();
        anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
        dropScript = GetComponent<ItemSource>();
        worldSettings = FindObjectOfType<WorldSettings>();
        meshy = GetComponentInChildren<SkinnedMeshRenderer>();

        patrolPoints = new Vector3[numPatrolPoints];

        if (!noPatrol)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                float height = 0;
                // Ensures the height is appropriate so its not on top the mountain or in the water.
                while (!(height >= ySpawnMin && height <= ySpawnMax))
                {
                    // Makes random patrol point near enemy
                    patrolPoints[i] = new Vector3(Random.Range(-patrolRadius, patrolRadius) + transform.position.x, 0,
                        Random.Range(-patrolRadius, patrolRadius) + transform.position.y);

                    // Finds which terrain the point is on.
                    Terrain[] terrains = Terrain.activeTerrains;
                    float minDistance = float.MaxValue;
                    int minIdx = -1;
                    for (int j = 0; j < terrains.Length; j++)
                    {
                        float distance = Vector3.Distance(terrains[j].GetPosition(), patrolPoints[i]);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            minIdx = j;
                        }
                    }

                    // Finds height at point
                    Terrain terrain = Terrain.activeTerrains[minIdx];
                    height = terrain.SampleHeight(patrolPoints[i]) + terrain.transform.position.y;
                    patrolPoints[i].y = height;
                }
            }
        }

        Initialize();
        elapsedTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!worldSettings.IsGameOver())
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer > patrolDistance)
            {
                nav.enabled = false;
                meshy.enabled = false;
            }
            else
            {
                nav.enabled = true;
                meshy.enabled = true;
            }

            switch (currentState)
            {
                case FSMStates.Patrol:
                    UpdatePatrol();
                    break;
                case FSMStates.Idle:
                    UpdateIdle();
                    break;
                case FSMStates.Chase:
                    UpdateChase();
                    break;
                case FSMStates.Attack:
                    UpdateAttack();
                    break;
                case FSMStates.Dead:
                    UpdateDead();
                    break;
                case FSMStates.Flee:
                    UpdateFlee();
                    break;
            }

            elapsedTime += Time.deltaTime;
        }
    }
    private void Initialize()
    {
        if (noPatrol)
        {
            currentState = FSMStates.Idle;
        }
        else
        {
            currentState = FSMStates.Patrol;
            FindNextDestination();
        }
    }

    void FindNextDestination()
    {
        nextDestination = patrolPoints[currentDestinationIndex];

        currentDestinationIndex = (currentDestinationIndex + 1) % patrolPoints.Length;
    }


    void UpdatePatrol()
    {
        anim.SetInteger("ActiveState", hasRunAnim ? 0 : 1);

        FaceTarget(nextDestination);
        if (nav.enabled)
        {
            nav.SetDestination(nextDestination);
            nav.stoppingDistance = 0;
            nav.speed = patrolSpeed;
        }

        // Helps avoid errors where the patrol point has an extreme change in height that is hard to get to.
        // Instead, it only depends on how close it is on the x and z.
        Vector3 tempPos = transform.position;
        tempPos.y = 0;
        Vector3 tempNextDest = nextDestination;
        tempNextDest.y = 0;

        if (Vector3.Distance(tempPos, tempNextDest) < 1)
        {
            FindNextDestination();
        }
        else if (distanceToPlayer < detectionRange)
        {
            if (lineOfSight)
            {
                // calculates if enemy is facing player
                Vector3 facing = player.transform.position - transform.position;

                // gets dot product of unit vectors
                float dot = Vector3.Dot(facing.normalized, transform.forward);
                if (distanceToPlayer < (detectionRange / 2f) || dot >= 0.35f)
                {
                    if (willFlee)
                    {
                        currentState = FSMStates.Flee;
                    }
                    else if (!isPassive)
                    {
                        currentState = FSMStates.Chase;
                    }
                }
            }
            else
            {
                if (willFlee)
                {
                    currentState = FSMStates.Flee;
                }
                else if (!isPassive)
                {
                    currentState = FSMStates.Chase;
                }
            }
        }
    }

    void UpdateIdle()
    {
        anim.SetInteger("ActiveState", 0);
        if (distanceToPlayer <= detectionRange)
        {
            if (lineOfSight)
            {
                // calculates if enemy is facing player
                Vector3 facing = player.transform.position - transform.position;

                // gets dot product of unit vectors
                float dot = Vector3.Dot(facing.normalized, transform.forward);
                if (distanceToPlayer < (detectionRange / 3f) || dot >= 0.35f)
                {
                    if (willFlee)
                    {
                        currentState = FSMStates.Flee;
                    }
                    else if (!isPassive)
                    {
                        currentState = FSMStates.Chase;
                    }
                }
            }
            else if (willFlee)
            {
                currentState = FSMStates.Flee;
            }
            else if (!isPassive)
            {
                currentState = FSMStates.Chase;
            }
        }

        nextDestination = transform.position;
        if (nav.enabled)
            nav.SetDestination(nextDestination);
    }

    void UpdateChase()
    {
        anim.SetInteger("ActiveState", 1);
        FaceTarget(player.transform.position);

        if (nav.enabled)
        {
            nav.SetDestination(player.transform.position);
            nav.stoppingDistance = attackRange;
            nav.speed = chaseSpeed;
        }

        if (distanceToPlayer <= attackRange)
        {
            currentState = FSMStates.Attack;
            anim.SetInteger("ActiveState", 2);
        }
        else if (distanceToPlayer > detectionRange)
        {
            if (noPatrol)
            {
                if (willFlee)
                    Debug.Log("Idle from chase");
                currentState = FSMStates.Idle;
            }
            else
            {
                currentState = FSMStates.Patrol;
            }
        }


    }
    void UpdateAttack()
    {
        FaceTarget(player.transform.position);

        HandleAttack();

        // Checks if player is now out of range.
        if (distanceToPlayer > attackRange &&
            distanceToPlayer <= detectionRange)
        {
            currentState = FSMStates.Chase;
            hasSwung = false;
            attackTime = 0;
            anim.SetInteger("ActiveState", 1);
        }
        else if (distanceToPlayer > detectionRange)
        {
            if (noPatrol)
            {
                currentState = FSMStates.Idle;
                hasSwung = false;
                attackTime = 0;
                anim.SetInteger("ActiveState", 0);
            }
            else
            {
                currentState = FSMStates.Patrol;
                hasSwung = false;
                attackTime = 0;
                anim.SetInteger("ActiveState", hasRunAnim ? 0 : 1);
            }
        }
    }

    void UpdateDead()
    {
        // stops in place, plays death animation
        anim.SetInteger("ActiveState", 3);
        nextDestination = transform.position;

        if (nav.enabled)
        {
            nav.SetDestination(nextDestination);
        }


        // remove normal collider and add new collider that lines up with
        // the enemy's death state
        if (GetComponent<ItemSource>() != null)
        {
            Collider[] colliders = GetComponents<Collider>();
            colliders[0].enabled = false;
            colliders[1].enabled = true;

            // change layer of enemy into interactable to loot
            gameObject.layer = LayerMask.NameToLayer("Interactive");
            gameObject.tag = "Inventory";
            dropScript.enabled = true;
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    // attempt to move away from the player until a good distance away from detection range
    // then, attempt to return to patrol points
    // attempt to move away from the player until a good distance away from detection range
    // then, attempt to return to patrol points
    void UpdateFlee()
    {

        if (distanceToPlayer <= detectionRange + 20.0f)
        {
            currentState = FSMStates.Flee;
        }
        else
        {
            if (noPatrol)
            {
                currentState = FSMStates.Idle;
            }
            else
            {
                currentState = FSMStates.Patrol;
            }
        }
        // uses same animation as chasing
        anim.SetInteger("ActiveState", 1);

        // get opposite direction away from player's relative position and move in that direction
        Vector3 opposite = transform.position - player.transform.position;
        opposite = opposite.normalized;
        nextDestination = (opposite * 50) + transform.position;
        FaceTarget(nextDestination);

        // Finds which terrain the point is on.
        Terrain[] terrains = Terrain.activeTerrains;
        float minDistance = float.MaxValue;
        int minIdx = -1;
        for (int j = 0; j < terrains.Length; j++)
        {
            float distance = Vector3.Distance(terrains[j].GetPosition(), nextDestination);
            if (distance < minDistance)
            {
                minDistance = distance;
                minIdx = j;
            }
        }

        // Finds height at point
        Terrain terrain = Terrain.activeTerrains[minIdx];
        float height = terrain.SampleHeight(nextDestination) + terrain.transform.position.y;
        nextDestination.y = height;

        if (nav.enabled)
            nav.SetDestination(nextDestination);
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }


    // attacks and deals damage to player when within attack range
    // and player is in front of an enemy at a certain angle
    void HandleAttack()
    {
        // Handles successful attack.
        attackTime += Time.deltaTime;
        if (attackTime > timeToDamage && !hasSwung) // When to deal damage based on animation
        {
            // calculates if enemy is facing player
            Vector3 facing = player.transform.position - transform.position;

            // gets dot product of unit vectors
            float dot = Vector3.Dot(facing.normalized, transform.forward);

            // attempts to attack even if won't hit
            // functioning as a sound cue to help time strafing
            AudioSource.PlayClipAtPoint(attackSound, transform.position);

            if (dot >= 0.8f)
            {
                // attack
                playerStats.ChangePlayerHealth(-attackDamage);
            }

            hasSwung = true;
        }

        if (attackTime > attackDuration) // Resets attack abilities when done
        {
            hasSwung = false;
            currentState = FSMStates.Chase;
            attackTime = 0;
            anim.SetInteger("ActiveState", 1);
        }
    }
}
