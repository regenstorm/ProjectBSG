using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

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

	Map map;

	// very bad implementation of EventEmitter/observer pattern
	Action onShotFired;
	Action onDyingAnimationDone;
	Action onReceivingDamageAnimationDone;

	void Awake() {
		shipSprite = transform.FindChild ("ship_sprite");
		anim = GetComponent<Animation> ();
		
		Transform healthText;
		healthText = transform.Find ("Canvas/Health Indicator");
		healthIndicator = healthText.GetComponent <Text> ();
		healthIndicator.text = Health.ToString();

		selectionIndicator = transform.Find ("SelectionIndicator");
	}

	// Use this for initialization
	void Start () {
		map = GameObject.Find ("BattleGround").GetComponent<Map> ();

		defaultTint = Map.FactionColor (Faction);
		ResetTint ();
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

	public void PlayAttackAnimation(Action onShotFired) {
		// FIXME: need a way to communicate battle outcome with the GameController (to display stats, messages, etc.)
		anim.Play("UnitAttacking");
		shootingSound.Play ();
		this.onShotFired = onShotFired;
	}

	public void PlayReceivingDamageAnimation(Action onAnimationDone) {
		anim.Play("UnitReceivingDamage");
	}

	public void TakeDamage (int damage, Action onReceivingAnimationDone, Action onDyingAnimationDone) {
		this.onReceivingDamageAnimationDone = onReceivingAnimationDone;
		this.onDyingAnimationDone = onDyingAnimationDone;

		if (damage > 0) {
			this.Health -= damage;
			if (this.Faction == Faction.HUMAN) {
				BattleController.Instance.synthsDamageDealt += damage;
			} else {
				BattleController.Instance.humansDamageDealt += damage;
			}
			if (this.Health <= 0) {
				map.DeregisterUnit (this);
			} 
			else {
				// FIXME: PLS animation
				healthIndicator.text = Health.ToString();
			}
		}

		anim.Play ("UnitReceivingDamage");
	}

	public bool IsFriendlyWith(Unit other) {
		return Faction == other.Faction;
	}

	public void EndTurn() {
		this.Dirty = true;
		shipSprite.GetComponent<SpriteRenderer> ().color -= new Color(0.15f,0.15f,0.15f,0f);
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
		if (onReceivingDamageAnimationDone != null)
			onReceivingDamageAnimationDone();
		
		if (this.Health <= 0) {
			if (this.Faction == Faction.HUMAN) {
				BattleController.Instance.humansDestroyed += 1;
			} else {
				BattleController.Instance.synthsDestroyed += 1;
			}
			dyingExplosionSound.Play ();
			Invoke ("DieAnimationDone", time: dyingExplosionSound.clip.length);
		}
	}

	public void AnimationShotFired() {
		if (onShotFired != null)
			onShotFired();
	}

	public void DieAnimationDone() {
		UnityEngine.Object.Destroy (gameObject);

		if (onDyingAnimationDone != null)
			onDyingAnimationDone ();
	}
}
