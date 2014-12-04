using UnityEngine;
using System.Collections;

public class TestResults : MonoBehaviour {

	public GUIStyle heading;
	public GUIStyle myStyle;

	private int enjoyment = 0;
	private int adaptable = 0;
	private string comment = "";

	bool e1 = false, e2 = false, e3 = false, e4 = false, e5 = false, e6 = false, e7 = false;
	bool a1 = false, a2 = false, a3 = false, a4 = false, a5 = false, a6 = false, a7 = false;

	bool eFilled = false, aFilled = false;
	bool resultsSend = false;
		
	private static TestResults instance;

	private TestResults() {}
	
	public static TestResults Instance
	{
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<TestResults>();
			}
			return instance;
		}
	}

	void Awake()
	{
		instance = this;

	}

#region URL related
	
	string baseurl = "http://www.janolesen.org/database.php?playerID=%0&mode=%1&curlevel=%3&towerinfo=%4&waveinfo=%5";

	string updateurl = "http://www.janolesen.org/update.php?playerID=%0&score=%2&curlevel=%3&towerinfo=%4&waveinfo=%5";

	string endurl = "http://www.janolesen.org/update.php?playerID=%0&score=%2&curlevel=%3&towerinfo=%4&waveinfo=%5&enjoyment=%6&adaptable=%7&comment=%8";

	public void SendStart()
	{
		StartCoroutine(StartUp());
	}

	IEnumerator StartUp() {
		
		string url = baseurl;

		url = url.Replace("%0", InteractionHandler.playerID.ToString());
		url = url.Replace("%1", InteractionHandler.mode);
		url = url.Replace("%3", (WaveHandler.Instance.curWave + 1).ToString());
		url = url.Replace("%4", InteractionHandler.towerinfo);
		url = url.Replace("%5", WaveHandler.waveinfo);

		WWW www = new WWW(url);
		
		yield return www;
		
		if(!string.IsNullOrEmpty(www.error)) {
			Debug.Log(www.error);
		}
	}

	public void UpdateResults()
	{
		StartCoroutine(UpdateTheResults());
	}

	IEnumerator UpdateTheResults() {

		string url = updateurl;

		url = url.Replace("%0", InteractionHandler.playerID.ToString());
		url = url.Replace("%2", WaveHandler.score.ToString());
		url = url.Replace("%3", (WaveHandler.Instance.curWave + 1).ToString());
		url = url.Replace("%4", InteractionHandler.towerinfo);
		url = url.Replace("%5", WaveHandler.waveinfo);

		WWW www = new WWW(url);
		
		yield return www;

		if(!string.IsNullOrEmpty(www.error)) {
			Debug.Log(www.error);
		}
	}
	
	IEnumerator EndResults() {

		string url = endurl;

		url = url.Replace("%0", InteractionHandler.playerID.ToString());
		url = url.Replace("%2", WaveHandler.score.ToString());
		url = url.Replace("%3", (WaveHandler.Instance.curWave + 1).ToString());
		url = url.Replace("%4", InteractionHandler.towerinfo);
		url = url.Replace("%5", WaveHandler.waveinfo);
		url = url.Replace("%6", enjoyment.ToString());
		url = url.Replace("%7", adaptable.ToString());

		comment = comment.Replace(" ", "%20");

		url = url.Replace("%8", comment);

		WWW www = new WWW(url);
		
		yield return www;
		
		if(!string.IsNullOrEmpty(www.error)) {
			Debug.Log(www.error);
		}
	}

#endregion
	

	void OnGUI()
	{
		if(!InteractionHandler.gameOver)
			return;

		GUI.Window(0, new Rect(20, 25, 750, 550), MyWindow, "Test Results");
	}

	void MyWindow(int windowID) {
		if(!resultsSend) {
			Enjoyment();
			Adaptable();
			Comment();

			if(eFilled && aFilled) {
				if(GUI.Button(new Rect(325, 450, 100, 20), "Send Results!")) {
					resultsSend = true;
					StartCoroutine(EndResults());
				}
			}
		} else {
			GUI.Label(new Rect(330, 40, 50, 30), "Thank you!! :)", heading);
		}

	}

	int eOffset = 200;
	int yOffsetE = 65;

	void Enjoyment()
	{
		GUI.Label(new Rect(250, 40, 200, 20), "How enjoyable was the game?", heading);

		GUI.Label(new Rect(100, yOffsetE + 3, 100, 30), "Annoying",myStyle);

		if(GUI.Toggle(new Rect(eOffset, yOffsetE, 30, 30), e1, ""))
		{
			e1 = true;
			e2 = false;
			e3 = false;
			e4 = false;
			e5 = false;
			e6 = false;
			e7 = false;
			
			enjoyment = 1;

			eFilled = true;
		}
		if(GUI.Toggle(new Rect(50 + eOffset, yOffsetE, 30, 30), e2, ""))
		{
			e1 = false;
			e2 = true;
			e3 = false;
			e4 = false;
			e5 = false;
			e6 = false;
			e7 = false;
			
			enjoyment = 2;

			eFilled = true;
		}
		if(GUI.Toggle(new Rect(100 + eOffset, yOffsetE, 30, 30), e3, ""))
		{
			e1 = false;
			e2 = false;
			e3 = true;
			e4 = false;
			e5 = false;
			e6 = false;
			e7 = false;
			
			enjoyment = 3;
			eFilled = true;
		}
		if(GUI.Toggle(new Rect(150 + eOffset, yOffsetE, 30, 30), e4, ""))
		{
			e1 = false;
			e2 = false;
			e3 = false;
			e4 = true;
			e5 = false;
			e6 = false;
			e7 = false;
			
			enjoyment = 4;
			eFilled = true;
		}
		if(GUI.Toggle(new Rect(200 + eOffset, yOffsetE, 30, 30), e5, ""))
		{
			e1 = false;
			e2 = false;
			e3 = false;
			e4 = false;
			e5 = true;
			e6 = false;
			e7 = false;
			
			enjoyment = 5;
			eFilled = true;
		}
		if(GUI.Toggle(new Rect(250 + eOffset, yOffsetE, 30, 30), e6, ""))
		{
			e1 = false;
			e2 = false;
			e3 = false;
			e4 = false;
			e5 = false;
			e6 = true;
			e7 = false;
			
			enjoyment = 6;
			eFilled = true;
		}
		if(GUI.Toggle(new Rect(300 + eOffset, yOffsetE, 30, 30), e7, ""))
		{
			e1 = false;
			e2 = false;
			e3 = false;
			e4 = false;
			e5 = false;
			e6 = false;
			e7 = true;
			
			enjoyment = 7;
			eFilled = true;
		}

		GUI.Label(new Rect(340 + eOffset, yOffsetE + 3, 100, 30), "Enjoyable", myStyle);
	}

	int yOffsetA = 165;

	void Adaptable()
	{
		GUI.Label(new Rect(220, yOffsetA - 25, 200, 20), "How adaptable did you find the waves?", heading);
		
		GUI.Label(new Rect(100, yOffsetA + 3, 100, 30), "Not adaptable", myStyle);
		
		if(GUI.Toggle(new Rect(eOffset, yOffsetA, 30, 30), a1, ""))
		{
			a1 = true;
			a2 = false;
			a3 = false;
			a4 = false;
			a5 = false;
			a6 = false;
			a7 = false;
			
			adaptable = 1;
			aFilled = true;
		}
		if(GUI.Toggle(new Rect(50 + eOffset, yOffsetA, 30, 30), a2, ""))
		{
			a1 = false;
			a2 = true;
			a3 = false;
			a4 = false;
			a5 = false;
			a6 = false;
			a7 = false;
			
			adaptable = 2;
			aFilled = true;
		}
		if(GUI.Toggle(new Rect(100 + eOffset, yOffsetA, 30, 30), a3, ""))
		{
			a1 = false;
			a2 = false;
			a3 = true;
			a4 = false;
			a5 = false;
			a6 = false;
			a7 = false;
			
			adaptable = 3;
			aFilled = true;
		}
		if(GUI.Toggle(new Rect(150 + eOffset, yOffsetA, 30, 30), a4, ""))
		{
			a1 = false;
			a2 = false;
			a3 = false;
			a4 = true;
			a5 = false;
			a6 = false;
			a7 = false;
			
			adaptable = 4;
			aFilled = true;
		}
		if(GUI.Toggle(new Rect(200 + eOffset, yOffsetA, 30, 30), a5, ""))
		{
			a1 = false;
			a2 = false;
			a3 = false;
			a4 = false;
			a5 = true;
			a6 = false;
			a7 = false;
			
			adaptable = 5;
			aFilled = true;
		}
		if(GUI.Toggle(new Rect(250 + eOffset, yOffsetA, 30, 30), a6, ""))
		{
			a1 = false;
			a2 = false;
			a3 = false;
			a4 = false;
			a5 = false;
			a6 = true;
			a7 = false;
			
			adaptable = 6;
			aFilled = true;
		}
		if(GUI.Toggle(new Rect(300 + eOffset, yOffsetA, 30, 30), a7, ""))
		{
			a1 = false;
			a2 = false;
			a3 = false;
			a4 = false;
			a5 = false;
			a6 = false;
			a7 = true;
			
			adaptable = 7;
			aFilled = true;
		}
		
		GUI.Label(new Rect(340 + eOffset, yOffsetA + 3, 100, 30), "Very adaptable", myStyle);
	}

	void Comment()
	{
		GUI.Label(new Rect(135, 250, 100, 30), "Any comments?", heading);

		comment = GUI.TextField(new Rect(125, 270, 500, 100), comment);
	}
}
