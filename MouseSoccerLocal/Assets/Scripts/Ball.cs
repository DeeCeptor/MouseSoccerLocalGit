using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Ball : MonoBehaviour 
{
    public static Ball ball;

    public Rigidbody2D physics;
    Animator anim;
    SpriteRenderer sprite;
    public TrailRenderer trail;

    [SerializeField]
    private float max_speed = 20f;

    Collider2D collider;

	void Awake () 
	{
        ball = this;
        physics = this.GetComponent<Rigidbody2D>();
        //anim = this.GetComponent<Animator>();
        trail = this.GetComponent<TrailRenderer>();
        collider = this.GetComponent<Collider2D>();
        sprite = this.GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
    }


    public void Reset(Vector2 position)
    {
        this.GetComponent<TrailRenderer>().Clear();
        physics.velocity = Vector2.zero;
        physics.angularVelocity = 0;
        this.transform.position = position;

        GetComponent<TrailRenderer>().startColor = Color.white;
        GetComponent<TrailRenderer>().endColor = Color.white;
    }


    public void SetCollisions(bool enable_colls)
    {
        if (enable_colls)
            sprite.color = Color.white;
        else
        {
            Color c = sprite.color;
            c.a = 0.5f;
            sprite.color = c;
        }

        collider.enabled = enable_colls;
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

        // Check if it's with a wall
        if (collision.gameObject.tag == "Wall" && Trial.trial is SoloKickStationaryIntoNet)
        {
            SoloKickStationaryIntoNet kick = (SoloKickStationaryIntoNet)Trial.trial;
            if (collision.contacts.Length > 0)
                kick.SetAccuracy(collision.contacts[0].point);
            else
                kick.SetAccuracy(this.transform.position);
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        //CollidedWithPlayer(collision.gameObject);
    }
    public void CollidedWithPlayer(GameObject obj)
    {
        
    }
}