using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WasThereLagSurvey : MonoBehaviour 
{
    public GameObject object_to_activate_next;


    private void OnEnable()
    {
        // Pause the game when this panel is brought up
        Time.timeScale = 0;
    }

    public void Next()
    {
        // Activate the next object if there is one
        if (object_to_activate_next != null)
        {
            object_to_activate_next.SetActive(true);
        }
        // No object to activate, simply resume game
        else
        {
            Time.timeScale = 1.0f;

            // Disable the parent
            this.GetComponentInParent<Canvas>().gameObject.SetActive(false);
        }

        this.gameObject.SetActive(false);
    }



	void Update () 
	{
		// Check for 'Y' or 'N' press
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Trial.trial.NoticedLagFromSurvey(true);
            Next();
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            Trial.trial.NoticedLagFromSurvey(false);
            Next();
        }
    }
}
