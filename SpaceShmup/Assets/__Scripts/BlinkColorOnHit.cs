using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BlinkColorOnHit : MonoBehaviour
{
    private static float blinkDuration = 0.1f; // seconds to show damage
    private static Color blinkColor = Color.red;

    [Header("Dynamic")]
    public bool showingColor = false;
    public float blinkCompleteTime; // Time to stop showing the color

    private Material[] materials;   // All materials of this & children
    private Color[] originalColors;
    private BoundsCheck bndCheck;

    void Awake()
    {
        bndCheck = GetComponentInParent<BoundsCheck>();

        // Get materials and their original colors
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
    }

    void Update()
    {
        if (showingColor && Time.time > blinkCompleteTime)
        {
            RevertColors();
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        // Check for collisions with ProjectileHero
        ProjectileHero p = coll.gameObject.GetComponent<ProjectileHero>();
        if (p != null)
        {
            if (bndCheck != null && !bndCheck.isOnScreen)
            {
                return; // Don't show damage if off-screen
            }
            SetColors();
        }
    }

    /// <summary>
    /// Sets the color of all materials to blinkColor and schedules the revert.
    /// </summary>
    void SetColors()
    {
        foreach (Material m in materials)
        {
            m.color = blinkColor;
        }

        showingColor = true;
        blinkCompleteTime = Time.time + blinkDuration;
    }

    /// <summary>
    /// Reverts all materials back to their original colors.
    /// </summary>
    void RevertColors()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
        showingColor = false;
    }
}