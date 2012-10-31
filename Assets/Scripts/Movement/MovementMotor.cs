using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class can be used like an interface.
/// Inherit from it to define your own movement motor that can control
/// the movement of characters, enemies, or other entities.
/// </summary>
public class MovementMotor : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The direction the character wants to move in, in world space.
	/// The vector should have a length between 0 and 1.
	/// </summary>
	[HideInInspector]
	public Vector3 movementDirection;
	
	/// <summary>
	/// Simpler motors might want to drive movement 
	/// based purely on a target.  In world space.
	/// </summary>
	[HideInInspector]
	public Vector3 movementTarget;

	/// <summary>
	/// The direction the character wants to face towards, 
	/// in world space.
	/// </summary>
	[HideInInspector]
	public Vector3 facingDirection;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Initializes a new instance of the <see cref="MovementMotor"/> class.
	/// </summary>
	public MovementMotor ()
	{
	}
	
	/////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
}
