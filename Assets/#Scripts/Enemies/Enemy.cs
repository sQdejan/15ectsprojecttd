using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ArmorType {LightArmor, MediumArmor, HeavyArmor};

public class Enemy : MonoBehaviour {

#region Variables

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
	private Vector3 startPosition;

	private List<Transform> waypointsRight = new List<Transform>();
	private List<Transform> waypointsLeft = new List<Transform>();
	private List<Transform> waypointsCurrent;
	private int curWaypointIndex = 0;
	private Vector3 direction;

	private float lastDistance = float.MaxValue;
	private float travelTime = 0; //This is used in to figure out which enemy to target in Tower.cs (highest value is targeted)

	private int level = 1;
	private float curStartHealth;
	private float curBounty = 1;
	private float bountyIncreaser = 1;

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
		curStartHealth = health;

		foreach(Transform t in waypointsPoolRight) {
			waypointsRight.Add(t);
		}

		foreach(Transform t in waypointsPoolLeft) {
			waypointsLeft.Add(t);
		}

		thisTransform = transform;
		startPosition = transform.position;
		startMoveSpeed = moveSpeed;

		gameObject.SetActive(false);

	}

#region Move Related

	void Update()
	{
		Move();
		travelTime += moveSpeed;
	}

	void Move()
	{
		//If distance is higher it means that the enemy has walked past the waypoint and has to find next
		float curDistance = Vector3.Distance(thisTransform.position, waypointsCurrent[curWaypointIndex].position);

		if(curDistance > lastDistance) {

			lastDistance = float.MaxValue;

			if(++curWaypointIndex >= waypointsCurrent.Count) {
				Terminate();
				if(--InteractionHandler.lifesRemaining <= 0) {
					InteractionHandler.gameOver = true;
					InteractionHandler.dGameOver(); //Calling delegate methods for end game
				}
				return;
			}

			WalkDirection();
		} else {
			lastDistance = curDistance;
		}

		thisTransform.position += direction * moveSpeed * Time.deltaTime;
	}

	//Calculate in what direction to move at
	void WalkDirection()
	{
		Vector3 dir = waypointsCurrent[curWaypointIndex].position - thisTransform.position;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		thisTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		direction = (waypointsCurrent[curWaypointIndex].position - thisTransform.position).normalized;
	}

#endregion

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
	
	//When enemy is dead or reached goal reset the stats
	void Terminate()
	{
		WaveHandler.enemiesDone++; //For updating when the last enemy has died == wave over

		travelTime = 0;
		curWaypointIndex = 0;
		lastDistance = float.MaxValue;
		moveSpeed = startMoveSpeed;
		thisTransform.position = startPosition;

		StopCoroutine("SlowRoutine");
		StopCoroutine("DoTRoutine");

		gameObject.SetActive(false);
	}

	void Bounty()
	{
		InteractionHandler.curGold += curBounty;
	}

	void UpdateHealth()
	{
		curStartHealth += healthIncreaser;
		//For every 4th lvl
		if(level % 4 == 0) {
			healthIncreaser *= 1.5f;
		} 
		health = curStartHealth;
	}

	//Will be called from WaveHandler.cs after each wave has finished
	public void LevelUp()
	{
		level++;

		if(level % 10 == 0) {
			bountyIncreaser *= 1.5f;
		}
		curBounty += bountyIncreaser;

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

#region Attack/Damage Related

	//If this enemy is the target then I send the object over to the projectile 
	//that handles the different kind of damage type and modifiers
//	void OnTriggerEnter2D(Collider2D col) {
//		
//		Projectile projectile = col.GetComponent<Projectile>();
//		
//		if(projectile.Target == thisTransform){
//			projectile.DamageType(this);
//			projectile.Reset();
//		}
//	}

	
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
		damage *= 1f - ((armor * 0.06f) / (1f + armor * 0.06f));
		health -= damage;



		if(health <= 0) {
			Bounty();
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

	IEnumerator DoTRoutine(float dotDamage)
	{
		for(int i = 0; i < 5; i++) {
			dotDamage *= 1f - ((poisonResistance * 0.06f) / (1f + poisonResistance * 0.06f));
			health -= dotDamage;
			if(health <= 0) {
				Bounty();
				Terminate();
			}
			yield return new WaitForSeconds(1f);
		}
	}
	
	IEnumerator SlowRoutine(float slow)
	{
		moveSpeed *= slow * (1 - slowResistance);
		yield return new WaitForSeconds(5f);
		moveSpeed = startMoveSpeed;
	}

#endregion

}
