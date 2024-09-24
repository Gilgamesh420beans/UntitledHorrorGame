using UnityEngine;
using System.Collections;

public class Pathfinder : MonoBehaviour {

	private Transform startPos, endPos;
	public Node startNode { get; set; }
	public Node goalNode { get; set; }
	
	public ArrayList pathArray;
	
	GameObject objStart, objEnd;
	private float elapsedTime = 0.0f;
	//Interval time between pathfinding
	
	public float intervalTime = 1.0f;
	
	void Start () {
		objStart = GameObject.FindGameObjectWithTag("Monster1");
		objEnd = GameObject.FindGameObjectWithTag("Player");
		
		pathArray = new ArrayList();
		FindPath();
	}
	
	void Update () {
		elapsedTime += Time.deltaTime;
		if (elapsedTime >= intervalTime) {
			elapsedTime = 0.0f;
			FindPath();
		}

		if (Input.GetButtonUp("Fire1")) {
			Plane groundPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
			
			// Generate a ray from the cursor position
			Ray RayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
			
			// Determine the point where the cursor ray intersects the plane.
			float HitDist = 0;
			
			// If the ray is parallel to the plane, Raycast will return false.
			if (groundPlane.Raycast(RayCast, out HitDist))
			{
				// Get the point along the ray that hits the calculated distance.
				Vector3 RayHitPoint = RayCast.GetPoint(HitDist);
				
				//objStart.transform.position = RayHitPoint;
			}
		}
		else if (Input.GetButtonUp("Fire2")) {
			Plane groundPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
			
			// Generate a ray from the cursor position
			Ray RayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
			
			// Determine the point where the cursor ray intersects the plane.
			float HitDist = 0;
			
			// If the ray is parallel to the plane, Raycast will return false.
			if (groundPlane.Raycast(RayCast, out HitDist))
			{
				// Get the point along the ray that hits the calculated distance.
				Vector3 RayHitPoint = RayCast.GetPoint(HitDist);
				
				//objEnd.transform.position = RayHitPoint;
			}
		}
	}
	
	public void FindPath() {
		startPos = objStart.transform;
		endPos = objEnd.transform;
		
		startNode = new Node(GridManager.instance.GetGridCellCenter(GridManager.instance.GetGridIndex(startPos.position)));
		goalNode = new Node(GridManager.instance.GetGridCellCenter(GridManager.instance.GetGridIndex(endPos.position)));
		
		pathArray = AStar.FindPath(startNode, goalNode);
	}
	
	void OnDrawGizmos() {
		if (pathArray == null)
			return;
		
		if (pathArray.Count > 0) {
			int index = 1;
			foreach (Node node in pathArray) {
				if (index < pathArray.Count) {
					Node nextNode = (Node)pathArray[index];
					Debug.DrawLine(node.position + new Vector3(0.0f,0.05f,0.0f), nextNode.position + new Vector3(0.0f,0.05f,0.0f), Color.green);
					index++;
				}
			}
		}
	}
}
