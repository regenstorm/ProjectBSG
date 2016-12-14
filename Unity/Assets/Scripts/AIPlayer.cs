using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class AIPlayer
{
	private Map map;
	private PathFinder<Vector3> pathFinder;

	public AIPlayer (Map map, PathFinder<Vector3> pathFinder)
	{
		this.map = map;
		this.pathFinder = pathFinder;
	}

	// General algorithm
	//
	// for each unit we control:
	//   if there are enemy units in attack range, choose one with lowest health and attack it
	//   else choose the closest enemy and move as close as possible to it

	public void DoTurn(IEnumerable<Unit> units) {
		ControlUnit (units.GetEnumerator ());

//		foreach (Unit u in units) {
//			yield return new WaitForSeconds(0.4f);
//			ControlUnit (u);
//		}
	}

	public void ControlUnit(IEnumerator<Unit> iterator) {
		if (!iterator.MoveNext ()) {
			return;
		}

		var unit = iterator.Current;
//		UnityEngine.Debug.Log (unit);
		map.SelectUnit (unit);

		AttackWeakestEnemyInRangeOf (unit, then: (attackMade) => {
			if (attackMade) {
				ControlNextUnit(iterator);
				return;
			}

			var nearestEnemy = map.NearestEnemyTo (unit);
			if (nearestEnemy) {
				// find the nearest node on the path leading to this enemy and move to that node
				var path = pathFinder.Path(
					unit.transform.localPosition, 
					nearestEnemy.transform.localPosition,
					map.AttackableNeighbors
				);

				if (path.Count() <= unit.MoveRange) {
					path = path.Take(path.Count() - 1);
				} else {
					path = path.Take(unit.MoveRange - 1);
				}
				
				map.MoveUnitAlongPath (
					unit,
					path,
					then: () => this.AttackWeakestEnemyInRangeOf(unit, then: (_) => {
						ControlNextUnit(iterator);
					}));
			}
		});
	}

	private void ControlNextUnit(IEnumerator<Unit> iterator) {
//		UnityEngine.Debug.Log("ending");
		map.EndSelectedUnitTurn ();
		ControlUnit (iterator);
	}

	/**
	 * Return whether an attack has been made.
	 */
	private void AttackWeakestEnemyInRangeOf(Unit unit, Action<Boolean> then) {
		var inRange = map.EnemiesInRange (unit);

		if (inRange.Count () > 0) {
			var weakest = findWeakestUnit (inRange);
			new AttackExecutor (
				attacker: unit, 
				receiver: weakest, 
				then: () => {
					then (true);
				}
			).Execute ();
		} else {
			then (false);
		}
	}

	private Unit findWeakestUnit(IEnumerable<Unit> units) {
		return units.OrderBy (u => u.Health).FirstOrDefault();
	}
}

