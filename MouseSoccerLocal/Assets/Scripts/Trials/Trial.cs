using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round_Record
{
    public int trial_id;     // What task are we doing?
    public int participant_id;
    public int round_number;
    public int ms_input_lag_of_round;
    public float round_time_taken = 0;
    public int practice_round = 0;     //  0 false, 1 true
    public int noticed_lag = 0;        //  0 false, 1 true

    public virtual new string ToString()
    {
        return trial_id + "," + participant_id + "," + round_number + "," + practice_round + "," + ms_input_lag_of_round + "," + round_time_taken + "," + noticed_lag;
    }
    public virtual string FieldNames()
    {
        return "trial_id,participant_id,round_number,practice_round,input_lag,time_for_round,noticed_lag";
    }
}

// Base class of a trial
// Trial consists of multiple rounds being executed over and over
public class Trial : MonoBehaviour 
{
    public static Trial trial;

    public List<Round_Record> round_results = new List<Round_Record>();

    public int trial_id = 0;    // What task are we doing? Stationary kick into net is 1, moving kick into net is 2, etc
    public int current_round = -1;   // Current round
    public int total_rounds = 0;    // How many rounds are we aiming for?
    public float time_for_current_round;    // In seconds, elapsed time of current trial
    public float total_time_for_trial;  // In seconds, how long this trial has lasted
    public bool trial_running = false;
    public bool round_running = false;
    public bool enforce_time_limit = false;     // Does this trial have a time limit for each round? Ex: each round is 3 seconds
    public float time_limit = 3.0f;     // Time limit for the current round

    public int survey_every_x_rounds = 15;      // When should we bring up the survey menu?
    public List<GameObject> survey_objects_to_activate = new List<GameObject>();
    public int practice_rounds_per_survey = 3;

    public List<int> input_delay_per_round = new List<int>();

    public string text_file_name = "Results.txt";

    public AudioSource timer_beeps;
    public AudioSource start_beep;

    public bool to_menu_after_trial = false;


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
        if (input_delay_per_round.Count > current_round)
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
        if (current_round >= 0)
            FinishRound();

        ResetBetweenRounds();

        current_round++;
        if (current_round < total_rounds)
        {
            // Wait to start so survey has a moment to pause
            StartCoroutine(WaitToStartRound());
        }
        else
        {
            StartCoroutine(WaitToFinishTrial());
        }
    }
    IEnumerator WaitToStartRound()
    {
        round_running = false;
        yield return new WaitForSeconds(0.01f);
        StartRound();
    }
    IEnumerator WaitToFinishTrial()
    {
        yield return new WaitForSeconds(1.0f);

        FinishTrial();
    }
    // Record anything we need to before resetting
    public virtual void FinishRound()
    {
        round_results[current_round].round_time_taken = time_for_current_round;
        round_results[current_round].round_number = current_round + 1;
        round_results[current_round].trial_id = trial_id;

        // Was this a practice round?
        if (practice_rounds_per_survey > 0
            && current_round % survey_every_x_rounds < practice_rounds_per_survey)
            round_results[current_round].practice_round = 1;

        // Should we bring up the survey window?
        if (current_round > 0 
            && survey_every_x_rounds > 0
            && (current_round + 1) % survey_every_x_rounds == 0)
        {
            ActivateSurvey();
        }
    }


    // Bring up the survey window, pausing the game
    public virtual void ActivateSurvey()
    {
        Debug.Log("Activating survey", this.gameObject);
        foreach (GameObject g in survey_objects_to_activate)
        {
            g.SetActive(true);
        }
    }
    public virtual void NoticedLagFromSurvey(bool noticedLag)
    {
        // Add to the last X trial records that this survey applied to
        for (int x = round_results.Count - survey_every_x_rounds; x < round_results.Count; x++)
        {
            round_results[x].noticed_lag = noticedLag ? 1 : 0;
        }
    }


    public virtual void ResetBetweenRounds()
    {
        total_time_for_trial += time_for_current_round;
        time_for_current_round = 0;
    }
    
    
    public virtual void FinishTrial()
    {        
        // Reset everything
        ResetTrial();

        round_running = false;
        trial_running = false;
        Debug.Log("Finished trial");

        if (to_menu_after_trial)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
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

        string text = "";

        // If file doesn't exist, add the header line
        if (!System.IO.File.Exists(path))
            text = round_results[0].FieldNames() + "\n" + text;  // Top line contains column names
        else
            text += "\n";

        text += string.Join(Environment.NewLine, results);
        // Append results onto end of file
        Debug.Log("Saving results to: " + path + ", " + text, this.gameObject);
        System.IO.File.AppendAllText(path, text);
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


    Rect gui_rect = new Rect(Screen.width - 200, Screen.height - 50, 200, 50);
    private void OnGUI()
    {
        if (Time.timeScale <= 0 && !trial_running)
            return;
        string display_string = "";
        display_string += "Round: " + current_round;
        if (enforce_time_limit)
            display_string += "\nTime remaining: " + (time_limit - time_for_current_round);
        GUI.Label(gui_rect, display_string);
    }
}
