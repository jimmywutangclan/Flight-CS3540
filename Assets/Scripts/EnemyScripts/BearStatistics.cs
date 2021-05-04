using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearStatistics : MonoBehaviour
{
    StatController bearHealth;
    // plays sound on enemy death
    public AudioClip deathSound;
    // Start is called before the first frame update
    void Start()
    {
        // bears start with 50 health and regenerate 1 health per minute
        bearHealth = new StatController(100f, 0f, 1f, 1f * 60f, false);
    }

    // Update is called once per frame
    void Update()
    {

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
        if (bearHealth.GetCurrentValue() == 0f)
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
        bearHealth.UpdateStatByAmount(amount);
        //Debug.Log("Health left of bear: " + bearHealth.GetCurrentValue().ToString());
    }
}
