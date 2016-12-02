using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {
	public const int ScalingFactor = 2;
	public const int Width = 4;
	public const int Height = 3;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Position ScreenToMapPosition(Vector3 screen) {
		// lossy conversion
		return new Position {
			row = (int) (screen.y / -ScalingFactor),
			column = (int) (screen.x / ScalingFactor)
		};
	}

	public Vector3 MapToScreenPosition(Position pos) {
		return new Vector3 {
			x = pos.column * ScalingFactor, 
			y = pos.row * -ScalingFactor
		};
	}

	public bool IsLegalPosition(Position pos) {
		return pos.column >= 0 && pos.column < Width 
			&& pos.row >= 0 && pos.row < Height;
	}
}
