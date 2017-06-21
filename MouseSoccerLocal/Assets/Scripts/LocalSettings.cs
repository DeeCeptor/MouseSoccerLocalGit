using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class GlobalSettings
{
    // 0 means no input delay. 5 means 5 fixedupdates will pass before the input is applied
    public static int InputDelayFrames
    {
        get { return input_delay; }
        set
        {
            if (value != InputDelayFrames)
            {
                // Reset the input queues of all players because the inputqueue length has changed
                foreach (Player p in ScoreManager.score_manager.players)
                {
                    p.GetComponent<SingleMouseMovement>().ResetInputQueues();
                }
            }
            input_delay = value;
        }
    } 
    private static int input_delay = 0;

    public const int input_delay_increment = 10;
    public const int ms_per_second = 1000;
}

public class LocalSettings : MonoBehaviour 
{
	void Start () 
	{
        //QualitySettings.vSyncCount = 1;    // Limits it to ~60fps
        Debug.Log(QualitySettings.vSyncCount);
        Application.targetFrameRate = 120;
        Debug.Log("Setting frame rate to: " + Application.targetFrameRate + ", and vSync to: " + QualitySettings.vSyncCount);
    }

    void Update()
    {
        //Debug.Log(Application.targetFrameRate + " " + QualitySettings.vSyncCount);
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            GlobalSettings.InputDelayFrames += GlobalSettings.input_delay_increment;
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
            GlobalSettings.InputDelayFrames = Mathf.Max(GlobalSettings.InputDelayFrames - GlobalSettings.input_delay_increment, 0);    // Can't reduce input delay any more
    }
}
