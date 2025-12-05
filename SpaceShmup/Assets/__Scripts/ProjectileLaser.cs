using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ProjectileLaser : MonoBehaviour
{
    public float maxLength = 30f;
    public float fadeDelay = 0.05f;

    private LineRenderer line;
    private float lastFiredTime;
    private float damagePerSec;
    private Color beamColor;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = false;
    }

    public void Setup(Color color, float dmgPerSec)
    {
        beamColor = color;
        damagePerSec = dmgPerSec;

        line.startColor = color;
        line.endColor = color;
    }

    public void FireFrom(Vector3 origin, Vector3 dir)
    {
        lastFiredTime = Time.time;
        line.enabled = true;

        Ray ray = new Ray(origin, dir.normalized);
        float beamEndDist = maxLength;

        // Single hit: closest collider along ray
        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                maxLength,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide))
        {
            float dmg = damagePerSec * Time.deltaTime;

            // Try shield first
            EnemyShield shield = hit.collider.GetComponentInParent<EnemyShield>();
            if (shield != null)
            {
                // Blink
                BlinkColorOnHit blinker = shield.GetComponent<BlinkColorOnHit>();
                if (blinker != null) blinker.SetColors();

                shield.TakeDamage(dmg);

                beamEndDist = hit.distance;
            }
            else
            {
                // Otherwise, try enemy
                Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    // Blink
                    BlinkColorOnHit blinker = enemy.GetComponent<BlinkColorOnHit>();
                    if (blinker != null) blinker.SetColors();

                    enemy.TakeDamage(dmg);

                    beamEndDist = hit.distance;
                }
                else
                {
                    // Hit something non-enemy: still stop beam there
                    beamEndDist = hit.distance;
                }
            }
        }

        Vector3 endPos = origin + dir.normalized * beamEndDist;

        line.positionCount = 2;
        line.SetPosition(0, origin);
        line.SetPosition(1, endPos);
    }


    void Update()
    {
        if (line.enabled && Time.time - lastFiredTime > fadeDelay)
        {
            line.enabled = false;
        }
    }
}
