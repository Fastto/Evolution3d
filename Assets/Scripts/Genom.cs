using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genom
{
    public NeuralNetwork neuralNetwork;
    public int maxSpeed;

    public int score = 0;

    public Genom() {
        maxSpeed = 15;

        neuralNetwork = new NeuralNetwork(new int[2] { 2, 2 });
    }

    public int getMaxSpeed() {
        return maxSpeed;
    }

    public Genom clone() {
        Genom newGenom = new Genom();
        newGenom.neuralNetwork = this.neuralNetwork.clone();
        newGenom.maxSpeed = this.maxSpeed;
        return newGenom;
    }
}
