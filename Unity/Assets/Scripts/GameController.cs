using System;

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

	public enum Faction {
		HUMAN,
		SYNTH
	}

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
}