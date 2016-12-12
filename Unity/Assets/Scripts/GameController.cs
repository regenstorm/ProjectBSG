using System;
using System.Collections.Generic;

public enum Faction {
	HUMAN,
	SYNTH
}

// Simple finite state machine abstraction
public enum GameState {
	IDLE,
	MOVE_TILE_SELECTION,
	MOVING,
	ATTACK_TILE_SELECTION,
	ATTACKING,
	UNIT_COMMAND_SELECTION,
};

public enum GameEvent {
	CANCEL,
	UNIT_STAY,
	DONE_ATTACKING,
	DONE_MOVING,
	UNIT_COMMAND_ATTACK,
	UNIT_COMMAND_WAIT,
	UNIT_MOVE,
	UNIT_ATTACK,
	UNIT_SELECT,
};

public class GameController
{
	private static GameController _instance;
	public static GameController Instance {
		get {
			if (_instance == null) {
				_instance = new GameController ();

			}
			return _instance;
		}

		private set { }
	}

	private GameState currentState = GameState.IDLE;
	private Faction currentFaction = Faction.SYNTH;

	public GameController ()
	{
	}

	public bool FriendsWith(Faction a, Faction b) {
		return a == b;
	}

	Faction NextFaction() {
		return currentFaction == Faction.SYNTH ? Faction.HUMAN : Faction.SYNTH;
	}

//	public GameState HandleEvent(GameEvent e) {
//		var nextState = currentState;
//
//
//		// C# syntax is verbose af : (
//		if (currentState == GameState.IDLE) {
//			if (e == GameEvent.UNIT_SELECT) {
//				nextState = GameState.MOVE_TILE_SELECTION;
//			}
//		}
//
//		if (currentState == GameState.MOVE_TILE_SELECTION) {
//			if (e == GameEvent.UNIT_MOVE) {
//				nextState = GameState.MOVING;
//			}
//			if (e == GameEvent.CANCEL) {
//				nextState = GameState.IDLE;
//			}
//		}
//
//		if (currentState == GameState.MOVING) {
//			if (e == GameEvent.DONE_MOVING) {
//				nextState = GameState.UNIT_COMMAND_SELECTION;
//			}
//		}
//
//		if (currentState == GameState.UNIT_COMMAND_SELECTION) {
//			if (e == GameEvent.UNIT_COMMAND_ATTACK) {
//				nextState = GameState.ATTACK_TILE_SELECTION;
//			}
//			if (e == GameEvent.CANCEL) {
//				nextState = GameState.IDLE;
//			}
//		}
//
//		if (currentState == GameState.ATTACK_TILE_SELECTION) {
//			if (e == GameEvent.UNIT_ATTACK) {
//				nextState = GameState.ATTACKING;
//			}
//			if (e == GameEvent.CANCEL) {
//				nextState = GameState.IDLE;
//			}
//		}
//
//		return nextState;
//	}
}