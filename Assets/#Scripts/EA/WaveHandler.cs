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
	
	public static int enemiesDone = 0; 
	public static EAWaveGenome genome;

	//Privates
	private List<Enemy> warriors = new List<Enemy>();
	private List<Enemy> mages = new List<Enemy>();
	private List<Enemy> rogues = new List<Enemy>();
	private List<Enemy> monks = new List<Enemy>();
	private List<Enemy> leftWave;
	private List<Enemy> rightWave;
	private int curWave = 0;

#endregion
	
	void Start()
	{
		InteractionHandler.dGameOver += StopGame; //Adding StopGame() to GameOver delegate

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
			enemiesDone = 0;
			Debug.Log("Wave " + (curWave) + " is over");
			if(++curWave <= waves) {
				EAWaveHandler.Instance.StartEAProcess(); //Remember something here like waiting with starting before EA process is over
				StartCoroutine("WaveWaiting");
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
	}

#endregion

}
