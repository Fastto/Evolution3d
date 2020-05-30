using System;

public class NeuralNetwork
{
    //private double learningRate;
    private Layer[] layers;


    public NeuralNetwork(int[] sizes)
    {
        //this.learningRate = learningRate;

        layers = new Layer[sizes.Length];
        for (int i = 0; i < sizes.Length; i++)
        {
            int nextSize = 0;
            if (i < sizes.Length - 1) nextSize = sizes[i + 1];
            layers[i] = new Layer(sizes[i], nextSize);
            for (int j = 0; j < sizes[i]; j++)
            {
                layers[i].biases[j] = UnityEngine.Random.value * .1 - .05;
                for (int k = 0; k < nextSize; k++)
                {
                    layers[i].weights[j,k] = UnityEngine.Random.value * .1 - .05;
                }
            }
        }
    }

    public NeuralNetwork(Layer[] layers)
    {
        this.layers = layers;
    }

    public double[] feedForward(double[] inputs)
    {

        layers[0].neurons = inputs;
        for (int i = 1; i < layers.Length; i++)
        {
            Layer l = layers[i - 1];
            Layer l1 = layers[i];
            for (int j = 0; j < l1.size; j++)
            {
                l1.neurons[j] = 0;
                for (int k = 0; k < l.size; k++)
                {
                    l1.neurons[j] += l.neurons[k] * l.weights[k,j];
                }
                l1.neurons[j] += l1.biases[j];
                l1.neurons[j] = (activationFunction(l1.neurons[j])) * 2 - 1;
            }
        }
        return layers[layers.Length - 1].neurons;
    }

    public void mutate(double rate, double chance)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            for (int j = 0; j < layers[i].weights.GetUpperBound(0) + 1; j++)
            {
                for (int g = 0; g < layers[i].weights.GetUpperBound(1) + 1; g++)
                {
                    if (UnityEngine.Random.value < chance)
                    {
                        layers[i].weights[j,g] += (UnityEngine.Random.value - 0.5d) * rate;

                        if (layers[i].weights[j,g] > 1) layers[i].weights[j,g] = 1;
                        if (layers[i].weights[j,g] < -1) layers[i].weights[j,g] = -1;
                    }
                }
            }

            for (int j = 0; j < layers[i].biases.Length; j++)
            {

                if (UnityEngine.Random.value < chance)
                {
                    layers[i].biases[j] += (UnityEngine.Random.value - 0.5d) * rate;

                    if (layers[i].biases[j] > 1) layers[i].biases[j] = 1;
                    if (layers[i].biases[j] < -1) layers[i].biases[j] = -1;
                }

            }
        }
    }

    public void crossWith(NeuralNetwork targetNetwork)
    {
        Layer[] layer = targetNetwork.layers;
        for (int i = 0; i < layers.Length; i++)
        {
            for (int j = 0; j < layers[i].weights.GetUpperBound(0) + 1; j++)
            {
                for (int g = 0; g < layers[i].weights.GetUpperBound(1) + 1; g++)
                {
                    if (UnityEngine.Random.value < 0.5)
                    {
                        layers[i].weights[j,g] = layer[i].weights[j,g];
                    }
                }
            }
            for (int j = 0; j < layers[i].biases.Length; j++)
            {
                if (UnityEngine.Random.value < 0.5)
                {
                    layers[i].biases[j] = layer[i].biases[j];
                }
            }
        }
    }

    public NeuralNetwork clone()
    {
        Layer[] newLayers = new Layer[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            int nextSize = 0;
            if (i < layers.Length - 1) nextSize = layers[i + 1].size;
            newLayers[i] = new Layer(layers[i].size, nextSize);
            for (int j = 0; j < layers[i].size; j++)
            {
                newLayers[i].biases[j] = layers[i].biases[j];
                for (int k = 0; k < nextSize; k++)
                {
                    newLayers[i].weights[j,k] = layers[i].weights[j,k];
                }
            }
        }

        return new NeuralNetwork(newLayers);
    }

    private double activationFunction(double x) {
        return (1 / (1 + Math.Exp(-x)));
    }

    private double deriativeFunction(double x)
    {
        return x * (1 - x);
    }

}
