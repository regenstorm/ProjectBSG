using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Unit : MonoBehaviour {
	public int MoveRange;
	public int AttackRange;
	public int Attack;
	public int Defense;
	public int Health;
	public int MaxHealth;
	public Faction Faction;
	public bool Dirty = false;

	Transform shipSprite;
	Text healthIndicator;
	Color defaultTint;

//	Map map;

	// Use this for initialization
	void Start () {
		Transform healthText;
		shipSprite = transform.FindChild ("ship_sprite");
		healthText = transform.Find ("Canvas/Health Indicator");
		healthIndicator = healthText.GetComponent <Text> ();
		healthIndicator.text = Health.ToString();

//		map = GameObject.Find ("BattleGround").GetComponent<Map> ();
		defaultTint = Map.FactionColor (Faction);
		ResetTint ();
	}

	// Update is called once per frame
	void Update () {
	}

	public void OnSelected() {
		shipSprite.GetComponent<Animator> ().SetTrigger ("selected");
	}

	public void OnDeselected() {
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

		shipSprite.GetComponent<Animator> ().SetTrigger ("receiveDamage");
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
		ResetTint ();
	}

	private void ResetTint() {
		shipSprite.GetComponent<SpriteRenderer> ().color = defaultTint;
	}
}
