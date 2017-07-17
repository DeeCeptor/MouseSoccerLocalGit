﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TeamPongRecord : Round_Record
{
    public int total_bounces = 0;      // How many times did the ball bounce off the paddle? The higher score, the better
    public int player_1_bounces, player_2_bounces;
    public int total_misses;          // Each time the ball slips past is a miss. Want a low score (0 is the best possible score)
    public int player_1_misses, player_2_misses;  // Which player missed the ball? 1 = Top of screen, 0 = Bottom of screen
    public float avg_dist_missed_by;     // Distance from the ball to the paddle when the ball entered 'end zone' (player screwed up)
    public float paddle_width, ball_radius, ball_speed, distance_between_players;
    public float ball_tat;  // Ball turn around time; how much time the player has to move before the ball reaches one end. distance (-radius/2) / ball_speed
    public float total_screen_width;

    public override string ToString()
    {
        return base.ToString() + "," + total_bounces + "," + player_1_bounces + "," + player_2_bounces
            + "," + total_misses + "," + player_1_misses + "," + player_2_misses + "," + avg_dist_missed_by 
            + "," + paddle_width + "," + ball_radius + "," + ball_speed + "," + distance_between_players + "," + ball_tat + "," + total_screen_width;
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",total_bounces,player_1_bounces,player_2_bounces" +
            ",total_misses,player_1_misses,player_2_misses,avg_dist_missed_by"
            + ",paddle_width,ball_radius,ball_speed,distance_between_players,ball_tat,total_screen_width";
    }
}


public class TeamPong : Trial 
{
    public Transform position_to_spawn_ball;
    public List<Transform> positions_to_spawn_player = new List<Transform>();
    public Text timer_text;
    public TeamPongRecord current_round_record;

    public TextAsset pong_settings;
    public List<float> ball_speeds = new List<float>();
    float current_ball_speed_of_round;  // Gotten from text file
    public List<float> distances_between_players = new List<float>();


    public override void StartTrial()
    {
        if (ScoreManager.score_manager.players.Count != 2)
            return;

        PopulatePongSettings();
        base.StartTrial();
    }
    public void PopulatePongSettings()
    {
        if (pong_settings == null)
            return;

        ball_speeds.Clear();
        string[] splits = { "\n" };
        string[] str_vals = pong_settings.text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        foreach (string s in str_vals)
        {
            // Ball speed, distance between players
            string[] items = s.Split(',');
            ball_speeds.Add(float.Parse(items[0]));
            distances_between_players.Add(float.Parse(items[1]));
        }
        Debug.Log("Done loading pong settings", this.gameObject);
    }


    public override void StartRound()
    {
        StopCoroutine(StopThenSpeedUpBall());
        current_ball_speed_of_round = ball_speeds[current_round];
        Ball.ball.max_speed = current_ball_speed_of_round;

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
        ScoreManager.score_manager.players[0].transform.position = new Vector2(0, -distances_between_players[current_round] / 2);
        ScoreManager.score_manager.players[1].transform.position = new Vector2(0, distances_between_players[current_round] / 2);

        // Ensure player has a collider enabled
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        ScoreManager.score_manager.players[1].GetComponent<SingleMouseMovement>().ResetKicks();

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
        Ball.ball.max_speed = current_ball_speed_of_round;
    }
    public void ResetAndShootBall(Vector2 position)
    {
        Ball.ball.Reset(position);
        // Set random x/y angle
        Ball.ball.physics.velocity = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), 0.5f);
    }


    public override void FinishRound()
    {
        // Pong specific calculations
        current_round_record.ball_radius = Ball.ball.GetComponent<CircleCollider2D>().radius * Ball.ball.transform.localScale.x;
        current_round_record.paddle_width = ScoreManager.score_manager.players[0].GetComponent<BoxCollider2D>().size.x * Ball.ball.transform.localScale.x;
        current_round_record.distance_between_players = Mathf.Abs(ScoreManager.score_manager.players[0].transform.position.y) * 2 - ScoreManager.score_manager.players[0].GetComponent<BoxCollider2D>().size.y;
        current_round_record.ball_speed = current_ball_speed_of_round;

        // Distance between players - (radius of ball)
        float total_distance_needed = current_round_record.distance_between_players - (current_round_record.ball_radius);
        current_round_record.ball_tat = total_distance_needed / current_ball_speed_of_round;

        current_round_record.total_screen_width = 2f * Camera.main.orthographicSize;

        base.FinishRound();
    }


    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new TeamPongRecord();
    }


    public override void FinishTrial()
    {
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();

        // Average how much we missed by
        foreach (TeamPongRecord r in round_results)
        {
            if (r.avg_dist_missed_by != 0)
                r.avg_dist_missed_by = r.avg_dist_missed_by / r.total_misses;
        }

        // Record our findings in a text file
        CreateTextFile();

        round_results.Clear();
        trial_running = false;
        round_running = false;

        base.FinishTrial();
    }


    // Pong ball entered end zone, finish round
    public void GoalScored(string goal_name = "none")
    {
        if (!trial_running)
            return;

        base.GoalScored();

        // Figure out which net was just scored on
        if (goal_name.Contains("Top"))  // Player 2
        {
            current_round_record.player_2_misses++;
        }
        else if (goal_name.Contains("Bottom"))  // Player 1
        {
            current_round_record.player_1_misses++;
        }
        // Shouldn't happen, check just in case
        else
        {

        }

        // Calculate how much the ball missed by
        float missed_by = Ball.ball.GetComponent<PongBall>().DistanceFromBall(ScoreManager.score_manager.players[0].transform.position);
        current_round_record.avg_dist_missed_by += missed_by;
        current_round_record.total_misses += 1;
        Debug.Log(goal_name + "Ball missed by " + missed_by + ", total misses: " + current_round_record.total_misses);

        ResetAndShootBall(position_to_spawn_ball.transform.localPosition);
        start_beep.Play();
        StopCoroutine(StopThenSpeedUpBall());
        StartCoroutine(StopThenSpeedUpBall());
    }
    IEnumerator StopThenSpeedUpBall()
    {
        float f = 0.05f;
        while (f < 1)
        {
            Ball.ball.max_speed = current_ball_speed_of_round * f;
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