using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour {
	public int ScalingFactor = 2;
	public int Width;
	public int Height;
	public GameObject MoveOverlayPrefab;

	private Transform unitsContainer;
	private Transform overlayContainer;
	private Unit selectedUnit;

	void Start () {
		this.Width = ScalingFactor * 4;
		this.Height = ScalingFactor * 3;
		unitsContainer = transform.Find ("Units");
		overlayContainer = transform.Find ("Overlay");
	}
	
	void Update () {
	}

	void OnMouseDown() {
		var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);

		if (selectedUnit) {
			// move the unit to the new position
			var newPos = RoundPosition(mousePos - transform.position);
			if (UnitCanMoveToPosition (selectedUnit, newPos)) {
				selectedUnit.transform.localPosition = newPos;
			}

			DeselectCurrentUnit ();
		}
	}

	public bool IsLegalPosition(Vector3 pos) {
		return pos.x >= 0 && pos.x < Width 
			&& pos.y <= 0 && pos.y > -Height;
	}

	public Unit UnitAtPosition(Vector3 pos) {
		foreach (Transform child in unitsContainer) {
			if (child.localPosition == pos) {
				return child.gameObject.GetComponent<Unit>();
			}
		}

		return null;
	}

	public void SelectUnit(Unit unit) {
		this.selectedUnit = unit;
		unit.OnSelected ();
		DrawLegalMovesOverlay ();
	}

	public void DeselectCurrentUnit() {
		selectedUnit.OnDeselected ();
		selectedUnit = null;
		ClearLegalMovesOverlay ();
	}

	public Vector3 RoundPosition(Vector3 pos) {
		return new Vector3 {
			x = Mathf.Round(pos.x / ScalingFactor) * ScalingFactor,
			y = Mathf.Round(pos.y / ScalingFactor) * ScalingFactor
		};
	}

	public void DrawLegalMovesOverlay() {
		var moves = LegalMoves (selectedUnit);
		foreach (Vector3 move in moves) {
			Instantiate (MoveOverlayPrefab, move, Quaternion.identity, overlayContainer);
		}
	}

	public void ClearLegalMovesOverlay() {
		foreach (Transform child in overlayContainer) {
			Object.Destroy (child.gameObject);
		}
	}

	public List<Vector3> LegalMoves(Unit unit) {
		List<Vector3> moves = new List<Vector3> ();
		// FIXME: simplistic algo, very likely wrong
		for (int i = -unit.MoveRange; i <= unit.MoveRange; i++) {
			for (int k = -unit.MoveRange; k <= unit.MoveRange; k++) {
				var x = i * ScalingFactor;
				var y = k * ScalingFactor;
				var step = unit.transform.localPosition + new Vector3 (x, y);

				if (!(i == 0 && k == 0) && IsLegalPosition(step)) {
					moves.Add (step);
				}
			}
		}

		return moves;
	}

	public bool UnitCanMoveToPosition(Unit unit, Vector3 pos) {
		// FIXME: slow, requires recalculating legal moves list every time
		return LegalMoves (unit).Contains(pos);
	}
}
