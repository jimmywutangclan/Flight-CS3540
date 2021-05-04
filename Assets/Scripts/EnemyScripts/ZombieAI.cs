using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Die
}

public class ZombieAI : MonoBehaviour
{
    public bool IsDead;
    public EnemyState status;
    int findIndex;
    GameObject[] stopPoints;
    GameObject player;
    NavMeshAgent agent;
    public Vector3 nextPlace;
    float lapsedTime;
    float distanceFromPlayer;
    public GameObject eyes;
    public Animator anim;
    public string stopPointTag = "Spot";

    public float chaseDistance = 15f;
    public float attackDistance = 1.5f;
    public int maxFOV = 70;
    public float attackRate = 1.5f;
    public int damageAmt = 15;

    private GameObject tempGO;

    // Start is called before the first frame update
    void Start()
    {
        IsDead = false;
        status = EnemyState.Patrol;
        findIndex = 0;
        stopPoints = GameObject.FindGameObjectsWithTag(stopPointTag);
        ShuffleStopPoints();
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        lapsedTime = 0;
        anim = GetComponent<Animator>();

        NextPoint();
    }

    // Update is called once per frame
    void Update()
    {
        distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);

        switch (status)
        {
            case EnemyState.Patrol:
                OnPatrol();
                break;
            case EnemyState.Chase:
                OnChase();
                break;
            case EnemyState.Attack:
                OnAttack();
                break;
            case EnemyState.Die:
                OnDie();
                break;
        }

        lapsedTime += Time.deltaTime;
    }

    void OnPatrol()
    {
        if (!IsDead)
        {
            anim.SetInteger("animState", 1);

            agent.stoppingDistance = 0;
            agent.speed = 2f;
            if (Vector3.Distance(transform.position, nextPlace) < 4)
            {
                NextPoint();
            }
            if (distanceFromPlayer <= chaseDistance && PlayerInFov())
            {
                status = EnemyState.Chase;
            }

            FaceTarget(nextPlace);

            agent.SetDestination(nextPlace);
        }
    }

    void OnChase()
    {
        if (!IsDead)
        {
            anim.SetInteger("animState", 2);

            nextPlace = player.transform.position;
            agent.stoppingDistance = attackDistance;
            agent.speed = 6f;

            if (distanceFromPlayer <= attackDistance)
            {
                status = EnemyState.Attack;
            }
            if (distanceFromPlayer > chaseDistance)
            {
                status = EnemyState.Patrol;
                NextPoint();
            }

            FaceTarget(nextPlace);

            agent.SetDestination(nextPlace);
        }
    }

    void OnAttack()
    {
        if (!IsDead)
        {
            anim.SetInteger("animState", 3);

            nextPlace = player.transform.position;
            agent.stoppingDistance = 0;

            if (distanceFromPlayer <= attackDistance)
            {
                status = EnemyState.Attack;
            }
            else if (distanceFromPlayer > attackDistance && distanceFromPlayer <= chaseDistance)
            {
                status = EnemyState.Chase;
            }
            else if (distanceFromPlayer > chaseDistance)
            {
                status = EnemyState.Patrol;
                NextPoint();
            }

            FaceTarget(nextPlace);

            DoAttack();
        }
    }

    void OnDie()
    {
        anim.SetInteger("animState", 4);

    }

    void NextPoint()
    {
        nextPlace = stopPoints[findIndex].transform.position;
        findIndex = (findIndex + 1) % stopPoints.Length;
        agent.SetDestination(nextPlace);
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = target - transform.position;
        directionToTarget.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10 * Time.deltaTime);
    }

    void DoAttack()
    {
        if (lapsedTime >= attackRate)
        {
            int damage = Random.Range(0, damageAmt);
            if (Random.Range(0, 100) < 40)
            {
                damage *= 2;
            }
            player.GetComponent<PlayerStatistics>().ChangePlayerHealth(-damage);
            lapsedTime = 0;
            Debug.Log("Attack dealt for " + damage);
        }
    }

    bool PlayerInFov()
    {
        Vector3 directionToPlayer = player.transform.position - eyes.transform.position;
        if (Vector3.Angle(directionToPlayer, eyes.transform.forward) <= maxFOV)
        {
            RaycastHit hit;
            if (Physics.Raycast(eyes.transform.position, directionToPlayer, out hit, chaseDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    //test
    private void OnDrawGizmos()
    {
        Vector3 frontView = eyes.transform.position + (eyes.transform.forward * chaseDistance);
        Vector3 left = Quaternion.Euler(0, maxFOV * 0.5f, 0) * frontView;
        Vector3 right = Quaternion.Euler(0, -maxFOV * 0.5f, 0) * frontView;

        Debug.DrawLine(eyes.transform.position, frontView, Color.cyan);
        Debug.DrawLine(eyes.transform.position, left, Color.yellow);
        Debug.DrawLine(eyes.transform.position, right, Color.yellow);
    }

    void ShuffleStopPoints()
    {
        for (int i = 0; i < stopPoints.Length; i++)
        {
            int rnd = Random.Range(0, stopPoints.Length);
            tempGO = stopPoints[rnd];
            stopPoints[rnd] = stopPoints[i];
            stopPoints[i] = tempGO;
        }
    }
}
