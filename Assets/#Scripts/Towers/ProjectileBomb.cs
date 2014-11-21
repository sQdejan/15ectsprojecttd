﻿using UnityEngine;
using System.Collections;
using System;

public class ProjectileBomb : Projectile {

	public float radius = 1f;

	private Vector3 targetPos;
	private LayerMask targetLayer;

	public override void Activate (Vector3 position, Transform target, float damage)
	{
		targetLayer = 1 << LayerMask.NameToLayer("Enemy");
		base.Activate (position, target, damage);
		targetPos = target.position;
	}

	//Overriding the Update function in the parent class, this one not seeking the enemy but a location.
	void Update()
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
		thisTransform.position += direction * travelSpeed * Time.deltaTime;

		float curDistance = Vector3.Distance(targetPos, thisTransform.position);

		if(curDistance < lastDistance) {
			lastDistance = curDistance;
		} else {
			Explode();
		}
	}

	void Explode()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(thisTransform.position.To2DVector(), radius, targetLayer);

		if(hits.Length > 0) {
			Enemy[] enemies = Array.ConvertAll(hits, item => item.gameObject.GetComponent<Enemy>());

			foreach(Enemy e in enemies) {
				e.TakeDamage(damage, attackType);
			}
		} 

		Reset();
	}
}
