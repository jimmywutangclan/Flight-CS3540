using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // how much damage the player makes in one attack
    public float attackDamage;

    // plays sound on player attack
    public AudioSource attackSound;

    // how close player must be for attack to hit
    private float attackRange;

    // so we can see what's equipped
    private ItemEquipper itemEquipper;

    // Start is called before the first frame update
    void Start()
    {
        attackRange = gameObject.GetComponent<PlayerController>().GetReach();
        itemEquipper = GetComponent<ItemEquipper>();
    }

    // Damages the enemy
    public void Attack(GameObject enemy)
    {
        attackSound.Play();

        if (attackRange >= Vector3.Distance(transform.position, enemy.transform.position))
        {
            EnemyStatistics enemyStats = enemy.GetComponent<EnemyStatistics>();
            float damage = itemEquipper.EquippedItem != null ? itemEquipper.EquippedItem.Properties.Damage.GetValueOrDefault(attackDamage) : attackDamage;
            enemyStats.ChangeEnemyHealth(-damage);
        }

    }
}
