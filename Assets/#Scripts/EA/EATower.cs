using UnityEngine;
using System.Collections;
using System;

public class EATower : MonoBehaviour {

#region Variables

	//These variables will be set after each round so it will correspond to the correct
	//tower. Now that I don't use projectiles I will have to be tower specific.

	//Publics
	public float radius = 2.5f;
	public float bombRadius = 0.75f;
	public float damage = 0;
	public float dotDamage;
	public float slow;
	public LayerMask targetLayer;
	public bool aimFrontEnemy;

	//Privates
	private delegate void ShootingMethod();
	private ShootingMethod shootingMethod;

	private Transform thisTransform;
	private EAEnemy curTarget;
	private bool canShoot = true;
	private AttackType attackType;

#endregion

	void Awake()
	{
		thisTransform = transform;

		gameObject.SetActive(false);
	}

	public void Initialize(TowerType tp) 
	{
		switch (tp) {
		case TowerType.Arrow:
			shootingMethod = ArrowShooting;
			attackType = AttackType.Piercing;
			break;
		case TowerType.Bomb:
			shootingMethod = BombShooting;
			attackType = AttackType.Siege;
			break;
		case TowerType.Frost:
			shootingMethod = SlowShooting;
			attackType = AttackType.Magic;
			break;
		case TowerType.Poison:
			shootingMethod = PoisonShooting;
			attackType = AttackType.Normal;
			break;
		default:
			break;
		}
	}

	void Update()
	{
		Targeting();
		
		if(canShoot && curTarget) {
			shootingMethod();
		}
	}
	
	void Targeting()
	{
		//Get all colliders within the radius, get all enemy scripts into an array and sort it comparing TravelTime
		Collider2D[] hits = Physics2D.OverlapCircleAll(thisTransform.position.To2DVector(), radius, targetLayer);
		if(hits.Length > 0) {
			bool findNewTarget = true;
			
			//The frost tower will always need to aim front target so I skip this test
			if(!aimFrontEnemy && curTarget) {
				//Check if current target still within radius, if so, keep that as target
				for(int i = 0; i < hits.Length; i++) {
					if(hits[i].gameObject == curTarget.gameObject) {
						findNewTarget = false;
						break;
					}
				}
			}
			
			if(findNewTarget) {
				EAEnemy[] hitsObjects = Array.ConvertAll(hits, item => item.gameObject.GetComponent<EAEnemy>());
				//Sort it so highest value comes first
				Array.Sort(hitsObjects, delegate (EAEnemy enemy1, EAEnemy enemy2){
					return enemy2.TravelTime.CompareTo(enemy1.TravelTime);
				});
				curTarget = hitsObjects[0];
			}
		} else {
			curTarget = null;
		}
	}

	//In here are the shooter methods that are added to a delegate type
	//this is because I have no projectiles so I will have to have all
	//functionality in the same script.
#region Shooter methods

	void ArrowShooting()
	{
		curTarget.TakeDamage(damage, attackType);
	}

	void PoisonShooting()
	{
		curTarget.TakeDamage(damage, attackType);
		curTarget.ApplyDoT(dotDamage);
	}

	void SlowShooting()
	{
		curTarget.TakeDamage(damage, attackType);
		curTarget.ApplySlow(slow);
		StartCoroutine(FireRateCoolDown());
	}

	void BombShooting()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(curTarget.transform.position.To2DVector(), bombRadius, targetLayer);
		
		if(hits.Length > 0) {
			EAEnemy[] enemies = Array.ConvertAll(hits, item => item.gameObject.GetComponent<EAEnemy>());
			
			foreach(EAEnemy e in enemies) {
				e.TakeDamage(damage, attackType);
			}
		} 

		StartCoroutine(FireRateCoolDown());
	}

	//This one is needed in Frost and Bomb shooting since it gives a more precise damage output.
	//Probably because of the fire rate cool down is different on the towers.
	//Setting it to null is enough for skipping one update.
	IEnumerator FireRateCoolDown()
	{
		canShoot = false;
		yield return null;
		canShoot = true;
	}

#endregion

}
