using UnityEngine;
using System.Collections;
using System;

public class ProjectileBomb : Projectile {

	public float bombRadius = 1f;

	private Vector3 targetPos;
	private LayerMask targetLayer;

	public override void Activate (Vector3 position, Transform target, float damage, float dotDamage, float slow, float travelSpeed, AttackType attackType)
	{
		targetLayer = 1 << LayerMask.NameToLayer("Enemy");
		base.Activate (position, target, damage, dotDamage, slow, travelSpeed, attackType);
		targetPos = target.position;
	}

	//Overriding the Update function in the parent class, this one not seeking the enemy but a location.
	void FixedUpdate()
	{
//		Debug.DrawLine(thisTransform.position, thisTransform.position + Vector3.up * radius);
//		Debug.DrawLine(thisTransform.position, thisTransform.position - Vector3.up * radius);
//		Debug.DrawLine(thisTransform.position, thisTransform.position + Vector3.right * radius);
//		Debug.DrawLine(thisTransform.position, thisTransform.position - Vector3.right * radius);

		Vector3 direction = (targetPos - thisTransform.position).normalized;
		
		//Rotate towards target
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		thisTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		
		//Move towards target
		thisTransform.position += direction * travelSpeed * Time.fixedDeltaTime;

		float curDistance = Vector3.Distance(targetPos, thisTransform.position);

		if(curDistance < lastDistance) {
			lastDistance = curDistance;
		} else {
			Explode();
		}
	}

	void Explode()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(thisTransform.position.To2DVector(), bombRadius, targetLayer);

		if(hits.Length > 0) {
			Enemy[] enemies = Array.ConvertAll(hits, item => item.gameObject.GetComponent<Enemy>());

			foreach(Enemy e in enemies) {
				e.TakeDamage(damage, attackType);
			}
		} 

		Reset();
	}
}
