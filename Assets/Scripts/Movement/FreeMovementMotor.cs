using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// A simple movement motor class for free linear movment along a place.
/// </summary>
public class FreeMovementMotor : MovementMotor
{
	
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	

	
	// Settings
	
	/// <summary>
	/// The walking speed.  Linear velocity.
	/// </summary>
	public float walkingSpeed = 5.0f;
	
	/// <summary>
	/// How crisp or snappy the walking speed is.
	/// Basically how quickly the character responds
	/// when told to walk in a given direction.
	/// </summary>
	public float walkingSnappiness = 50f;
	
	/// <summary>
	/// The turning smoothing.
	/// Basically how quickly the character rotates
	/// when told to walk in a new direction.
	/// </summary>
	public float turningSmoothing = 0.3f;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Initializes a new instance of the 
	/// <see cref="FreeMovementMotor"/> class.
	/// </summary>
	public FreeMovementMotor()
	{
	}
	
	/// <summary>
	/// A fixed update that runs once per frame.
	/// @NOTE: Check frequency
	/// </summary>
	public void FixedUpdate()
	{
		// Handle the movement of the character
		Vector3 targetVelocity = movementDirection * walkingSpeed;
		Vector3 deltaVelocity = targetVelocity - rigidbody.velocity;
		if(rigidbody.useGravity)
		{
			deltaVelocity.y = 0;
		}
		rigidbody.AddForce (deltaVelocity * walkingSnappiness, ForceMode.Acceleration);
		
		// Setup player to face facingDirection, or if that is zero, then the movementDirection
		Vector3 faceDir = facingDirection;
		if(faceDir == Vector3.zero)
		{
			faceDir = movementDirection;
		}
		
		// Make the character rotate towards the target rotation
		if(faceDir == Vector3.zero)
		{
			rigidbody.angularVelocity = Vector3.zero;
		}
		else
		{
			float rotationAngle = AngleAroundAxis (transform.forward, faceDir, Vector3.up);
			rigidbody.angularVelocity = (Vector3.up * rotationAngle * turningSmoothing);
		}
	}
	
	/// <summary>
	/// The angle between dirA and dirB around an axis.
	/// </summary>
	/// <returns>
	/// The angle in radians.
	/// </returns>
	/// <param name='dirA'>
	/// Direction a.
	/// </param>
	/// <param name='dirB'>
	/// Direction b.
	/// </param>
	/// <param name='axis'>
	/// The Axis of rotation.
	/// </param>
	public float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
	{
		// Project A and B onto the plane orthogonal target axis
	    dirA = dirA - Vector3.Project (dirA, axis);
	    dirB = dirB - Vector3.Project (dirB, axis);
		
		// Find (positive) angle between A and B
	    float angle = Vector3.Angle (dirA, dirB);
	   
	    // Return angle multiplied with 1 or -1
	    return angle * (Vector3.Dot (axis, Vector3.Cross (dirA, dirB)) < 0 ? -1 : 1);
	}
	
	//////////////////////////////////////////////////
	
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
	
};


