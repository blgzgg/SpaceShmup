using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileMissile : MonoBehaviour
{
    [Header("Inscribed")]
    public float turnSpeed = 360f;   // Degrees per second the missile can turn

    [Header("Dynamic")]
    public float speed = 20f;        // Forward speed

    private Rigidbody rigid;
    private Transform target;
    private Vector3 lastDir;         // Last movement direction (normalized)
    private bool hasTarget = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Called by Weapon when the missile is spawned.
    /// </summary>
    /// <param name="t">Enemy transform to lock onto (can be null).</param>
    /// <param name="initialDir">Initial movement direction.</param>
    /// <param name="projSpeed">Missile speed.</param>
    public void SetTarget(Transform t, Vector3 initialDir, float projSpeed)
    {
        target = t;
        hasTarget = (t != null);
        lastDir = initialDir.normalized;
        speed = projSpeed;
        rigid.velocity = lastDir * speed;
    }

    void Update()
    {
        // If we had a target, check if we've completely lost it
        if (hasTarget)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                // Target destroyed or deactivated – stop homing, keep lastDir
                hasTarget = false;
                target = null;
            }
        }

        if (hasTarget && target != null)
        {
            // Compute direction to target in XY plane
            Vector3 toTarget = target.position - transform.position;
            toTarget.z = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                toTarget.Normalize();

                // Smoothly rotate lastDir toward target direction
                Vector3 newDir = Vector3.RotateTowards(
                    lastDir,
                    toTarget,
                    Mathf.Deg2Rad * turnSpeed * Time.deltaTime,
                    0f
                );

                lastDir = newDir.normalized;
            }
        }

        // Move along lastDir whether we currently have a target or not
        rigid.velocity = lastDir * speed;

        // Face the direction of travel (top-down: z forward)
        if (lastDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
