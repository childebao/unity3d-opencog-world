using UnityEngine;
using System.Collections;

/// <summary>
/// Text adventure manager.
/// Seems to be tightly coupled with MoodBoxes.
/// @TODO: Add references to MoodBoxes back in.
/// </summary>
public class TextAdventureManager : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The player.
	/// </summary>
	Transform player;
	
	/// <summary>
	/// The playable mood boxes.
	/// @TODO: Add these back in.
	/// </summary>
	//MoodBox[] playableMoodBoxes;
	
	/// <summary>
	/// The time per char.
	/// </summary>
	float timePerChar = 0.125f;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The current mood box.
	/// </summary>
	private int currentMoodBox = 0;
	
	/// <summary>
	/// The text animation.
	/// </summary>
	private int textAnimation = 0;
	
	/// <summary>
	/// The timer.
	/// </summary>
	private float timer = 0.0f;
	
	/// <summary>
	/// The cam offset.
	/// </summary>
	private Vector3 camOffset = Vector3.zero;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	public void Start () 
	{
		if (!player)
			player = GameObject.FindWithTag ("Player").transform;	
				
		GameObject leftIcon = new GameObject ("Left Arrow", typeof(GUIText));
		GameObject rightIcon = new GameObject ("Right Arrow", typeof(GUIText));
	
	#if UNITY_IPHONE || UNITY_ANDROID
		leftIcon.guiText.text = "<";
	#else
		leftIcon.guiText.text = "< backspace";		
	#endif
		
		leftIcon.guiText.font = guiText.font;
		leftIcon.guiText.material = guiText.material;
		leftIcon.guiText.anchor = TextAnchor.UpperLeft;
		leftIcon.gameObject.layer = ( LayerMask.NameToLayer ("Adventure"));
		
		leftIcon.transform.position.Set(0.01f, 0.1f, leftIcon.transform.position.z);
	
	#if UNITY_IPHONE || UNITY_ANDROID
		rightIcon.guiText.text = ">";
	#else
		rightIcon.guiText.text = "space >";		
	#endif
		rightIcon.guiText.font = guiText.font;
		rightIcon.guiText.material = guiText.material;
		rightIcon.guiText.anchor = TextAnchor.UpperRight;
		rightIcon.gameObject.layer = ( LayerMask.NameToLayer ("Adventure"));
				
		rightIcon.transform.position.Set(0.99f, 0.1f, rightIcon.transform.position.z);		
	}
	
	/// <summary>
	/// Raises the enable event.
	/// </summary>
	public void OnEnable () 
	{	
		textAnimation = 0;
		timer = timePerChar;
		
		camOffset = Camera.main.transform.position - player.position;
		
		BeamToBox (currentMoodBox);
			
		if (player) {
			PlayerMoveController ctrler = player.GetComponent<PlayerMoveController> ();
			ctrler.enabled = false;		
		}
		
		guiText.enabled = true;
	}
	
	/// <summary>
	/// Raises the disable event.
	/// </summary>
	public void OnDisable () 
	{
		// and back to normal player control
		
		if (player) {
			PlayerMoveController ctrler = player.GetComponent<PlayerMoveController> ();
			ctrler.enabled = true;		
		}
		
		guiText.enabled = false;	
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update () 
	{
		guiText.text = "Fallaçade \n \n";
		//guiText.text += playableMoodBoxes[currentMoodBox].data.adventureString.Substring (0, textAnimation);
		
		Debug.Log (guiText.text);
		
//		if (textAnimation >= playableMoodBoxes[currentMoodBox].data.adventureString.Length ) {
//				
//		}
//		else {
//			timer -= Time.deltaTime;
//			if (timer <= 0.0f) {
//				textAnimation++;
//				timer = timePerChar;
//			}
//		}
		
		CheckInput ();
	}
	
	/// <summary>
	/// Beams to box.
	/// </summary>
	/// <param name='index'>
	/// Index.
	/// </param>
	public void BeamToBox (int index) 
	{
//		if (index > playableMoodBoxes.Length)
//			return;
			
//		player.position = playableMoodBoxes[index].transform.position;
		Camera.main.transform.position = player.position + camOffset;
		textAnimation = 0;
		timer = timePerChar;
	}
	
	/// <summary>
	/// Checks the input.
	/// </summary>
	public void CheckInput () 
	{
		int input = 0;
		
		#if UNITY_IPHONE || UNITY_ANDROID
	   		for (var touch : Touch in Input.touches) {
	        	if (touch.phase == TouchPhase.Ended && touch.phase != TouchPhase.Canceled) {
	            	if (touch.position.x < Screen.width / 2)
	            		input = -1;
	            	else 
	            		input = 1;
	        	}
	    	}
		#else
			if (Input.GetKeyUp (KeyCode.Space))
				input = 1;
			else if (Input.GetKeyUp (KeyCode.Backspace))
				input = -1;
		#endif
		
//		if (input != 0) 
//		{
//			if (textAnimation < playableMoodBoxes[currentMoodBox].data.adventureString.Length) {
//				textAnimation = playableMoodBoxes[currentMoodBox].data.adventureString.Length;
//				input = 0;
//			}
//		}
				
//		if (input != 0) 
//		{
//			if ((currentMoodBox - playableMoodBoxes.Length) == -1 && input < 0) 
//				input = 0;
//			if (currentMoodBox == 0 && input < 0)
//				input = 0;
//				
//			if (input) 
//			{
//				currentMoodBox = (input + currentMoodBox) % playableMoodBoxes.Length;
//				BeamToBox (currentMoodBox);
//			}
//		}
	}
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
}



