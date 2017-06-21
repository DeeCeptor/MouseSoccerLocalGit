using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRect : MonoBehaviour 
{
    public static Rect camera_rect;
    public static Rect arena_rect;

    public GameObject topright;
    public GameObject bottomleft;

	void Start () 
	{
        var bottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
        var topRight = Camera.main.ScreenToWorldPoint(new Vector3(
            Camera.main.pixelWidth, Camera.main.pixelHeight));

        camera_rect = new Rect(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x - bottomLeft.x,
            topRight.y - bottomLeft.y);

        arena_rect = new Rect(bottomleft.transform.position.x,
            bottomleft.transform.position.y,
            topright.transform.position.x - bottomleft.transform.position.x,
            topright.transform.position.y - bottomleft.transform.position.y);
    }
}
