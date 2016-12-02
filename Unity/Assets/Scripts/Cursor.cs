using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour {
	public Position position = new Position { row = 0 , column = 0 };
	private Map map;

	// Use this for initialization
	void Start () {
		map = GameObject.Find ("BattleGround").GetComponent<Map>();
	}
	
	// Update is called once per frame
	void Update () {
		var offsetRow = 0;
		var offsetColumn = 0;

		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			offsetRow = -1;
		}

		if (Input.GetKeyDown (KeyCode.DownArrow)) {
			offsetRow = 1;
		}
			
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			offsetColumn = -1;
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			offsetColumn = 1;
		}

		var newPos = position.translate (offsetRow, offsetColumn);

		if (!(offsetRow == 0 && offsetColumn == 0) && map.IsLegalPosition (newPos)) {
			this.transform.position = map.MapToScreenPosition (newPos);
			this.position = newPos;
		}
	}
}
