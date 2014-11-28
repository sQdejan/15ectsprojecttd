using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {
	
	public Sprite greenBar;
	public Sprite redBar;
	public float yOffset = 0.2f;

	private Transform target;
	private Enemy eTarget;
	private Transform thisTransform;

	private bool changedBar = false;

	void Awake()
	{
		thisTransform = transform;
		gameObject.SetActive(false);	
	}

	void FixedUpdate () {
		thisTransform.position = target.position + (Vector3.up * yOffset);

		if(!changedBar && eTarget.health / eTarget.CurStartHealth < 0.3f) {
			GetComponent<SpriteRenderer>().sprite = redBar;
			changedBar = true;
		}

		thisTransform.localScale = new Vector3(eTarget.health / eTarget.CurStartHealth, 1, 1); 
	}

	public void SetTarget(Enemy e) 
	{
		target = e.transform;
		eTarget = e;
		gameObject.SetActive(true);
	}

	public void Disable()
	{
		changedBar = false;
		GetComponent<SpriteRenderer>().sprite = greenBar;
		gameObject.SetActive(false);
	}
}
