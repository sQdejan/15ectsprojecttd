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
	public int waveSize = 30;
	public int waves;
	public float timeBetweenWaves = 15f;
	public float spawnTimeBetweenEnemies = 0.5f;
	
	[HideInInspector]
	public static int enemiesDone = 0; //Probably not the best idea if more spawners should be available <-- IS fixed
	[HideInInspector]
	public static float totalTravelTime = 0; //Used for setting the fitness
	
	//Privates
	private List<Enemy> warriors = new List<Enemy>();
	private List<Enemy> mages = new List<Enemy>();
	private List<Enemy> rogues = new List<Enemy>();
	private List<Enemy> monks = new List<Enemy>();
	private List<Enemy> leftWave;
	private List<Enemy> rightWave;
	private int curWave = 0;

	//EA Related
	const int SIZE_OF_POPULATION = 6;
	private WaveChromosome[] population = new WaveChromosome[SIZE_OF_POPULATION]; 

#endregion
	
	void Start()
	{
		InteractionHandler.dGameOver += StopGame; //Adding StopGame() to GameOver delegate

		for(int i = 0; i < SIZE_OF_POPULATION; i++) {
			population[i] = new WaveChromosome(waveSize);
		}

		foreach(Transform t in warriorPool.transform) { warriors.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in magePool.transform) { mages.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in roguePool.transform) { rogues.Add(t.GetComponent<Enemy>()); }
		foreach(Transform t in monkPool.transform) { monks.Add(t.GetComponent<Enemy>()); }
		
		StartCoroutine("FirstWave");
	}
	
	void Update()
	{
		if(InteractionHandler.gameOver){
			return;
		}

		if(enemiesDone >= waveSize) {
			//Read the total travel time for fitness
			population[curWave++].fitness = totalTravelTime;
			WaveHandler.totalTravelTime = 0;

			enemiesDone = 0;
			Debug.Log("Wave " + (curWave) + " is over");
			if(curWave < waves) {
				StartCoroutine("WaveWaiting");
			} else {
				Debug.Log("Game is over, no more waves!");

				for(int i = 0; i < population.Length; i++) {
					Debug.Log (population[i].fitness);
				}
			}
		}
	}

	void StopGame()
	{
		StopAllCoroutines();

		//Maybe just let it run out
		foreach(Enemy e in leftWave) {
			e.TakeDamage(float.MaxValue, AttackType.NormalAttack);
		}

		foreach(Enemy e in rightWave) {
			e.TakeDamage(float.MaxValue, AttackType.NormalAttack);
		}
	}

	IEnumerator SpawnWaves()
	{
		ReadGene();

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

			yield return new WaitForSeconds(spawnTimeBetweenEnemies);
		}
	}
	
	IEnumerator WaveWaiting()
	{
		foreach(Enemy e in warriors) { e.LevelUp(); }
		foreach(Enemy e in mages) { e.LevelUp(); }
		foreach(Enemy e in rogues) { e.LevelUp(); }
		foreach(Enemy e in monks) { e.LevelUp(); }

		yield return new WaitForSeconds(timeBetweenWaves);
		
		StartCoroutine("SpawnWaves");
	}

	IEnumerator FirstWave()
	{
		yield return new WaitForSeconds(timeBetweenWaves);

		StartCoroutine("SpawnWaves");
	}

#region EA-related

	void EvaluateGeneration()
	{
		
	}
	
	void ProduceNextGeneration()
	{
		
	}
	
	void ReadGene()
	{
		rightWave = new List<Enemy>();
		leftWave = new List<Enemy>();

		int amountOfWarriors = population[curWave].Chromosome[0];
		int amountOfMages = population[curWave].Chromosome[1];
		int amountOfRogues = population[curWave].Chromosome[2];
		int amountOfMonks = population[curWave].Chromosome[3];

		int splitWarriors = population[curWave].Chromosome[4];
		int splitMages = population[curWave].Chromosome[5];
		int splitRogues = population[curWave].Chromosome[6];
		int splitMonks = population[curWave].Chromosome[7];

		//The for-loops does not start from 1 so this is needed for the splitting
		int curIndex = 1;

		int warriorsLeft = (int)((float)amountOfWarriors / 100 * splitWarriors + 0.5f);
		int magesLeft = (int)((float)amountOfMages / 100 * splitMages + 0.5f);
		int roguesLeft = (int)((float)amountOfRogues / 100 * splitRogues + 0.5f);
		int monksLeft = (int)((float)amountOfMonks / 100 * splitMonks + 0.5f);

		//Warriors
		for(int i = 0; i < amountOfWarriors; i++) { 
			if(curIndex <= warriorsLeft) {
				leftWave.Add(warriors[i]);
				warriors[i].waypointPoolToUse = 0;
			} else {
				rightWave.Add(warriors[i]);
				warriors[i].waypointPoolToUse = 1;
			}
			curIndex++;
		}
		curIndex = 1;

		//Mages
		for(int i = amountOfWarriors; i < amountOfWarriors + amountOfMages; i++) { 
			if(curIndex <= magesLeft) {
				leftWave.Add(mages[i]);
				mages[i].waypointPoolToUse = 0;
			} else {
				rightWave.Add(mages[i]);
				mages[i].waypointPoolToUse = 1;
			}
			curIndex++;
		}
		curIndex = 1;

		//Rogues
		for(int i = amountOfWarriors + amountOfMages; i < amountOfWarriors + amountOfMages + amountOfRogues; i++) { 
			if(curIndex <= roguesLeft) {
				leftWave.Add(rogues[i]);
				rogues[i].waypointPoolToUse = 0;
			} else {
				rightWave.Add(rogues[i]);
				rogues[i].waypointPoolToUse = 1;
			}
			curIndex++;
		}
		curIndex = 1;

		//Rogues
		for(int i = amountOfWarriors + amountOfMages + amountOfRogues; i < amountOfWarriors + amountOfMages + amountOfRogues + amountOfMonks; i++) { 
			if(curIndex <= monksLeft) {
				leftWave.Add(monks[i]);
				monks[i].waypointPoolToUse = 0;
			} else {
				rightWave.Add(monks[i]);
				monks[i].waypointPoolToUse = 1;
			}
			curIndex++;
		}

	}

#endregion

}
