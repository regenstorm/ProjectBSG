using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
	Transform shipSprite;
	Map map;
	bool selected = false;

	// Use this for initialization
	void Start () {
		shipSprite = transform.FindChild ("ship_sprite");
		map = GameObject.Find ("BattleGround").GetComponent<Map> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void OnSelected() {
		this.selected = true;
		print ("selected");
		shipSprite.Translate (new Vector3(0, 0.1f));
	}

	public void OnDeselected() {
		this.selected = false;
		shipSprite.Translate (new Vector3(0, -0.1f));
	}

	void OnMouseDown() {
		print ("mouse");
		map.SelectUnit (this);
	}
}
