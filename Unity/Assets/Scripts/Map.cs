using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Map : MonoBehaviour {
	public int ScalingFactor = 2;
	public int Width;
	public int Height;
	public GameObject MoveOverlayPrefab;
	public GameObject AttackOverlayPrefab;
	public GameObject gridCellPrefab;

	private Transform unitsContainer;
	private Transform overlayContainer;
	private Transform gridContainer;
	private Unit selectedUnit;
	private PathFinder<Vector3> pathFinder;

	void Start () {
		pathFinder = new PathFinder<Vector3> ();

		this.Width = ScalingFactor * 15;
		this.Height = ScalingFactor * 10;
		unitsContainer = transform.Find ("Units");
		overlayContainer = transform.Find ("Overlay");
		gridContainer = transform.Find ("Grid");

		FillColliderBox ();
		GenerateGrid ();
	}

	void FillColliderBox() {
		// This collider is for mouse click detection
		var collider = GetComponent<BoxCollider> ();
		collider.center = new Vector2 (this.Width / 2 - 1, -this.Height / 2 + 1);
		collider.size = new Vector2 (this.Width, this.Height);
	}

	void GenerateGrid() {
		for (int x = 0; x < Width; x += ScalingFactor) {
			for (int y = 0; y > -Height; y -= ScalingFactor) {
				if (x % 4 == (y % 4 == 0 ? 0 : 2)) {
					Instantiate (gridCellPrefab, new Vector3(x, y), Quaternion.identity, gridContainer);
				}
			}
		}
	}

	void Update () {
	}

	void OnMouseDown() {
		var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);

		if (selectedUnit) {
			// move the unit to the new position
			var newPos = RoundPosition(mousePos - transform.position);
			ClearLegalMovesOverlay ();
			MoveUnit (selectedUnit, newPos);
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
		if (this.selectedUnit) {
			DeselectCurrentUnit ();
		}

		this.selectedUnit = unit;
		unit.OnSelected ();
		DrawLegalMovesOverlay ();
	}

	public void DeselectCurrentUnit() {
		selectedUnit.OnDeselected ();
		selectedUnit = null;
		ClearLegalMovesOverlay ();
	}

	public void MoveUnit(Unit unit, Vector3 pos) {
		if (UnitCanMoveToPosition (selectedUnit, pos)) {
			StartCoroutine (MoveUnitInSequence(unit, pathFinder.Path (selectedUnit.transform.localPosition, pos, this.MoveableNeighbors)));
		}
	}

	private IEnumerator MoveUnitInSequence(Unit unit, IEnumerable<Vector3> path) {
		// FIXME: Do some kind of movement animation here
		// https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
		foreach (var step in path) {
			unit.transform.localPosition = step;
			yield return new WaitForSeconds(0.2f);
		}

		// FIXME: using Count() could be slow
		if (EnemiesInRange (unit).Count () > 0) {
			DrawLegalAttacksOverlay ();
		} else {
			DeselectCurrentUnit ();
		}
	}

	public Vector3 RoundPosition(Vector3 pos) {
		return new Vector3 {
			x = Mathf.Round(pos.x / ScalingFactor) * ScalingFactor,
			y = Mathf.Round(pos.y / ScalingFactor) * ScalingFactor
		};
	}

	public void DrawLegalAttacksOverlay() {
		var cells = LegalStationaryAttackMoves (selectedUnit);

		foreach (Vector3 cell in cells) {
			Instantiate (AttackOverlayPrefab, cell, Quaternion.identity, overlayContainer);
		}
	}

	public void DrawLegalMovesOverlay() {
		var moves = LegalMoves (selectedUnit);
		foreach (Vector3 cell in moves) {
			Instantiate (MoveOverlayPrefab, cell, Quaternion.identity, overlayContainer);
		}

		var attackMoves = LegalAttackMoves (selectedUnit);
		var attackOverlayCells = new HashSet<Vector3> (attackMoves).Except(new HashSet<Vector3>(moves));
		foreach (Vector3 cell in attackOverlayCells) {
			Instantiate (AttackOverlayPrefab, cell, Quaternion.identity, overlayContainer);
		}

//		Alternative implementation, not sure which is faster.
//
//		foreach (Vector3 cell in attackMoves) {
//			if (!moves.Contains(cell)) {
//				//Instantiate (AttackOverlayPrefab, cell, Quaternion.identity, overlayContainer);
//			}
//		}
	}

	public void ClearLegalMovesOverlay() {
		foreach (Transform child in overlayContainer) {
			Object.Destroy (child.gameObject);
		}
	}

	private IEnumerable<Unit> EnemiesInRange(Unit unit) {
		var enemies = new List<Unit> ();

		foreach (Vector3 cell in LegalStationaryAttackMoves(unit)) {
			var other = UnitAtPosition (cell);
			if (other != null && !GameController.Instance.FriendsWith(other.Faction, unit.Faction)) {
				enemies.Add (other);
			}
		}

		return enemies;
	}

	public bool UnitCanMoveToPosition(Unit unit, Vector3 pos) {
		// FIXME: slow, requires recalculating legal moves list every time. Caching every turn?
		return LegalMoves (unit).Contains(pos);
	}

	private IEnumerable<Vector3> Neighbors(Vector3 pos, System.Func<Vector3, Vector3, bool> inclusionPredicate) {
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				var neighbor = new Vector3 {
					x = i * ScalingFactor,
					y = j * -ScalingFactor
				} + pos;

				if (i != j && i != -j && IsLegalPosition(neighbor) && inclusionPredicate(pos, neighbor)) {
					yield return neighbor;
				}
			}
		}
	}

	public IEnumerable<Vector3> LegalStationaryAttackMoves(Unit unit) {
		// legal attack cells immediately surrounding the unit
		return from pair in pathFinder.DistancesToNode (selectedUnit.transform.localPosition, this.AttackableNeighbors)
				where pair.Value <= selectedUnit.AttackRange && pair.Value > 0
			select pair.Key;
	}

	public IEnumerable<Vector3> LegalAttackMoves(Unit unit) {
		// all possible legal attack cells of the unit
		return from pair in pathFinder.DistancesToNode (unit.transform.localPosition, this.AttackableNeighbors)
				where pair.Value <= unit.MoveRange + unit.AttackRange && pair.Value > 0
			select pair.Key;
	}

	public IEnumerable<Vector3> LegalMoves(Unit unit) {
		return from pair in pathFinder.DistancesToNode(unit.transform.localPosition, this.MoveableNeighbors)
				where pair.Value <= unit.MoveRange && pair.Value > 0
			select pair.Key;
	}

	private IEnumerable<Vector3> AttackableNeighbors(Vector3 pos) {
		return Neighbors (pos, (current, neighbor) => {
			var other = UnitAtPosition (neighbor);
			return other == null || !GameController.Instance.FriendsWith (selectedUnit.Faction, other.Faction);
		});
	}

	private IEnumerable<Vector3> MoveableNeighbors(Vector3 pos) {
		return Neighbors (pos, (current, neighbor) => UnitAtPosition (neighbor) == null);
	}
}
