using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour {
	public float spinSpeed = 90f;
	public float rotation = 0.0f;
	public float objectHeight = 0.0f;
	private Vector3 spinnerSize;
	
	// Use this for initialization
	void Start () {
		gameObject.transform.position = transform.parent.position;
		calcSpinnerSize();
		// get parents bounding box height
		if (objectHeight == 0.0)
            calcObjectHeight();
		//Debug.Log("height is " + objectHeight);
		//Debug.Log("spinner size is " + spinnerSize);
		//Debug.Log("parent position is " + transform.parent.position);
		transform.position = transform.parent.position + spinnerSize + (Vector3.up * 1.01f * objectHeight);
		//Debug.Log("transform position is " + transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		// Reposition Spinner, since the object it's on might rotate due to physics.
		transform.position = transform.parent.position + spinnerSize + (Vector3.up * 1.01f * objectHeight);
		//Debug.Log("set position of spinner to " + gameObject.transform.position);
		rotation += spinSpeed * Time.deltaTime;
		if (rotation > 360f) {
			rotation = rotation % 360f;
		}
		gameObject.transform.eulerAngles = new Vector3(0, rotation,0);
	
	}

    public void calcObjectHeight()
    {
        objectHeight = VerticalSizeCalculator.getHeight(transform.parent, transform);
    }
	
	void calcSpinnerSize()
	{
		var m = gameObject.GetComponentInChildren<MeshRenderer>();
		spinnerSize = new Vector3(0.0f,m.bounds.size.y,0.0f);
	}
}
