using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwivelGunAimer : MonoBehaviour
{
    [Header("Inscribed")]
    public float rotateSpeed = 360f;       // Degrees per second
    public float maxTargetRange = 100f;    // How far it can see enemies
    public float maxAngle = 90f;           // Max rotation from forward in degrees

    [Header("Dynamic")]
    [SerializeField] private Transform _currentTarget; // Backing field for the property
    public Transform currentTarget
    {
        get => _currentTarget;
        private set => _currentTarget = value;
    }

    // Baseline local rotation (the “straight ahead” pose)
    private Quaternion baseLocalRot;
    private Vector3 baseLocalEuler;
    private float currentAngle;            // Offset around local X from baseline

    private float baseX;
    private float baseY;
    private float baseZ;

    void Awake()
    {
        // Capture the rotation the model already has in the Inspector
        Vector3 e = transform.localEulerAngles;

        baseX = e.x;  // e.g. -90
        baseY = e.y;  // e.g. -90
        baseZ = e.z;  // e.g. 90
    }

    void Update()
    {
        Transform target = FindClosestEnemy()?.transform;
        if (target == null) return;

        Vector3 toEnemy = target.position - transform.position;
        toEnemy.z = 0f;

        if (toEnemy.sqrMagnitude < 0.0001f) return;

        // Angle the gun needs to swivel around its X-axis
        float angle = Mathf.Atan2(toEnemy.y, toEnemy.x) * Mathf.Rad2Deg;

        // Clamp to swivel arc
        angle = Mathf.Clamp(angle, -90f, 90f); // adjust as needed

        // Smooth movement
        float newX = Mathf.MoveTowards(
            transform.localEulerAngles.x,
            baseX + angle,
            rotateSpeed * Time.deltaTime
        );

        // 🔥 Only X changes — Y and Z stay exactly as the prefab defines them
        transform.localEulerAngles = new Vector3(newX, baseY, baseZ);
    }

    Enemy FindClosestEnemy()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float closestDistSq = maxTargetRange * maxTargetRange;
        Vector3 p = transform.position;

        foreach (Enemy e in enemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;

            Vector3 d = e.transform.position - p;
            d.z = 0f; // top-down: ignore Z
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