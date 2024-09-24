using UnityEngine;
using System.Collections;

public class AStar {

	public static List closedList, openList;

	private static float CalculateEuclideanCost(Node startNode, Node endNode) {
		// find vector between positions
		Vector3 vecCost = startNode.position - endNode.position;
		// return the magnitude
		return vecCost.magnitude;
	}


	public static ArrayList FindPath(Node start, Node goal) {

		// create the closed list of nodes, initially empty
		closedList = new List();

		// create the open list of nodes
		openList = new List();

		// A. add the start node to the open list

		openList.Add(start);

		// B. Calculate h, g and f of start node
		start.g = 0.0f;
		start.h = CalculateEuclideanCost(start, goal);
		start.f = start.g + start.h;

		Node currentNode = null;

		// while (we have not reached our goal (openList.Length != 0))
		while (openList.Length != 0) {

			// C. consider the best node in the open list (the node with the lowest f value) - call it the current node
		    currentNode = openList.First();

            
			// if the current node is the goal then we're done
			if (currentNode.position == goal.position) {
				return CalculatePath(currentNode);
			}

			// D. Move the current node to the closed list (& remove it from the open list)
            closedList.Add(currentNode);
			openList.Remove(currentNode);

			// get all of the current nodes neighbors
			ArrayList neighbours = new ArrayList();
			GridManager.instance.GetNeighbours(currentNode, neighbours);
	
			// for each neighbor node
			for (int i = 0; i < neighbours.Count; i++) {
				
				Node neighbourNode = (Node)neighbours[i];

				// if the neighbour node is in the closed list then ignore it (continue)
				if (closedList.Contains(neighbourNode)) {
					continue;
				}

				// if the neighbour node is not on the open list add it
				if (!openList.Contains(neighbourNode)) {
					// E. Set the parent of the neighbour node to be the current node
				    neighbourNode.parent = currentNode;

                    // F. Calculate g, h and f of neighbourNode
                    neighbourNode.g = currentNode.g + CalculateEuclideanCost(neighbourNode, currentNode);
                    neighbourNode.h = CalculateEuclideanCost(neighbourNode, goal);
                    neighbourNode.f = neighbourNode.g + neighbourNode.h;

                    // G. Add neighbourNode to open list
                    openList.Add(neighbourNode);
                }
				// if it's already on the open list, check to see if this path to the node is better
				else {

					float gTentative = 0f;

                    // H. calculate what the g value would be if we go through the current node
                    gTentative = currentNode.g + CalculateEuclideanCost(neighbourNode, currentNode);

                    // if the path is shorter
                    if (gTentative < neighbourNode.g) {
                        // I. set the parent of neighbourNode to be the currentNode
                        neighbourNode.parent = currentNode;

                        // J. Set g value of neighbourNode to gTentative, recalculate h and f for neighbourNode
                        // Update the g value to the tentative g value
                        neighbourNode.g = gTentative;

                        // Recalculate the h value (heuristic)
                        neighbourNode.h = CalculateEuclideanCost(neighbourNode, goal);

                        // Recalculate the f value
                        neighbourNode.f = neighbourNode.g + neighbourNode.h;


                        // sort the list to put best f value at start
                        openList.Sort();
					}
				}
			}
		}

		if ((currentNode == null) || (currentNode.position != goal.position)) {
			Debug.LogError("Goal Not Found");
			return null;
		}

		return CalculatePath(currentNode);
	}
	

	private static ArrayList CalculatePath(Node node) {
		ArrayList list = new ArrayList();
		while (node != null) {
			list.Add(node);
			node = node.parent;
		}
		list.Reverse();
		return list;
	}
}
