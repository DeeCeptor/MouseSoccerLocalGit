using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoloKickStationaryIntoNetRecord : Round_Record
{
    public int ball_position = 0;   // Where did the ball spawn?
    public bool scored = false;


    public SoloKickStationaryIntoNetRecord()
    {

    }


    public override string ToString()
    {
        return base.ToString() + "," + ball_position + "," + scored;
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",ball_position,scored";
    }
}


public class SoloKickStationaryIntoNet : Trial 
{
    public List<Transform> positions_to_spawn_ball = new List<Transform>();
    public Transform position_to_spawn_player;
    public Text timer_text;
    public SoloKickStationaryIntoNetRecord current_round_record;

    public override void StartTrial()
    {
        base.StartTrial();
    }

    int ball_pos_counter = 0;
    public override void StartRound()
    {
        base.StartRound();
        round_running = false;

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);
        current_round_record.ball_position = (int)(ball_pos_counter % positions_to_spawn_ball.Count);
        current_round_record.participant_id = GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];
        Debug.Log("num results " + round_results.Count);

        // Spawn ball rolling in right direction
        if (Ball.ball == null)
        {
            // Spawn new ball
            GameObject go = ScoreManager.score_manager.SpawnBall(positions_to_spawn_ball[0].transform.localPosition);
        }
        else
        {
            // Get position
            // Ball position
            Ball.ball.Reset(positions_to_spawn_ball[(int)(ball_pos_counter % positions_to_spawn_ball.Count)].transform.position);
            ball_pos_counter++;
        }

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

        // Can collide with ball
        Ball.ball.SetCollisions(true);

        start_beep.Play();
        round_running = true;
    }


    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new SoloKickStationaryIntoNetRecord();
    }


    public override void FinishTrial()
    {
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();

        // Record our findings in a text file
        CreateTextFile();

        round_results.Clear();
        trial_running = false;
        round_running = false;

        base.FinishTrial();
    }


    public override void GoalScored()
    {
        if (!trial_running)
            return;

        base.GoalScored();
        current_round_record.scored = true;

        if (trial_running)
            NextRound();
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
