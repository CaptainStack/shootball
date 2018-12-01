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
		if (GameObject.FindGameObjectsWithTag("Music").Length > 1) {
			Destroy(GameObject.FindGameObjectsWithTag("Music")[1]);
		}
	}

	public void SwitchScene(string vNewScene) {
		if (vNewScene != null) {
			SceneManager.LoadScene (vNewScene);
			if (vNewScene != "Arena") {
				Cursor.visible = true;
			}
			if (SceneManager.GetActiveScene().name == "Main" && vNewScene == "Controls" ||
				SceneManager.GetActiveScene().name == "Controls" && vNewScene == "Main") {
				DontDestroyOnLoad(GameObject.FindGameObjectsWithTag("Music")[0]);
			} else {
				if (GameObject.FindGameObjectsWithTag("Music").Length > 0) {
					Destroy(GameObject.FindGameObjectsWithTag("Music")[0]);
				}
			}
		}
	}

	public void QuitGame() {
		Application.Quit();
	}
}
