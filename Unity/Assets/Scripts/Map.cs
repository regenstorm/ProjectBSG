using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;

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
	public PathFinder<Vector3> pathFinder;

	private GameState gameState = GameState.IDLE;
	private Faction currentFaction = Faction.HUMAN;
	private Dictionary<Faction, HashSet<Unit>> unitsOfFaction;
	private AIPlayer aiPlayer;
	private Coroutine aiPlayerDoTurnCoroutine;

	public void RegisterUnit(Unit unit) {
		unitsOfFaction [unit.Faction].Add (unit);
	}

	public void DeregisterUnit(Unit unit) {
		unitsOfFaction [unit.Faction].Remove (unit);
	}

	public Faction NextFaction() {
		return currentFaction == Faction.SYNTH ? Faction.HUMAN : Faction.SYNTH;
	}

	public static Color FactionColor(Faction faction) {
		return faction == Faction.SYNTH ? Color.HSVToRGB(0.64f,0.84f,0.6f) : Color.HSVToRGB(0.00f,0.84f,0.6f);
	}

	private void NextTurn() {
		if (aiPlayerDoTurnCoroutine != null) {
			StopCoroutine (aiPlayerDoTurnCoroutine);
		}

		currentFaction = NextFaction();
		foreach (var unit in unitsOfFaction[currentFaction]) {
			unit.ResetTurn ();
		}
		BattleController.Instance.turnNumber++;
		UpdateFactionIndicator ();

		if (currentFaction == Faction.HUMAN) {
			aiPlayerDoTurnCoroutine = StartCoroutine(aiPlayer.DoTurn (UnitsOfFaction(currentFaction)));
		}
	}

	private void UpdateFactionIndicator() {
		var indicator = GameObject.Find ("FactionIndicator").GetComponent<Text> ();
		indicator.text = currentFaction.ToString() + " Turn: " + (BattleController.Instance.turnNumber / 2 +1).ToString();
		indicator.color = FactionColor (currentFaction);
	}

	void Start () {
		pathFinder = new PathFinder<Vector3> ();

		unitsOfFaction = new Dictionary<Faction, HashSet<Unit>> ();
		unitsOfFaction.Add (Faction.HUMAN, new HashSet<Unit> ());
		unitsOfFaction.Add (Faction.SYNTH, new HashSet<Unit> ());

		this.Width = ScalingFactor * 12;
		this.Height = ScalingFactor * 6;
		unitsContainer = transform.Find ("Units");
		overlayContainer = transform.Find ("Overlay");
		gridContainer = transform.Find ("Grid");

		aiPlayer = new AIPlayer (this, pathFinder);

		FillColliderBox ();
		GenerateGrid ();
		PlaceUnits ();
		UpdateFactionIndicator ();

		MusicManager.instance.Play (MusicManager.MusicTheme.Battle);
		NextTurn ();
	}

	void PlaceUnits ()
	{
		// NOTE: factions and locations must have the same size.
		// In fact, they're like an array of 2-tuples splitted into 2 arrays.
		var factions = new Faction[] {
			Faction.HUMAN,
			Faction.HUMAN,
			Faction.HUMAN,
			Faction.SYNTH,
			Faction.SYNTH,
			Faction.SYNTH,
		};

		var locations = new Vector3[] {
			new Vector3(0, 0),
			new Vector3(2, 0),
			new Vector3(4, 0),
			new Vector3(10, 0),
			new Vector3(6, -4),
			new Vector3(2, -6),
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

	void OnMouseDown() {
		if (currentFaction == Faction.HUMAN) {
			return;
		}

		if (gameState != GameState.MOVE_TILE_SELECTION
		    && gameState != GameState.IDLE
		    && gameState != GameState.ATTACK_TILE_SELECTION) 
		{
			print (gameState);
			return;
		}

		var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		var clickedTile = RoundPosition (mousePos - transform.position);
		var unit = UnitAtPosition(clickedTile);

		if (selectedUnit) {
			if (gameState == GameState.MOVE_TILE_SELECTION) {
				if (UnitCanMoveToPosition (selectedUnit, clickedTile)) {
					Action showAttackOverlay = () => {
						// FIXME: using Count() could be slow
						if (EnemiesInRange (selectedUnit).Count () > 0) {
							DrawLegalAttacksOverlay ();
							DrawAttackTargetIndicators ();
							gameState = GameState.ATTACK_TILE_SELECTION;
						} else {
							EndSelectedUnitTurn ();
						}
					};

					MoveUnit (selectedUnit, clickedTile, then: showAttackOverlay);
				} else {
					DeselectCurrentUnit ();
					gameState = GameState.IDLE;
				}
			}

			if (gameState == GameState.ATTACK_TILE_SELECTION) {
				// check if a valid attack target has been chosen
				if (unit && UnitIsLegalAttackTarget (unit)) {
					new AttackExecutor (
						attacker: selectedUnit,
						receiver: unit,
						then: () => EndSelectedUnitTurn ()
					).Execute();
				}
			}
		} else if (unit) {
			SelectUnit (unit);
		}
	}

	public void EndSelectedUnitTurn() {
		selectedUnit.EndTurn ();
		DeselectCurrentUnit ();
		gameState = GameState.IDLE;

		// check if battle is over
		if (unitsOfFaction[NextFaction()].Count() == 0) {
			BattleController.Instance.turnNumber++;
			ConcludeBattle (whoWon: currentFaction);
			return;
		}

		// check if player's turn is done
		var allUnitsUsed = UnitsOfFaction(currentFaction).All (unit => unit.Dirty);
		if (allUnitsUsed) {
			NextTurn ();
		}
	}

	private void ConcludeBattle(Faction whoWon) {
		BattleController.Instance.whoWon = whoWon;
		BattleController.Instance.turnNumber = BattleController.Instance.turnNumber / 2 + 1;
		UnityEngine.SceneManagement.SceneManager.LoadScene ("BattleConclusion");
	}

	private bool UnitIsLegalAttackTarget(Unit other) {
		return !other.IsFriendlyWith (selectedUnit) && EnemiesInRange(selectedUnit).Contains(other);
	}

	public bool IsLegalPosition(Vector3 pos) {
		return pos.x >= 0 && pos.x < Width
			&& pos.y <= 0 && pos.y > -Height;
	}

	private IEnumerable<Unit> AllUnits() {
		var units = new List<Unit> ();
		foreach (Transform child in unitsContainer) {
			units.Add (child.GetComponent<Unit> ());
		}
		return units;
	}

	public IEnumerable<Unit> UnitsOfFaction(Faction faction) {
		return from unit in AllUnits ()
		       where unit.Faction == faction
		       select unit;
	}

	public Unit UnitAtPosition(Vector3 pos) {
		return AllUnits ().FirstOrDefault (unit => unit.transform.localPosition == pos);
	}

	public void SelectUnit(Unit unit) {
		if (unit.Faction != currentFaction || unit.Dirty) {
			return;
		}

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

	public void MoveUnit(Unit unit, Vector3 pos, Action then) {
		var path = pathFinder.Path (selectedUnit.transform.localPosition, pos, this.MoveableNeighbors);
		MoveUnitAlongPath (unit, path, then);
	}

	public void MoveUnitAlongPath(Unit unit, IEnumerable<Vector3> path, Action then) {
		ClearLegalMovesOverlay ();
		gameState = GameState.MOVING;
		StartCoroutine (MoveUnitInSequence (unit, path, then));
	}

	private IEnumerator MoveUnitInSequence(Unit unit, IEnumerable<Vector3> path, Action then) {
		// FIXME: Do some kind of movement animation here
		// https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
		foreach (var step in path) {
			unit.transform.localPosition = step;
			yield return new WaitForSeconds(0.2f);
		}

		then.Invoke ();
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
			UnityEngine.Object.Destroy (child.gameObject);
		}
	}

	public IEnumerable<Unit> EnemiesInRange(Unit unit) {
		return (from cell in LegalStationaryAttackMoves (unit)
		  let other = UnitAtPosition (cell)
		  where other != null && !BattleController.Instance.FriendsWith (other.Faction, unit.Faction)
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

	public IEnumerable<Vector3> AllNeighbors(Vector3 pos) {
		return Neighbors (pos, (current, neighbors) => true);
	}

	public IEnumerable<Vector3> AttackableNeighbors(Vector3 pos) {
		return Neighbors (pos, (current, neighbor) => {
			var other = UnitAtPosition (neighbor);
			return other == null || !BattleController.Instance.FriendsWith (selectedUnit.Faction, other.Faction);
		});
	}

	public IEnumerable<Vector3> MoveableNeighbors(Vector3 pos) {
		return Neighbors (pos, (current, neighbor) => UnitAtPosition (neighbor) == null);
	}

	private struct UnitDistance {
		public Unit unit;
		public int distance;
	}

	public Unit NearestEnemyTo(Unit unit) {
		var distances = pathFinder.DistancesToNode (unit.transform.localPosition, this.AllNeighbors);

		return UnitsOfFaction (NextFaction ())
			.Select ((u, i) => new UnitDistance {
				unit = u,
				distance = distances [u.transform.localPosition]
			})
			.OrderBy(ud => ud.distance)
			.Select(ud => ud.unit)
			.FirstOrDefault();
	}
}
