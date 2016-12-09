using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShowWhoWon : MonoBehaviour {

	// Use this for initialization
	void Start () {
//		;
		GetComponent<Text>().text = BattleController.Instance.whoWon.ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
