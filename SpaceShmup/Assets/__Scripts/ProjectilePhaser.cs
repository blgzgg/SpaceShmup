using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePhaser : MonoBehaviour
{
    public float amplitude = 0.5f;      // How wide the wave movement is
    public float frequency = 5f;        // How fast the wave oscillates
    public float sinDir = 1;            // +1 or -1 for right/left wave

    private Rigidbody rigid;
    private Vector3 startPos;
    private float birthTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        startPos = transform.position;   // Now this uses the SHOT POSITION
        birthTime = Time.time;
    }

    public void SetSinDir(float d)
    {
        sinDir = d;
    }

    void Update()
    {
        Vector3 pos = rigid.position;
        float u = Time.time - birthTime;

        // Add sinusoidal offset to the X axis
        pos.x = startPos.x + Mathf.Sin(u * frequency) * amplitude * sinDir;

        rigid.MovePosition(pos);
    }
}

