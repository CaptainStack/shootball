﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreGoal : MonoBehaviour {
	public Tds_GameManager vGameManager;
    public int playerNumber;
    public Transform player1;
    public Transform player2;
    public Transform ball;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Ball") {
            if (playerNumber == 1) {
                vGameManager.vMainPlayer.Score += 1;
                vGameManager.ScoreText.text = vGameManager.vMainPlayer.Score.ToString();
            }
            else {
                vGameManager.vPlayer2.Score += 1;
                vGameManager.ScoreTextP2.text = vGameManager.vPlayer2.Score.ToString();
            }
            other.GetComponent<BallScript>().ResetPosition();
            if (vGameManager.vMainPlayer.Score >= 3) {
				Cursor.visible = true;
				SceneManager.LoadScene ("Main");
			}
            else if (vGameManager.vPlayer2.Score >= 3)  {
                Cursor.visible = true;
                SceneManager.LoadScene("Main");
            }
		}
	}
}
