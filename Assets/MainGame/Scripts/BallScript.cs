using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour 
{
    private Vector3 startPos;

    // Use this for initialization
    void Start ()
    {
        startPos = this.transform.position;
    }
    
    // Update is called once per frame
    void Update ()
    {
        
    }

    public void ResetPosition()
    {
        this.transform.position = startPos;
        this.transform.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        this.transform.GetComponent<Rigidbody2D>().angularVelocity = 0f;
    }
}
