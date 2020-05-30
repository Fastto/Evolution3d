using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evolution : Singleton<Evolution>
{
    public GameObject cellPrefab;
    public GameObject foodPrefab;

    private List<Genom> leaderBoard;
    private int leaderBoardSize;

    private float timeToNewFood;
    private float noFoodTime;

    private int minCellQty;

    private int foodQtyInNewGeneration;
    private int cellQtyInNewGeneration;
    private int newFoodQtyToGenerateDuringGeneration;

    protected Evolution() { }

    private void Awake()
    {
        leaderBoard = new List<Genom>();
        noFoodTime = 0f;

        minCellQty = 1;

        leaderBoardSize = 10;

        foodQtyInNewGeneration = 10;
        cellQtyInNewGeneration = 10;

        timeToNewFood = .2f;
        newFoodQtyToGenerateDuringGeneration = 1;
    }

    void Start()
    {
        newGeneration();
    }

    void Update()
    {
    
    }

    private void FixedUpdate()
    {
        noFoodTime += Time.deltaTime;

        if (noFoodTime > timeToNewFood)
        {
            generateFood(newFoodQtyToGenerateDuringGeneration);
        }

        Cell[] cells = GameObject.FindObjectsOfType<Cell>();
        if (cells.Length <= minCellQty)
        {
            newGeneration();
        }
    }

    private void newGeneration() {
        cleanScene();

        generateFood(foodQtyInNewGeneration);
        generateCells(cellQtyInNewGeneration);
    
        Debug.Log(getLeadersStatistics());
    }

    private string getLeadersStatistics() {
        string leadersStatistics = "";
        foreach (Genom genom in leaderBoard)
        {
            leadersStatistics += genom.score + " ";
        }

        return leadersStatistics;
    }

    private void generateCells(int cellsQty) {
        for (int i = 0; i < cellsQty; i++)
        {
            generateNewAgeCell();
        }
    }

    private void generateFood(int qty) {
        //generate food 
        for (int i = 0; i < qty; i++)
        {
            GameObject newFood = Instantiate(foodPrefab);
            Vector3 newPosition = new Vector3(Random.value * 30 - 15, .5f, Random.value * 30 - 15);
            newFood.transform.SetPositionAndRotation(newPosition, Quaternion.identity);
        }

        noFoodTime = 0;
    }

    private void cleanScene() {
        GameObject[] allCells = GameObject.FindGameObjectsWithTag("Cell");
        GameObject[] allFood = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject gameObject in allCells) {
            Destroy(gameObject);
        }
        foreach (GameObject gameObject in allFood)
        {
            Destroy(gameObject);
        }
    }

    public void publishToLeaderBoard(Cell cell) {
        if (!leaderBoard.Contains(cell.genome))
        {
            if (leaderBoard.Count < leaderBoardSize)
            {
                leaderBoard.Add(cell.genome);
            }
            else if (leaderBoard[leaderBoard.Count - 1].score < cell.genome.score)
            {
                leaderBoard[leaderBoard.Count - 1] = cell.genome;
            }
        }

        leaderBoard.Sort((x, y) => y.score.CompareTo(x.score));
    }

    public void cellWantChild(Cell cell) {
        generateCell(cell.genome);
    }

    private void generateNewAgeCell() {
        Genom genom;
        if (leaderBoard != null && leaderBoard.Count > 0)
        {
            genom = leaderBoard[Random.Range(0, leaderBoard.Count - 1)];
        }
        else
        {
            genom = null;
        }
        generateCell(genom);
    }

    private void generateCell(Genom genom) {
        GameObject newCell = Instantiate(cellPrefab);
        
        if (genom != null)
        {
            Cell nc = newCell.GetComponent<Cell>();
            Debug.Log(nc.genome);
            nc.genome = genom.clone();
            if (Random.value > 0.5)
            {
                nc.genome.neuralNetwork.mutate(0.2, 1);
            }
            else if(leaderBoard.Count > 1)
            {
                nc.genome.neuralNetwork.crossWith(leaderBoard[Random.Range(0, leaderBoard.Count - 1)].neuralNetwork);
            }
        }
       
        Vector3 newPosition = new Vector3(Random.value * 40 - 20, 1, Random.value * 40 - 20);
        newCell.transform.SetPositionAndRotation(newPosition, Quaternion.identity);
    }


}
