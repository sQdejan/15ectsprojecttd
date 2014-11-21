using UnityEngine;
using System.Collections;

public enum AttackType {NormalAttack, PiercingAttack, SiegeAttack, MagicAttack};

public abstract class Projectile : MonoBehaviour {

#region Variables

	//Publics
	public float travelSpeed = 0.5f;
	public AttackType attackType;

	[HideInInspector]
	public bool available = true;

	//Privates
	private Vector3 originalPosition;
	private Transform target;

	//Protected
	protected float damage = 1f;
	protected Transform thisTransform;
	protected float lastDistance = float.MaxValue;
	
#endregion

#region Accessors

	public Transform Target 
	{
		get {
			return target;
		}
	}

#endregion

	public virtual void DamageType(Enemy enemy){}

	void Awake()
	{
		originalPosition = transform.position;
		thisTransform = transform;
	}

	void Update()
	{
		float curDistance = Vector3.Distance(thisTransform.position, target.position);

		if(curDistance < lastDistance) {
			lastDistance = curDistance;
		} else {
			DamageType(target.GetComponent<Enemy>());
			Reset();
		}

		Vector3 direction = (target.position - thisTransform.position).normalized;

		//Rotate towards target
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		thisTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		//Move towards target
		thisTransform.position += direction * travelSpeed * Time.deltaTime;

		if(!target.gameObject.activeSelf) {
			Reset();
		}
	}

	public virtual void Activate(Vector3 position, Transform target, float damage)
	{
		gameObject.SetActive(true);
		thisTransform.position = position;
		this.target = target;
		available = false;
		this.damage = damage;
	}

	public void Reset()
	{
		lastDistance = float.MaxValue;
		available = true;
		thisTransform.position = originalPosition;
		gameObject.SetActive(false);
	}
}
