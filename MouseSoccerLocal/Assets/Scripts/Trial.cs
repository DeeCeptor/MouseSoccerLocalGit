using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round_Record
{
    public int number_of_players;
    public int ms_input_lag_of_round;

    public virtual new string ToString()
    {
        return ms_input_lag_of_round + "ms" + ",players:" + number_of_players;
    }
}

// Base class of a trial
// Trial consists of multiple rounds being executed over and over
public class Trial : MonoBehaviour 
{
    public static Trial trial;

    public List<Round_Record> round_results = new List<Round_Record>();

    public int current_round = -1;   // Current round
    public int total_rounds = 0;    // How many rounds are we aiming for?
    public float time_for_current_round;    // In seconds, elapsed time of current trial
    public float total_time_for_trial;  // In seconds, how long this trial has lasted
    public bool trial_running = false;
    public bool round_running = false;
    public bool enforce_time_limit = false;     // Does this trial have a time limit for each round? Ex: each round is 3 seconds
    public float time_limit = 3.0f;

    public List<int> input_delay_per_round = new List<int>();

    public string text_file_name = "Results.txt";

    public AudioSource timer_beeps;
    public AudioSource start_beep;


    public virtual void Awake()
    {
        trial = this;
    }
    public virtual void Start()
    {

    }

    
    public virtual void StartTrial()
    {
        ResetTrial();
        MultiMouseManager.mouse_manager.players_can_join = false;
        trial_running = true;
        NextRound();
    }


    // A trial consists of multiple rounds
    public virtual void StartRound()
    {
        round_running = true;

        // Set the alotted input delay for this round
        if (input_delay_per_round.Count > current_round && input_delay_per_round[current_round] != null)
        {
            GlobalSettings.InputDelayFrames = input_delay_per_round[current_round];
            
            Debug.Log("Setting input delay to: " + input_delay_per_round[current_round], this.gameObject);
        }

        Debug.Log("Starting round " + current_round);
    }
    public virtual void StopRound()
    {
        trial_running = false;
    }


    public virtual void NextRound()
    {
        ResetBetweenRounds();

        current_round++;
        if (current_round < total_rounds)
        {
            StartRound();
        }
        else
        {
            FinishTrial();
        }
    }


    public virtual void ResetBetweenRounds()
    {
        total_time_for_trial += time_for_current_round;
        time_for_current_round = 0;
    }
    
    
    public virtual void FinishTrial()
    {
        // Record some stuff


        // Reset everything
        ResetTrial();

        trial_running = false;
        Debug.Log("Finished trial");
    }
	

    public virtual void ResetTrial()
    {
        total_time_for_trial = 0;
        time_for_current_round = 0;
    }


    public virtual void GoalScored()
    {

    }


    public void CreateTextFile()
    {
        string[] results = new string[round_results.Count];

        for (int x = 0; x < round_results.Count; x++)
        {
            results[x] = round_results[x].ToString();
        }
        string path = Application.dataPath + "/" + text_file_name;    // Application.persistentDataPath
        string text = string.Join(Environment.NewLine, results);
        Debug.Log("Saving results to: " + path + ", " + text, this.gameObject);
        System.IO.File.WriteAllText(path, text);
    }


    public virtual void Update () 
	{
        if (trial_running)
        {
            total_time_for_trial += Time.deltaTime;

            if (round_running)
            {
                time_for_current_round += Time.deltaTime;

                if (enforce_time_limit)
                {
                    if (time_for_current_round >= time_limit)
                        NextRound();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && !trial_running)
                StartTrial();
        }
	}
}
