using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GridTest
{
    public class GameLogic : MonoBehaviour
    {

        public GameObject CellPrefab;
        private GameObject[,] Grid;
        private GameObject lastClicked;
        private GameObject newClicked;
        private int pathCost;
        List<AStar.Point> path;
        private bool easiestRoute;
        private string route;
        private List<int> heights;

        // Use this for initialization
        void Start()
        {
            easiestRoute = false;
            route = "Easiest Route";
            pathCost = 0;
            path = new List<AStar.Point>();
            Grid = new GameObject[50, 50];
            heights = new List<int>();
            GenerateGrid();
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 30, 100, 20), "Path cost: " + pathCost);
            if (GUILayout.Button(route))
            {
                easiestRoute = !easiestRoute;
                if (route == "Easiest Route") route = "Fastest Route";
                else route = "Easiest Route";
            }
        }

        private void GenerateGrid()
        {
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    GameObject go;
                    go = Instantiate(CellPrefab, new Vector3(i, j), transform.rotation) as GameObject;
                    go.transform.parent = transform;
                    int random = UnityEngine.Random.Range(0, 127);
                    go.GetComponent<CellScript>().SetHeight(random);
                    heights.Add(random);
                    Grid[i, j] = go;
                }
            }
        }

        // Called from CellScript, when a cell is clicked.
        // Handles currently selected cells and calls AStarSearch() to initiate pathing.
        public void newClick(GameObject source)
        {
            if (lastClicked != null) lastClicked.GetComponent<CellScript>().loseSelection();
            if (newClicked != null) lastClicked = newClicked;
            newClicked = source;
            if (lastClicked != null) AStarSearch(lastClicked, newClicked);

        }

        // Clears the previous path, searches the new path using AStar.StartAStar(), tells cells on new path to draw a sphere and counts the path cost.
        void AStarSearch(GameObject start, GameObject finish)
        {
            foreach (AStar.Point point in path)
            {
                Grid[point.x, point.y].GetComponent<CellScript>().remmovePath();
            }

            this.GetComponent<AStar>().SetMap(Grid, easiestRoute, heights);
            path = GetComponent<AStar>().StartAStar((int)start.transform.position.x, (int)start.transform.position.y, (int)finish.transform.position.x, (int)finish.transform.position.y);
            pathCost = 0;
            int currentHeight = 0;
            int prevHeight = lastClicked.GetComponent<CellScript>().Height;
            //lastClicked.GetComponent<CellScript>().isPath();
            foreach (AStar.Point point in path)
            {
                currentHeight = Grid[point.x, point.y].GetComponent<CellScript>().Height;
                Grid[point.x, point.y].GetComponent<CellScript>().isPath();
                if (prevHeight < currentHeight) pathCost = pathCost + currentHeight - prevHeight;
                prevHeight = currentHeight;
            }
        }
    }


}