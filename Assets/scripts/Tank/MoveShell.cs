using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveShell : MonoBehaviour {

    public float speed = 0;
    public float mass = 1f;
    public float drag = 1f;
    public float force = 2f;
    private float acceleration;
    float ySpeed = 0f;
    float gravity = -9.8f;
    float gravityAcceleration = 0f;

    void Start()
    {
        acceleration = force / mass;
        speed += acceleration;       
        gravityAcceleration = gravity/mass;
    }

    void Update()
    {
        speed *= (1 - Time.deltaTime * drag);
        ySpeed += gravityAcceleration * Time.deltaTime * 0.01f;
        transform.Translate(speed * Time.deltaTime, ySpeed, 0);
    }
}
