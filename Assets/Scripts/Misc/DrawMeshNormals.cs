using UnityEngine;
using System.Collections;

/** This script draws the normals of a gameobject that has a meshfilter associated with it.
 * The normals are shown in the "Scene" view as opposed to the running game.
 * This makes it useful to visualise the normals of a "chunk" and ensure they are correct. 
 */
[RequireComponent (typeof (MeshFilter))]
public class DrawMeshNormals : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		
	}

	// Update is called once per frame
	void Update ()
	{
		MeshFilter mfilter = GetComponent<MeshFilter>();
		var vertices = mfilter.mesh.vertices;
		var normals = mfilter.mesh.normals;
		
		if (normals[0] == Vector3.zero) Debug.LogWarning("zero length normal");
		else 
			for (long i =0; i < vertices.Length; i++) {
				
				Debug.DrawRay(transform.position+vertices[i], normals[i]);
				
			}
	}
}

