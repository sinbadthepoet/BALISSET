using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionDragTest : MonoBehaviour
{
    public float drag = 1;
    public float velocity;

    // Update is called once per frame
    void FixedUpdate()
    {

        Drag();
        Move();

    }

    void Drag()
    {
        float multiplier = 1.0f - drag * Time.fixedDeltaTime;
        if (multiplier < 0.0f) multiplier = 0.0f;
        velocity = multiplier * velocity;
    }

    //x = 0.9;
    //vel = 1.8;

    void Move()
    {
        transform.Translate(transform.up * velocity * Time.fixedDeltaTime);
    }
}
