using UnityEngine;
using System.Collections;

public class ProjectileArrow : Projectile {

	public override void DamageType (Enemy enemy)
	{
		enemy.TakeDamage(damage, attackType);
	}
}
