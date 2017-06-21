using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RavingBots.MultiInput;

[RequireComponent(typeof(Player))]
public class SingleMouseMovement : MonoBehaviour 
{
    Player player;
    Rigidbody2D physics;

    AudioSource kick_sound;

    // Stores inputs. Used for adding input delay
    Queue<Vector2> input_queue = new Queue<Vector2>();

    // What sensitivty are we currently using? Between min_sensitivity and max_sensitivity, 1 starts at 1
    // Can be altered using mouse wheel
    public float sensitivity = 1f;
    private const float min_sensitivity = 0.2f;
    private const float max_sensitivity = 2.0f;
    private const float sensitivity_increment = 0.1f;
    private const float speed_factor = 0.03f;   // Multiplied by sensitivity to get how far we move this frame

    private const float kick_force_multiplier = 2000f;

    Vector2 cur_input;  // From the current fixedupdate frame

    public bool disable_collider_after_kick = false;
    public int number_of_kicks = 0;

    int sitting_still_for_frames = 0;
    int sitting_Still_needed_to_stop_ball = 20;
    int stop_ball_cooldown = -100;      // After stoping the ball, ensure they can't stop it again for a bit


    void Awake () 
	{
        player = this.GetComponent<Player>();
        physics = this.GetComponent<Rigidbody2D>();
        kick_sound = this.GetComponent<AudioSource>();
    }


    float rotation_target;
    Vector3 position_delta;

    public void Adjust_Sensitivty(float adjustment)
    {
        sensitivity = Mathf.Clamp(sensitivity + (adjustment * sensitivity_increment), min_sensitivity, max_sensitivity);
        Debug.Log("New sensitivity: " + sensitivity);
    }


    void FixedUpdate () 
	{
        ///////////////////////////////////////////////////////////
        // INPUT
        // Adjust sensitivity
        float mouse_wheel = player.input[InputCode.MouseWheel].Value;
        if (mouse_wheel != 0)
        {
            Adjust_Sensitivty(mouse_wheel);
        }

        // Get current device input
        cur_input = new Vector2(player.input[InputCode.MouseX].Value, player.input[InputCode.MouseY].Value);

        // Place input in our queue
        input_queue.Enqueue(cur_input);
        if (input_queue.Count > GlobalSettings.InputDelayFrames)
        {
            cur_input = input_queue.Dequeue();
        }
        else
            cur_input = Vector2.zero;

        // Check if we've been still for a bit
        if (cur_input == Vector2.zero)
            sitting_still_for_frames++;
        else
            sitting_still_for_frames = 0;

        ////////////////////////////////////////////////////////


        // Change player potential position based on inputs of device
        position_delta = cur_input * (speed_factor * sensitivity);
        Vector3 potential_position = this.transform.position += position_delta;

        /////////////////////////////////////////////////////
        // ROTATION
        if (position_delta != Vector3.zero)
        {
            Vector2 pos = position_delta.normalized;
            float angleRadians = Mathf.Atan2(pos.y, pos.x);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            if (angleDegrees > 1f || angleDegrees < -1f)
                rotation_target = angleDegrees;
            //physics.MoveRotation(rotation_target);
        }
        physics.MoveRotation(Mathf.LerpAngle(physics.rotation, rotation_target, Time.deltaTime * 10));
        /////////////////////////////////////////////////////

        // Add force to move us where we should be
        //physics.AddForce(potential_position);
        physics.MovePosition(potential_position);

        // Keep the player within view of the screen
        this.transform.position = new Vector2(
            Mathf.Clamp(potential_position.x, CameraRect.arena_rect.xMin, CameraRect.arena_rect.xMax),
            Mathf.Clamp(potential_position.y, CameraRect.arena_rect.yMin, CameraRect.arena_rect.yMax));


    }


    public void EnableCollisions(bool enable)
    {
        this.GetComponent<Collider2D>().enabled = enable;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            KickBall(collision);
        }
    }

    public void KickBall(Collision2D collision)
    {
        number_of_kicks++;

        //Rigidbody2D ball_physics = collision.gameObject.GetComponent<Rigidbody2D>();
        Rigidbody2D ball_physics = Ball.ball.physics;

        // We've been sitting still for a bit, stop the ball where it is (soccer player takes control of the ball)
        if (sitting_still_for_frames >= sitting_Still_needed_to_stop_ball)
        {
            ball_physics.velocity = Vector2.zero;
            sitting_still_for_frames = stop_ball_cooldown;
        }
        // Kick the ball, adding force to it
        else
        {
            Debug.Log("Kick " + cur_input.magnitude);
            ball_physics.AddForce(cur_input * kick_force_multiplier);

            // Change colour of ball trail
            //collision.gameObject.GetComponent<TrailRenderer>().startColor = player.team_colour;
            //collision.gameObject.GetComponent<TrailRenderer>().endColor = player.team_colour;
            if (Ball.ball.line_renderer.startColor != player.team_colour)
            {
                // Change trail colour if it doesn't match the current player
                Ball.ball.line_renderer.startColor = player.team_colour;
                Ball.ball.line_renderer.endColor = player.team_colour;
            }
        }

        // If kick effect was big, make noise and effect
        if (cur_input.magnitude > 0.13f && !kick_sound.isPlaying)
        {
            kick_sound.Play();
        }

        if (disable_collider_after_kick)
            this.GetComponent<Collider2D>().enabled = false;
    }
    public void ResetKicks()
    {
        this.GetComponent<Collider2D>().enabled = true;
        number_of_kicks = 0;
    }


    public void ResetInputQueues()
    {
        input_queue.Clear();
        Debug.Log("Resetting input queue");
    }
}
