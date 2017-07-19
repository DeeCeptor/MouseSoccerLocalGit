using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongBall : MonoBehaviour 
{
    Rigidbody2D physics;
    public float max_x_angle = 0.75f;
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            HitPlayer(collision);
        }
    }
    public void HitPlayer(Collider2D collision)
    {
        Debug.Log("Hit player" + collision.gameObject.name);
        if (time_since_last_bounce < bounce_score_cooldown )
            return;

        collision.gameObject.GetComponent<AudioSource>().Play();

        if (Trial.trial.trial_running)
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


        float contact_x = this.transform.position.x;
        // Debug.Log(collision.contacts[0].point.x + " , position " +);
        // Figure out where we contacted the paddle
        float difference = Mathf.Abs(Mathf.Abs(collision.gameObject.transform.position.x) - Mathf.Abs(contact_x));
        float sign = collision.gameObject.transform.position.x > contact_x ? -1.0f : 1.0f;
        float signed_difference = sign * difference;
        float signed_normalized_difference = Mathf.Clamp(signed_difference / (collision.gameObject.transform.localScale.x / 2), -1.0f, 1.0f);
        
        // https://gamedev.stackexchange.com/questions/4253/in-pong-how-do-you-calculate-the-balls-direction-when-it-bounces-off-the-paddl
        // Add slight angle to ball based on where we collided with paddle
        Vector2 new_dir = new Vector2(signed_normalized_difference * max_x_angle, -Mathf.Sign(physics.velocity.y) * 0.5f).normalized;
        
        // Do a raycast see if this intersects left or right wall
        RaycastHit2D r = Physics2D.Raycast(this.transform.position, new_dir, 30, LayerMask.GetMask(new string[] { "Walls" }));

        if (r.transform != null)
            Debug.LogWarning(r.transform.name);

        // If it does, simply inverse X direction
        if (r.transform != null && (r.transform.name.Contains("Left") || r.transform.name.Contains("Right")))
        {
            Debug.LogWarning("Changing dir, hit " + r.transform.name);
            new_dir.x = -new_dir.x;
        }

        physics.velocity = new_dir * Ball.ball.max_speed;
    }


    /*
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
        if (time_since_last_bounce < bounce_score_cooldown)
            return;

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
        Debug.Log(collision.contacts[0].point.x + " , position " + this.transform.position.x);
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
    }
    */


    public float DistanceFromBall(Vector2 position)
    {
        return Mathf.Abs(Ball.ball.transform.position.x - position.x);
    }
}
