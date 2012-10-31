using UnityEngine;
using System.Collections;

/// <summary>
/// This camera smoothes out rotation around the y-axis and height.
/// Horizontal Distance to the target is always fixed.
/// 
/// There are many different ways to smooth the rotation but doing it this way gives you a lot of control over how the camera behaves.
/// 
/// For every of those smoothed values we calculate the wanted value and the current value.
/// Then we smooth it using the Lerp function.
/// Then we apply the smoothed values to the transform's position.
/// </summary>
public class SmoothFollow : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The target we are following.
	/// </summary>
	public Transform target;
	
	/// <summary>
	/// The target related position.
	/// </summary>
	public Vector3 targetRelatedPos;
	
	/// <summary>
	/// The distance in the x-z plane to the target.
	/// </summary>
	public float distance = 10.0f;
	
	/// <summary>
	/// The height we want the camera to be above the target.
	/// </summary>
	public float height = 5.0f;
	
	/// <summary>
	/// The height damping.
	/// </summary>
	public float heightDamping = 2.0f;
	
	/// <summary>
	/// The rotation damping.
	/// </summary>
	public float rotationDamping = 3.0f;
	
	[AddComponentMenu("Camera-Control/Smooth Follow")]
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update() 
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
		// Early out if we don't have a target
		if (!target)
			return;
		
		// Calculate the current rotation angles
		float wantedRotationAngle = target.eulerAngles.y;
		float wantedHeight = target.position.y + height;
	
		float currentRotationAngle = transform.eulerAngles.y;
		float currentHeight = transform.position.y;
	
		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
	
		// Damp the height
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
	
		// Convert the angle into a rotation
		Quaternion currentRotation = Quaternion.Euler (0.0f, currentRotationAngle, 0.0f);
	
		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		transform.position = target.position;
		transform.position -= currentRotation * Vector3.forward * distance;
	
		// Set the height of the camera
		transform.position.Set(transform.position.x, currentHeight, transform.position.z);
	
		// Always look at the target
		transform.LookAt (target.position + targetRelatedPos);
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



