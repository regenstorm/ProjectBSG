using System;
using System.Collections.Generic;
using UnityEngine;


public class PathFinder<NodeType>
{
	// A function that returns the neighbors of any node in a graph, basically this function defines the graph.
	public delegate IEnumerable<NodeType> GraphNeighbors(NodeType node);
	private delegate void NeighborHandler<T>(NodeType current, NodeType neighbor, Dictionary<NodeType, T> container);
	private delegate bool StoppingPredicate(NodeType current);

	// http://www.redblobgames.com/pathfinding/tower-defense/
	private Dictionary<NodeType, T> BFS<T>(
		NodeType start,
		T initialValue,
		StoppingPredicate stoppingPredicate,
		GraphNeighbors graphNeighbors,
		NeighborHandler<T> neighborHandler) {
		Queue<NodeType> frontier = new Queue<NodeType> ();
		var accumulator = new Dictionary<NodeType, T> ();

		frontier.Enqueue (start);
		accumulator[start] = initialValue;

		while (frontier.Count > 0) {
			var current = frontier.Dequeue ();

			if (stoppingPredicate (current)) {
				break;
			}

			foreach (var neighbor in graphNeighbors(current)) {
				if (!accumulator.ContainsKey(neighbor)) {
					frontier.Enqueue (neighbor);
					neighborHandler(current, neighbor, accumulator);
				}
			}
		}

		return accumulator;
	}

	public IEnumerable<NodeType> Path(NodeType start, NodeType pos, GraphNeighbors graphNeighbors) {
		var comeFrom = BFS<NodeType> (
			start,
			start,
			current => current.Equals(pos),
			graphNeighbors,
			(current, neighbor, acc) => {
				acc[neighbor] = current;
			});

		var path = new List<NodeType> ();

		// backtrack the dictionary to build the path
		var c = pos;
		path.Add (c);
		while (true) {
			var parent = comeFrom [c];
			if (parent.Equals (start)) {
				break;
			} else {
				path.Insert (0, parent);
				c = parent;
			}
		}

		return path;
	}

	public Dictionary<NodeType, int> DistancesToNode(NodeType node, GraphNeighbors graph) {
		// FIXME: cache this every turn
		return BFS<int>(
			node,
			0,
			_ => false,
			graph,
			(current, neighbor, acc) => {
				acc[neighbor] = 1 + acc[current];
			});
	}
}

