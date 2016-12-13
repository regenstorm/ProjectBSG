using System;

/**
 * An object to execute the attack between 2 units. We need 
 * this class to handle the time synchronization issue: only 
 * do something after showing all the attacking and receiving 
 * damage animation.
 */
public class AttackExecutor
{
	private Unit attacker;
	private Unit receiver;
	private Action onDone;

	public AttackExecutor (Unit attacker, Unit receiver, Action then)
	{
		this.attacker = attacker;
		this.receiver = receiver;
		this.onDone = then;
	}

	public void Execute() 
	{
		int damage = (int) ((attacker.Attack - receiver.Defense) * UnityEngine.Random.Range (0.5f, 1.5f));
		attacker.PlayAttackAnimation (onShotFired: () => this.DoDamage(damage));
	}

	private void DoDamage(int damage) {
		receiver.TakeDamage (
			damage: damage, 
			onReceivingAnimationDone: () => {
				if (receiver.Health > 0 && this.onDone != null) {
					this.onDone();
				}

				// else: the onDyingAnimationDone callback will be called instead
			}, 
			onDyingAnimationDone: () => {
				UnityEngine.Debug.Log("dying animation done");
				this.onDone();
			});
	}
}

