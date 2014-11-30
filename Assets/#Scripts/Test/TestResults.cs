using UnityEngine;
using System.Collections;

public class TestResults : MonoBehaviour {
	

	#region URL related
	
	string baseurl = "http://www.janolesen.org/database.php?name=hejpik";
	
	IEnumerator SendTheResults() {
		
		string url = baseurl;
		
		WWW www = new WWW(url);
		
		yield return www;

		if(!string.IsNullOrEmpty(www.error)) {
			Debug.Log(www.error);
		}

		Debug.Log("I have sended the results");
	}
	
	#endregion
	

	void OnGUI()
	{
		if(GUI.Button(new Rect(0, 0, 100, 20), "Send Results"))
		{			
			StartCoroutine(SendTheResults());
		}
	}
}
