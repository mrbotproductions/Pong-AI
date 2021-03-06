using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public int generations;
    public int populationSize;//creates population size
    public Player player;

    private int[] layers = new int[3] { 7, 3, 1 };//initializing network to the right size

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    public List<NeuralNetwork> networks;

    public int curGeneration = 1;
    public int curPopulation = 0;

    void Start()// Start is called before the first frame update
    {
        InitNetworks();
        StartCoroutine(RunAI());
        Time.timeScale = 1f;
    }

    public IEnumerator RunAI()
    {
        //for (int i = 0; i < generations; i++)
        //{
        //    for (int j = 0; j < populationSize; j++)
        //    {
        //        player.resetGame();
        //        player.network = networks[j]; //deploys network to each learner
        //        player.startGame();
        //        while (player.gameInSession)
        //        {
        //            // wait till AI dies   
        //        }
        //        player.updateFitness();
        //    }
        //    SortNetworks();
        //}
                yield return new WaitForSeconds(1);

                curPopulation++;

                if (curPopulation > populationSize)
                {
                    curGeneration++;
                    curPopulation = 1;
                    SortNetworks(); //update all neural networks with genetic algorithm
                }

                if (curGeneration > generations)
                    yield break;

                player.initGame();
                player.network = networks[curPopulation-1]; //deploys network to each learner
                player.startGame();
    }



    /*
    Credit to Kip Parker for the methods InitNetworks and SortNetworks.
    Found on https://github.com/kipgparker/MutationNetwork
    */

    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Scripts/Pre-trained.txt");//on start load the network save
            networks.Add(net);
        }
    }

    public void SortNetworks()
    {
        networks.Sort();
        networks[populationSize - 1].Save("Assets/Scripts/Save.txt");//saves networks weights and biases to file, to preserve network performance
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }
    }
}
