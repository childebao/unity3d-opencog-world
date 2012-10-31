using UnityEngine;

using System.Collections.Generic;
using System.Collections;

public class OCObjectRepository : MonoBehaviour
{
	static OCObjectRepository ocor = null;
	public static OCObjectRepository get() {
		if (ocor == null) {
			GameObject OCObjects = GameObject.Find("Objects") as GameObject;
        	ocor = OCObjects.GetComponent<OCObjectRepository>();
		}
		return ocor;
	}
	
    public void Start () {
        Initiate();
    }

    public void Initiate(){
		WorldGameObject theWorld = GameObject.Find("World").GetComponent<WorldGameObject>();
        //foreach(Transform child in transform) {
			//Objects.Add(child.gameObject.GetInstanceID(), child.gameObject);
			// Unfortunately this doesn't yet work because chunks are generated after the game starts.
			
			//child.position = theWorld.getGroundBelowPoint(child.position) + Vector3.up;
			//Debug.Log("Set start position for " + child.gameObject.name + " to " + child.position);
			
        //}
    }
	
	public void DumpToLog() {
		GameObject[] ocobjects = GameObject.FindGameObjectsWithTag("OCObject");
		foreach(GameObject go in ocobjects) {
            Debug.Log(go.name + "[" + go.GetInstanceID() + "]");
        }
	}

    public GameObject GetOCObject(int id){
		GameObject[] ocobjects = GameObject.FindGameObjectsWithTag("OCObject");
		foreach(GameObject go in ocobjects) {
			if (id == go.GetInstanceID()) return go;
        }
		return null;
    }

    public GameObject[] GetAllObjects(){
		GameObject[] ocobjects = GameObject.FindGameObjectsWithTag("OCObject");
        return ocobjects;
    }

    public GameObject GetObjectByName(string name)
    {
		GameObject go = GameObject.Find(name);
        if (go.tag == "OCObject") return go;
        else return null;
    }
}
