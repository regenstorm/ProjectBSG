﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Map : MonoBehaviour {
	public int ScalingFactor = 2;
	public int Width;
	public int Height;
	public GameObject MoveOverlayPrefab;
	public GameObject gridCellPrefab;

	private Transform unitsContainer;
	private Transform overlayContainer;
	private Transform gridContainer;
	private Unit selectedUnit;

	void Start () {
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
			MoveUnit (selectedUnit, newPos);

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
			StartCoroutine (MoveUnitInSequence(unit, Path (selectedUnit, pos)));
		}
	}

	private IEnumerator MoveUnitInSequence(Unit unit, IEnumerable<Vector3> path) {
		foreach (var step in path) {
			unit.transform.localPosition = step;
			yield return new WaitForSeconds(0.2f);
		}
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

	public IEnumerable<Vector3> LegalMoves(Unit unit) {
		var distances = BFS<int>(
			unit.transform.localPosition,
			0,
			_ => false,
			(current, neighbor, acc) => {
				acc[neighbor] = 1 + acc[current];
			});

		return from pair in distances
		       where pair.Value <= unit.MoveRange && pair.Value > 0
		       select pair.Key;
	}

	public IEnumerable<Vector3> Path(Unit unit, Vector3 pos) {
		var comeFrom = BFS<Vector3> (
			unit.transform.localPosition,
			unit.transform.localPosition,
			current => current.Equals(pos),
			(current, neighbor, acc) => {
				acc[neighbor] = current;
			});

		List<Vector3> path = new List<Vector3> ();

		// backtrack the dictionary to build the path
		var c = pos;
		path.Add (c);
		while (true) {
			var parent = comeFrom [c];
			if (parent.Equals (unit.transform.localPosition)) {
				break;
			} else {
				path.Insert (0, parent);
				c = parent;
			}
		}

		print (string.Join(",", path.Select(v => v.ToString()).ToArray()));

		return path;
	}

	///////////////////
	//
	// Pathfinding stuffs. Should they be in a new class?
	// http://www.redblobgames.com/pathfinding/tower-defense/

	public delegate void NeighborHandler<T>(Vector3 current, Vector3 neighbor, Dictionary<Vector3, T> container);
	public delegate bool StoppingPredicate(Vector3 current);

	public Dictionary<Vector3, T> BFS<T>(Vector3 start, T initialValue, StoppingPredicate stoppingPredicate, NeighborHandler<T> neighborHandler) {
		Queue<Vector3> frontier = new Queue<Vector3> ();
		var accumulator = new Dictionary<Vector3, T> ();

		frontier.Enqueue (start);
		accumulator[start] = initialValue;

		while (frontier.Count > 0) {
			var current = frontier.Dequeue ();

			if (stoppingPredicate (current)) {
				break;
			}

			foreach (var neighbor in neighborPositions(current)) {
				if (!accumulator.ContainsKey(neighbor)) {
					frontier.Enqueue (neighbor);
					neighborHandler(current, neighbor, accumulator);
				}
			}
		}

		return accumulator;
	}

	public bool UnitCanMoveToPosition(Unit unit, Vector3 pos) {
		// FIXME: slow, requires recalculating legal moves list every time. Caching every turn?
		return LegalMoves (unit).Contains(pos);
	}

	IEnumerable<Vector3> neighborPositions(Vector3 pos) {
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				var neighbor = new Vector3 {
					x = i * ScalingFactor,
					y = j * -ScalingFactor
				} + pos;

				if (i != j && i != -j && IsLegalPosition(neighbor)) {
					yield return neighbor;
				}
			}
		}
	}
}
