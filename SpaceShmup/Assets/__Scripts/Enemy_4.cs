using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyShield))]
public class Enemy_4 : Enemy
{
    [Header("Enemy_4 Inscribed Fields")]
    public float duration = 4;

    private EnemyShield[] allShields;
    private EnemyShield thisShield;
    private Vector3 p0, p1;
    private float timeStart;

    void Start()
    {
        allShields = GetComponentsInChildren<EnemyShield>();
        thisShield = GetComponent<EnemyShield>();
    }

    void InitMovement()
    {
        // Set p0 to the old p1
        p0 = p1;

        // Assign a new on-screen location to p1
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;

        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // Ensure movement goes to a different quadrant
        if (p0.x * p1.x > 0 && p0.y * p1.y > 0)
        {
            if (Mathf.Abs(p0.x) > Mathf.Abs(p0.y))
            {
                p1.x *= -1;
            }
            else
            {
                p1.y *= -1;
            }
        }

        // Reset interpolation timer
        timeStart = Time.time;
    }


    public override void Move()
    {
        float u = (Time.time - timeStart) / duration;

        if (u>=1)
        {
            InitMovement();
            u = 0;
        }
        u = u - 0.15f * Mathf.Sin(u * 2 * Mathf.PI);
        pos = (1 - u) * p0 + u * p1;
    }

    /// <summary>
    /// Collision handling for Enemy_4, enabling shield protection behavior.
    /// </summary>
    void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;

        // Ensure collision was caused by a ProjectileHero
        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();
        if (p != null)
        {

            // Destroy projectile immediately
            Destroy(otherGO);

            // Only apply damage if Enemy_4 is on screen
            if (bndCheck.isOnScreen)
            {

                // Determine which specific child object was hit
                GameObject hitGO = coll.contacts[0].thisCollider.gameObject;
                if (hitGO == otherGO)
                {
                    hitGO = coll.contacts[0].otherCollider.gameObject;
                }

                // Calculate damage from weapon definition
                float dmg = Main.GET_WEAPON_DEFINITION(p.type).damageOnHit;

                // Try applying damage to the shield that was hit
                bool shieldFound = false;
                foreach (EnemyShield es in allShields)
                {
                    if (es.gameObject == hitGO)
                    {
                        es.TakeDamage(dmg);
                        shieldFound = true;
                    }
                }

                // If no shield was found for the hit, damage the core shield
                if (!shieldFound)
                {
                    thisShield.TakeDamage(dmg);
                }

                // If thisShield is still active, the ship is not destroyed yet
                if (thisShield.isActive) return;

                // Notify Main that the ship has been destroyed
                if (!calledShipDestroyed)
                {
                    Main.SHIP_DESTROYED(this);
                    calledShipDestroyed = true;

                    Debug.Log("Dumb Bullshit");
                    Destroy(gameObject);
                }

                // Destroy this Enemy_4 ship
                
            }

        }
        else
        {
            Debug.Log("Enemy_4 hit by non-ProjectileHero: " + otherGO.name);
        }
    }
}
