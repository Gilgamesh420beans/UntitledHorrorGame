using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour {

	private static GridManager s_Instance = null;

	public static GridManager instance {
		get {
			if (s_Instance == null) {
				s_Instance = FindObjectOfType(typeof(GridManager))
					as GridManager;
				if (s_Instance == null)
					Debug.Log("Could not locate a GridManager " +
					          "object. \n You have to have exactly " +
					          "one GridManager in the scene.");
			}
			return s_Instance;
		}
	}

	public int numOfRows;
	public int numOfColumns;
	public float gridCellSize;
	public bool showGrid = true;
	public bool showObstacleBlocks = true;
	public bool allowDiagonal = true;
	public Color gridColor = Color.blue;
	private Vector3 origin = new Vector3();
	List<Vector3> obstacleBlockList;

	public Node[,] nodes { get; set; }

	public Vector3 Origin {
		get { return origin; }
	}

	void Awake() {
		obstacleBlockList = new List<Vector3>();
		CalculateObstacles();
	}

	// Find all the obstacles on the map
	void CalculateObstacles() {
		nodes = new Node[numOfColumns, numOfRows];
		int index = 0;
		for (int i = 0; i < numOfRows; i++) {
			for (int j = 0; j < numOfColumns; j++) {
				Vector3 cellPos = GetGridCellCenter(index);
				Node node = new Node(cellPos);
				nodes[j, i] = node;

				// cast a ray from above the center of each cell
				RaycastHit hit;
				if (Physics.Raycast(cellPos + new Vector3(0, 10f, 0), -Vector3.up, out hit, 10f))
				{
					// if it collides with an object tagged 'Obstacle' mark the cell
					// as being an obstacle
					if (hit.collider.gameObject.tag == "Obstacle")
					{
						nodes[j, i].MarkAsObstacle();
						obstacleBlockList.Add(cellPos);
					}
				}
				index++;
			}
		}
	}

	public Vector3 GetGridCellCenter(int index) {
		Vector3 cellPosition = GetGridCellPosition(index);
		cellPosition.x += (gridCellSize / 2.0f);
		cellPosition.z += (gridCellSize / 2.0f);
		return cellPosition;
	}

	public Vector3 GetGridCellPosition(int index) {
		int row = GetRow(index);
		int col = GetColumn(index);
		float xPosInGrid = col * gridCellSize;
		float zPosInGrid = row * gridCellSize;
		return Origin + new Vector3(xPosInGrid, 0.0f, zPosInGrid);
	}

	public int GetGridIndex(Vector3 pos) {
		if (!IsInBounds(pos)) {
			return -1;
		}
		pos -= Origin;
		int col = (int)(pos.x / gridCellSize);
		int row = (int)(pos.z / gridCellSize);
		return (row * numOfColumns + col);
	}

	public bool IsInBounds(Vector3 pos) {
		float width = numOfColumns * gridCellSize;
		float height = numOfRows* gridCellSize;

		return ((pos.x >= Origin.x) && (pos.x <= Origin.x + width) && (pos.z <= Origin.z + height) && (pos.z >= Origin.z));
	}

	public int GetRow(int index) {
		int row = index / numOfColumns;
		return row;
	}

	public int GetColumn(int index) {
		int col = index % numOfColumns;
		return col;
	}

	public void GetNeighbours(Node node, ArrayList neighbors) {
		Vector3 neighborPos = node.position;
		int neighborIndex = GetGridIndex(neighborPos);

		int row = GetRow(neighborIndex);
		int column = GetColumn(neighborIndex);

		if (allowDiagonal) {
			for (int i=row-1; i<=row+1; i++) {
				for (int j=column-1; j<=column+1; j++) {
					if (!((i==row) && (j==column))) {
						AssignNeighbour(i, j, neighbors);
					}
				}
			}
		}
		else {
			AssignNeighbour(row-1, column, neighbors);
			AssignNeighbour(row+1, column, neighbors);
			AssignNeighbour(row, column-1, neighbors);
			AssignNeighbour(row, column+1, neighbors);
		}
	}

	void AssignNeighbour(int row, int column, ArrayList neighbors) {
		if (row != -1 && column != -1 &&
		    row < numOfRows && column < numOfColumns) {

			Node nodeToAdd = nodes[column, row];
			if (!nodeToAdd.bObstacle) {
				neighbors.Add(nodeToAdd);
			}
		}
	}

	void OnDrawGizmos() {
		if (showGrid) {
			DebugDrawGrid(transform.position, numOfRows, numOfColumns, gridCellSize, gridColor);
		}

		if (showObstacleBlocks) {
			Vector3 cellSize = new Vector3(gridCellSize, 1.0f, gridCellSize);
			Gizmos.color = new Color(1, 0, 0, 0.5f);
			if (obstacleBlockList != null) {
				foreach (Vector3 data in obstacleBlockList) {
					Gizmos.DrawCube(GetGridCellCenter(GetGridIndex(data)), cellSize);
				}
			}
		}
	}

	public void DebugDrawGrid(Vector3 origin, int numRows, int numCols,float cellSize, Color color) {
		float width = (numCols * cellSize);
		float height = (numRows * cellSize);
		// Draw the horizontal grid lines
		for (int i = 0; i < numRows + 1; i++) {
			Vector3 startPos = new Vector3(0.0f, 0.05f, 0.0f) + i * cellSize * new Vector3(0.0f, 0.0f, 1.0f);
			Vector3 endPos = startPos + width * new Vector3(1.0f, 0.0f, 0.0f);
			Debug.DrawLine(startPos, endPos, color);
		}

		// Draw the vertial grid lines
		for (int i = 0; i < numCols + 1; i++) {
			Vector3 startPos = new Vector3(0.0f, 0.05f, 0.0f) + i * cellSize * new Vector3(1.0f, 0.0f, 0.0f);
			Vector3 endPos = startPos + height * new Vector3(0.0f, 0.0f, 1.0f);
			Debug.DrawLine(startPos, endPos, color);
		}
	}	
}

