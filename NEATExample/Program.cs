﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyNEAT;
using MyNEAT.Domains;

namespace NEATExample
{

    static class Program
    {

        static void Main()
        {
            SolveCartPole();
            //Test();
            Console.ReadKey();
        }

        static void Test()
        {
            Random gen = new Random();
            Genome genome = new Genome(3, 2);

            Console.Write(genome + "\n\n");

            for (int i = 0; i < 500; i++)
            {
                genome = genome.CreateOffSpring(gen);
            }


            Console.Write(genome + "\n\n");


            Network network = new Network(genome);

            double[] pr = network.Predict(new double[] { -0.3, 0.2, 2 });

            string str = "";
            for (int i = 0; i < pr.Length; i++)
            {
                str += pr[i] + ", ";
            }
            Console.Write(str);

            Console.WriteLine(network);
        }

        static void SolveCartPole()
        {
            float elitism = 0.4f;
            int generations = 5000;
            int pop = 100;
            Random generator = new Random();

            List<Genome> population = new List<Genome>();

            //create initial pop
            for (int i = 0; i < pop; i++)
            {
                population.Add(new Genome(4, 1));
            }

            for (int i = 0; i < generations; i++)
            {
                foreach (Genome genome in population)
                {
                    SinglePoleBalancingEnvironment env = new SinglePoleBalancingEnvironment();
                    Network network = new Network(genome);
                    SinglePoleStateData s = env.SimulateTimestep(true);
                    while (true)
                    {
                        if (s._done == true)
                        {
                            genome.fitness = (float)(s._reward - Math.Sqrt(Math.Sqrt(genome.GetComplexity())));
                            //genome.fitness = s._reward;
                            break;
                        }

                        bool a = network.Predict(new double[] { s._cartPosX / env._trackLengthHalf,
                        s._cartVelocityX / 0.75,
                        s._poleAngle / SinglePoleBalancingEnvironment.TwelveDegrees,
                        s._poleAngularVelocity})[0] > 0;

                        env.SimulateTimestep(a);
                    }
                }
                float sum = 0;
                float comp_sum = 0;
                float mx = -10000;
                foreach (Genome genome in population)
                {
                    comp_sum += genome.GetComplexity();
                    sum += genome.fitness;
                    if (genome.fitness > mx)
                    {
                        mx = genome.fitness;
                    }
                }
                Console.Write("Generation: " + i + ", " + "Average fitness: " + sum / population.Count + ", " + "Max Fitness: " + mx + ", " + "Average complexity " + comp_sum / population.Count + "\n");

                //breed
                int toSelect = (int)(population.Count - elitism * population.Count);
                List<Genome> addToPop = new List<Genome>();
                for (; toSelect > 0; toSelect--)
                {
                    var g1 = population[generator.Next(population.Count)];
                    population.Remove(g1);
                    var g2 = population[generator.Next(population.Count)];
                    population.Remove(g2);
                    var g3 = population[generator.Next(population.Count)];
                    population.Add(g1);
                    population.Add(g2);

                    List<Genome> genomes = new List<Genome>(new Genome[] { g1, g2, g3 });
                    genomes.Sort(new Comparer2());

                    population.Remove(genomes[0]);

                    addToPop.Add(genomes[1].CreateOffSpring(generator, genomes[2]));

                }
                population.AddRange(addToPop);
            }
        }
    }
    class Comparer2 : IComparer<Genome>
    {
        public int Compare(Genome x, Genome y)
        {
            int compareDate = x.fitness.CompareTo(y.fitness);
            return compareDate;
        }
    }
}
