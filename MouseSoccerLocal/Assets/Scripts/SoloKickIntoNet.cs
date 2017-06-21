using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoloKickIntoNetRecord : Round_Record
{
    public bool scored = false;


    public SoloKickIntoNetRecord()
    {

    }


    public override string ToString()
    {
        return scored + "," + base.ToString();
    }
}


public class SoloKickIntoNet : Trial 
{
    public Transform position_to_spawn_ball;
    public Transform position_to_spawn_player;
    public Text timer_text;
    //public List<SoloKickIntoNetRecord> round_results = new List<SoloKickIntoNetRecord>();  // Each round is an entry in this list
    //bool successful_this_round = false;
    public SoloKickIntoNetRecord current_round_record;

    public override void StartTrial()
    {
        base.StartTrial();

        //ScoreManager.score_manager.CmdReset();
        //ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().disable_collider_after_kick = true;
    }


    public override void StartRound()
    {
        base.StartRound();
        round_running = false;



        // Spawn ball rolling in right direction
        if (Ball.ball == null)
        {
            // Spawn new ball
            GameObject go = ScoreManager.score_manager.SpawnBall(position_to_spawn_ball.transform.localPosition);
        }
        else
        {
            // Ball position
            Ball.ball.Reset(position_to_spawn_ball.transform.position);
        }

        // Add entry to list for whether we were successful or not
        round_results.Add(current_round_record);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];
        current_round_record.number_of_players = ScoreManager.score_manager.players.Count;

        // Put player in correct spot
        ScoreManager.score_manager.players[0].transform.position = position_to_spawn_player.transform.position;
        // Ensure player has a collider enabled
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = false;

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
        Ball.ball.physics.velocity = new Vector2(0, -10);
        // Allow player movement
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().enabled = true;

        start_beep.Play();
        round_running = true;
    }


    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        current_round_record = new SoloKickIntoNetRecord();
    }


    public override void FinishTrial()
    {
        base.FinishTrial();
        ScoreManager.score_manager.players[0].GetComponent<SingleMouseMovement>().ResetKicks();

        // Record our findings in a text file
        CreateTextFile();

        round_results.Clear();
        trial_running = false;
        round_running = false;
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


    Rect gui_rect = new Rect(Screen.width - 200, Screen.height - 50, 200, 50);
    private void OnGUI()
    {
        if (!trial_running)
            return;
        string display_string = "";
        display_string += "Round: " + current_round;
        if (enforce_time_limit)
            display_string += "\nTime remaining: " + (time_limit - time_for_current_round);
        GUI.Label(gui_rect, display_string);
    }
}
