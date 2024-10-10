using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;

public class Pathfinder : MonoBehaviour
{
    public enum PathfindingState
    {
        Active,    // When pathfinding is running
        Paused     // When pathfinding is paused (target unreachable)
    }

    private Transform startPos, endPos;
    public Node startNode { get; set; }
    public Node goalNode { get; set; }

    public ArrayList pathArray;

    public PathfindingState pathfindingState = PathfindingState.Active;  // Default state is Active

    GameObject objStart, objEnd;  // Monster and current target
    private float elapsedTime = 0.0f;
    public float intervalTime = 1.0f;

    public string currentAlgorithm = "astar";  // Default algorithm

    void Start()
    {
        // Set the start position to the monster
        objStart = GameObject.FindGameObjectWithTag("Monster1");

        // Set the default target to the Player
        objEnd = GameObject.FindGameObjectWithTag("Player");
        pathArray = new ArrayList();

        // Initial pathfinding call with the default player target
        FindPath(currentAlgorithm, objEnd);
    }

    void Update()
    {
        if (pathfindingState == PathfindingState.Paused)
        {
            //Debug.Log("Pathfinding is paused. No calculations being performed.");
            return;  // Skip pathfinding when paused
        }

        elapsedTime += Time.deltaTime;

        // Recalculate the path at regular intervals
        if (elapsedTime >= intervalTime)
        {
            elapsedTime = 0.0f;
            // Use the same target previously set by FindPath and continue pathfinding
            FindPath(currentAlgorithm, objEnd);  // objEnd is unchanged unless FindPath is called from an external source
        }
    }

    // Function to resume pathfinding when it is paused
    public void ResumePathfinding()
    {
        if (pathfindingState == PathfindingState.Paused)
        {
            //Debug.Log("Resuming pathfinding.");
            pathfindingState = PathfindingState.Active;  // Set pathfinding state to active
        }
    }
    // Modified FindPath function with two inputs: algorithm type and target object
    public void FindPath(string algorithmType, GameObject target)
    {
        if (pathfindingState == PathfindingState.Paused)
        {
            //Debug.LogWarning("Pathfinding is paused. Cannot calculate path.");
            return;
        }

        // Set the start position (the monster)
        startPos = objStart.transform;

        // Set the target as the end position only if a valid target is provided
        if (target != null)
        {
            endPos = target.transform;
            objEnd = target;  // Update objEnd to reflect the new target
        }
        else
        {
            //Debug.LogError("Target is null! Cannot calculate path.");
            return;
        }

        // Get the start and goal nodes based on grid positions
        int startGridIndex = GridManager.instance.GetGridIndex(startPos.position);
        int goalGridIndex = GridManager.instance.GetGridIndex(endPos.position);

        if (startGridIndex == -1 || goalGridIndex == -1)
        {
            //Debug.LogError("Either the start or goal position is out of bounds.");
            pathfindingState = PathfindingState.Paused;  // Pause pathfinding if the start or goal is out of bounds
            return;
        }

        startNode = new Node(GridManager.instance.GetGridCellCenter(startGridIndex));
        goalNode = new Node(GridManager.instance.GetGridCellCenter(goalGridIndex));

        // Check if the goal node is valid (i.e., not blocked or out of bounds)
        if (goalNode == null || goalNode.bObstacle)
        {
            //Debug.LogError("Goal node is invalid or blocked. Pathfinding paused.");
            pathfindingState = PathfindingState.Paused;  // Pause pathfinding if the goal is blocked
            return;
        }

        // Switch between different algorithms based on the input string
        switch (algorithmType.ToLower())
        {
            case "astar":
                //Debug.Log("Using A* algorithm");
                pathArray = AStar.FindPath(startNode, goalNode);
                break;

            case "breadthfirst":
                Debug.Log("Using Breadth-First Search algorithm");
                // pathArray = BreadthFirstSearch.FindPath(startNode, goalNode);
                break;

            case "righthandrule":
                Debug.Log("Using Right-Hand Rule algorithm");
                // pathArray = RightHandRule.FindPath(startNode, goalNode);
                break;

            default:
                Debug.LogError("Unknown algorithm type: " + algorithmType);
                break;
        }

        // Check if a valid path was found
        if (pathArray == null || pathArray.Count == 0)
        {
            //Debug.LogError("No valid path found. Pathfinding paused.");
            pathfindingState = PathfindingState.Paused;  // Pause pathfinding if no valid path found
        }
        else
        {
            pathfindingState = PathfindingState.Active;  // Resume pathfinding when a valid path is found
        }
    }

    // Draw path gizmo for debugging purposes
    void OnDrawGizmos()
    {
        if (pathArray == null)
            return;

        if (pathArray.Count > 0)
        {
            int index = 1;
            foreach (Node node in pathArray)
            {
                if (index < pathArray.Count)
                {
                    Node nextNode = (Node)pathArray[index];
                    Debug.DrawLine(node.position + new Vector3(0.0f, 0.05f, 0.0f), nextNode.position + new Vector3(0.0f, 0.05f, 0.0f), Color.green);
                    index++;
                }
            }
        }
    }
}
