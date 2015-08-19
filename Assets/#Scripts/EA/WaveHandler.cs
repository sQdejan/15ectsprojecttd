using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveHandler : MonoBehaviour {

#region Variables
	
	//Publics
	public GameObject warriorPool;
	public GameObject magePool;
	public GameObject roguePool;
	public GameObject monkPool;
	public GameObject healthBarPool;
	public int waveSize = 30;
	public int waves;
	public float timeBetweenWaves = 15f;
	public float spawnTimeBetweenEnemies = 0.5f;
	public int curWave = 0;
	
	public static int enemiesDone = 0; 
	public static EAWaveGenome genome;
	public static bool amIRunning = false;
	public static bool wonGame = false;

	public static string waveinfo = "";
	public static int score = 0;


	//Privates
	private static WaveHandler instance;

	private List<Enemy> warriors = new List<Enemy>();
	private List<Enemy> mages = new List<Enemy>();
	private List<Enemy> rogues = new List<Enemy>();
	private List<Enemy> monks = new List<Enemy>();
	private List<Enemy> leftWave;
	private List<Enemy> rightWave;

	private List<HealthBar> healthBars = new List<HealthBar>();

	private float waveMaxHealth = 0;
	
#endregion

	private WaveHandler() {}
	
	public static WaveHandler Instance
	{
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<WaveHandler>();
			}
			return instance;
		}
	}

	void Start()
	{
		instance = this;

		InteractionHandler.dGameOver += StopGame; //Adding StopGame() to GameOver delegate

		foreach(Transform t in warriorPool.transform) { warriors.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in magePool.transform) { mages.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in roguePool.transform) { rogues.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in monkPool.transform) { monks.Add(t.GetComponent<Enemy>()); }

		foreach(Transform t in healthBarPool.transform) { healthBars.Add(t.GetComponent<HealthBar>()); }

//		StartCoroutine("FirstWave");
	}
	
	void FixedUpdate()
	{
		if(InteractionHandler.gameOver){
			return;
		}

		if(enemiesDone >= waveSize) {
			enemiesDone = 0;
			if(++curWave < waves) {
				foreach(Enemy e in warriors) { e.LevelUp(); }
				foreach(Enemy e in mages) { e.LevelUp(); }
				foreach(Enemy e in rogues) { e.LevelUp(); }
				foreach(Enemy e in monks) { e.LevelUp(); }

				foreach(HealthBar h in healthBars) { h.Disable(); }

				amIRunning = false;
				EAWaveHandler.Instance.StartEAProcess();
			} else {
				wonGame = true;
				curWave--;
				InteractionHandler.gameOver = true;
				amIRunning = false;
			}
		}
	}

	public void StartWaveHandler()
	{
		CreateGenomeString();
//		TestResults.Instance.UpdateResults();
//		StartCoroutine(WaveWaiting());
		amIRunning = true;
		StartCoroutine("SpawnWaves");
	}

	void CreateGenomeString()
	{
		for(int i = 0; i < genome.Chromosome.Length; i++) {
			if(i > 3) {
				if(genome.Chromosome[i] < 10) {
					waveinfo += "0";
				}
			}
			waveinfo += genome.Chromosome[i].ToString();
		}

		waveinfo += "-";
	}

	void StopGame()
	{
		StopAllCoroutines();
		amIRunning = false;

		foreach(HealthBar h in healthBars) { h.Disable(); }

		//Maybe just let it run out
		foreach(Enemy e in leftWave) {
			e.Terminate();
		}

		foreach(Enemy e in rightWave) {
			e.Terminate();
		}
	}

	IEnumerator SpawnWaves()
	{
		ReadChromosome();

		bool shouldContinue = true;
		int leftLength = leftWave.Count;
		int rightLength = rightWave.Count;
		int lIndex = 0;
		int rIndex = 0;
		int healthBarIndex = 0;

		while(shouldContinue) {

			shouldContinue = false;

			if(lIndex < leftLength) {
				leftWave[lIndex++].Spawn(false);
				healthBars[healthBarIndex++].SetTarget(leftWave[lIndex - 1]);
				shouldContinue = true;
			}

			if(rIndex < rightLength) {
				rightWave[rIndex++].Spawn(false);
				healthBars[healthBarIndex++].SetTarget(rightWave[rIndex - 1]);
				shouldContinue = true;
			}

			float time = 0;

			while(time < spawnTimeBetweenEnemies) {
				time += Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate();
			}
		}
	}
	
	IEnumerator WaveWaiting()
	{
		float time = 0;
		
		while(time < timeBetweenWaves) {
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		
		StartCoroutine("SpawnWaves");
	}

	IEnumerator FirstWave()
	{
		float time = 0;
		
		while(time < timeBetweenWaves) {
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		StartCoroutine("SpawnWaves");
	}
	
	void ReadChromosome()
	{
		rightWave = new List<Enemy>();
		leftWave = new List<Enemy>();

		int orderWarrior = genome.Chromosome[0];
		int orderMages = genome.Chromosome[1];
		int orderRogues = genome.Chromosome[2];
		int orderMonks = genome.Chromosome[3];

		int amountOfWarriors = genome.Chromosome[4];
		int amountOfMages = genome.Chromosome[5];
		int amountOfRogues = genome.Chromosome[6];
		int amountOfMonks = genome.Chromosome[7];

		int splitWarriors = genome.Chromosome[8];
		int splitMages = genome.Chromosome[9];
		int splitRogues = genome.Chromosome[10];
		int splitMonks = genome.Chromosome[11];

		int warriorsLeft = (int)((float)amountOfWarriors / 100 * splitWarriors + 0.5f);
		int magesLeft = (int)((float)amountOfMages / 100 * splitMages + 0.5f);
		int roguesLeft = (int)((float)amountOfRogues / 100 * splitRogues + 0.5f);
		int monksLeft = (int)((float)amountOfMonks / 100 * splitMonks + 0.5f);

		int tmpIndex = 1;

		while(tmpIndex <= 4) {
			int goingLeft = 0;
			int amountOfEnemies = 0;
			List<Enemy> curEnemyList = new List<Enemy>();

			if(tmpIndex == orderWarrior) {
				goingLeft = warriorsLeft;
				amountOfEnemies = amountOfWarriors;
				curEnemyList = warriors;
			} else if (tmpIndex == orderMages) {
				goingLeft = magesLeft;
				amountOfEnemies = amountOfMages;
				curEnemyList = mages;
			} else if (tmpIndex == orderRogues) {
				goingLeft = roguesLeft;
				amountOfEnemies = amountOfRogues;
				curEnemyList = rogues;
			} else if (tmpIndex == orderMonks) {
				goingLeft = monksLeft;
				amountOfEnemies = amountOfMonks;
				curEnemyList = monks;
			}

			for(int i = 0; i < amountOfEnemies; i++) { 
				if(i < goingLeft) {
					leftWave.Add(curEnemyList[i]);
					curEnemyList[i].waypointPoolToUse = 0;
				} else {
					rightWave.Add(curEnemyList[i]);
					curEnemyList[i].waypointPoolToUse = 1;
				}
			}

			tmpIndex++;
		}

		
		waveMaxHealth = 0;
		
		//Calculate max health
		foreach(Enemy e in leftWave) {
			waveMaxHealth += e.health;
		}
		
		foreach(Enemy e in rightWave) {
			waveMaxHealth += e.health;
		}
	}	
}
