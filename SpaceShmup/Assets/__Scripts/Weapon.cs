using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eWeaponType
{
    none,
    blaster,
    spread,
    phaser,
    missile,
    laser,
    shield,
    swivel
}

[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.none;

    [Tooltip("Letter to show on the PowerUp Cube")]
    public string letter;

    [Tooltip("Color of PowerUp Cube")]
    public Color powerUpColor = Color.white;

    [Tooltip("Prefab of Weapon model that is attached to the Player Ship")]
    public GameObject weaponModelPrefab;

    [Tooltip("Prefab of projectile that is fired")]
    public GameObject projectilePrefab;

    [Tooltip("Color of the Projectile that is fired")]
    public Color projectileColor = Color.white;

    [Tooltip("Damage caused when a single Projectile hits an Enemy")]
    public float damageOnHit = 0;

    [Tooltip("Damage caused per second by the Laser")]
    public float damagePerSec = 0;

    [Tooltip("Seconds to delay between shots")]
    public float delayBetweenShots = 0;

    [Tooltip("Velocity of individual Projectiles")]
    public float velocity = 50;
}

public class Weapon : MonoBehaviour
{
    public static Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    [Tooltip("Setting this manually while playing does not work properly.")]
    private eWeaponType _type = eWeaponType.none;

    public WeaponDefinition def;
    public float nextShotTime; // Time the Weapon will fire next

    private GameObject weaponModel;
    private Transform shotPointTrans;

    private ProjectileLaser laserInstance;

    void Start()
    {
        // Set up PROJECTILE_ANCHOR if it has not already been done
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);

        // Call SetType() for the default _type set in the Inspector
        SetType(_type);

        // Find the fireEvent of a Hero Component in the parent hierarchy
        Hero hero = GetComponentInParent<Hero>();
        if (hero != null) hero.fireEvent += Fire;
    }

    public eWeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;

        if (type == eWeaponType.none)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }

        // Get the WeaponDefinition for this type from Main
        def = Main.GET_WEAPON_DEFINITION(_type);

        // Destroy any old model and then attach a new one
        if (weaponModel != null) Destroy(weaponModel);

        weaponModel = Instantiate(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0; // Allowed to fire immediately after setting type
    }

    private void Fire()
    {
        // Weapon inactive? Stop.
        if (!gameObject.activeInHierarchy) return;

        // Not enough time passed? Stop.
        if (Time.time < nextShotTime) return;

        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        switch (type)
        {
            case eWeaponType.blaster:
                p = MakeProjectile();
                if (p != null) p.vel = vel;
                break;

            case eWeaponType.spread:
                p = MakeProjectile();
                if (p != null) p.vel = vel;

                p = MakeProjectile();
                if (p != null)
                {
                    p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                    p.vel = p.transform.rotation * vel;
                }

                p = MakeProjectile();
                if (p != null)
                {
                    p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                    p.vel = p.transform.rotation * vel;
                }
                break;

            case eWeaponType.phaser:
                // Left phaser
                p = MakeProjectile();
                if (p != null)
                {
                    ProjectilePhaser phL = p.GetComponent<ProjectilePhaser>();
                    if (phL != null) phL.SetSinDir(-1);
                    p.vel = vel;
                }

                // Right phaser
                p = MakeProjectile();
                if (p != null)
                {
                    ProjectilePhaser phR = p.GetComponent<ProjectilePhaser>();
                    if (phR != null) phR.SetSinDir(1);
                    p.vel = vel;
                }
                break;

            case eWeaponType.laser:
                FireLaser();
                break;

            case eWeaponType.swivel:
                FireSwivel();
                break;

            case eWeaponType.missile:
                FireMissile();
                break;
        }
    }

    private ProjectileHero MakeProjectile()
    {
        GameObject go = Instantiate(def.projectilePrefab, PROJECTILE_ANCHOR);

        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;
        go.transform.position = pos;

        if (p != null)
        {
            p.type = type;
        }

        nextShotTime = Time.time + def.delayBetweenShots;

        return p;
    }

    private void FireLaser()
    {
        // Create the beam once and reuse it
        if (laserInstance == null)
        {
            GameObject laserGO = Instantiate(def.projectilePrefab, PROJECTILE_ANCHOR);
            laserInstance = laserGO.GetComponent<ProjectileLaser>();

            if (laserInstance == null)
            {
                Debug.LogError("Laser projectilePrefab is missing ProjectileLaser component!");
                return;
            }

            laserInstance.Setup(def.projectileColor, def.damagePerSec);
        }

        Vector3 origin = shotPointTrans.position;
        origin.z = 0;

        Vector3 dir = Vector3.up; // laser shoots straight up in world space

        laserInstance.FireFrom(origin, dir);
    }

    // == Swivel weapon (existing behavior, now using helper) ==================

    private void FireSwivel()
    {
        Vector3 origin = shotPointTrans.position;
        origin.z = 0f;

        Enemy closest = FindClosestEnemy(origin);

        // Decide initial direction
        Vector3 dir;
        if (closest != null)
        {
            dir = closest.transform.position - origin;
            dir.z = 0f;
            dir.Normalize();
        }
        else
        {
            // No enemies: just fire straight up
            dir = Vector3.up;
        }

        // Spawn projectile
        ProjectileHero p = MakeProjectile();
        if (p == null) return;

        float speed = def.velocity;

        // Initial straight velocity
        p.vel = dir * speed;

        // Give homing behavior (swivel homing – drops target off-screen in its own script)
        ProjectileHoming homing = p.GetComponent<ProjectileHoming>();
        if (homing != null)
        {
            homing.SetTarget(closest != null ? closest.transform : null, dir, speed);
        }
    }

    // == Missile weapon (new) =================================================

    private void FireMissile()
    {
        Vector3 origin = shotPointTrans.position;
        origin.z = 0f;

        Enemy closest = FindClosestEnemy(origin);

        // Decide initial direction
        Vector3 dir;
        if (closest != null)
        {
            dir = closest.transform.position - origin;
            dir.z = 0f;
            dir.Normalize();
        }
        else
        {
            // No enemies: just fire straight up
            dir = Vector3.up;
        }

        // Spawn projectile
        ProjectileHero p = MakeProjectile();
        if (p == null) return;

        float speed = def.velocity;

        // Initial straight velocity
        p.vel = dir * speed;

        // Give missile homing behavior (no off-screen drop)
        ProjectileMissile missile = p.GetComponent<ProjectileMissile>();
        if (missile != null)
        {
            missile.SetTarget(closest != null ? closest.transform : null, dir, speed);
        }
    }

    // == Shared helper ========================================================

    private Enemy FindClosestEnemy(Vector3 origin)
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float closestDistSq = Mathf.Infinity;

        foreach (Enemy e in enemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;

            Vector3 d = e.transform.position - origin;
            d.z = 0f;
            float distSq = d.sqrMagnitude;

            if (distSq < closestDistSq)
            {
                closest = e;
                closestDistSq = distSq;
            }
        }

        return closest;
    }
}
