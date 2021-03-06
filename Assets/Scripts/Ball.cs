using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    public float speed = 5f;
    public float initialSpeed = 5f;
    public float speedIncreasesPerPoints = 5;
    public float maxSpeed = 10f;

    public Player player;

    public Rigidbody2D leftWallRigidBody;
    public Rigidbody2D rightWallRigidBody;

    public Rigidbody2D ballRigidBody;
    public Vector2 origin;

    // Start is called before the first frame update
    void Start()
    {
        initialSpeed = speed;
        ballRigidBody = GetComponent<Rigidbody2D>();
        origin = transform.position;
    }

    public void startBall()
    {
        //float initialAngle = UnityEngine.Random.Range((float)(5 * Math.PI / 4), (float)(7 * Math.PI / 4));
        float initialAngle = 285 * (float) Math.PI / 180;
        Vector2 nextVelocity = new Vector2((float)(Math.Cos(initialAngle) * speed), (float)Math.Sin(initialAngle) * speed);
        ballRigidBody.velocity = nextVelocity;
    }

    void FixedUpdate()
    {
        float vx = ballRigidBody.velocity.x;
        float vy = ballRigidBody.velocity.y;

        if (ballRigidBody.transform.position.x >= rightWallRigidBody.position.x)
        {
            ballRigidBody.velocity = new Vector2(-vx, vy);
        }
        else if (ballRigidBody.transform.position.x <= leftWallRigidBody.position.x)
        {
            ballRigidBody.velocity = new Vector2(-vx, vy);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;

        float vx = ballRigidBody.velocity.x;
        float vy = ballRigidBody.velocity.y;

        if (collision.gameObject.tag == "LeftWall")
        {
            ballRigidBody.velocity = new Vector2(-vx, vy);
        }
        else if (collision.gameObject.tag == "RightWall")
        {
            ballRigidBody.velocity = new Vector2(-vx, vy);
        }
        else if (collision.gameObject.tag == "TopWall")
        {
            ballRigidBody.velocity = new Vector2(vx, -vy);
        }
        else if (collision.gameObject.tag == "BottomWall")
        {
            player.resetGame();
        }
    }

}
