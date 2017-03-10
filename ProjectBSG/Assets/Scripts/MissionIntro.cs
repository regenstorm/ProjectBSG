using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MissionIntro : MonoBehaviour {

	public Button nextButton;

	// Use this for initialization
	void Start () {
		nextButton.onClick.AddListener (NextButtonPressed);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void NextButtonPressed() {
		SceneManager.LoadScene ("IntroBattle");
	}
}
