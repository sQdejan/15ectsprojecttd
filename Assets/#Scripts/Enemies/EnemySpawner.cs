using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour {

#region Variables

	//Publics
	public GameObject enemyPool;
	public int waves;
	public float timeBetweenWaves = 15f;
	public float spawnTimeBetweenEnemies = 0.5f;

	[HideInInspector]
	public static int enemiesDone = 0; //Probably not the best idea if more spawners should be available
	[HideInInspector]
	public static float totalDamageTaken = 0;

	//Privates
	private List<Enemy> enemies = new List<Enemy>();
	private int curWave = 0;

	public float timeSpeed;

#endregion
	
	void Start()
	{

		Time.timeScale = timeSpeed;

		foreach(Transform t in enemyPool.transform) {
			enemies.Add(t.GetComponent<Enemy>());
		}

		StartCoroutine(SpawnWaves());
	}

	void Update()
	{
		if(enemiesDone >= enemies.Count) {
			enemiesDone = 0;
			Debug.Log("Wave " + (curWave) + " is over");
			if(curWave < waves) {
				StartCoroutine(WaveWaiting());
			} else {
				Debug.Log(EnemySpawner.totalDamageTaken);
				Debug.Log("Game is over, no more waves!");
			}
		}
	}

	IEnumerator SpawnWaves()
	{
		yield return new WaitForSeconds(10f);

		for(int i = 0; i < enemies.Count; i++) {
			yield return new WaitForSeconds(spawnTimeBetweenEnemies);
			enemies[i].Spawn();
		}

		curWave++;
	}

	IEnumerator WaveWaiting()
	{
		yield return new WaitForSeconds(timeBetweenWaves);

		StartCoroutine(SpawnWaves());
	}

}
