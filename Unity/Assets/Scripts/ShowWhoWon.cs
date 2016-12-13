using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowWhoWon : MonoBehaviour {

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
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
