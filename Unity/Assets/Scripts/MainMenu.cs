using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	public Button newGameButton;
	public Button exitGameButton;
	public Button loadGameButton; //LOL
	public Button creditsButton;

	void Start () {
		newGameButton.onClick.AddListener (NewGame);
		exitGameButton.onClick.AddListener (ExitGame);
		creditsButton.onClick.AddListener (LoadCredits);

		MusicManager.instance.Play (MusicManager.MusicTheme.MainMenu);
	}

	public void NewGame() {
		SceneManager.LoadScene ("MissionSelection");
	}

	public void LoadGame() {
		//ROFL. Would never be called. NEVER.
	}

	public void LoadCredits() {
		
	}

	public void ExitGame() {
		Debug.Log ("Exiting game...");
		Application.Quit();
	}
		
}
