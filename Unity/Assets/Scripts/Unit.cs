using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
	public int MoveRange = 2;

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
		shipSprite.Translate (new Vector3(0, 0.1f));
	}

	public void OnDeselected() {
		this.selected = false;
		shipSprite.Translate (new Vector3(0, -0.1f));
	}

	void OnMouseDown() {
		map.SelectUnit (this);
	}
}
