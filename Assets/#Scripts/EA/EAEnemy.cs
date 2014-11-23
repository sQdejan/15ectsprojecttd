using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EAEnemy : MonoBehaviour {

#region variables

	//Publics
	public Transform waypointsPoolRight;
	public Transform waypointsPoolLeft;
	public float moveSpeed = 0.7f; 
	public float health = 0;
	public float healthIncreaser = 10;
	public float poisonResistance = 0;	
	public float slowResistance = 0;	
	public float armor = 0;				
	public ArmorType armorType;
	public float PRI; //Poison Resistance Increase
	public float SRI; //Slow
	public float ARI; //Armor
	public int PRILVL; //After what level should it increase
	public int SRILVL;
	public int ARILVL;

	[HideInInspector]
	public int waypointPoolToUse = 0; //0 for left, 1 for right

	//Privates
	private Transform thisTransform;
	private float startMoveSpeed;
	private float startHealth;

	private List<Transform> waypointsLeft = new List<Transform>();
	private List<Transform> waypointsRight = new List<Transform>();
	private List<Transform> waypointsCurrent;
	private int curWaypointIndex = 0;
	private Vector3 direction;

	private float travelTime = 0;
	private int level = 1;

#endregion

#region Accessors

	public float TravelTime
	{
		get {
			return travelTime;
		}
	}

#endregion

	void Awake()
	{
		thisTransform = transform;
		startMoveSpeed = moveSpeed;
		startHealth = health;

		foreach(Transform t in waypointsPoolLeft) { waypointsLeft.Add(t); }
		foreach(Transform t in waypointsPoolRight) { waypointsRight.Add(t); }

		gameObject.SetActive(false);
	}

	//What is special about this enemy is the way I move it compared to the "original" enemy.
	//I move it stepwise here not using time to smoothen the movement. This will give me more
	//correct movement when speeding up the process.
	void Update () {

		if(Vector3.Distance(waypointsCurrent[curWaypointIndex].position, thisTransform.position) < startMoveSpeed / 2) {
			if(++curWaypointIndex >= waypointsCurrent.Count) {
				Terminate();
				return;
			}
			WalkDirection();
		}

		thisTransform.position += direction * moveSpeed;
		travelTime += moveSpeed;
	}

	//Calculate in what direction to move at
	void WalkDirection()
	{
		Vector3 dir = waypointsCurrent[curWaypointIndex].position - thisTransform.position;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		thisTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		direction = (waypointsCurrent[curWaypointIndex].position - thisTransform.position).normalized;
	}

#region Miscellaneous

	public void Spawn()
	{
		//0 for left, 1 for right
		if(waypointPoolToUse == 0) {
			waypointsCurrent = waypointsLeft;
		} else {
			waypointsCurrent = waypointsRight;
		}
		
		gameObject.SetActive(true);
		thisTransform.position = waypointsCurrent[curWaypointIndex++].position;
		WalkDirection();
	}
	
	void Terminate()
	{
		EAWaveHandler.totalTravelTime += travelTime;

		travelTime = 0;
		curWaypointIndex = 0;
		health = startHealth;
		moveSpeed = startMoveSpeed;
		
		StopCoroutine("SlowRoutine");
		StopCoroutine("DoTRoutine");
		EAWaveHandler.enemiesDone++;

		gameObject.SetActive(false);
	}

	void UpdateHealth()
	{
		startHealth += healthIncreaser;
		//For every 4th lvl
		if(level % 4 == 0) {
			healthIncreaser *= 1.5f;
		} 

		health = startHealth;
	}
	
	//Will be called from WaveHandler.cs after each wave has finished
	public void LevelUp()
	{
		level++;
		
		if(level % PRILVL == 0) {
			poisonResistance += PRI;
		}
		
		if(level % SRILVL == 0) {
			slowResistance += SRI;
		}
		
		if(level % ARILVL == 0) {
			armor += ARI;
		}
		
		UpdateHealth();
	}

#endregion

#region Damage related

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
		damage *= 1 - ((armor * 0.06f) / (1f + armor * 0.06f));
		health -= damage;

//		Debug.Log("Damage is " + damage + " and total damage before is " + EAWaveHandler.totalDamageTaken);
		EAWaveHandler.totalDamageTaken += damage;

		if(health <= 0) {
//			Debug.Log("Total damage before " + EAWaveHandler.totalDamageTaken + " health " + health);
			EAWaveHandler.totalDamageTaken += health;
//			Debug.Log("Total damage after " + EAWaveHandler.totalDamageTaken);
			Terminate();
		}
	}

	public void ApplyDoT(float dotDamage)
	{
		if(gameObject.activeSelf){
			StopCoroutine("DoTRoutine");
			StartCoroutine("DoTRoutine", dotDamage);
		}
	}
	
	public void ApplySlow(float slow)
	{	
		if(gameObject.activeSelf) {
			StopCoroutine("SlowRoutine");
			moveSpeed = startMoveSpeed;
			StartCoroutine("SlowRoutine", slow);
		}
	}

	//I yield null because it will go fast and no pauses is needed - the null will skip a frame so it is needed
	IEnumerator DoTRoutine(float dotDamage)
	{
		for(int i = 0; i < 5; i++) {
			dotDamage *= 1f - ((poisonResistance * 0.06f) / (1f + poisonResistance * 0.06f));
			health -= dotDamage;
			EAWaveHandler.totalDamageTaken += dotDamage;
			if(health <= 0) {
				Terminate();
			}
			yield return null;
		}
	}

	//I yield null because it will go fast and no pauses is needed - the null will skip a frame so it is needed
	IEnumerator SlowRoutine(float slow)
	{
		moveSpeed *= slow * (1 - slowResistance);
		yield return null;
		moveSpeed = startMoveSpeed;
	}

#endregion
	
}
