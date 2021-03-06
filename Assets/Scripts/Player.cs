using UnityEngine;
using System.Collections;
using System;

public class Player : MonoBehaviour
{
    public float speed = 10f;
    public Ball ball;
    public Score score;
    public ParticleSystem collisionBomb;
    public AudioSource firework;

    // Player Slider
    public Rigidbody2D playerRigidBody;
    public float width { get; set; }
    public Vector2 position { get; set; }

    public Rigidbody2D leftWallRigidBody;
    public Rigidbody2D rightWallRigidBody;
    public Rigidbody2D topWallRigidBody;
    public Rigidbody2D bottomWallRigidBody; 

    private bool hitLeftWall = false;
    private bool hitRightWall = false;

    public bool gameInSession = false;
    private Vector2 origin;

    private float[] input = new float[7]; // input to NN [distance_to_ball_x, distance_to_ball_y, velocity_ball_x, velocity_ball_y]
    public NeuralNetwork network;
    public Manager manager;

    private float maxWidthDistance;
    private float maxHeightDistance;

    private float extraPoints;

    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody2D>();
        width = GetComponent<BoxCollider2D>().size.x;
        position = transform.position;
        origin = transform.position;

        maxWidthDistance = rightWallRigidBody.position.x - leftWallRigidBody.position.x;
        maxHeightDistance = topWallRigidBody.position.y - bottomWallRigidBody.position.y;

    }

    void FixedUpdate()
    {
        if (gameInSession)
        {
            if (score.getScore() >= 1501)
            {
                resetGame();
                network.fitness = -1;
                return;
            }

            input[0] = Math.Abs(transform.position.x - leftWallRigidBody.position.x) / maxWidthDistance;
            input[1] = Math.Abs(ball.transform.position.x - leftWallRigidBody.position.x) / maxWidthDistance;
            input[2] = Math.Abs(ball.transform.position.y - bottomWallRigidBody.position.y) / maxHeightDistance;
            input[3] = (ball.transform.position.x - transform.position.x) / maxWidthDistance;
            input[4] = (ball.transform.position.y - transform.position.y) / maxHeightDistance;
            input[5] = ball.ballRigidBody.velocity.x / ball.maxSpeed;
            input[6] = ball.ballRigidBody.velocity.y / ball.maxSpeed;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] > 1.2 || input[i] < -1.2)
                {
                    resetGame();
                }
            }

            float[] output = network.FeedForward(input);//Call to network to feedforward

            //Debug.Log("Output: " + output[0]);

            if (output[0] >= 0)
            {
                if (hitRightWall)
                    return;
                extraPoints += 0.0001f;
                playerRigidBody.velocity = new Vector2(speed, 0);
            }
            else
            {
                if (hitLeftWall)
                    return;
                extraPoints += 0.0001f;
                playerRigidBody.velocity = new Vector2(speed * -1, 0);
            }
            //else
            //{
            //    stopPlayer();
            //}
        }

        // game start
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    startGame();
        //}
        //else if (Input.GetKey(KeyCode.LeftArrow))
        //{
        //    if (hitLeftWall)
        //        return;
        //    playerRigidBody.velocity = new Vector2(speed * -1, 0);
        //}
        //else if (Input.GetKey(KeyCode.RightArrow))
        //{
        //    if (hitRightWall)
        //        return;
        //    playerRigidBody.velocity = new Vector2(speed, 0);
        //}
        //else
        //    stopPlayer();
        position = transform.position;
    }

    public void startGame()
    {
        if (!gameInSession)
        {
            gameInSession = true;
            ball.startBall();
            score.resetScore();
        }
    }

    public void initGame()
    {
        gameInSession = false;
        ball.ballRigidBody.velocity = Vector2.zero;
        ball.ballRigidBody.position = ball.origin;
        ball.speed = ball.initialSpeed;
        playerRigidBody.position = origin;
        playerRigidBody.velocity = Vector2.zero;
        extraPoints = 0;
    }

    public void resetGame()
    {
        gameInSession = false;
        ball.ballRigidBody.velocity = Vector2.zero;
        ball.ballRigidBody.position = ball.origin;
        ball.speed = ball.initialSpeed;
        playerRigidBody.position = origin;
        playerRigidBody.velocity = Vector2.zero;

        updateFitness();
        StartCoroutine(manager.RunAI());
    }

    private void stopPlayer()
    {
        playerRigidBody.velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;

        if (collision.gameObject.tag == "Ball")
        {
            // increment score
            score.increaseScore();

            // play audio
            playAudio();

            // play animation
            playAnimation();

            float vx = ball.ballRigidBody.velocity.x;
            float vy = ball.ballRigidBody.velocity.y;

            float ballX = ball.transform.position.x;
            float playerLeft = position.x - (width / 2);
            float distanceFromPlayer = ballX - playerLeft;
            double angle = (3 * Math.PI / 4) - (distanceFromPlayer / width) * (Math.PI / 2);
            Vector2 nextVelocity = new Vector2((float)(Math.Cos(angle) * ball.speed), (float)Math.Sin(angle) * ball.speed);
            ball.ballRigidBody.velocity = nextVelocity;
            Debug.Log("velocity.x = " + nextVelocity.x + ", velocity.y = " + nextVelocity.y + ", angle = " + angle * 180 / Math.PI);

            // add extra pts if AI hits at a high angle
            // angle >= 125 degrees or angle <= 55 degrees
            // reward these extreme angles with 3 points
            if (angle >= 2.18166 || angle <=  0.959931) {
                extraPoints += 2;
            }

            // increase speed by 1 for every X points in score
            ball.speed = Math.Min(ball.maxSpeed, ball.initialSpeed + (float)Math.Floor(score.getScore() / ball.speedIncreasesPerPoints));
            // Debug.Log("speed = " + speed);
        }
        else if (collision.gameObject.tag == "LeftWall")
        {
            hitLeftWall = true;
            stopPlayer();
        }
        else if (collision.gameObject.tag == "RightWall")
        {
            hitRightWall = true;
            stopPlayer();
        }
    }

    private void playAnimation()
    {
        var explosion = Instantiate(collisionBomb, transform.position, Quaternion.identity);
        explosion.Play();
        StartCoroutine(destroyPlayAnimation(explosion));
    }

    IEnumerator destroyPlayAnimation(ParticleSystem explosion)
    {
        yield return new WaitForSeconds(1);
        explosion.Stop();
        Destroy(explosion.gameObject);
    }

    private void playAudio()
    {
        firework.Play();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "LeftWall")
        {
            hitLeftWall = false;
        }
        else if (collision.gameObject.tag == "RightWall")
        {
            hitRightWall = false;
        }
    }

    public void updateFitness()
    {
        Vector2 distToBall2D = transform.position - ball.transform.position;
        float distToBall = Mathf.Abs(distToBall2D.x);
        float normalizedDistToBall = Math.Abs(maxWidthDistance - distToBall) / maxWidthDistance;

        network.fitness = score.getScore() + normalizedDistToBall + extraPoints;//updates fitness of network for sorting
        Debug.Log("fitness: " + network.fitness);
        Debug.Log("extra pts: " + extraPoints);
    }
}