using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Receiver item.
/// @NOTE: This seems like an observer class, research better C# implementations.
/// </summary>
[Serializable]
public class ReceiverItem
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The receiver.
	/// </summary>
	public GameObject receiver;
	
	/// <summary>
	/// The action.
	/// </summary>
	public string action = "OnSignal";
	
	/// <summary>
	/// The delay.
	/// </summary>
	public float delay;
	
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
	/// Initializes a new instance of the <see cref="ReceiverItem"/> class.
	/// </summary>
	public ReceiverItem ()
	{
	}
	
	/// <summary>
	/// Sends the with delay.
	/// </summary>
	/// <param name='sender'>
	/// Sender.
	/// </param>
	public IEnumerator SendWithDelay(MonoBehaviour sender)
	{
		yield return new WaitForSeconds(delay);
		if(receiver)
			receiver.SendMessage(action);
		else
			Debug.LogWarning("No receiver of signal \""+action+"\" on object "
				+sender.name+" ("+sender.GetType().Name+")", sender);
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


