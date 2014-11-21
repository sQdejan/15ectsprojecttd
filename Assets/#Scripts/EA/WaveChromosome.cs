using System.Collections;
using UnityEngine;

public class WaveChromosome  {

#region variables

	public float fitness;

	private static System.Random rndGenerator = new System.Random();
	private static int waveSize;
	private int[] chromosome = new int[8]; 	//1. Amount of Warriors, 5. % Split of Warriors
											//2. Amount of Mages, 6. % Split of Mages
											//3. Amount of Rogues, 7. % Split of Rogues
											//4. Amount of Monks, 8. % Split of Monks

#endregion

#region accessors

	public int[] Chromosome
	{
		get {
			return chromosome;
		}
	}

#endregion

	public WaveChromosome(int waveSize)
	{
		WaveChromosome.waveSize = waveSize;

		//Initialization
		chromosome[0] = rndGenerator.Next(5, 9);
		chromosome[1] = rndGenerator.Next(5, 9);
		chromosome[2] = rndGenerator.Next(5, 9);
		chromosome[3] = waveSize - chromosome[2] - chromosome[1] - chromosome[0];

		chromosome[4] = rndGenerator.Next(20, 80);
		chromosome[5] = rndGenerator.Next(20, 80);
		chromosome[6] = rndGenerator.Next(20, 80);
		chromosome[7] = rndGenerator.Next(20, 80);
	}

	public WaveChromosome Mutate()
	{
		return null;
	}

}
