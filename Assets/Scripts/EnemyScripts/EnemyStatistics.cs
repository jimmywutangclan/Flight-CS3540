using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatistics : MonoBehaviour
{
    [SerializeField]
    private float maxHealth;
    // plays sound on enemy death
    public AudioClip deathSound;

    StatController enemyHealth;

    // Start is called before the first frame update
    void Start()
    {
        // bears start with 50 health and regenerate 1 health per minute
        enemyHealth = new StatController(maxHealth, 0f, 1f, 1f * 60f, false);
    }

    void FixedUpdate()
    {
        if ((GetComponent<ZombieAI>() == null && GetComponent<EnemyStatesNav>().currentState != EnemyStatesNav.FSMStates.Dead) ||
            (GetComponent<ZombieAI>() != null && !GetComponent<ZombieAI>().IsDead))
        {
            // check if enemy is dead
            CheckDeath();
            // update health regen of enemy
            HandleHealthTick();
        }
    }

    void HandleHealthTick()
    {
        // heals bear
        //bearHealth.UpdateStatOnTick(1);
    }

    void CheckDeath()
    {
        // checks bear health
        if (enemyHealth.GetCurrentValue() == 0f)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
            if (GetComponent<ZombieAI>() != null)
            {
                GetComponent<ZombieAI>().status = EnemyState.Die;
                Destroy(gameObject, 2f);
            }
            else
            {
                GetComponent<EnemyStatesNav>().currentState = EnemyStatesNav.FSMStates.Dead;
            }
        }
    }

    // Change health of enemy
    public void ChangeEnemyHealth(float amount)
    {
        enemyHealth.UpdateStatByAmount(amount);
    }
}
