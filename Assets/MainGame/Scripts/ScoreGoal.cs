using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreGoal : MonoBehaviour {
	public Tds_GameManager vGameManager;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Ball") {
			vGameManager.vMainPlayer.Score += 1;
			vGameManager.ScoreText.text = vGameManager.vMainPlayer.Score.ToString();
			if (vGameManager.vMainPlayer.Score >= 3) {
				Cursor.visible = true;
				SceneManager.LoadScene ("Main");
			}
		}
	}
}
