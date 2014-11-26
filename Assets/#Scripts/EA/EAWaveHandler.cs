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
	public GameObject towerPool;
	public int waveSize = 30;
	public int generations = 1;

	public static int enemiesDone = 0;
	public static float totalDamageTaken = 0; //Probably for fitness
	public static float totalTravelTime = 0; //Probably for fitness

	//Privates
	private static EAWaveHandler instance;

	private List<EAEnemy> warriors = new List<EAEnemy>();
	private List<EAEnemy> mages = new List<EAEnemy>();
	private List<EAEnemy> rogues = new List<EAEnemy>();
	private List<EAEnemy> monks = new List<EAEnemy>();
	private List<EAEnemy> leftWave;
	private List<EAEnemy> rightWave;

	private List<EATower> towers = new List<EATower>();

	private float time; //FOR MEASUREMENTS
	private float waveMaxHealth;
	private int curGeneration = 1;
	private int curWave = 0;
	private int towersActivated = 0;

	const int SIZE_OF_POPULATION = 5;
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

		foreach(Transform t in warriorPool.transform) { warriors.Add(t.GetComponent<EAEnemy>()); }
		foreach(Transform t in magePool.transform) { mages.Add(t.GetComponent<EAEnemy>()); }
		foreach(Transform t in roguePool.transform) { rogues.Add(t.GetComponent<EAEnemy>()); }
		foreach(Transform t in monkPool.transform) { monks.Add(t.GetComponent<EAEnemy>()); }

		foreach(Transform t in towerPool.transform) { towers.Add(t.GetComponent<EATower>()); }

		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//Create the wave flow

		if(enemiesDone >= waveSize) {

			enemiesDone = 0;
			population[curWave].fitnessDamage = totalDamageTaken / waveMaxHealth * 100;
			population[curWave].fitnessTravel = totalTravelTime;

			totalDamageTaken = 0;
			totalTravelTime = 0;

			if(++curWave < SIZE_OF_POPULATION) {
				StartCoroutine(SpawnWaves());
			} else {
				Debug.Log(Time.realtimeSinceStartup - time);
				PrintFitness();

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
		time = Time.realtimeSinceStartup;
		gameObject.SetActive(true);
		SetupTowers();

		foreach(EAEnemy e in warriors) { e.LevelUp(); }
		foreach(EAEnemy e in mages) { e.LevelUp(); }
		foreach(EAEnemy e in rogues) { e.LevelUp(); }
		foreach(EAEnemy e in monks) { e.LevelUp(); }

		StartCoroutine(SpawnWaves());
	}

	void SetupTowers()
	{
		foreach(Tower t in InteractionHandler.currentTowers) {
			towers[towersActivated].transform.position = new Vector3(t.transform.position.x - 20, t.transform.position.y - 20, t.transform.position.z);
			towers[towersActivated].radius = t.radius;
			towers[towersActivated].damage = t.damage;
			towers[towersActivated].dotDamage = t.dotDamage;
			towers[towersActivated].slow = t.slow;
			towers[towersActivated].aimFrontEnemy = t.aimFrontEnemy;

			towers[towersActivated].Initialize(t.towerType);

			towers[towersActivated].gameObject.SetActive(true);

			towersActivated++;
		}
	}

	void ShutDownTowers()
	{
		for(int i = 0; i < towersActivated; i++) {
			towers[i].gameObject.SetActive(false);
		}

		towersActivated = 0;
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
				leftWave[lIndex++].Spawn();
				shouldContinue = true;
			}
			
			if(rIndex < rightLength) {
				rightWave[rIndex++].Spawn();
				shouldContinue = true;
			}
			
			yield return null;
		}
	}

#region EA Related

	void EvaluateGeneration()
	{
		
	}
	
	void ProduceNextGeneration()
	{
		
	}

	void ReadChromosome()
	{
		rightWave = new List<EAEnemy>();
		leftWave = new List<EAEnemy>();
		
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
			
			if(orderWarrior == tmpIndex) {
				//				Debug.Log("With index " + tmpIndex + " I send warriors and " + warriorsLeft + " warriors go left");
				for(int i = 0; i < amountOfWarriors; i++) { 
					if(i < warriorsLeft) {
						leftWave.Add(warriors[i]);
						warriors[i].waypointPoolToUse = 0;
					} else {
						rightWave.Add(warriors[i]);
						warriors[i].waypointPoolToUse = 1;
					}
				}
			} else if (orderMages == tmpIndex) {
				//				Debug.Log("With index " + tmpIndex + " I send mages and " + magesLeft + " mages go left");
				for(int i = 0; i < amountOfMages; i++) { 
					if(i < magesLeft) {
						leftWave.Add(mages[i]);
						mages[i].waypointPoolToUse = 0;
					} else {
						rightWave.Add(mages[i]);
						mages[i].waypointPoolToUse = 1;
					}
				}
			} else if (orderRogues == tmpIndex) {
				//				Debug.Log("With index " + tmpIndex + " I send rogues and " + roguesLeft + " rogues go left");
				for(int i = 0; i < amountOfRogues; i++) { 
					if(i < roguesLeft) {
						leftWave.Add(rogues[i]);
						rogues[i].waypointPoolToUse = 0;
					} else {
						rightWave.Add(rogues[i]);
						rogues[i].waypointPoolToUse = 1;
					}
				}
			} else if (orderMonks == tmpIndex) {
				//				Debug.Log("With index " + tmpIndex + " I send monks and " + monksLeft + " monks go left");
				for(int i = 0; i < amountOfMonks; i++) { 
					if(i < monksLeft) {
						leftWave.Add(monks[i]);
						monks[i].waypointPoolToUse = 0;
					} else {
						rightWave.Add(monks[i]);
						monks[i].waypointPoolToUse = 1;
					}
				}
			}
			
			tmpIndex++;
		}

		waveMaxHealth = 0;

		//Calculate max health
		foreach(EAEnemy e in leftWave) {
			waveMaxHealth += e.health;
		}

		foreach(EAEnemy e in rightWave) {
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
