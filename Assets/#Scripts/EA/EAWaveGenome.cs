using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EAWaveGenome  {

#region variables

	public float fitnessDamage;
	public float fitnessTravel;

	private static System.Random rndGenerator = new System.Random();
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
	}

#endregion

	public EAWaveGenome(int waveSize)
	{
		EAWaveGenome.waveSize = waveSize;

		//Initialization
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

		chromosome[4] = rndGenerator.Next(4, 10);
		chromosome[5] = rndGenerator.Next(4, 10);
		chromosome[6] = rndGenerator.Next(4, 10);
		chromosome[7] = waveSize - chromosome[6] - chromosome[5] - chromosome[4];

		chromosome[8] = rndGenerator.Next(20, 80);
		chromosome[9] = rndGenerator.Next(20, 80);
		chromosome[10] = rndGenerator.Next(20, 80);
		chromosome[11] = rndGenerator.Next(20, 80);
	}

	public EAWaveGenome Mutate()
	{
		return null;
	}

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
