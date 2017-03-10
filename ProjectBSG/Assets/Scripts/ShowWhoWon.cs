using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShowWhoWon : MonoBehaviour {

	public Button backButton;

	// Use this for initialization
	void Start () {
		GetComponent<Text>().text = (BattleController.Instance.whoWon.ToString() == "SYNTH" ? "Synths" : "Humans") + " have won in " + 
			BattleController.Instance.turnNumber.ToString() + " turns";

		switch (BattleController.Instance.whoWon) {
		case Faction.HUMAN:
			MusicManager.instance.Play (MusicManager.MusicTheme.BattleWon);
			break;
		case Faction.SYNTH:
			MusicManager.instance.Play (MusicManager.MusicTheme.BattleLost);
			break;
		}

		Debug.Log (backButton);
		backButton.onClick.AddListener(() => { 
			Debug.Log("Back button pressed");
			SceneManager.LoadScene("MainMenu");}
		);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
