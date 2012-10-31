using UnityEngine;
using System.Collections;

/// <summary>
/// Health flash.
/// </summary>
public class HealthFlash : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The player health.
	/// </summary>
	public Health playerHealth;
	
	/// <summary>
	/// The health material.
	/// </summary>
	public Material healthMaterial;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The health blink.
	/// </summary>
	private float healthBlink = 1.0f;
	
	/// <summary>
	/// The one over max health.
	/// </summary>
	private float oneOverMaxHealth = 0.5f;
	
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
		oneOverMaxHealth = 1.0f / playerHealth.maxHealth;	
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update () 
	{
		float relativeHealth = playerHealth.health * oneOverMaxHealth;
		healthMaterial.SetFloat ("_SelfIllumination", relativeHealth * 2.0f * healthBlink);
		
		if (relativeHealth < 0.45f) 
			healthBlink = Mathf.PingPong (Time.time * 6.0f, 2.0f);
		else 
			healthBlink = 1.0f;
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

