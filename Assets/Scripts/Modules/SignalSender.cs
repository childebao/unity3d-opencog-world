using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary>
/// Signal sender.
/// @NOTE:  This seems like the subject of an Observer pattern, investigate.
/// </summary>
[Serializable]
public class SignalSender
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The only once.
	/// </summary>
	public bool onlyOnce;
	
	/// <summary>
	/// The receivers.
	/// </summary>
	public ReceiverItem[] receivers;
	
	/// <summary>
	/// The has fired.
	/// </summary>
	private bool hasFired = false;
	
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
	/// Initializes a new instance of the <see cref="SignalSender"/> class.
	/// </summary>
	public SignalSender ()
	{
	}
	
	/// <summary>
	/// Sends the signals.
	/// </summary>
	/// <param name='sender'>
	/// Sender.
	/// </param>
	public void SendSignals (MonoBehaviour sender) 
	{
		if (hasFired == false || onlyOnce == false) 
		{
			for (var i = 0; i < receivers.GetLength(0); i++) 
			{
				sender.StartCoroutine (receivers[i].SendWithDelay(sender));
			}
			hasFired = true;
		}
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


