using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Unit : MonoBehaviour {
	public int MoveRange = 2;
	public int AttackRange = 1;
	public int Attack = 20;
	public int Defense = 10;
	public int Health = 50;
	public int MaxHealth = 50;
	public Faction Faction = Faction.SYNTH;
	public bool Dirty = false;

	Transform shipSprite;
	Text healthIndicator;
//	Map map;

	// Use this for initialization
	void Start () {
		Transform healthText;
		shipSprite = transform.FindChild ("ship_sprite");
		healthText = transform.Find ("Canvas/Health Indicator");
		healthIndicator = healthText.GetComponent <Text> ();
		healthIndicator.text = Health.ToString();

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
		other.TakeDamage(this.Attack);
	}

	public void TakeDamage (int enemyAttack){
		int damage = (int)((enemyAttack - this.Defense) * Random.Range (0.5f, 1.5f));
		if (damage > 0) {
			this.Health -= damage;
			if (this.Health <= 0) {
				Object.Destroy (gameObject);
			} 
			else {
				// FIXME: PLS animation
				healthIndicator.text = Health.ToString();
			}
		}
	}

	public bool IsFriendlyWith(Unit other) {
		return Faction == other.Faction;
	}

	public void EndTurn() {
		this.Dirty = true;
		var grayTint = new Color (0.5f, 0.5f, 0.5f);
		shipSprite.GetComponent<SpriteRenderer> ().color = grayTint;
	}

	public void ResetTurn() {
		this.Dirty = false;
		var noTint = new Color (1, 1, 1);
		shipSprite.GetComponent<SpriteRenderer> ().color = noTint;
	}
}
