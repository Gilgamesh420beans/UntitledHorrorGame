using UnityEngine;
using System.Collections;

public class List {

	private ArrayList nodes = new ArrayList();
	
	public int Length {
		get { return this.nodes.Count; }
	}
	
	public bool Contains(object node) {
		return this.nodes.Contains(node);
	}
	
	public Node First() {
		if (this.nodes.Count > 0) {
			return (Node)this.nodes[0];
		}
		return null;
	}
	
	public void Add(Node node) {
		this.nodes.Add(node);
		this.nodes.Sort(new ListOrderComparer());
		//this.nodes.Sort ();
	}
	
	public void Remove(Node node) {
		this.nodes.Remove(node);
		this.nodes.Sort(new ListOrderComparer());
		//this.nodes.Sort ();
	}

	public void Sort() {
		this.nodes.Sort(new ListOrderComparer());
	}

	// this function is need so the list will sort by fn
	public class ListOrderComparer : IComparer {
		static Node n1;
		static Node n2;
		
		public int Compare( object a, object b ) {
			n1 = a as Node;
			n2 = b as Node;
			
			if ( n1.f > n2.f ) return 1;
			if ( n1.f < n2.f ) return -1;
			return 0;
		}
	}
}
