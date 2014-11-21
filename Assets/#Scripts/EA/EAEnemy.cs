using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EAEnemy : MonoBehaviour {

	public float moveSpeed;
	public float timeInterval;
	public Transform waypointsPool;
	public ArmorType armorType;
	public float health = 100;

	private float time = 0;
	private Transform thisTransform;

	private List<Transform> waypoints = new List<Transform>();
	private int curWaypointIndex = 0;
	private Vector3 direction;
	private float startTime;

	void Start()
	{
		startTime = Time.realtimeSinceStartup;
//		Time.timeScale = 100;
		thisTransform = transform;

		foreach(Transform t in waypointsPool) {
			waypoints.Add(t);
		}

		thisTransform.position = waypoints[curWaypointIndex++].position;
		WalkDirection();
	}

	// Update is called once per frame
	void Update () {
//		Debug.Log(startTime);
//		if(time < timeInterval) {
//			time += Time.deltaTime;
//		} else {
			if(Vector3.Distance(waypoints[curWaypointIndex].position, thisTransform.position) < moveSpeed / 2) {
				curWaypointIndex++;
				Debug.Log(curWaypointIndex + " " + (Time.realtimeSinceStartup - startTime));
				WalkDirection();
			}
//			time = 0;
			thisTransform.position += direction * moveSpeed;
//		}
	}

	void WalkDirection()
	{
		Vector3 dir = waypoints[curWaypointIndex].position - thisTransform.position;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		thisTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		direction = (waypoints[curWaypointIndex].position - thisTransform.position).normalized;
	}

	public void TakeDamage(float damage, AttackType at)
	{
		//Increase/Deacrease damage depending on armor type and attack type 
		switch (armorType) {
		case ArmorType.LightArmor:
			if(at == AttackType.PiercingAttack) {
				damage *= 1.75f;
			} else if (at == AttackType.MagicAttack) {
				damage *= 1.25f;
			} else if (at == AttackType.SiegeAttack) {
				damage *= 0.75f;
			}
			break;
			
		case ArmorType.MediumArmor:
			if(at == AttackType.NormalAttack) {
				damage *= 1.5f;
			} else if (at == AttackType.SiegeAttack) {
				damage *= 1.25f;
			} else if (at == AttackType.PiercingAttack) {
				damage *= 0.75f;
			} else if (at == AttackType.MagicAttack) {
				damage *= 0.75f;
			}
			break;
			
		case ArmorType.HeavyArmor:
			if(at == AttackType.MagicAttack) {
				damage *= 1.75f;
			} else if (at == AttackType.NormalAttack) {
				damage *= 0.75f;
			} else if (at == AttackType.PiercingAttack) {
				damage *= 0.75f;
			} 
			break;
			
		default:
			break;
		}
		
		//Take armor into account
//		health -= damage * (1 - ((armor * 0.06f) / (1f + armor * 0.06f)));

		health -= damage;

//		if(health <= 0) {
//			Bounty();
//			Terminate();
//		}
	}
}
