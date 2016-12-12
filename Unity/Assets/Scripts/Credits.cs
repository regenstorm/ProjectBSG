using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour {

	public Button backButton;

	// Use this for initialization
	void Start () {
		backButton.onClick.AddListener (BackButtonPressed);
	}

	public void BackButtonPressed () {
		Tracking.instance.TrackEvent (TrackingEventTypes.Credits, "Back button pressed");
		SceneManager.LoadScene ("MainMenu");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
