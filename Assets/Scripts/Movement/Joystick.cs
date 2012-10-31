using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GUITexture))]

/// <summary>
/// Joystick.
/// </summary>
public class Joystick : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Is it a touch pad?
	/// </summary>
	public bool touchPad;
	
	/// <summary>
	/// The touch zone.
	/// </summary>
	public Rect touchZone;
	
	/// <summary>
	/// The dead zone.
	/// </summary>
	public float deadZone = 0;
	
	/// <summary>
	/// The normalize.
	/// </summary>
	public bool normalize = false;
	
	/// <summary>
	/// The position.
	/// </summary>
	public Vector2 position;
	
	/// <summary>
	/// The tap count.
	/// </summary>
	public int tapCount;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Joystick"/> class.
	/// </summary>
	public Joystick ()
	{
	}
	
	#if !UNITY_IPHONE && !UNITY_ANDROID
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	public void Awake()
	{
		gameObject.active = false;
	}
	
	#else
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	public void Start()
	{
		//Cache this component at startup instead of looking up every frame
		gui = GetComponent<GUITexture>();
		
		//Store the default rect for the gui, so we can snap back to it
		defaultRect = gui.pixelInset;
		
		defaultRect.x += transform.position.x * Screen.width;// + gui.pixelInset.x; // - Screen.width * 0.5;
		defaultRect.y += transform.position.y * Screen.height;// - Screen.height * 0.5;
		
		transform.position.x = 0.0;
		transform.position.y = 0.0;
		
		if(touchPad)
		{
			// If a texture has been assigned, then use the rect from the gui 
			//	as out touchzone
			if(gui.texture)
				touchZone = defaultRect;
		}
		else
		{
			// This is an offset for touch input to match with the top left
			// corner of the GUI
			guiTouchOffset.x = defaultRect.width * 0.5;
			guiTouchOffset.y = defaultRect.height * 0.5;
			
			// Cache the center of the GUI, since it doesn't change
			guiCenter.x = defaultRect.x + guiTouchOffset.x;
			guiCenter.y = defaultRect.y + guiTouchOffset.y;
			
			// Let's build the GUI boundary, so we can clamp joystick movement
			guiBoundary.min.x = defaultRect.x - guiTouchOffset.x;
			guiBoundary.max.x = defaultRect.x + guiTouchOffset.x;
			guiBoundary.min.y = defaultRect.y - guiTouchOffset.y;
			guiBoundary.max.y = defaultRect.y + guiTouchOffset.y;
		}
	}
	
	/// <summary>
	/// Disable this instance.
	/// </summary>
	public void Disable ()
	{
		gameObject.active = false;
		enumeratedJoysticks = false;
	}
	
	/// <summary>
	/// Resets the joystick.
	/// </summary>
	public void ResetJoystick ()
	{
		// Release the finger control and set the joystick back to the 
		// 	default position
		gui.pixelInset = defaultRect;
		lastFingerId = -1;
		position = Vector2.zero;
		finderDownPos = Vector2.zero;
		
		if(touchPad)
			gui.color.a = 0.025;
	}
	
	/// <summary>
	/// Latcheds the finer.
	/// </summary>
	/// <param name='fingerId'>
	/// Finger identifier.
	/// </param>
	public void LatchedFiner( int fingerId )
	{
		// If another joystick has latched this funder, then we must release it
		if(lastFinderId == fingerId)
			ResetJoystick();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update()
	{
		if (!enumeratedJoysticks) 
		{
			// Collect all joysticks in the game, so we can relay finger 
			//	latching messages
			joysticks = FindObjectsOfType (Joystick) as Joystick[];
			enumeratedJoysticks = true;
		}	
			
		int count = Input.touchCount;
		
		// Adjust the tap time window while it still available
		if (tapTimeWindow > 0)
			tapTimeWindow -= Time.deltaTime;
		else
			tapCount = 0;
		
		if (count == 0) {
			ResetJoystick ();
		}
		else 
		{
			for (int i = 0; i < count; i++) 
			{
				Touch touch = Input.GetTouch (i);			
				Vector2 guiTouchPos = touch.position - guiTouchOffset;
		
				bool shouldLatchFinger = false;
				if (touchPad) 
				{				
					if (touchZone.Contains (touch.position))
						shouldLatchFinger = true;
				}
				else if (gui.HitTest (touch.position)) 
				{
					shouldLatchFinger = true;
				}
		
				// Latch the finger if this is a new touch
				if (shouldLatchFinger && (lastFingerId == -1 || lastFingerId != touch.fingerId)) 
				{
					if (touchPad) 
					{
						gui.color.a = 0.15;
						
						lastFingerId = touch.fingerId;
						fingerDownPos = touch.position;
						fingerDownTime = Time.time;
					}
					
					lastFingerId = touch.fingerId;
					
					// Accumulate taps if it is within the time window
					if (tapTimeWindow > 0) 
					{
						tapCount++;
					}
					else 
					{
						tapCount = 1;
						tapTimeWindow = tapTimeDelta;
					}
												
					// Tell other joysticks we've latched this finger
					foreach (Joystick j in joysticks) 
					{
						if (j != null && j != this)
							j.LatchedFinger (touch.fingerId);
					}						
				}				
		
				if (lastFingerId == touch.fingerId) 
				{
					// Override the tap count with what the iPhone SDK reports if it is greater
					// This is a workaround, since the iPhone SDK does not currently track taps
					// for multiple touches
					if (touch.tapCount > tapCount)
						tapCount = touch.tapCount;
					
					if (touchPad) 
					{	
						// For a touchpad, let's just set the position directly based on distance from initial touchdown
						position.x = Mathf.Clamp ((touch.position.x - fingerDownPos.x) / (touchZone.width / 2), -1, 1);
						position.y = Mathf.Clamp ((touch.position.y - fingerDownPos.y) / (touchZone.height / 2), -1, 1);
					}
					else 
					{					
						// Change the location of the joystick graphic to match where the touch is
						position.x = (touch.position.x - guiCenter.x) / guiTouchOffset.x;
						position.y = (touch.position.y - guiCenter.y) / guiTouchOffset.y;
					}
					
					if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
						ResetJoystick ();
				}			
			}
		}
	
		// Calculate the length. This involves a squareroot operation,
		// so it's slightly expensive. We re-use this length for multiple
		// things below to avoid doing the square-root more than one.
		float length  = position.magnitude;
		
		
		if (length < deadZone) 
		{
			// If the length of the vector is smaller than the deadZone radius,
			// set the position to the origin.
			position = Vector2.zero;
		}
		else 
		{
			if (length > 1) 
			{
				// Normalize the vector if its length was greater than 1.
				// Use the already calculated length instead of using Normalize().
				position = position / length;
			}
			else if (normalize) 
			{
				// Normalize the vector and multiply it with the length adjusted
				// to compensate for the deadZone radius.
				// This prevents the position from snapping from zero to the deadZone radius.
				position = position / length * Mathf.InverseLerp (length, deadZone, 1);
			}
		}
		
		if (!touchPad) 
		{
			// Change the location of the joystick graphic to match the position
			gui.pixelInset.x = (position.x - 1) * guiTouchOffset.x + guiCenter.x;
			gui.pixelInset.y = (position.y - 1) * guiTouchOffset.y + guiCenter.y;
		}
	}
	
	#endif
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Classes
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Boundary.
	/// </summary>
	public class Boundary
	{
		public Vector2 min = Vector2.zero;
		public Vector2 max = Vector2.zero;
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The joysticks.
	/// </summary>
	static private Joystick[] joysticks;
	
	/// <summary>
	/// Are joysticks enumerated?
	/// </summary>
	static private bool enumeratedJoysticks = false;
	
	/// <summary>
	/// The tap time delta.
	/// </summary>
	static private float tapTimeDelta = 0.3f;
	
	/// <summary>
	/// The last finder identifier.
	/// </summary>
	private object lastFinderId = -1;
	
	/// <summary>
	/// The tap time window.
	/// </summary>
	private float tapTimeWindow;
	
	/// <summary>
	/// The finger down position.
	/// </summary>
	private Vector2 fingerDownPos;
	
	/// <summary>
	/// The finder down time.
	/// </summary>
	private float finderDownTime;
	
	/// <summary>
	/// The first delta time.
	/// </summary>
	private float firstDeltaTime = 0.5f;
	
	/// <summary>
	/// The GUI.
	/// </summary>
	private GUITexture gui;
	
	/// <summary>
	/// The default rect.
	/// </summary>
	private Rect defaultRect;
	
	/// <summary>
	/// The GUI boundary.
	/// </summary>
	private Boundary guiBoundary = new Boundary();
	
	/// <summary>
	/// The GUI touch offset.
	/// </summary>
	private Vector2 guiTouchOffset;
	
	/// <summary>
	/// The GUI center.
	/// </summary>
	private Vector2 guiCenter;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
}


