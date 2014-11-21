using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyOLD : MonoBehaviour {
	
	#region Variables
	
	//Constants for minmax normalization
	const float MAX_HEALTH = 2000;
	const float MAX_POISON_RESISTANCE = 25;
	const float MAX_SLOW_RESISTANCE = 0.5f;
	const float MAX_ARMOR = 25;
	const float MAX_ARMOR_TYPE = 3;
	
	//Publics
	public Transform waypointsPool;
	public float health = 100;			//Max health 2000
	public float poisonResistance = 0;	//Max resistance 25
	public float slowResistance = 0;	//Max resistance 0.50
	public float armor = 0;				//Max armor 25
	public ArmorType armorType;			//Max armorType is 3 - Light = 1, Medium = 2, Heavy = 3
	
	//Privates
	private Transform thisTransform;
	private float originalHealth;
	private float originalMoveSpeed;
	private Vector3 originalPosition;
	
	private List<Transform> waypoints = new List<Transform>();
	private int curWaypointIndex = 0;
	private Vector3 direction;
	
	private bool died = false;
	
	private float lastDistance = float.MaxValue;
	private float travelTime = 0; //This is used in to figure out which enemy to target in Tower.cs (highest value is targeted)
	
	private float maxMoveSpeed = 1.2f;
	public float moveSpeed; //Current movespeed is calculated when spawned
	private float penaltyMoveSpeed = 0.6f; //This is the highest movespeed penalty, look at CalculateMoveSpeed().
	
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
		originalPosition = transform.position;
		
		foreach(Transform t in waypointsPool) {
			waypoints.Add(t);
		}
	}
	
	void Update()
	{
		Move();
		travelTime += moveSpeed;
	}
	
	void Move()
	{
		//If distance is higher it means that the enemy has walked past the waypoint and has to find next
		float curDistance = Vector3.Distance(thisTransform.position, waypoints[curWaypointIndex].position);
		
		if(curDistance > lastDistance) {
			
			lastDistance = float.MaxValue;
			
			if(++curWaypointIndex >= waypoints.Count) {
				ReachedGoal();
				return;
			}
			
			WalkDirection();
		} else {
			lastDistance = curDistance;
		}
		
		thisTransform.position += direction * moveSpeed * Time.deltaTime;
	}
	
	public void Spawn()
	{
		gameObject.SetActive(true);
		thisTransform.position = waypoints[curWaypointIndex++].position;
		
		CalculateMovementSpeed();
		
		originalHealth = health;
		originalMoveSpeed = moveSpeed;
		
		WalkDirection();
	}
	
	//For the EA not to just bump all up values I will have to have some sort of punishment for maxing the values
	void CalculateMovementSpeed() 
	{
		moveSpeed = maxMoveSpeed - penaltyMoveSpeed * ((NormalizeHealth() + NormalizeArmor() + NormalizeArmorType() + NormalizePoisonResistance() + NormalizeSlowResistance()) / 5);
	}
	
	void WalkDirection()
	{
		Vector3 dir = waypoints[curWaypointIndex].position - thisTransform.position;
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		thisTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		direction = (waypoints[curWaypointIndex].position - thisTransform.position).normalized;
	}
	
	void ReachedGoal()
	{
		Despawn();
	}
	
	void Dead()
	{
		died = true;
		Despawn();
	}
	
	//Reset the object for next wave
	void Despawn()
	{
		StopCoroutine("SlowRoutine");
		StopCoroutine("DoTRoutine");
		thisTransform.position = originalPosition;
		health = originalHealth;
		moveSpeed = originalMoveSpeed;
		curWaypointIndex = 0;
		lastDistance = float.MaxValue;
		EnemySpawner.enemiesDone++;
		gameObject.SetActive(false);
	}
	
	#region Attack/Damage Related
	
	//If this enemy is the target then I send the object over to the projectile 
	//that handles the different kind of damage type and modifiers
	void OnTriggerEnter2D(Collider2D col) {
		
		Projectile projectile = col.GetComponent<Projectile>();
		
		if(projectile.Target == thisTransform){
//			projectile.DamageType(this); <-- this = Enemy.cs
			projectile.Reset();
		}
	}
	
	
	public void TakeDamage(float damage, AttackType at)
	{
		//Increase/Deacrease damage depending on armor type and attack type 
		switch (armorType) {
		case ArmorType.LightArmor:
			if(at == AttackType.PiercingAttack) {
				damage *= 2f;
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
				damage *= 0.5f;
			}
			break;
			
		case ArmorType.HeavyArmor:
			if(at == AttackType.MagicAttack) {
				damage *= 2f;
			} else if (at == AttackType.NormalAttack) {
				damage *= 0.75f;
			} else if (at == AttackType.PiercingAttack) {
				damage *= 0.5f;
			} 
			break;
			
		default:
			break;
		}
		
		//Take armor into account
		health -= damage * (1 - ((armor * 0.06f) / (1f + armor * 0.06f)));
		
		if(health <= 0) {
			Dead();
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
			moveSpeed = originalMoveSpeed;
			StartCoroutine("SlowRoutine", slow);
		}
		
	}
	
	IEnumerator DoTRoutine(float dotDamage)
	{
		for(int i = 0; i < 5; i++) {
			health -= dotDamage * (1 - ((poisonResistance * 0.06f) / (1f + poisonResistance * 0.06f)));
			if(health <= 0) {
				Dead();
			}
			yield return new WaitForSeconds(1f);
		}
	}
	
	IEnumerator SlowRoutine(float slow)
	{
		moveSpeed *= slow * (1 - slowResistance);
		yield return new WaitForSeconds(5f);
		moveSpeed = originalMoveSpeed;
	}
	
	#endregion
	
	#region Normalizers based on minmax normalization
	
	float NormalizeHealth() {
		return health / MAX_HEALTH; 
	}
	
	float NormalizePoisonResistance() {
		return poisonResistance / MAX_POISON_RESISTANCE;
	}
	
	float NormalizeSlowResistance() {
		return slowResistance / MAX_SLOW_RESISTANCE;
	}
	
	float NormalizeArmor() {
		return armor / MAX_ARMOR;
	}
	
	float NormalizeArmorType() {
		return (float)armorType / MAX_ARMOR_TYPE;
	}
	
	#endregion
	
}
