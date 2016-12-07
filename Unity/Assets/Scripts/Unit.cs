using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
	public int MoveRange = 2;
	public int AttackRange = 1;
	public int Attack = 20;
	public int Defense = 10;
	public int Health = 50;
	public int MaxHealth = 50;
	public Faction Faction = Faction.SYNTH;

	Transform shipSprite;
//	Map map;

	// Use this for initialization
	void Start () {
		shipSprite = transform.FindChild ("ship_sprite");
//		map = GameObject.Find ("BattleGround").GetComponent<Map> ();
	}

	// Update is called once per frame
	void Update () {
	}

	public void OnSelected() {
		shipSprite.Translate (new Vector3(0, 0.1f));
	}

	public void OnDeselected() {
		shipSprite.Translate (new Vector3(0, -0.1f));
	}

	public void Fight(Unit other) {
		// FIXME: need a way to communicate battle outcome with the GameController (to display stats, messages, etc.)
		var damage = Mathf.RoundToInt((Attack - other.Defense) * Random.Range (0.5f, 1.5f));
		other.Health -= damage;

		if (other.Health <= 0) {
			Object.Destroy (other);
		}
	}

	public bool IsFriendlyWith(Unit other) {
		return Faction == other.Faction;
	}
}
