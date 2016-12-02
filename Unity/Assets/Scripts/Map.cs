using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {
	public int ScalingFactor = 2;
	public int Width;
	public int Height;
	private GameObject unitsContainer;
	private Unit selectedUnit;

	void Start () {
		this.Width = ScalingFactor * 4;
		this.Height = ScalingFactor * 3;
		unitsContainer = transform.Find ("Units").gameObject;
	}
	
	void Update () {
	}

	void OnMouseDown() {
		if (selectedUnit) {
			// move the unit to the new position
			var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			selectedUnit.transform.localPosition = RoundPosition(mousePos - transform.position);
			DeselectCurrentUnit ();
		}
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
		print ("selecting");
		this.selectedUnit = unit;
		unit.OnSelected ();
	}

	public void DeselectCurrentUnit() {
		selectedUnit.OnDeselected ();
		selectedUnit = null;
	}

	public Vector3 RoundPosition(Vector3 pos) {
		return new Vector3 {
			x = Mathf.Round(pos.x / ScalingFactor) * ScalingFactor,
			y = Mathf.Round(pos.y / ScalingFactor) * ScalingFactor
		};
	}
}
