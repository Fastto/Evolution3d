using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private Genom genome;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        setGenom(new Genom());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //looking direction for the near food
        Vector3 vectorToNearFood = getVectorToNearFood();
        Vector3 vectorToNearCell = getVectorToNearCell();
        if (vectorToNearFood.magnitude > 0 || vectorToNearCell.magnitude > 0)
        {
            //double[] direction2d = getGenom().neuralNetwork.feedForward(new double[3] { vectorToNearFood.normalized.x, vectorToNearFood.normalized.z, genome.size });

            //Vector3 direction3d = new Vector3((float)direction2d[0], 0, (float)direction2d[1]);
            //float force = (float)direction2d[2];


            Vector3 direction3d = genome.getVectorToGoal(vectorToNearFood, vectorToNearCell);
            direction3d.y = 0;
            float force = 1;

            Vector3 movement = direction3d.normalized * getGenom().getMaxSpeed() * force * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;
           
            //Debug.DrawLine(transform.position, transform.position + vectorToNearFood, Color.green);

            genome.doStep(force, Time.deltaTime);
            if (genome.isDied())
            {
                Destroy(this.gameObject);
            }

            float currentScale = transform.localScale.y;
            float newScale = 1 + (genome.size/5);
            transform.localScale = new Vector3(newScale, newScale, newScale);
            if (newScale > currentScale)
            {
                newPosition.y = newScale;
            }
            transform.position = newPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Food") {
            eat();
            Evolution.Instance.publishToLeaderBoard(this);
            Destroy(other.gameObject);
        }

        if (other.tag == "Cell")
        {
            Cell cell = other.GetComponent<Cell>();
            eatCell(cell);
            Evolution.Instance.publishToLeaderBoard(this);
        }

        if (other.tag == "Ground") {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Cell")
        {
            Cell cell = other.GetComponent<Cell>();
            eatCell(cell);
            Evolution.Instance.publishToLeaderBoard(this);
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

        //return new Vector3(0, 0, 0);
        return toFood;
    }

    private Vector3 getVectorToNearCell()
    {
        Cell[] cells = GameObject.FindObjectsOfType<Cell>();

        Vector3 toCell;
        if (cells.Length > 1)
        {
            KeyValuePair<float, Cell> nearCell = new KeyValuePair<float, Cell>(1000, this);

            foreach (Cell cell in cells)
            {
                if(cell == this)
                {
                    continue;
                }

                Vector3 toMove = cell.transform.position - transform.position;
                if (toMove.magnitude < nearCell.Key)
                {
                    nearCell = new KeyValuePair<float, Cell>(toMove.magnitude, cell);
                }
            }

            toCell = nearCell.Value.transform.position - transform.position;
        }
        else
        {
            toCell = new Vector3(0, 0, 0);
        }

        return toCell;
    }

    private void eat() {
        genome.eat(1);

        if (genome.isReadyToDivide())
        {
            Evolution.Instance.cellDivides(this);
        }
    }

    private void eatCell(Cell cell)
    {
        float energy = cell.damage(genome.getDamage());
        genome.eat(energy, false);
        if (genome.isReadyToDivide())
        {
            Evolution.Instance.cellDivides(this);
        }
    }

    public float damage(float damage)
    {
        float lossEnergy = Mathf.Max(0, damage - genome.getArmor());
        genome.size -= lossEnergy;
        return lossEnergy;
    }

    public Genom getGenom() {
        return genome;
    }

    public void setGenom(Genom g)
    {
        genome = g;
        gameObject.GetComponent<Renderer>().material.color = genome.color;
    }

    public void divide() {
        genome.divide();
    }
}
