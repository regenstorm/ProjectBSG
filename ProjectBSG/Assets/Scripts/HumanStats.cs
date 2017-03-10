using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HumanStats : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Text>().text = "Units lost:\n" + BattleController.Instance.humansDestroyed.ToString() + "\nDamage dealt:\n"
			+ BattleController.Instance.humansDamageDealt.ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
