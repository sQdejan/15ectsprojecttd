using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Tower : MonoBehaviour { 

#region Variables

	//Publics
	public float fireRateCoolDown = 0.5f;
	public GameObject projectilePool;
	public float radius = 2.5f;
    public float damage = 0;
	public float cost = 50;
	public bool aimFrontEnemy = false;

	[HideInInspector]
	public bool available = true; //If the tower is free to be build
	[HideInInspector]
	public int level = 1;
	[HideInInspector]
	public float curNetWorth = 0;
	[HideInInspector]
	public GameObject tileReplaced; //The tile that the tower replaced. Needs to be activated if tower is sold.

	//Privates
	private Transform thisTransform;
	private Vector3 startPosition;
	private List<Projectile> projectiles = new List<Projectile>();
	private bool canShoot = true;

	private float startCost;
	private float startDamage;

	private GameObject curTarget;
	private LayerMask targetLayer;

#endregion

	void Awake()
	{
		startCost = cost;
		startDamage = damage;

		foreach(Transform t in projectilePool.transform) {
			projectiles.Add(t.GetComponent<Projectile>());
		}

		thisTransform = transform;
		startPosition = transform.position;
		targetLayer = 1 << LayerMask.NameToLayer("Enemy");
	}

	void Update()
	{
//		Debug.DrawLine(thisTransform.position, thisTransform.position + Vector3.up * radius);
//		Debug.DrawLine(thisTransform.position, thisTransform.position - Vector3.up * radius);
//		Debug.DrawLine(thisTransform.position, thisTransform.position + Vector3.right * radius);
//		Debug.DrawLine(thisTransform.position, thisTransform.position - Vector3.right * radius);

		if(!available) {
			Targeting();

			if(canShoot && curTarget) {
				Shooting();
			}
		}
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
				Enemy[] hitsObjects = Array.ConvertAll(hits, item => item.gameObject.GetComponent<Enemy>());
				//Sort it so highest value comes first
				Array.Sort(hitsObjects, delegate (Enemy enemy1, Enemy enemy2){
					return enemy2.TravelTime.CompareTo(enemy1.TravelTime);
				});
				curTarget = hitsObjects[0].gameObject;
			}

			Vector3 dir = curTarget.transform.position - thisTransform.position;
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			thisTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		} else {
			curTarget = null;
		}
	}

	void Shooting()
	{
		for(int i = 0; i < projectiles.Count; i++) {
			if(projectiles[i].available) {
				projectiles[i].Activate(thisTransform.position, curTarget.transform, damage);
				break;
			}
		}

		StartCoroutine(FireRateCoolDown());
	}

	IEnumerator FireRateCoolDown()
	{
		canShoot = false;
		yield return new WaitForSeconds(fireRateCoolDown);
		canShoot = true;
	}

	public void ResetTower()
	{
		available = true;
		canShoot = true;
		thisTransform.position = startPosition;
		cost = startCost;
		damage = startDamage;
		level = 1;
		curNetWorth = 0;
		RangeIndicator.selected = false;
		tileReplaced.SetActive(true);
		gameObject.SetActive(false);
	}
}
