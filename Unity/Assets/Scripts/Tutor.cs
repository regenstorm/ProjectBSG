using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tutor : MonoBehaviour {
	public UnityEngine.UI.Text TutorialText;
	private bool tutorialDone = false;

	// Use this for initialization
	void Start () {
		TutorialText.text = 
			"Hello commander Copper-42. Your units are blue and enemy units are red. " +
			"Touch one of your units to start controlling it. I believe you can lead us all through this menace!";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Hide() {
		TutorialText.enabled = false;
		GetComponent<Image> ().enabled = false;
	}

	public void UnitSelected() {
		if (tutorialDone) {
			Hide ();
			return;
		}

		TutorialText.text = 
			"Great! Now you can see some blue and red tiles. " + 
			"The blue ones you can move to and the red ones you can attack. " + 
			"Now select a blue tile by tapping on clicking on it to move your unit over.";
	}

	public void UnitMoved() {
		TutorialText.text = 
			"Now you can see that the blue tiles disappeared because there " + 
			"is one or more enemy units in your attack range. Tap or click that unit to attack.";
	}

	public void UnitAttacked() {
		TutorialText.text = 
			"Your unit becomes deactivated after the move. " + 
			"Select another unit and continue. Remember, you can " + 
			"make a unit stay where it is by tapping on its current tile.";
		tutorialDone = true;
	}
}
