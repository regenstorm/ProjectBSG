using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MissionSelection : MonoBehaviour {

	public Button introMission;

	// Use this for initialization
	void Start () {
		introMission.onClick.AddListener (IntroMissionButtonPressed);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void IntroMissionButtonPressed () {
		SceneManager.LoadScene ("MissionIntro");
	}
}
