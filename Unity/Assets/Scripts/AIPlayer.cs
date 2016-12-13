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

	public IEnumerator DoTurn(IEnumerable<Unit> units) {
		foreach (Unit u in units) {
			yield return new WaitForSeconds(3);
			ControlUnit (u);
		}
	}

	public void ControlUnit(Unit unit) {
		map.SelectUnit (unit);

		var attackMade = AttackWeakestEnemyInRangeOf (unit);
		if (attackMade) {
			return;
		}

		var nearestEnemy = map.NearestEnemyTo (unit);
		if (nearestEnemy) {
			// find the nearest node on the path leading to this enemy and move to that node
			var path = pathFinder.BestPathTowards(
				unit.transform.localPosition, 
				nearestEnemy.transform.localPosition,
				map.AttackableNeighbors,
				unit.MoveRange - 1
			);

			map.MoveUnitAlongPath (unit, path, then: () => this.AttackWeakestEnemyInRangeOf(unit));
		}
	}

	/**
	 * Return whether an attack has been made.
	 */
	private bool AttackWeakestEnemyInRangeOf(Unit unit) {
		var inRange = map.EnemiesInRange (unit);

		if (inRange.Count () > 0) {
			var weakest = findWeakestUnit (inRange);
			new AttackExecutor (
				attacker: unit, 
				receiver: weakest, 
				then: () => map.EndSelectedUnitTurn ()
			).Execute();
			return true;
		}

		return false;
	}

	private Unit findWeakestUnit(IEnumerable<Unit> units) {
		return units.OrderBy (u => u.Health).FirstOrDefault();
	}
}

