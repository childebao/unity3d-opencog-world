using UnityEngine;
using System.Collections;

/// <summary>
/// MMO style camera.
/// </summary>
public class MMOStyleCamera : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The target.
	/// </summary>
	public Transform target;
	
	
	/// <summary>
	/// The distance.
	/// </summary>
	public float distance = 5.5f;
	
	/// <summary>
	/// The x speed.
	/// </summary>
	public float xSpeed = 50.0f;
	
	/// <summary>
	/// The y speed.
	/// </summary>
	public float ySpeed = 160.0f;
	
	/// <summary>
	/// The y minimum limit.
	/// </summary>
	public int yMinLimit = -10;
	
	/// <summary>
	/// The y max limit.
	/// </summary>
	public int yMaxLimit = 80;
	
	/// <summary>
	/// The distance minimum.
	/// </summary>
	public int distanceMin = 2;
	
	/// <summary>
	/// The distance max.
	/// </summary>
	public int distanceMax = 20;
	
	[AddComponentMenu("Camera-Control/Mouse Orbit")]
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The target related position.
	/// </summary>
	private Vector3 targetRelatedPos = new Vector3(0, 0.8f, 0);	// default for robot
	
	/// <summary>
	/// The x.
	/// </summary>
	private float x = 0.0f;
	
	/// <summary>
	/// The y.
	/// </summary>
	private float y = 0.0f;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	public void Start () 
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x + 20;
		
		// Make the rigid body not change rotation
		if (rigidbody) {
			rigidbody.freezeRotation = false;
		}
	}
	
	public void Update () // Update is called once per frame
	{
//		if (Input.GetKeyDown ("q")) {
//			 target = GameObject.Find("Player").transform;
//		}
		if (Input.GetKeyDown ("w")) {
			 target = GameObject.Find("Ghost").transform;
		}
		if (Input.GetKeyDown ("e")) {
			 target = GameObject.Find("Girl").transform;
		}
		if (Input.GetKeyDown ("r")) {
			 target = GameObject.Find("Robot").transform;
		}
	}
	
	/// <summary>
	/// Lates the update.
	/// </summary>
	public void LateUpdate () 
	{
	    if (target) {
	
		if(Input.GetKey("mouse 1")) {
			// Screen.showCursor = false;
			x += Input.GetAxis("Mouse X") * xSpeed * distance* 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
		}
	// 	else {
	// 		Screen.showCursor = true;
	// 	}
		    
		y = ClampAngle(y, (float)yMinLimit, (float)yMaxLimit);
		Quaternion rotation = Quaternion.Euler(y, x, 0);
	        distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel")*5, distanceMin, distanceMax);
	
	        RaycastHit hit = new RaycastHit();
	        if (Physics.Linecast (target.position + targetRelatedPos, transform.position, out hit)) {
		        // distance -= hit.distance;
	                distance -= Mathf.Sqrt(hit.distance/6.0f); // smooth into unblocked position
	        }
	        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + targetRelatedPos;
	
	        transform.rotation = rotation;
	        transform.position = position;
	    }
	}
	
	/// <summary>
	/// Clamps the angle.
	/// </summary>
	/// <param name='angle'>
	/// Angle.
	/// </param>
	/// <param name='min'>
	/// Minimum.
	/// </param>
	/// <param name='max'>
	/// Max.
	/// </param>
	public static float ClampAngle (float angle, float min, float max) 
	{
		
		if (angle < -360) {
			angle += 360;
		}
		if (angle > 360) {
			angle -= 360;
		}
		return Mathf.Clamp (angle, min, max);
	}	
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
}

