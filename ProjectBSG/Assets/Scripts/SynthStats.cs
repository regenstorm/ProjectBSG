using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SynthStats : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Text>().text = "Units lost:\n" + BattleController.Instance.synthsDestroyed.ToString() + "\nDamage dealt:\n"
			+ BattleController.Instance.synthsDamageDealt.ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
