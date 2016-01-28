using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{

    private int width;
    private int height;
    private Node[,] nodes;
    private Node startNode;
    private Node endNode;
    private List<Node> resetNodes;
    private bool easiestRoute;
    private List<Node> nextNodes;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    // Sets the map for pathing, must be called before running the pathing.
    // map: array of tiles
    // easiestRoute: should the easiest route be located (as little climbing as possible)
    // heights: list of height values for the cells, in order
    public void SetMap(GameObject[,] map, bool easiestRoute, List<int> heights)
    {
        this.easiestRoute = easiestRoute;
        resetNodes = new List<Node>();
        nextNodes = new List<Node>();
        this.width = map.GetLength(0);
        this.height = map.GetLength(1);
        this.nodes = new Node[this.width, this.height];
        if (easiestRoute) //save heights to nodes for easiest route
        {
            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    nodes[x, y] = new Node(x, y, heights[x + 1 * y + 1]);
                }
            }
        }
        else
        {
            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    this.nodes[x, y] = new Node(x, y, true); // TRUE = Walkable for all tiles
                }
            }
        }

    }

    // Starts the pathing
    public List<Point> StartAStar(int startX, int startY, int endX, int endY)
    {
        this.startNode = this.nodes[startX, startY];
        startNode.State = NodeState.Open;
        resetNodes.Add(startNode);
        this.endNode = this.nodes[endX, endY];

        return FindPath();
    }

    /// <summary>
    /// Attempts to find a path from the start location to the end location based on the supplied SearchParameters
    /// </summary>
    /// <returns>A List of Points representing the path. If no path was found, the returned list is empty.</returns>
    public List<Point> FindPath()
    {
        // The start node is the first entry in the 'open' list
        List<Point> path = new List<Point>();
        bool success = Search(startNode);
        if (success)
        {
            // If a path was found, follow the parents from the end node to build a list of locations
            Node node = this.endNode;
            while (node.ParentNode != null)
            {
                path.Add(node.Location);
                node = node.ParentNode;
            }

            // Reverse the list so it's in the correct order when returned
            path.Reverse();
        }
        /* // If the map is not reloaded after every pathing, the NodeStates must be reset
        foreach (Node n in resetNodes)
        {
            n.State = NodeState.Untested;
        }
        resetNodes.Clear();
        */
        nextNodes.Clear();
        return path;
    }

    public bool Search(Node currentNode)
    {
        // Set the current node to Closed since it cannot be traversed more than once
        currentNode.State = NodeState.Closed;
        nextNodes.Remove(currentNode);

        //nextNodes = GetAdjacentWalkableNodes(currentNode);
        List<Node> moreNodes = GetAdjacentWalkableNodes(currentNode); // Look for more nodes around the currentNode
        foreach (Node n in moreNodes)
        {
            nextNodes.Add(n); // Add the new nodes to possible nodes to be checked, so the cheapest non-checked node can be found faster. Topias
        }

        // Calculate traversalcost
        foreach (Node n in nextNodes)
        {
            n.H = Node.GetTraversalCost(n.Location, endNode.Location);
        }


        // Sort by F-value so that the shortest possible routes are considered first
        if (easiestRoute) nextNodes.Sort((node1, node2) => node1.G.CompareTo(node2.G)); // When looking for the easiest path, only the climbed height (G here) Topias
        else nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
        foreach (var nextNode in nextNodes)
        {
            // Check whether the end node has been reached
            if (nextNode.Location.Equals(this.endNode.Location))
            {
                return true;
            }
            else
            {
                // If not, check the next set of nodes
                if (Search(nextNode)) // Note: Recurses back into Search(Node)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns any nodes that are adjacent to <paramref name="fromNode"/> and may be considered to form the next step in the path
    /// </summary>
    /// <param name="fromNode">The node from which to return the next possible nodes in the path</param>
    /// <returns>A list of next possible nodes in the path</returns>
    private List<Node> GetAdjacentWalkableNodes(Node fromNode)
    {
        List<Node> walkableNodes = new List<Node>();
        IEnumerable<Point> nextLocations = GetAdjacentLocations(fromNode.Location);

        foreach (var location in nextLocations)
        {
            int x = location.x;
            int y = location.y;

            // Stay within the grid's boundaries
            if (x < 0 || x >= this.width || y < 0 || y >= this.height)
                continue;

            Node node = this.nodes[x, y];
            // Ignore non-walkable nodes
            if (!node.IsWalkable)
                continue;

            // Ignore already-closed nodes
            if (node.State == NodeState.Closed)
                continue;

            // Already-open nodes are only added to the list if their G-value is lower going via this route.
            if (node.State == NodeState.Open)
            {
                if (easiestRoute) // Use GetTraversalHeight() and ParentNodeHeight when looking for the easiest route. Topias
                {
                    float traversalCost = Node.GetTraversalHeight(fromNode, node);
                    float gTemp = fromNode.G + traversalCost;
                    if (gTemp < node.G)
                    {
                        node.ParentNodeHeight = fromNode;
                        walkableNodes.Add(node);
                    }
                }
                else
                {
                    float traversalCost = Node.GetTraversalCost(node.Location, node.ParentNode.Location);
                    float gTemp = fromNode.G + traversalCost;
                    if (gTemp < node.G)
                    {
                        node.ParentNode = fromNode;
                        walkableNodes.Add(node);
                    }
                }

            }
            else
            {
                // If it's untested, set the parent and flag it as 'Open' for consideration
                if (easiestRoute) node.ParentNodeHeight = fromNode; // Again, ParentNodeHeight with easiest route. Topias
                else node.ParentNode = fromNode;
                node.State = NodeState.Open;
                walkableNodes.Add(node);

                //resetNodes.Add(node);
            }
        }

        return walkableNodes;
    }

    /// <summary>
    /// Returns the eight locations immediately adjacent (orthogonally and diagonally) to <paramref name="fromLocation"/>
    /// </summary>
    /// <param name="fromLocation">The location from which to return all adjacent points</param>
    /// <returns>The locations as an IEnumerable of Points</returns>
    private static IEnumerable<Point> GetAdjacentLocations(Point fromLocation)
    {
        return new Point[]
        {
            // Only using vertically and horizontally adjacent nodes. Topias
                //new Point(fromLocation.x-1, fromLocation.y-1),
                new Point(fromLocation.x-1, fromLocation.y  ),
                //new Point(fromLocation.x-1, fromLocation.y+1),
                new Point(fromLocation.x,   fromLocation.y+1),
                //new Point(fromLocation.x+1, fromLocation.y+1),
                new Point(fromLocation.x+1, fromLocation.y  ),
                //new Point(fromLocation.x+1, fromLocation.y-1),
                new Point(fromLocation.x,   fromLocation.y-1)
        };
    }

    public struct Point
    {
        public int x, y;
        public Point(int px, int py)
        {
            x = px;
            y = py;
        }
    }

    public enum NodeState
    {
        Untested,
        Open,
        Closed
    }

    public class Node
    {
        private Node parentNode;

        /// <summary>
        /// The node's location in the grid
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// True when the node may be traversed, otherwise false
        /// </summary>
        public bool IsWalkable { get; set; }

        /// <summary>
        /// Cost from start to here
        /// </summary>
        public float G { get; set; }

        /// <summary>
        /// Estimated cost from here to end
        /// </summary>
        public float H { get; set; }

        /// <summary>
        /// Flags whether the node is open, closed or untested by the PathFinder
        /// </summary>
        public NodeState State { get; set; }

        public int NodeHeight { get; set; }

        /// <summary>
        /// Estimated total cost (F = G + H)
        /// </summary>
        public float F
        {
            get { return this.G + this.H; }
        }

        /// <summary>
        /// Gets or sets the parent node. The start node's parent is always null.
        /// </summary>
        public Node ParentNode
        {
            get { return this.parentNode; }
            set
            {
                // When setting the parent, also calculate the traversal cost from the start node to here (the 'G' value)
                this.parentNode = value;

                this.G = this.parentNode.G + GetTraversalCost(this.Location, this.parentNode.Location);
            }
        }

        // GetTraversalHeigh() with easiest path Topias
        public Node ParentNodeHeight
        {
            get { return this.parentNode; }
            set
            {
                this.parentNode = value;
                this.G = this.parentNode.G + GetTraversalHeight(this.parentNode, this);
            }
        }

        /// <summary>
        /// Creates a new instance of Node.
        /// </summary>
        /// <param name="x">The node's location along the X axis</param>
        /// <param name="y">The node's location along the Y axis</param>
        /// <param name="isWalkable">True if the node can be traversed, false if the node is a wall</param>
        /// <param name="endLocation">The location of the destination node</param>
        public Node(int x, int y, bool isWalkable)
        {
            this.Location = new Point(x, y);
            this.State = NodeState.Untested;
            this.IsWalkable = isWalkable;
            //this.H = GetTraversalCost(this.Location, endLocation);
            this.G = 0;
        }
        // Save the height parameter, every node is walkable.
        // Don't calculate distance between all the nodes and the end location, should be wasted effort for most nodes, rather do the calculation when it is needed.
        // Topias
        public Node(int x, int y, int height)
        {
            this.Location = new Point(x, y);
            this.State = NodeState.Untested;
            this.NodeHeight = height;
            this.IsWalkable = true;
            //this.H = GetTraversalCost(this.Location, endLocation);
            this.G = 0;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}: {2}", this.Location.x, this.Location.y, this.State);
        }

        /// <summary>
        /// Gets the distance between two points
        /// </summary>
        public static float GetTraversalCost(Point location, Point otherLocation)
        {
            float deltaX = otherLocation.x - location.x;
            float deltaY = otherLocation.y - location.y;
            return (float)System.Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        // Height needed to climp between nodes.
        // Topias
        public static float GetTraversalHeight(Node currentNode, Node nextNode)
        {
            if (currentNode.NodeHeight < nextNode.NodeHeight) return nextNode.NodeHeight - currentNode.NodeHeight;
            return 0f;
        }



    }
}
