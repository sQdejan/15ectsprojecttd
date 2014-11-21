using UnityEngine;
using System.Collections;
using System;

public class EATower : MonoBehaviour {

	public float fireRateCoolDown = 0.5f;
	public float radius = 2.5f;
	public float damage = 0;
	public LayerMask targetLayer;
	public bool aimFrontEnemy;

	private Transform thisTransform;
	private EAEnemy curTarget;
	private bool canShoot = true;

	void Awake()
	{
		thisTransform = transform;
	}

	void Update()
	{
//				Debug.DrawLine(thisTransform.position, thisTransform.position + Vector3.up * radius);
//				Debug.DrawLine(thisTransform.position, thisTransform.position - Vector3.up * radius);
//				Debug.DrawLine(thisTransform.position, thisTransform.position + Vector3.right * radius);
//				Debug.DrawLine(thisTransform.position, thisTransform.position - Vector3.right * radius);
		
//		if(!available) {
			Targeting();
			
			if(canShoot && curTarget) {
				Shooting();
			}
//		}
	}
	
	void Targeting()
	{
		//Get all colliders within the radius, get all enemy scripts into an array and sort it comparing TravelTime
		Collider2D[] hits = Physics2D.OverlapCircleAll(thisTransform.position.To2DVector(), radius, targetLayer);
		if(hits.Length > 0) {
			bool findNewTarget = true;
			
			//The frost tower will always need to aim front target so I skip this test
			if(!aimFrontEnemy) {
				//Check if current target still within radius, if so, keep that as target
				for(int i = 0; i < hits.Length; i++) {
					if(hits[i].gameObject == curTarget) {
						findNewTarget = false;
						break;
					}
				}
			}
			
			if(findNewTarget) {
				EAEnemy[] hitsObjects = Array.ConvertAll(hits, item => item.gameObject.GetComponent<EAEnemy>());
				//Sort it so highest value comes first
//				Array.Sort(hitsObjects, delegate (EAEnemy enemy1, EAEnemy enemy2){
//					return enemy2.TravelTime.CompareTo(enemy1.TravelTime);
//				});
				curTarget = hitsObjects[0];
			}
		} else {
			curTarget = null;
		}
	}
	
	void Shooting()
	{
//		for(int i = 0; i < projectiles.Count; i++) {
//			if(projectiles[i].available) {
//				projectiles[i].Activate(thisTransform.position, curTarget.transform, damage);
//				break;
//			}
//		}
		curTarget.TakeDamage(10, AttackType.MagicAttack);
		
		StartCoroutine(FireRateCoolDown());
	}

	IEnumerator FireRateCoolDown()
	{
		canShoot = false;
		yield return new WaitForSeconds(fireRateCoolDown /* 12.5f*/);
		canShoot = true;
	}

}
