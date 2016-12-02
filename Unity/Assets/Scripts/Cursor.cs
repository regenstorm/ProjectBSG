using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour {
	private Map map;

	// Use this for initialization
	void Start () {
		map = GameObject.Find ("BattleGround").GetComponent<Map>();
	}
	
	// Update is called once per frame
	void Update () {
		var offsetY = 0;
		var offsetX = 0;
		var moveStep = map.ScalingFactor;

		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			offsetY = moveStep;
		}

		if (Input.GetKeyDown (KeyCode.DownArrow)) {
			offsetY = -moveStep;
		}
			
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			offsetX = -moveStep;
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			offsetX = moveStep;
		}

		var newPos = new Vector3 {
			x = this.transform.localPosition.x + offsetX,
			y = this.transform.localPosition.y + offsetY,
		};

		if (!(offsetY == 0 && offsetX == 0) && map.IsLegalPosition (newPos)) {
			this.transform.localPosition = newPos;
		}
	}
}
