using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent(typeof(CharacterController))]

/// <summary>
/// Player move controller.
/// </summary>
public class PlayerMoveController : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The movement motor.
	/// </summary>
	public MovementMotor motor;
	
	// Objects to drag in
	
	/// <summary>
	/// The avatar's transform.
	/// </summary>
	public Transform avatar;
	
	/// <summary>
	/// The cursor prefab.
	/// @NOTE: Why is this needed?
	/// </summary>
	public GameObject cursorPrefab;
	
	/// <summary>
	/// The joystick prefab.
	/// @NOTE: Why is this needed?
	/// </summary>
	public GameObject joystickPrefab;
	
	// Settings
	
	/// <summary>
	/// The camera smoothing.
	/// </summary>
	public float cameraSmoothing = 0.01f;
	
	/// <summary>
	/// The camera preview.
	/// </summary>
	public float cameraPreview = 2.0f;
	
	// Cursor Settings
	
	/// <summary>
	/// The height of the cursor plane.
	/// </summary>
	public float cursorPlaneHeight = 0.0f;
	
	/// <summary>
	/// The cursor facing camera.
	/// </summary>
	public float cursorFacingCamera = 0.0f;
	
	/// <summary>
	/// The cursor smaller with distance.
	/// </summary>
	public float cursorSmallerWithDistance = 0.0f;
	
	/// <summary>
	/// The cursor smaller when close.
	/// </summary>
	public float cursorSmallerWhenClose = 1.0f;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The main camera.
	/// </summary>
	private Camera mainCamera;
	
	/// <summary>
	/// The cursor object.
	/// </summary>
	private Transform cursorObject;
	
	/// <summary>
	/// The joystick left.
	/// </summary>
	private Joystick joystickLeft;
	
	/// <summary>
	/// The joystick right.
	/// </summary>
	private Joystick joystickRight;
	
	/// <summary>
	/// The main camera transfom.
	/// </summary>
	private Transform mainCameraTransform;
	
	/// <summary>
	/// The camera velocity.
	/// </summary>
	private Vector3 cameraVelocity = Vector3.zero;
	
	/// <summary>
	/// The camera offset.
	/// </summary>
	private Vector3 cameraOffset = Vector3.zero;
	
	/// <summary>
	/// The init offset to player.
	/// </summary>
	private Vector3 initOffsetToPlayer;
	
	/// <summary>
	/// Prepare a cursor point varibale. 
	/// This is the mouse position on PC and controlled 
	/// by the thumbstick on mobiles.
	/// </summary>
	private Vector3 cursorScreenPosition;
	
	/// <summary>
	/// The player movement plane.
	/// </summary>
	private Plane playerMovementPlane;
	
	/// <summary>
	/// The joystick right G.
	/// </summary>
	private GameObject joystickRightGO;
	
	/// <summary>
	/// The screen movement space.
	/// </summary>
	private Quaternion screenMovementSpace;
	
	/// <summary>
	/// The screen movement forward.
	/// </summary>
	private Vector3 screenMovementForward;
	
	/// <summary>
	/// The screen movement right.
	/// </summary>
	private Vector3 screenMovementRight;
	
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Initializes a new instance of the 
	/// <see cref="PlayerMoveController"/> class.
	/// </summary>
	public PlayerMoveController ()
	{
	}
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	public void Awake()
	{
		motor.movementDirection = Vector2.zero;
		motor.facingDirection = Vector2.zero;
		
		// Set main camera
		mainCamera = Camera.main;
		mainCameraTransform = mainCamera.transform;
		
		// Ensure we have avatar set
		// Default to using the transform this component is on
		if (!avatar)
			avatar = transform;
		
		initOffsetToPlayer = mainCameraTransform.position - avatar.position;
		
		#if UNITY_IPHONE || UNITY_ANDROID
			if (joystickPrefab) 
			{
				// Create left joystick
				GameObject joystickLeftGO  = Instantiate (joystickPrefab) as GameObject;
				joystickLeftGO.name = "Joystick Left";
				joystickLeft = joystickLeftGO.GetComponent<Joystick>();
				
				// Create right joystick
				joystickRightGO = Instantiate (joystickPrefab) as GameObject;
				joystickRightGO.name = "Joystick Right";
				joystickRight = joystickRightGO.GetComponent<Joystick>();			
			}
		#elif !UNITY_FLASH
			if (cursorPrefab) 
			{
				cursorObject = (Instantiate (cursorPrefab) as GameObject).transform;
			}
		#endif
		
		// Save camera offset so we can use it in the first frame
		cameraOffset = mainCameraTransform.position - avatar.position;
		
		// Set the initial cursor position to the center of the screen
		cursorScreenPosition = new Vector3 (0.5f * Screen.width, 0.5f * Screen.height, 0.0f);
		
		// caching movement plane
		playerMovementPlane = new Plane (avatar.up, avatar.position + avatar.up * cursorPlaneHeight);

	}
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	public void Start () 
	{
		#if UNITY_IPHONE || UNITY_ANDROID
			// Move to right side of screen
			GUITexture guiTex  = joystickRightGO.GetComponent<GUITexture>();
			guiTex.pixelInset.x = Screen.width - guiTex.pixelInset.x - guiTex.pixelInset.width;			
		#endif	
		
		// it's fine to calculate this on Start () as the camera is static in rotation
		
		screenMovementSpace = Quaternion.Euler (0, mainCameraTransform.eulerAngles.y, 0);
		screenMovementForward = screenMovementSpace * Vector3.forward;
		screenMovementRight = screenMovementSpace * Vector3.right;	
	}
	
	/// <summary>
	/// Raises the disable event.
	/// </summary>
	public void OnDisable () 
	{
		if (joystickLeft) 
			joystickLeft.enabled = false;
		
		if (joystickRight)
			joystickRight.enabled = false;
	}
	
	/// <summary>
	/// Raises the enable event.
	/// </summary>
	public void OnEnable () 
	{
		if (joystickLeft) 
			joystickLeft.enabled = true;
		
		if (joystickRight)
			joystickRight.enabled = true;
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update()
	{
		// HANDLE AVATAR MOVEMENT DIRECTION
		#if UNITY_IPHONE || UNITY_ANDROID
			movementDirection = joystickLeft.position.x * screenMovementRight 
				+ joystickLeft.position.y * screenMovementForward;
		#else
			motor.movementDirection = Input.GetAxis ("Horizontal") * screenMovementRight 
				+ Input.GetAxis ("Vertical") * screenMovementForward;
		#endif
		
		// Make sure the direction vector doesn't exceed a length of 1
		// so the avatar can't move faster diagonally than horizontally or vertically
		if (motor.movementDirection.sqrMagnitude > 1)
			motor.movementDirection.Normalize();
		
		
		// HANDLE AVATAR FACING DIRECTION AND SCREEN FOCUS POINT
		
		// First update the camera position to take into account how much the avatar moved since last frame
		//mainCameraTransform.position = Vector3.Lerp (mainCameraTransform.position, avatar.position 
		//	+ cameraOffset, Time.deltaTime * 45.0f * deathSmoothoutMultiplier);
		
		// Set up the movement plane of the avatar, so screenpositions
		// can be converted into world positions on this plane
		//playerMovementPlane = new Plane (Vector3.up, avatar.position + avatar.up * cursorPlaneHeight);
		
		// optimization (instead of newing Plane):
		
		playerMovementPlane.normal = avatar.up;
		playerMovementPlane.distance = -avatar.position.y + cursorPlaneHeight;
		
		// used to adjust the camera based on cursor or joystick position
		
		Vector3 cameraAdjustmentVector  = Vector3.zero;
		
		#if UNITY_IPHONE || UNITY_ANDROID
		
			// On mobiles, use the thumb stick and convert it into screen movement space
			facingDirection = joystickRight.position.x * screenMovementRight 
				+ joystickRight.position.y * screenMovementForward;
					
			cameraAdjustmentVector = facingDirection;		
		
		#else
		
			#if !UNITY_EDITOR && (UNITY_XBOX360 || UNITY_PS3)
	
				// On consoles use the analog sticks
				float axisX = Input.GetAxis("LookHorizontal");
				float axisY = Input.GetAxis("LookVertical");
				facingDirection = axisX * screenMovementRight + axisY * screenMovementForward;
		
				cameraAdjustmentVector = facingDirection;		
			
			#else
		
				// On PC, the cursor point is the mouse position
				Vector3 cursorScreenPosition = Input.mousePosition;
							
				// Find out where the mouse ray intersects with the movement plane of the player
				Vector3 cursorWorldPosition = ScreenPointToWorldPointOnPlane 
					(cursorScreenPosition, playerMovementPlane, mainCamera);
				
				float halfWidth = Screen.width / 2.0f;
				float halfHeight = Screen.height / 2.0f;
				float maxHalf = Mathf.Max (halfWidth, halfHeight);
				
				// Acquire the relative screen position			
				Vector3 posRel = cursorScreenPosition - new Vector3 (halfWidth, halfHeight, cursorScreenPosition.z);		
				posRel.x /= maxHalf; 
				posRel.y /= maxHalf;
							
				cameraAdjustmentVector = posRel.x * screenMovementRight + posRel.y * screenMovementForward;
				cameraAdjustmentVector.y = 0.0f;	
										
				// The facing direction is the direction from the avatar to the cursor world position
				motor.facingDirection = (cursorWorldPosition - avatar.position);
				motor.facingDirection.y = 0;			
				
				// Draw the cursor nicely
				HandleCursorAlignment (cursorWorldPosition);
				
			#endif
			
		#endif
			
		// HANDLE CAMERA POSITION
			
		// Set the target position of the camera to point at the focus point
		Vector3 cameraTargetPosition  = avatar.position + initOffsetToPlayer + cameraAdjustmentVector * cameraPreview;
		
		// Apply some smoothing to the camera movement
		mainCameraTransform.position = Vector3.SmoothDamp (mainCameraTransform.position, cameraTargetPosition, ref cameraVelocity, cameraSmoothing);
		
		// Save camera offset so we can use it in the next frame
		cameraOffset = mainCameraTransform.position - avatar.position;
	}
	
	/// <summary>
	/// Planes the ray intersection.
	/// </summary>
	/// <returns>
	/// The ray intersection.
	/// </returns>
	/// <param name='plane'>
	/// Plane.
	/// </param>
	/// <param name='ray'>
	/// Ray.
	/// </param>
	public static Vector3 PlaneRayIntersection (Plane plane, Ray ray) 
	{
		float dist = 0.0f;
		plane.Raycast (ray, out dist);
		return ray.GetPoint (dist);
	}
	
	/// <summary>
	/// Screens the point to world point on plane.
	/// </summary>
	/// <returns>
	/// The point to world point on plane.
	/// </returns>
	/// <param name='screenPoint'>
	/// Screen point.
	/// </param>
	/// <param name='plane'>
	/// Plane.
	/// </param>
	/// <param name='camera'>
	/// Camera.
	/// </param>
	public static Vector3 ScreenPointToWorldPointOnPlane (Vector3 screenPoint , Plane plane, Camera camera) 
	{
		// Set up a ray corresponding to the screen position
		Ray ray = camera.ScreenPointToRay (screenPoint);
		
		// Find out where the ray intersects with the plane
		return PlaneRayIntersection (plane, ray);
	}
	
	/// <summary>
	/// Handles the cursor alignment.
	/// </summary>
	/// <param name='cursorWorldPosition'>
	/// Cursor world position.
	/// </param>
	public void HandleCursorAlignment (Vector3 cursorWorldPosition) 
	{
		if (!cursorObject)
			return;
		
		// HANDLE CURSOR POSITION
		
		// Set the position of the cursor object
		cursorObject.position = cursorWorldPosition;
		
		#if !UNITY_FLASH
			// Hide mouse cursor when within screen area, since we're showing game cursor instead
			Screen.showCursor = (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width 
				|| Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height);
		#endif
		
		
		// HANDLE CURSOR ROTATION
		
		Quaternion cursorWorldRotation = cursorObject.rotation;
		if (motor.facingDirection != Vector3.zero)
			cursorWorldRotation = Quaternion.LookRotation (motor.facingDirection);
		
		// Calculate cursor billboard rotation
		Vector3 cursorScreenspaceDirection = Input.mousePosition - mainCamera.WorldToScreenPoint 
				(transform.position + avatar.up * cursorPlaneHeight);
		cursorScreenspaceDirection.z = 0;
		Quaternion cursorBillboardRotation  = mainCameraTransform.rotation * Quaternion.LookRotation 
				(cursorScreenspaceDirection, -Vector3.forward);
		
		// Set cursor rotation
		cursorObject.rotation = Quaternion.Slerp (cursorWorldRotation, cursorBillboardRotation, cursorFacingCamera);
		
		
		// HANDLE CURSOR SCALING
		
		// The cursor is placed in the world so it gets smaller with perspective.
		// Scale it by the inverse of the distance to the camera plane to compensate for that.
		float compensatedScale  = 0.1f * Vector3.Dot (cursorWorldPosition - mainCameraTransform.position, 
				mainCameraTransform.forward);
		
		// Make the cursor smaller when close to avatar
		float cursorScaleMultiplier = Mathf.Lerp (0.7f, 1.0f, Mathf.InverseLerp 
				(0.5f, 4.0f, motor.facingDirection.magnitude));
		
		// Set the scale of the cursor
		cursorObject.localScale = Vector3.one * Mathf.Lerp (compensatedScale, 1, cursorSmallerWithDistance) 
				* cursorScaleMultiplier;
		
		// DEBUG - REMOVE LATER
		if (Input.GetKey(KeyCode.O)) cursorFacingCamera += Time.deltaTime * 0.5f;
		if (Input.GetKey(KeyCode.P)) cursorFacingCamera -= Time.deltaTime * 0.5f;
		cursorFacingCamera = Mathf.Clamp01(cursorFacingCamera);
		
		if (Input.GetKey(KeyCode.K)) cursorSmallerWithDistance += Time.deltaTime * 0.5f;
		if (Input.GetKey(KeyCode.L)) cursorSmallerWithDistance -= Time.deltaTime * 0.5f;
		cursorSmallerWithDistance = Mathf.Clamp01(cursorSmallerWithDistance);
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


