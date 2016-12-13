using UnityEngine;
using System.Collections;

public class Tutor : MonoBehaviour {
	public UnityEngine.UI.Text TutorialText;
	public bool Active = true;

	bool unitSelected = false;
	bool unitMoved = false;
	bool unitAttacked = false;

	// Use this for initialization
	void Start () {
		TutorialText.text = 
			"Your units are blue and enemy units are red. " +
			"Touch one of your units to start controlling it.";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Hide() {
	}

	public void UnitSelected() {
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
	}
}
