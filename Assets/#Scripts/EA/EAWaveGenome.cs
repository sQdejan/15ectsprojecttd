using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EAWaveGenome {

#region variables

	public float fitnessDamage;
	public float fitnessTravel;
	public float fitnessEnemiesDied;
	public float totalFitness;
	public bool haveIbeenTested = false;

	public static System.Random rndGenerator = new System.Random();
	private static int waveSize;
	private int[] chromosome = new int[12]; //1. Warrios order in wave, 5. Amount of Warriors, 9. % Split of Warriors
											//2. Mages order in wave, 6. Amount of Mages, 10. % Split of Mages
											//3. Rogues order in wave, 7. Amount of Rogues, 11. % Split of Rogues
											//4. Monks  order in wave, 8. Amount of Monks, 12. % Split of Monks

#endregion

#region accessors

	public int[] Chromosome
	{
		get {
			return chromosome;
		}

		set {
			chromosome = value;
		}
	}

#endregion

	public EAWaveGenome(int waveSize)
	{
		EAWaveGenome.waveSize = waveSize;

		//Initialization
		//Order of enemies
		List<int> numbers = new List<int>();
		numbers.Add(1); numbers.Add(2); numbers.Add(3); numbers.Add(4);

		int index = rndGenerator.Next(0, numbers.Count);
		chromosome[0] = numbers[index];
		numbers.RemoveAt(index);

		index = rndGenerator.Next(0, numbers.Count);
		chromosome[1] = numbers[index];
		numbers.RemoveAt(index);

		index = rndGenerator.Next(0, numbers.Count);
		chromosome[2] = numbers[index];
		numbers.RemoveAt(index);

		chromosome[3] = numbers[0];

		//Amount of enemies
		chromosome[4] = rndGenerator.Next(4, 11);
		chromosome[5] = rndGenerator.Next(4, 11);
		chromosome[6] = rndGenerator.Next(4, 11);
		chromosome[7] = waveSize - chromosome[6] - chromosome[5] - chromosome[4];

		//How many goes left
		chromosome[8] = rndGenerator.Next(20, 80);
		chromosome[9] = rndGenerator.Next(20, 80);
		chromosome[10] = rndGenerator.Next(20, 80);
		chromosome[11] = rndGenerator.Next(20, 80);

		Mutate();
	}

	public EAWaveGenome(int[] c) {
		this.chromosome = c;
	}

#region Mutation

	public int[] Mutate()
	{
		int[] c = (int[])this.chromosome.Clone();

		MutateSwapNumbers(c);
		MutateAmountOfEnemies(c);
		MutateLeftGoing(c);
		return c;
	}

	void MutateSwapNumbers(int[] c)
	{
		int amountOfSwaps = rndGenerator.Next(0, 5);

		for(int i = 0; i < amountOfSwaps; i++) {
			int swap1 = rndGenerator.Next(0, 4);
			int swap2 = rndGenerator.Next(0, 4);

			int tmp = c[swap1];
			c[swap1] = c[swap2];
			c[swap2] = tmp;
		}
	}

	void MutateAmountOfEnemies(int[] c)
	{
		int whichToIncrease = rndGenerator.Next(0, 31);
	
		//The probability of being increased is based on the amount already present.
		//Warriors, Mages, Rogues, Monks
		if(whichToIncrease <= c[4]) {
			whichToIncrease = 4;
		} else if (whichToIncrease <= c[4] + c[5]) {
			whichToIncrease = 5;
		} else if (whichToIncrease <= c[4] + c[5] + c[6]) {
			whichToIncrease = 6;
		} else {
			whichToIncrease = 7;
		}

		int increaseWith = 1;

		if(c[whichToIncrease] > waveSize - increaseWith) {
			increaseWith = waveSize - c[whichToIncrease];
		}

		if(increaseWith > 0) {
			//Increase the winner with three
			c[whichToIncrease] += increaseWith;

			//Now I remove three from the wave with the lowest number, and if not possible then the next one
			Dictionary<int, int> tmpDict = new Dictionary<int, int>();

			for(int i = 4; i <= 7; i++) {
				if(i != whichToIncrease) {
					tmpDict.Add(i, c[i]);
				}
			}

			var sortedDict = from entry in tmpDict orderby entry.Value ascending select entry;
			int currentDeleted = 0;

			foreach(KeyValuePair<int, int> entry in sortedDict) {
				if(entry.Value <= increaseWith - currentDeleted) {
					c[entry.Key] -= entry.Value;
					currentDeleted += entry.Value;
				} else {
					c[entry.Key] -= increaseWith - currentDeleted;
					break;
				}

				if(currentDeleted >= increaseWith) {
					break;
				}
			}
		}
	}

	void MutateLeftGoing(int[] c)
	{
		int howManyToChange = rndGenerator.Next(0, 5);

		for(int i = 0; i < howManyToChange; i++) {
			int whichToChange = rndGenerator.Next(8, 12);

			int upOrDown = rndGenerator.Next(0, 101);

			//I leave 1 percent so there is a change that it will go the other way
			if(c[whichToChange] < upOrDown) {
				if(c[whichToChange] > 10) {
					c[whichToChange] -= 5;
				}
			} else {
				if(c[whichToChange] < 90) {
					c[whichToChange] += 5;
				}
			}
		}
	}

#endregion

	void PrintChromosome()
	{
		Debug.Log("Start");
		string chromString = "";
		for(int i = 0; i < chromosome.Length; i++) {
			chromString += chromosome[i];
			chromString += " ";
		}
		Debug.Log(chromString);

		Debug.Log("End");
	}

}
