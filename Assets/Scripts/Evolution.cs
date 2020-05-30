using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evolution : Singleton<Evolution>
{
    public GameObject cellPrefab;
    public GameObject foodPrefab;


    private List<Genom> leaderBoard = new List<Genom>();
    private float noFoodTime;


    private int leaderBoardSize = 10;

    public float timeToNewFood = .2f;

    public int newFoodQtyToGenerateDuringGeneration = 1;

    public int minCellQty = 0;

    public int foodQtyInNewGeneration = 10;
    public int cellQtyInNewGeneration = 10;

    private int worldSize = 50;


    protected Evolution() { }

    void Start()
    {
        noFoodTime = 0f;
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
            leadersStatistics += genom.getScore() + " ";
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
            Vector3 newPosition = new Vector3(Random.value * worldSize - worldSize/2, .5f, Random.value * worldSize - worldSize / 2);
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
        if (!leaderBoard.Contains(cell.getGenom()))
        {
            if (leaderBoard.Count < leaderBoardSize)
            {
                leaderBoard.Add(cell.getGenom());
            }
            else if (leaderBoard[leaderBoard.Count - 1].getScore() < cell.getGenom().getScore())
            {
                leaderBoard[leaderBoard.Count - 1] = cell.getGenom();
            }
        }

        leaderBoard.Sort((x, y) => y.getScore().CompareTo(x.getScore()));
    }

    public void cellDivides(Cell cell) {
        cell.divide();
        generateCell(cell.getGenom(), true);
    }

    private void generateNewAgeCell() {
        Genom genom;
        if (leaderBoard != null && leaderBoard.Count > 0)
        {
            genom = leaderBoard[Random.Range(0, leaderBoard.Count)];
        }
        else
        {
            genom = null;
        }
        generateCell(genom);
    }

    private void generateCell(Genom genom, bool mutationOnly = false) {
        GameObject newCell = Instantiate(cellPrefab);
        
        if (genom != null)
        {
            Cell nc = newCell.GetComponent<Cell>();

            Genom g = genom.clone();
            if (mutationOnly || Random.value > 0.5)
            {
                nc.getGenom().mutate();
            }
            else if(leaderBoard.Count > 1)
            {
                nc.getGenom().crossWith(leaderBoard[Random.Range(0, leaderBoard.Count)]);
            }

            nc.setGenom(g);
        }
       
        Vector3 newPosition = new Vector3(Random.value * worldSize - worldSize / 2, 1, Random.value * worldSize - worldSize / 2);
        newCell.transform.SetPositionAndRotation(newPosition, Quaternion.identity);
    }


}
