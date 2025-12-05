using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSwivel : MonoBehaviour
{
    public float turnSpeed = 360f;
    // public float speed = 20f;

    private Rigidbody rigid;
    private Transform target;
    private Vector3 lastDir;
    private bool hasTarget = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }



    // I need to make the swivel gun do a few things, we will identify the closest enemy, the swivel gun will automatically rotate to face that enemy. When the swivel gun fires it will shoot a projectile in the direction it is facing.
    // The projectile will remain locked on the enemy that was closest at the time of firing , and will home in on that enemy until it hits or the enemy is destroyed. If the ship is destroyed or flies off screen, the projectile will continue flying in the final direction when the enemy was lost.


    void Update()
    {
        if (!hasTarget)
        {
            // Find the closest enemy
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            float closestDist = Mathf.Infinity;
            GameObject closestEnemy = null;
            foreach (GameObject enemy in enemies)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = enemy;
                }
            }
            if (closestEnemy != null)
            {
                target = closestEnemy.transform;
                hasTarget = true;
            }
        }
        else
        {
            // Rotate towards the target
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
            // If the target is destroyed or off screen, lose the target
            if (target == null)
            {
                hasTarget = false;
                target = null;
            }
        }
    }


}
