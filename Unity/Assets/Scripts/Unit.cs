using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class Unit : MonoBehaviour {
	public int MoveRange;
	public int AttackRange;
	public int Attack;
	public int Defense;
	public int Health;
	public int MaxHealth;
	public Faction Faction;
	public bool Dirty = false;
	public AudioSource shootingSound;
	public AudioSource dyingExplosionSound;

	Transform shipSprite;
	Text healthIndicator;
	Color defaultTint;
	Animation anim;
	Transform selectionIndicator;
	Unit unitUnderAttack;

	Map map;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animation> ();

		Transform healthText;
		shipSprite = transform.FindChild ("ship_sprite");
		healthText = transform.Find ("Canvas/Health Indicator");
		healthIndicator = healthText.GetComponent <Text> ();
		healthIndicator.text = Health.ToString();

		map = GameObject.Find ("BattleGround").GetComponent<Map> ();
		defaultTint = Map.FactionColor (Faction);
		ResetTint ();

		selectionIndicator = transform.Find ("SelectionIndicator");

		map.RegisterUnit (this);
	}

	// Update is called once per frame
	void Update () {
	}

	public void OnSelected() {
		selectionIndicator.GetComponent<SpriteRenderer> ().enabled = true;
		anim.Play ("UnitSelection");
	}

	public void OnDeselected() {
		selectionIndicator.GetComponent<SpriteRenderer> ().enabled = false;
		anim.Stop ("UnitSelection");
	}

	public void Fight(Unit other) {
		// FIXME: need a way to communicate battle outcome with the GameController (to display stats, messages, etc.)
		anim.Play("UnitAttacking");
		shootingSound.Play ();
		unitUnderAttack = other;
	}

	public void TakeDamage (int enemyAttack){
		int damage = (int)((enemyAttack - this.Defense) * Random.Range (0.5f, 1.5f));
		if (damage > 0) {
			this.Health -= damage;
			if (this.Health <= 0) {
				map.DeregisterUnit (this);
			} 
			else {
				// FIXME: PLS animation
				healthIndicator.text = Health.ToString();
			}
		}

		anim.Play("UnitReceivingDamage");
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

	// Animation callbacks

	public void ReceivingDamageAnimationDone() {
		if (this.Health <= 0) {
			dyingExplosionSound.Play ();
			Object.Destroy (gameObject, t: dyingExplosionSound.clip.length);
		}
	}

	public void AnimationShotFired() {
		unitUnderAttack.TakeDamage(this.Attack);
		unitUnderAttack = null;
	}
}
