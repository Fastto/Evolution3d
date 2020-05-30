using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public float save = 2;

    public Genom genome;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    private void Awake()
    {
        genome = new Genom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //looking direction for the near food
        Vector3 vectorToNearFood = getVectorToNearFood();
        if (vectorToNearFood.magnitude > 0)
        {
            double[] dir = genome.neuralNetwork.feedForward(new double[2] { vectorToNearFood.normalized.x, vectorToNearFood.normalized.z });

            Vector3 dirdir = new Vector3((float)dir[0], 0, (float)dir[1]);
            Vector3 newPosition = transform.position + dirdir.normalized * Time.deltaTime * genome.getMaxSpeed();
            transform.position = newPosition;

            Debug.DrawLine(transform.position, transform.position + vectorToNearFood, Color.green);
            Debug.DrawLine(transform.position, transform.position + dirdir * 10, Color.red);


            save -= 2f * Time.deltaTime;
            if (save <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Food") {
            eat();
            Evolution.Instance.publishToLeaderBoard(this);
            Destroy(other.gameObject);
        }

        if (other.tag == "Ground") {
            Destroy(this.gameObject);
        }
    }

    private Vector3 getVectorToNearFood() {
        Food[] foods = GameObject.FindObjectsOfType<Food>();
        Vector3 toFood;
        if (foods.Length > 0)
        {
            Vector3 toNextFood = foods[0].transform.position - transform.position;
            KeyValuePair<float, Food> nearFood = new KeyValuePair<float, Food>(toNextFood.magnitude, foods[0]);

            foreach (Food food in foods)
            {
                Vector3 toMove = food.transform.position - transform.position;
                if (toMove.magnitude < nearFood.Key)
                {
                    nearFood = new KeyValuePair<float, Food>(toMove.magnitude, food);
                }
            }

            toFood = nearFood.Value.transform.position - transform.position;  
        }
        else
        {
            toFood = new Vector3(0, 0, 0);
        }

        return toFood;
    }

    private void eat() {
        genome.score++;
        save += 1;

        if (genome.score % 5 == 0) {
            Evolution.Instance.cellWantChild(this);
        }
    }
}
