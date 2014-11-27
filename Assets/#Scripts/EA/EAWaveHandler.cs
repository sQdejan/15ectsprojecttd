using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EAWaveHandler : MonoBehaviour {

#region Variables

	//Publics
	public GameObject warriorPool;
	public GameObject magePool;
	public GameObject roguePool;
	public GameObject monkPool;
	public GameObject arrowTowerPool;
	public GameObject poisonTowerPool;
	public GameObject bombTowerPool;
	public GameObject frostTowerPool;
	public float spawnTimeBetweenEnemies = 0.5f;
	public int waveSize = 30;
	public int generations = 1;

	public static int enemiesDone = 0;
	public static float totalDamageTaken = 0; //Probably for fitness
	public static float totalTravelTime = 0; //Probably for fitness

	//Privates
	private static EAWaveHandler instance;

	private List<Enemy> warriors = new List<Enemy>();
	private List<Enemy> mages = new List<Enemy>();
	private List<Enemy> rogues = new List<Enemy>();
	private List<Enemy> monks = new List<Enemy>();
	private List<Enemy> leftWave;
	private List<Enemy> rightWave;

	private List<Tower> arrowTowers = new List<Tower>();
	private List<Tower> poisonTowers = new List<Tower>();
	private List<Tower> bombTowers = new List<Tower>();
	private List<Tower> frostTowers = new List<Tower>();

	private List<Tower> activeTowers = new List<Tower>();

	private float time; //FOR MEASUREMENTS
	private float waveMaxHealth;
	private int curGeneration = 1;
	private int curWave = 0;

	const int SIZE_OF_POPULATION = 10;
	private EAWaveGenome[] population = new EAWaveGenome[SIZE_OF_POPULATION]; 

#endregion

	private EAWaveHandler() {}
	
	public static EAWaveHandler Instance
	{
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<EAWaveHandler>();
			}
			return instance;
		}
	}

	// Use this for initialization
	void Start () {

		instance = this;

		for(int i = 0; i < SIZE_OF_POPULATION; i++) {
			population[i] = new EAWaveGenome(waveSize);
		}

		//Set the first wave to one of members from the population
		WaveHandler.genome = population[0];

		//Enemies
		foreach(Transform t in warriorPool.transform) { warriors.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in magePool.transform) { mages.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in roguePool.transform) { rogues.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in monkPool.transform) { monks.Add(t.GetComponent<Enemy>()); }

		//Towers
		foreach(Transform t in arrowTowerPool.transform) { arrowTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in poisonTowerPool.transform) { poisonTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in bombTowerPool.transform) { bombTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in frostTowerPool.transform) { frostTowers.Add(t.GetComponent<Tower>()); }

		gameObject.SetActive(false);
	}

	void FixedUpdate () {
		//Create the wave flow

		if(enemiesDone >= waveSize) {

			enemiesDone = 0;
			population[curWave].fitnessDamage = totalDamageTaken;
			population[curWave].fitnessTravel = totalTravelTime;
			totalDamageTaken = 0;
			totalTravelTime = 0;

			//Remember this should end after amount of generations hihihihi ^^
			if(++curWave < SIZE_OF_POPULATION) {
				StartCoroutine(SpawnWaves());
			} else {
				Debug.Log(Time.realtimeSinceStartup - time);
//				PrintFitness();
				curWave = 0;

				if(++curGeneration > generations) {
					curGeneration = 1;
					ShutDownTowers();
					gameObject.SetActive(false);
					return;
				}

				StartCoroutine(SpawnWaves());
			}
		}
	}

	public void StartEAProcess()
	{
		SetupTowers();
		Time.timeScale = 100;
		time = Time.realtimeSinceStartup;
		gameObject.SetActive(true);
		StartCoroutine(SpawnWaves());
	}

	void SetupTowers()
	{

		for(int i = 0; i < 4; i++) {
			int tmpIndex = 0;
			List<Tower> curList;
			List<Tower> curClassList;

			if(i == 0) {
				curList = InteractionHandler.currentArrowTowers;
				curClassList = arrowTowers;
			} else if (i == 1) {
				curList = InteractionHandler.currentPoisonTowers;
				curClassList = poisonTowers;
			} else if (i == 2) {
				curList = InteractionHandler.currentBombTowers;
				curClassList = bombTowers;
			} else {
				curList = InteractionHandler.currentFrostTowers;
				curClassList = frostTowers;
			}

			//Arrow Towers
			foreach(Tower t in curList) {
				curClassList[tmpIndex].transform.position = new Vector3(t.transform.position.x - 20, t.transform.position.y - 20, t.transform.position.z);
				curClassList[tmpIndex].damage = t.damage;
				curClassList[tmpIndex].dotDamage = t.dotDamage;
				curClassList[tmpIndex].slow = t.slow;
				curClassList[tmpIndex].available = false;

				activeTowers.Add(curClassList[tmpIndex]);

				curClassList[tmpIndex].gameObject.SetActive(true);
				
				tmpIndex++;
			}
			tmpIndex = 0;
		}
	}


	void ShutDownTowers()
	{
		foreach(Tower t in activeTowers) {
			t.available = true;
			t.gameObject.SetActive(false);
		}

		activeTowers.Clear();
	}

	IEnumerator SpawnWaves()
	{
		ReadChromosome();

		bool shouldContinue = true;
		int leftLength = leftWave.Count;
		int rightLength = rightWave.Count;
		int lIndex = 0;
		int rIndex = 0;
		
		while(shouldContinue) {
			
			shouldContinue = false;
			
			if(lIndex < leftLength) {
				leftWave[lIndex++].Spawn(true);
				shouldContinue = true;
			}
			
			if(rIndex < rightLength) {
				rightWave[rIndex++].Spawn(true);
				shouldContinue = true;
			}
			
			float time = 0;
			
			while(time < spawnTimeBetweenEnemies) {
				time += Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate();
			}
		}
	}

#region EA Related

	void EvaluateGeneration()
	{
		
	}
	
	void ProduceNextGeneration()
	{
		for(int i = 0; i < SIZE_OF_POPULATION; i++) {
			population[i].Mutate();
		}
	}

	void ReadChromosome()
	{
		rightWave = new List<Enemy>();
		leftWave = new List<Enemy>();
		
		int orderWarrior = population[curWave].Chromosome[0];
		int orderMages = population[curWave].Chromosome[1];
		int orderRogues = population[curWave].Chromosome[2];
		int orderMonks = population[curWave].Chromosome[3];
		
		int amountOfWarriors = population[curWave].Chromosome[4];
		int amountOfMages = population[curWave].Chromosome[5];
		int amountOfRogues = population[curWave].Chromosome[6];
		int amountOfMonks = population[curWave].Chromosome[7];
		
		int splitWarriors = population[curWave].Chromosome[8];
		int splitMages = population[curWave].Chromosome[9];
		int splitRogues = population[curWave].Chromosome[10];
		int splitMonks = population[curWave].Chromosome[11];
		
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
					curEnemyList[i].waypointPoolToUse = 2;
				} else {
					rightWave.Add(curEnemyList[i]);
					curEnemyList[i].waypointPoolToUse = 3;
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

#endregion

	void PrintFitness()
	{
		for(int i = 0; i < SIZE_OF_POPULATION; i++) {
			Debug.Log("% Damage taken of max damage = " + population[i].fitnessDamage + " Travel = " + population[i].fitnessTravel);
		}
	}
}
