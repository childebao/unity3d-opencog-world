using UnityEngine;
using System.Collections;

public class BatterySpin : MonoBehaviour {
	private float rotationY;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Transform p = transform.parent;
		transform.RotateAround(p.position,  p.up, 60* Time.deltaTime);
	}
}
