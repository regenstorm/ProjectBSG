using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public Button newGameButton;
	public Button exitGameButton;
	public Button loadGameButton; //LOL
	public Button creditsButton;

	void Awake() {
	}

	void Start () {
		newGameButton.onClick.AddListener (NewGame);
		exitGameButton.onClick.AddListener (ExitGame);
		creditsButton.onClick.AddListener (LoadCredits);
		Tracking.instance.TrackDevice ();
		Tracking.instance.StartSession ();
	}

	public void NewGame() {
		Tracking.instance.TrackEvent (TrackingEventTypes.MainMenu, "New game button pressed");
		SceneManager.LoadScene ("MissionSelection");
	}

	public void LoadGame() {
		//ROFL. Would never be called. NEVER.
	}

	public void LoadCredits() {
		SceneManager.LoadScene ("Credits");
		Tracking.instance.TrackEvent (TrackingEventTypes.MainMenu, "Credits button pressed");
	}

	public void ExitGame() {
		Debug.Log ("Exiting game...");
		Tracking.instance.TrackEvent (TrackingEventTypes.MainMenu, "Exit game button pressed");
		Application.Quit();
	}
		
}
