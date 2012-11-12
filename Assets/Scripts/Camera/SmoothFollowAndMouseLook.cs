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
public class SmoothFollowAndMouseLook : MonoBehaviour
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
	
	public float distanceDamping = 2.0f;
	
	/// <summary>
	/// The rotation damping.
	/// </summary>
	public float rotationDamping = 3.0f;
	
	[AddComponentMenu("Camera-Control/Smooth Follow")]
	
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;
	
	[AddComponentMenu("Camera-Control/Mouse Look")]
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	float rotationX = 0F;
	float rotationY = 0F;
	
	Quaternion originalRotation;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////

	public void Awake () // Called when the script instance is being loaded.
	{
		
	}
	
	public void Start () // Use this for initialization
	{
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		originalRotation = transform.localRotation;
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
	
	public void LateUpdate () // Called once per frame after all Update calls
	{
		// Early out if we don't have a target
		if (!target)
			return;
		
		Quaternion wantedRotation = target.localRotation;
		
		if (axes == RotationAxes.MouseXAndY)
		{
			// Read the mouse input axis
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

			rotationX = ClampAngle (rotationX, minimumX, maximumX);
			rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
			
			wantedRotation = wantedRotation * xQuaternion * yQuaternion;
		}
		else if (axes == RotationAxes.MouseX)
		{
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationX = ClampAngle (rotationX, minimumX, maximumX);

			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			wantedRotation = wantedRotation * xQuaternion;
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = ClampAngle (rotationY, minimumY, maximumY);

			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
			wantedRotation = wantedRotation * yQuaternion;
		}
		
		
		
        //Input..mousePosition = new Vector2(Screen.width / 2, Screen.height / 2);
		
		// Calculate the current rotation angles
		float wantedRotationAngleY = wantedRotation.eulerAngles.y;
		float wantedRotationAngleX = wantedRotation.eulerAngles.x;
		float wantedHeight = target.position.y + height;
		float wantedDistance = (target.position - transform.position).magnitude;
	
		if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
        {
            wantedDistance *= 1.5f;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            wantedDistance /= 1.5f;
        }
		else
		{
			distance = (target.position - transform.position).magnitude;
		}
		
		float currentRotationAngleY = transform.eulerAngles.y;
		float currentRotationAngleX = transform.eulerAngles.x;
		float currentHeight = transform.position.y;
		float currentDistance = (target.position - transform.position).magnitude;
	
		// Damp the rotation around the y-axis
		currentRotationAngleY = Mathf.LerpAngle (currentRotationAngleY, wantedRotationAngleY, rotationDamping * Time.deltaTime);
		
		// Damp the rotation around the x-axis
		currentRotationAngleX = Mathf.LerpAngle (currentRotationAngleX, wantedRotationAngleX, rotationDamping * Time.deltaTime);
	
		// Damp the height
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
		
		// Damp the distance
		currentDistance = Mathf.Lerp(currentDistance, wantedDistance, distanceDamping * Time.deltaTime);
	
		// Convert the angle into a rotation
		Quaternion currentRotation = Quaternion.Euler (currentRotationAngleX, currentRotationAngleY, 0.0f);
	
		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		transform.position = target.position;
		transform.position -= currentRotation * Vector3.forward * currentDistance;
	
		// Set the height of the camera
		transform.position.Set(transform.position.x, currentHeight, transform.position.z);
	
		// Always look at the target
		transform.LookAt (target.position + targetRelatedPos);
	}
	
	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}
			
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	void FixedUpdate () // Called every fixed framerate frame, used mostly for physics
	{
		
	}
	
	void OnJointBreak (float breakForce) //Called when a joint attached to the same game object broke.
	{
		Debug.Log ("Joint Broke!, force: " + breakForce);
	}
	
	void OnLevelWasLoaded (int level) //called after a new level was loaded.
	{
		Debug.Log ("Level " + level + " was loaded.");
	}
	
	void OnGUI () //OnGUI is called for rendering and handling GUI events.
	{
		if (GUI.Button (new Rect (10, 10, 150, 100), "I am a button"))
			Debug.Log ("You clicked the button!");
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit)
	{
	}
	
	void OnCollisionEnter (Collision collision)
	{
	}

	void OnCollisionStay (Collision collision)
	{
	}

	void OnCollisionExit (Collision collision)
	{
	}
	
	void OnTriggerEnter (Collider collider)
	{
	}

	void OnTriggerStay (Collider collider)
	{
	}

	void OnTriggerExit (Collider collider)
	{
	}
	
	void OnMouseDown ()
	{
	}

	void OnMouseUp ()
	{
	}

	void OnMouseEnter ()
	{
	}

	void OnMouseOver ()
	{
	}

	void OnMouseExit ()
	{
	}

	void OnMouseDrag ()
	{
	}
	
	void OnBecameVisible ()
	{
	}

	void OnBecameInvisible ()
	{
	}

	void OnWillRenderObject () //Called once for each camera if the object is visible
	{
	}

	void OnRenderObject () //Called on object after camera has rendered the scene
	{
	}
	
	void OnApplicationPause (bool pause)
	{
	}

	void OnApplicationFocus (bool focus)
	{
	}

	void OnApplicationQuit ()
	{
	}
		
	void OnServerInitialized () //Called on the server whenever a Network.InitializeServer was invoked and has completed.
	{
		Debug.Log ("Server initialized and ready");
	}

	void OnPlayerConnected (NetworkPlayer player) //Called on the server whenever a new player has successfully connected.
	{
		Debug.Log ("Player  connected from " + player.ipAddress + ":" + player.port);
	}

	void OnPlayerDisconnected (NetworkPlayer player) //Called on the server whenever a player disconnected from the server.
	{
		Debug.Log ("Clean up after player " + player);
		Network.RemoveRPCs (player);
		Network.DestroyPlayerObjects (player);
	}
	
	void OnConnectedToServer () //Called on the client when you have successfully connected to a server
	{
		Debug.Log ("Connected to server");
	}

	void OnFailedToConnect (NetworkConnectionError error) //Called on the client when a connection attempt fails for some reason.
	{
		Debug.Log ("Could not connect to server: " + error);
	}

	void OnDisconnectedFromServer (NetworkDisconnection info) //Called on the client when the connection was lost or you disconnected from the server.
	{
		Debug.Log ("Disconnected from server: " + info);
	}
	
	void OnFailedToConnectToMasterServer (NetworkConnectionError info) //Called on clients or servers when there is a problem connecting to the MasterServer.
	{
		Debug.Log ("Could not connect to master server: " + info);
	}

	void OnMasterServerEvent (MasterServerEvent msEvent) //Called on clients or servers when reporting events from the MasterServer.
	{
		Debug.Log ("MasterServer event: " + msEvent);
	}
	
	void OnNetworkInstantiate (NetworkMessageInfo info) //Called on objects which have been network instantiated with Network.Instantiate
	{
		Debug.Log ("New object instantiated by " + info.sender);
	}

	void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info) //Used to customize synchronization of variables in a script watched by a network view.
	{
	}
	
	void OnEnable () //Called when MonoBehaviour is loaded
	{
		Debug.Log (string.Format ("MonoBehaviour[{0}].OnEnable", gameObject.name + "\\" + GetType ().Name));
	}
	
	void OnDisable () //Called when MonoBehaviour goes out of scope
	{
		Debug.Log (string.Format ("MonoBehaviour[{0}].OnDisable", gameObject.name + "\\" + GetType ().Name));
	}

	void OnDestroy () //Called when MonoBehaviour is about to be destroyed.
	{
		Debug.Log (string.Format ("MonoBehaviour[{0}].OnDestroy", gameObject.name + "\\" + GetType ().Name));
	}			
			
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////			
			

}



