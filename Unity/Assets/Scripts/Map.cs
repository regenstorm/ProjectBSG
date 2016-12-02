using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {
	public int ScalingFactor = 2;
	public int Width;
	public int Height;

	// Use this for initialization
	void Start () {
		this.Width = ScalingFactor * 4;
		this.Height = ScalingFactor * 3;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool IsLegalPosition(Vector3 pos) {
		return pos.x >= 0 && pos.x < Width 
			&& pos.y <= 0 && pos.y > -Height;
	}
}
