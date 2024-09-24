using UnityEngine;
using System.Collections;
using System;

public class Node : IComparable {
	
	public float f;
	public float g;
	public float h;
	public bool bObstacle;
	public Node parent;
	public Vector3 position;

	public Node() {
		this.h = 0.0f;
		this.g = 1.0f;
		this.f = this.h + this.g;
		this.bObstacle = false;
		this.parent = null;
	}

	public Node(Vector3 pos) {
		this.h = 0.0f;
		this.g = 1.0f;
		this.f = this.h + this.g;
		this.bObstacle = false;
		this.parent = null;
		this.position = pos;
	}

	public void MarkAsObstacle() {
		this.bObstacle = true;
	}

	public int CompareTo(object obj) {
		Node node = (Node)obj;
		// Negative value means object comes before this in the sort
		// order.
		if (this.h < node.h)
			return -1;
		// Positive value means object comes after this in the sort
		// order.
		if (this.h > node.h) return 1;
		return 0;
	}
}
