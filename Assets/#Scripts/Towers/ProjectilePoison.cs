using UnityEngine;
using System.Collections;

public class ProjectilePoison : Projectile {

	public override void DamageType (Enemy enemy)
	{
		enemy.TakeDamage(damage, attackType);
		enemy.ApplyDoT(dotDamage);
	}
}
