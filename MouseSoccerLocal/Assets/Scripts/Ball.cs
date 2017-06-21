using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Ball : MonoBehaviour 
{
    public static Ball ball;

    public Rigidbody2D physics;
    Animator anim;

    [SerializeField]
    private float max_speed = 20f;


	void Awake () 
	{
        ball = this;
        physics = this.GetComponent<Rigidbody2D>();
        //anim = this.GetComponent<Animator>();
	}
    private void Start()
    {

    }


    public void Reset(Vector2 position)
    {
        this.GetComponent<TrailRenderer>().Clear();
        physics.velocity = Vector2.zero;
        this.transform.position = position;

        GetComponent<TrailRenderer>().startColor = Color.white;
        GetComponent<TrailRenderer>().endColor = Color.white;
    }


    private void FixedUpdate()
    {
        // Clamp max speed
        if (physics.velocity.magnitude > max_speed)
        {
            physics.velocity *= max_speed / physics.velocity.magnitude;
        }
    }



    private void Update()
    {
        // Change animation speed based on ball's velocity
        //anim.speed = physics.velocity.magnitude / 10f;
        if (anim != null)
        {
            anim.speed = 0;

            var dir = (Vector3)physics.velocity - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }


    public void BallTouched(Player p)
    {

    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //CollidedWithPlayer(collision.gameObject);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        //CollidedWithPlayer(collision.gameObject);
    }
    public void CollidedWithPlayer(GameObject obj)
    {
        
    }
}