using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	int i = 0;
	float t = 0;

	float rt = 0;
	bool didPrint = false;

	void Start()
	{
//		Time.timeScale *= 100;
		Debug.Log(Time.timeScale);
		StartCoroutine(lol ());
	}

//	void FixedUpdate()
//	{
//		i++;
//
//		if(!didPrint && Time.realtimeSinceStartup - rt > 1) {
//			Debug.Log("LOL " + (Time.realtimeSinceStartup - rt));
//			Debug.Log(i);
//			didPrint = true;
//		}
//	}

	IEnumerator lol ()
	{
		rt = Time.realtimeSinceStartup;
		float time = 0;

		while(time < 1){
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		Debug.Log(Time.realtimeSinceStartup - rt);
	}
}
