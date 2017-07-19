using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoloPongRecord : Round_Record
{
    public int paddle_bounces = 0;      // How many times did the ball bounce off the paddle? The higher score, the better
    public int misses;          // Each time the ball slips past is a miss. Want a low score (0 is the best possible score)
    public float avg_missed_by;     // Distance from the ball to the paddle when the ball entered 'end zone' (player screwed up)


    public override string ToString()
    {
        return base.ToString() + "," + paddle_bounces + "," + misses + "," + avg_missed_by;
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",paddle_bounces,misses,avg_missed_by";
    }
}


public class SoloPong : Trial 
{
    public Transform position_to_spawn_ball;
    public Transform position_to_spawn_player;
    public Text timer_text;
    public SoloPongRecord current_round_record;

    float normal_ball_max_speed;

    public override void StartTrial()
    {
        normal_ball_max_speed = Ball.ball.max_speed;
        base.StartTrial();

        //ScoreManager.score_manager.CmdReset();
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().disable_collider_after_kick = true;
    }


    public override void StartRound()
    {
        this.StopAllCoroutines();
        Ball.ball.max_speed = normal_ball_max_speed;

        base.StartRound();
        round_running = false;

        // Spawn ball rolling in right direction
        if (Ball.ball == null)
        {
            // Spawn new ball
            ScoreManager.score_manager.SpawnBall(position_to_spawn_ball.transform.localPosition);
        }
        else
        {
            // Ball position
            Ball.ball.Reset(position_to_spawn_ball.transform.position);
        }

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);
        current_round_record.participant_id = GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];

        // Put player in correct spot
        ScoreManager.score_manager.players[0].transform.position = position_to_spawn_player.transform.position;
        // Ensure player has a collider enabled
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = false;
        Ball.ball.SetCollisions(false);

        StartCoroutine(StartRoundIn());
    }
    IEnumerator StartRoundIn()
    {
        float time_countdown = 3.0f;
        timer_text.gameObject.SetActive(true);
        int prev_time = 0;
        while (time_countdown > 0)
        {
            time_countdown -= Time.deltaTime;

            int new_time = (int)time_countdown + 1;
            timer_text.text = "" + new_time;
            if (new_time != prev_time)
                timer_beeps.Play();
            prev_time = new_time;
            yield return null;
        }
        timer_text.gameObject.SetActive(false);


        // Ball velocity
        ResetAndShootBall(position_to_spawn_ball.transform.localPosition);
        // Allow player movement
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = true;
        Ball.ball.SetCollisions(true);

        start_beep.Play();
        round_running = true;
        Ball.ball.max_speed = normal_ball_max_speed;
    }
    public void ResetAndShootBall(Vector2 position)
    {
        Ball.ball.Reset(position);
        // Set random x/y angle
        Ball.ball.physics.velocity = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), 0.5f);
    }

    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new SoloPongRecord();
    }


    public override void FinishTrial()
    {
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();

        // Average how much we missed by
        foreach (SoloPongRecord r in round_results)
        {
            if (r.avg_missed_by != 0)
                r.avg_missed_by = r.avg_missed_by / r.misses;
        }

        // Record our findings in a text file
        CreateTextFile();

        round_results.Clear();
        trial_running = false;
        round_running = false;

        base.FinishTrial();
    }


    // Pong ball entered end zone, finish round
    public override void GoalScored()
    {
        if (!trial_running)
            return;

        base.GoalScored();

        // Calculate how much the ball missed by
        float missed_by = Ball.ball.GetComponent<PongBall>().DistanceFromBall(ScoreManager.score_manager.players[0].transform.position);
        current_round_record.avg_missed_by += missed_by;
        current_round_record.misses += 1;
        Debug.Log("Ball missed by " + missed_by + ", total misses: " + current_round_record.misses);

        ResetAndShootBall(position_to_spawn_ball.transform.localPosition);
        start_beep.Play();
        StartCoroutine(StopThenSpeedUpBall());
    }
    IEnumerator StopThenSpeedUpBall()
    {
        float f = 0.05f;
        while (f < 1)
        {
            Ball.ball.max_speed = normal_ball_max_speed * f;
            f += Time.deltaTime * 0.5f;
            yield return 1;
        }
    }


    public override void Start () 
	{
        //StartTrial();
	}
	

	public override void Update () 
	{
        base.Update();
	}
}
