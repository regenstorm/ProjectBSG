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
	public GameObject AttackIndicatorPrefab;
	public GameObject UnitPrefab;

	private Transform unitsContainer;
	private Transform overlayContainer;
	private Transform gridContainer;
	private Unit selectedUnit;
	private PathFinder<Vector3> pathFinder;

	private GameState gameState = GameState.IDLE;

	void Start () {
		pathFinder = new PathFinder<Vector3> ();

		this.Width = ScalingFactor * 15;
		this.Height = ScalingFactor * 10;
		unitsContainer = transform.Find ("Units");
		overlayContainer = transform.Find ("Overlay");
		gridContainer = transform.Find ("Grid");

		FillColliderBox ();
		GenerateGrid ();

		PlaceUnits ();
	}

	void PlaceUnits ()
	{
		// NOTE: factions and locations must have the same size.
		// In fact, they're like an array of 2-tuples splitted into 2 arrays.
		var factions = new Faction[] {
			Faction.HUMAN,
			Faction.SYNTH
		};

		var locations = new Vector3[] {
			new Vector3(0, 0),
			new Vector3(4, 0),
		};

		for (var i = 0; i < locations.Length; i++) {
			var location = locations [i];
			var faction = factions [i];
			var unit = (GameObject) Instantiate (UnitPrefab, location, Quaternion.identity, unitsContainer);
			unit.GetComponent<Unit> ().Faction = faction;
		}
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
				if (x % (ScalingFactor * 2) == (y % (ScalingFactor * 2) == 0 ? 0 : ScalingFactor)) {
					Instantiate (gridCellPrefab, new Vector3(x, y), Quaternion.identity, gridContainer);
				}
			}
		}
	}

	void Update () {
	}

	void OnMouseDown() {
		var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		var clickedTile = RoundPosition (mousePos - transform.position);
		var unit = UnitAtPosition(clickedTile);

		if (selectedUnit) {
			if (gameState == GameState.MOVE_TILE_SELECTION) {
				MoveUnit (selectedUnit, clickedTile);
			}

			if (gameState == GameState.ATTACK_TILE_SELECTION) {
				// check if a valid attack target has been chosen
				if (unit && UnitIsLegalAttackTarget (unit)) {
					selectedUnit.Fight (unit);

					DeselectCurrentUnit ();
					gameState = GameState.IDLE;
				}
			}
		} else if (unit) {
			SelectUnit (unit);
		}
	}

	private bool UnitIsLegalAttackTarget(Unit other) {
		return !other.IsFriendlyWith (selectedUnit) && EnemiesInRange(selectedUnit).Contains(other);
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
		gameState = GameState.MOVE_TILE_SELECTION;
	}

	public void DeselectCurrentUnit() {
		selectedUnit.OnDeselected ();
		selectedUnit = null;
		ClearLegalMovesOverlay ();
	}

	public void MoveUnit(Unit unit, Vector3 pos) {
		ClearLegalMovesOverlay ();

		if (UnitCanMoveToPosition (selectedUnit, pos)) {
			gameState = GameState.MOVING;
			StartCoroutine (MoveUnitInSequence (unit, pathFinder.Path (selectedUnit.transform.localPosition, pos, this.MoveableNeighbors)));
		} else {
			DeselectCurrentUnit ();
			gameState = GameState.IDLE;
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
			DrawAttackTargetIndicators ();
			gameState = GameState.ATTACK_TILE_SELECTION;
		} else {
			DeselectCurrentUnit ();
			gameState = GameState.IDLE;
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

	private void DrawAttackTargetIndicators() {
		foreach (var unit in EnemiesInRange(selectedUnit)) {
			var location = unit.transform.localPosition;
			Instantiate (AttackIndicatorPrefab, location, Quaternion.identity, overlayContainer);
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
		return (from cell in LegalStationaryAttackMoves (unit)
		  let other = UnitAtPosition (cell)
		  where other != null && !GameController.Instance.FriendsWith (other.Faction, unit.Faction)
		  select other);
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
				where pair.Value <= unit.MoveRange
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
