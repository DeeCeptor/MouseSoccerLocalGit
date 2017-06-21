using RavingBots.MultiInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Team team;
    public IDevice input;


	void Awake ()
    {
        if (ScoreManager.score_manager != null)
            ScoreManager.score_manager.players.Add(this);
    }
    private void Start()
    {
        ScoreManager.score_manager.AssignTeam(this.gameObject);
        ScoreManager.score_manager.SetPlayerColours(team, this.gameObject);
    }
}
