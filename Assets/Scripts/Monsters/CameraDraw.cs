using UnityEngine;
using System.Collections;

public class CameraDraw : MonoBehaviour {
	
	static Material lineMaterial;
	public Pathfinder pathFinder;

	void Start() {
		GameObject objStart = GameObject.FindGameObjectWithTag("Start");
		pathFinder = objStart.GetComponent<Pathfinder>();
	}

	static void CreateLineMaterial() {
		if( !lineMaterial ) {
			//lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
			//                            "SubShader { Pass { " +
			//                            "    Blend SrcAlpha OneMinusSrcAlpha " +
			//                            "    ZWrite Off Cull Off Fog { Mode Off } " +
			//                            "    BindChannels {" +
			//                            "      Bind \"vertex\", vertex Bind \"color\", color }" +
			//                            "} } }" );
		    Shader shader = Shader.Find("Custom/LineShader");
            lineMaterial = new Material(shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
	
	void OnPostRender() {

		if (GridManager.instance.showGrid) {
			// Set your materials
			CreateLineMaterial ();
			GL.PushMatrix();
			// set the current material
			lineMaterial.SetPass( 0 );

			float width = (GridManager.instance.numOfColumns * GridManager.instance.gridCellSize);
			float height = (GridManager.instance.numOfRows * GridManager.instance.gridCellSize);

			// Draw the horizontal grid lines
			for (int i = 0; i < GridManager.instance.numOfRows + 1; i++) {
				Vector3 startPos = new Vector3(0.0f, 0.05f, 0.0f) + i * GridManager.instance.gridCellSize * new Vector3(0.0f, 0.0f, 1.0f);
				Vector3 endPos = startPos + width * new Vector3(1.0f, 0.0f, 0.0f);

				GL.Begin( GL.LINES );
				GL.Color( GridManager.instance.gridColor );
				GL.Vertex3( startPos.x, startPos.y, startPos.z );
				GL.Vertex3( endPos.x, endPos.y, endPos.z );
				GL.End();
			}
			
			// Draw the vertial grid lines
			for (int i = 0; i < GridManager.instance.numOfColumns + 1; i++) {
				Vector3 startPos = new Vector3(0.0f, 0.05f, 0.0f) + i * GridManager.instance.gridCellSize * new Vector3(1.0f, 0.0f, 0.0f);
				Vector3 endPos = startPos + height * new Vector3(0.0f, 0.0f, 1.0f);

				GL.Begin( GL.LINES );
				GL.Color( GridManager.instance.gridColor );
				GL.Vertex3( startPos.x, startPos.y, startPos.z );
				GL.Vertex3( endPos.x, endPos.y, endPos.z );
				GL.End();
			}

			// draw path

			if (pathFinder.pathArray != null) {			
				if (pathFinder.pathArray.Count > 0) {
					int index = 1;
					foreach (Node node in pathFinder.pathArray) {
						if (index < pathFinder.pathArray.Count) {
							Node nextNode = (Node)pathFinder.pathArray[index];
							//Debug.DrawLine(node.position + new Vector3(0.0f,0.05f,0.0f), nextNode.position + new Vector3(0.0f,0.05f,0.0f), Color.green);
							GL.Begin( GL.LINES );
							GL.Color( new Color(0f,1f,0f) );
							GL.Vertex3( node.position.x, node.position.y+0.05f, node.position.z );
							GL.Vertex3( nextNode.position.x, nextNode.position.y+0.05f, nextNode.position.z );
							GL.End();
							index++;
						}
					}
				}
			}

			GL.PopMatrix();
		}
	}
}
