using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class PowerUp : MonoBehaviour
{

    [Header("Inscribed")]
    [Tooltip("x holds a min value and y a max value for a Random.Range() call.")]
    public Vector2 rotMinMax = new Vector2(15, 90);

    [Tooltip("x holds a min value and y a max value for a Random.Range() call.")]
    public Vector2 driftMinMax = new Vector2(.25f, 2);

    public float lifeTime = 10;   // PowerUp will exist for # seconds
    public float fadeTime = 4;    // Then it fades over # seconds

    [Header("Dynamic")]
    public eWeaponType type;      // The type of the PowerUp
    public GameObject cube;       // Reference to the PowerCube child
    public TextMesh letter;       // Reference to the TextMesh
    public Vector3 rotPerSecond;  // Euler rotation speed for PowerCube
    public float birthTime;       // Time.time this was instantiated

    private Rigidbody rigid;
    private BoundsCheck bndCheck;
    private Material cubeMat;

    void Awake()
    {
        // Find the Cube reference (there’s only a single child)
        cube = transform.GetChild(0).gameObject;

        // Find the TextMesh and other components
        letter = GetComponent<TextMesh>();
        rigid = GetComponent<Rigidbody>();
        bndCheck = GetComponent<BoundsCheck>();
        cubeMat = cube.GetComponent<Renderer>().material;

        // Set a random velocity
        Vector3 vel = Random.onUnitSphere;
        vel.z = 0;         // Flatten to XY plane
        vel.Normalize();   // Normalize length to 1
        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        rigid.velocity = vel;

        // Reset rotation
        transform.rotation = Quaternion.identity;

        // Randomize cube rotation per second
        rotPerSecond = new Vector3(
            Random.Range(rotMinMax[0], rotMinMax[1]),
            Random.Range(rotMinMax[0], rotMinMax[1]),
            Random.Range(rotMinMax[0], rotMinMax[1])
        );

        birthTime = Time.time;
    }

    void Update()
    {
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);

        // Fade out over time
        float u = (Time.time - (birthTime + lifeTime)) / fadeTime;

        if (u >= 1)
        {
            Destroy(gameObject);
            return;
        }

        if (u > 0)
        {
            // Fade cube
            Color c = cubeMat.color;
            c.a = 1f - u;
            cubeMat.color = c;

            // Fade letter slightly less
            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }

        if (!bndCheck.isOnScreen)
        {
            Destroy(gameObject);
        }
    }

    public eWeaponType typeProp
    {
        get { return type; }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(wt);
        cubeMat.color = def.powerUpColor;
        letter.text = def.letter;
        type = wt;
    }

    /// <summary>
    /// Called when a PowerUp is collected by the Hero.
    /// </summary>
    public void AbsorbedBy(GameObject target)
    {
        Destroy(gameObject);
    }
}
