using UnityEngine;
using System.Collections;

public sealed class RangeIndicator : MonoBehaviour {

	public static bool selected = false;

	private static RangeIndicator instance;
	private Transform thisTransform;
	private Vector3 startPosition;
	private static Transform target;
	private static Vector3 scaleVector;

	private RangeIndicator() {}

	public static RangeIndicator Instance
	{
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<RangeIndicator>();
			}
			return instance;
		}
	}

	void Start()
	{
		thisTransform = transform;
		startPosition = transform.position;
	}

	void Update()
	{
		if(selected) {
			thisTransform.localScale = scaleVector;
			thisTransform.position = target.position;
		} else {
			thisTransform.position = startPosition;
		}
	}

	public static void SetTarget(Transform t, float s) 
	{
		target = t;
		scaleVector = new Vector3(s, s, 1);
	}
}
