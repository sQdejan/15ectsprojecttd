using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EAWaveHandler : MonoBehaviour {

	public GameObject warriorPool;
	public GameObject magePool;
	public GameObject roguePool;
	public GameObject monkPool;
	public int waveSize = 30;

	[HideInInspector]
	public static int enemiesDone = 0;
	[HideInInspector]
	public static float totalDamageTaken = 0; //Probably for fitness
	[HideInInspector]
	public static float totalTravelTime = 0; //Probably for fitness

	private List<EAEnemy> warriors = new List<EAEnemy>();
	private List<EAEnemy> mages = new List<EAEnemy>();
	private List<EAEnemy> rogues = new List<EAEnemy>();
	private List<EAEnemy> monks = new List<EAEnemy>();
	private List<EAEnemy> leftWave;
	private List<EAEnemy> rightWave;

	private float time; //FOR MEASUREMENTS
	private int curWave = 0;
	private bool firstWave = true;

	const int SIZE_OF_POPULATION = 6;
	private WaveChromosome[] population = new WaveChromosome[SIZE_OF_POPULATION]; 

	// Use this for initialization
	void Start () {

		foreach(Transform t in warriorPool.transform) { warriors.Add(t.GetComponent<EAEnemy>()); }
		foreach(Transform t in magePool.transform) { mages.Add(t.GetComponent<EAEnemy>()); }
		foreach(Transform t in roguePool.transform) { rogues.Add(t.GetComponent<EAEnemy>()); }
		foreach(Transform t in monkPool.transform) { monks.Add(t.GetComponent<EAEnemy>()); }

	}
	
	// Update is called once per frame
	void Update () {
		//Create the wave flow
	}

	IEnumerator SpawnWaves()
	{
		ReadGene();

		time = Time.realtimeSinceStartup;

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

	
	void EvaluateGeneration()
	{
		
	}
	
	void ProduceNextGeneration()
	{
		
	}

	void ReadGene()
	{
		rightWave = new List<EAEnemy>();
		leftWave = new List<EAEnemy>();
		
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
}
