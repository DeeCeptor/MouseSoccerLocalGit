using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongBall : MonoBehaviour 
{
    Rigidbody2D physics;
    float max_x_angle = 0.75f;
    float time_since_last_bounce;
    float bounce_score_cooldown = 0.1f;

	void Start () 
	{
        physics = this.GetComponent<Rigidbody2D>();

        // Start the ball moving
        physics.velocity = new Vector2(0.5f, 0.5f) * Ball.ball.max_speed;
    }


    void Update () 
	{
        time_since_last_bounce += Time.deltaTime;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            HitPlayer(collision);
        }
    }
    public void HitPlayer(Collision2D collision)
    {
        Debug.Log("Hit player" + collision.gameObject.name);

        if (Trial.trial.trial_running && time_since_last_bounce >= bounce_score_cooldown)
        {
            time_since_last_bounce = 0;

            // Add a score
            if (Trial.trial.trial_running && Trial.trial is SoloPong)
            {
                SoloPong pong = (SoloPong)Trial.trial;
                pong.current_round_record.paddle_bounces += 1;

                ScoreManager.score_manager.blue_score += 1;
            }
            else if (Trial.trial.trial_running && Trial.trial is TeamPong)
            {
                TeamPong pong = (TeamPong)Trial.trial;
                pong.current_round_record.total_bounces += 1;

                ScoreManager.score_manager.blue_score += 1;

                if (collision.gameObject.GetComponent<Player>().player_id == ScoreManager.score_manager.players[0].player_id)
                {
                    pong.current_round_record.player_1_bounces++;
                }
                else
                    pong.current_round_record.player_2_bounces++;
            }
        }



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

        collision.gameObject.GetComponent<AudioSource>().Play();

        //Debug.Log(new_dir + ":" + signed_normalized_difference, this.gameObject);

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


    public float DistanceFromBall(Vector2 position)
    {
        return Mathf.Abs(Ball.ball.transform.position.x - position.x);
    }
}
