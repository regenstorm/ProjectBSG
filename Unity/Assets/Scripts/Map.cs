using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {
	public int ScalingFactor = 2;
	public int Width;
	public int Height;
	private GameObject unitsContainer;
	private Unit selectedUnit;

	// Use this for initialization
	void Start () {
		this.Width = ScalingFactor * 4;
		this.Height = ScalingFactor * 3;
		unitsContainer = transform.Find ("Units").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool IsLegalPosition(Vector3 pos) {
		return pos.x >= 0 && pos.x < Width 
			&& pos.y <= 0 && pos.y > -Height;
	}

	public Unit UnitAtPosition(Vector3 pos) {
		foreach (Transform child in unitsContainer.transform) {
			if (child.localPosition == pos) {
				return child.gameObject.GetComponent<Unit>();
			}
		}

		return null;
	}

	public void SelectUnit(Unit unit) {
		this.selectedUnit = unit;
		unit.OnSelected ();
	}

	public void DeselectCurrentUnit() {
		selectedUnit.OnDeselected ();
	}
}
