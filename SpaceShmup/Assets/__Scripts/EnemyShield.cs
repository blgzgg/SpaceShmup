using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BlinkColorOnHit))]
public class EnemyShield : MonoBehaviour
{

    [Header("Inscribed")]
    public float health = 10;

    private List<EnemyShield> protectors = new List<EnemyShield>();
    private BlinkColorOnHit blinker;

    void Start()
    {
        blinker = GetComponent<BlinkColorOnHit>();
        blinker.ignoreOnCollisionEnter = true;

        if (transform.parent == null) return;

        EnemyShield shieldParent = transform.parent.GetComponent<EnemyShield>();
        if (shieldParent != null)
        {
            shieldParent.AddProtector(this);
        }
    }

    /// <summary>
    /// Called by another EnemyShield to join the protectors of this EnemyShield.
    /// </summary>
    public void AddProtector(EnemyShield shieldChild)
    {
        protectors.Add(shieldChild);
    }

    /// <summary>
    /// Shortcut for checking and setting active state.
    /// </summary>
    public bool isActive
    {
        get { return gameObject.activeInHierarchy; }
        private set { gameObject.SetActive(value); }
    }

    /// <summary>
    /// Called to distribute damage among protector shields and then this shield.
    /// </summary>
    public float TakeDamage(float dmg)
    {

        // Attempt to pass damage to protector shields
        foreach (EnemyShield es in protectors)
        {
            if (es.isActive)
            {
                dmg = es.TakeDamage(dmg);
                if (dmg == 0) return 0; // All damage absorbed
            }
        }

        // If we reach here, this shield must absorb the damage
        blinker.SetColors();

        

        health -= dmg;

        if (health <= 0)
        {
            // Deactivate this shield
            isActive = false;
            // Return any extra damage that exceeded this shield's health
            return -health;
        }

        return 0; // Damage fully absorbed
    }
}
