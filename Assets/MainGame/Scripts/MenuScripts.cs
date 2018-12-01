using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScripts : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// SceneManager.LoadScene ("demo");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SwitchScene(string vNewScene) {
		if (vNewScene != null) {
			SceneManager.LoadScene (vNewScene);
		}
	}

	public void QuitGame() {
		Application.Quit();
	}
}
