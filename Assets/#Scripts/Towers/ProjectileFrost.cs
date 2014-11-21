using UnityEngine;
using System.Collections;

public class ProjectileFrost : Projectile {

	public float slow;

	public override void DamageType (Enemy enemy)
	{
		enemy.TakeDamage(damage, attackType);
		enemy.ApplySlow(slow);
	}

}
