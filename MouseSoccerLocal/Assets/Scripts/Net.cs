using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Net : MonoBehaviour
{
    public Team teams_goal;    // Blue or red
    public string scoring_message;
    public bool reset_ball_position = false;

    public Transform top_of_net;
    public Transform bottom_of_net;


    void Start()
    {

    }


    void Update()
    {

    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ball")
        {
            StartCoroutine(GoalScored(other));
        }
    }
    public void Effects()
    {
        ParticleSystem ps = this.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
            ps.Play();
        AudioSource a = this.GetComponent<AudioSource>();
        if (a != null)
            a.Play();
    }


    IEnumerator GoalScored(Collider2D other)
    {
        switch (teams_goal)
        {
            case Team.Blue:
                ScoreManager.score_manager.RedScored(1);
                break;
            case Team.Red:
                ScoreManager.score_manager.BlueScored(1);
                break;
        }

        Effects();

        if (reset_ball_position)
        {
            // Respawn the ball after a second
            other.gameObject.SetActive(false);

            yield return new WaitForSeconds(1f);

            other.gameObject.SetActive(true);
            other.gameObject.transform.position = Vector2.zero;
            other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        yield return null;
    }
}
