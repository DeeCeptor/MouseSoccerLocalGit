using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongBall : MonoBehaviour 
{
    Rigidbody2D physics;
    float max_x_angle = 0.75f;

	void Start () 
	{
        physics = this.GetComponent<Rigidbody2D>();


        // Start the ball moving
        physics.velocity = new Vector2(0.5f, 0.5f) * Ball.ball.max_speed;
    }


    void Update () 
	{
		
	}



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            float contact_x = collision.contacts[0].point.x;

            // Figure out where we contacted the paddle
            float difference = Mathf.Abs(Mathf.Abs(collision.gameObject.transform.position.x) - Mathf.Abs(contact_x));
            float sign = collision.gameObject.transform.position.x > contact_x ? -1.0f : 1.0f;
            float signed_difference = sign * difference;
            float signed_normalized_difference = Mathf.Clamp(signed_difference / (collision.gameObject.transform.localScale.x / 2), -1.0f, 1.0f);

            // https://gamedev.stackexchange.com/questions/4253/in-pong-how-do-you-calculate-the-balls-direction-when-it-bounces-off-the-paddl
            // Add slight angle to ball based on where we collided with paddle
            Vector2 new_dir = new Vector2(signed_normalized_difference * max_x_angle, 0.5f).normalized;
            physics.velocity = new_dir * Ball.ball.max_speed;

            Debug.Log(new_dir + ":" + signed_normalized_difference, this.gameObject);

            /*
            BoxCollider2D box = collision.gameObject.GetComponent<BoxCollider2D>();
            Vector2 col_size = box.size;

            // Figure out the X position the ball hit the paddle at
            float x_intersect = (collision.gameObject.transform.position.x + col_size.x / 2) - this.transform.position.x;
            float normalized_x_intersect = x_intersect / (col_size.x / 2);

            float bounceAngle = normalized_x_intersect * 75f;

            Vector2 direction = new Vector2(Ball.ball.max_speed * Mathf.Cos(bounceAngle),
                                              Ball.ball.max_speed * - Mathf.Sin(bounceAngle));
            physics.velocity = direction  * Ball.ball.max_speed;

            Debug.Log(x_intersect + ";" + normalized_x_intersect + " dir:" + direction, this.gameObject);*/
        }

        // Find the normal and set our velocity to it
    }
}
