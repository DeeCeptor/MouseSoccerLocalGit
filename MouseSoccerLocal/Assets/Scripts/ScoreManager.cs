using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public enum Team {  Blue, Red }

public class ScoreManager : MonoBehaviour 
{
    public static ScoreManager score_manager;
    public Text blue_score_text;
    public Text red_score_text;
    public Text timer_text;

    public int blue_score;
    public int red_score;
    public int player_count = 0;
    public float time_remaining = 60f;
    public int red_players = 0;
    public int blue_players = 0;

    // Dictionary of teams, and number of players on each team
    public Dictionary<string, int> players_on_teams = new Dictionary<string, int>();

    public Sprite blue_sprite;
    public Sprite red_sprite;

    public List<Player> players = new List<Player>();

    public GameObject ball_prefab;

    bool debug_view = true;


    void Awake () 
	{
        score_manager = this;

        players_on_teams.Add("Blue", 0);
        players_on_teams.Add("Red", 0);

        Cursor.visible = false;
	}
    private void Start()
    {
        //Network.Connect("127.0.0.1");
        //Network.Disconnect();
    }


    public GameObject SpawnBall(Vector3 position)
    {
        GameObject go = Instantiate(ball_prefab, position, Quaternion.identity);
        Ball.ball = go.GetComponent<Ball>();
        return go;
    }


    public void EnablePlayerCollisions(bool enable)
    {
        foreach (Player p in players)
        {
            p.GetComponent<Collider2D>().enabled = enable;
        }
    }


    public void AssignTeam(GameObject new_player)
    {
        if (red_players > blue_players)
        {
            new_player.GetComponent<Player>().team = Team.Blue;
            blue_players++;
        }
        else
        {
            new_player.GetComponent<Player>().team = Team.Red;
            red_players++;
        }
    }

    public void SetPlayerColours(Team team, GameObject player)
    {
        switch (team)
        {
            case Team.Red:
                player.GetComponent<SpriteRenderer>().sprite = red_sprite;
                break;
            case Team.Blue:
                player.GetComponent<SpriteRenderer>().sprite = blue_sprite;
                break;
        }
    }


    public void BlueScored(int amount)
    {
        blue_score += amount;
        Debug.Log("Blue scored " + amount);
        SendMessage("GoalScored");
    }
    public void RedScored(int amount)
    {
        red_score += amount;
        Debug.Log("Red scored " + amount);
        SendMessage("GoalScored");
    }


    public void CmdReset()
    {
        ResetScore();
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject go in balls)
        {
            go.transform.position = Vector3.zero;
        }
    }
    public void ResetScore()
    {
        blue_score = 0;
        red_score = 0;
    }
    

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.R))
            CmdReset();
        */
        if (Input.GetKeyDown(KeyCode.M))
        {
            Cursor.visible = !Cursor.visible;
        }

        // Poll the multimouseinputmanager for time between fixed update frames


        blue_score_text.text = "" + blue_score;
        red_score_text.text = "" + red_score;

        // Server updates
        time_remaining -= Time.deltaTime;
        timer_text.text = "" + (int) time_remaining;

        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;


        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    float deltaTime = 0.0f; // Used for non-fixed framerate

    int frame_times_to_average = 1000;
    int cur_frames_counted = 0;
    float total_time = 0;
    float average_ms_per_frame;    // Averaged over frame_times_to_average frames
    private void FixedUpdate()
    {
        if (cur_frames_counted >= frame_times_to_average)
        {
            DisplayFixedFrameRate();
        }

        // Get average of delta times between certain frames
        // Returns the time since the last FixedUpdate
        float ms_since_last_fixed_update = Time.deltaTime * GlobalSettings.ms_per_second;
        total_time += ms_since_last_fixed_update;
        cur_frames_counted++;
    }


    public void DisplayFixedFrameRate()
    {
        average_ms_per_frame = total_time / frame_times_to_average;       // Averaged ms per frame
        float expected_ms_between_fixed_updates = Time.fixedDeltaTime * GlobalSettings.ms_per_second;          // Expected time between fixedupdates
        //Debug.Log("Averaged ms for a frame: " + average_ms_per_frame + ", expected ms between frames: " + expected_ms_between_fixed_updates + " Time:" + Time.time);

        cur_frames_counted = 0;
        total_time = 0;
    }


    void OnGUI()
    {
        if (debug_view)
        {
            float ms_per_frame = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string fpstext = string.Format("ms per frame: {0:0.0} ms, fps: ({1:0.})", ms_per_frame, fps);
            if (GlobalSettings.InputDelayFrames > 0)
                fpstext += "\nIntroduced Input Delay (ms): " + GlobalSettings.InputDelayFrames * Time.fixedDeltaTime * GlobalSettings.ms_per_second;
            GUI.Label(new Rect(Vector2.zero, Vector2.one * 200), "Average ms per fixed update: " + average_ms_per_frame + "\n" + fpstext);
        }
    }
}
