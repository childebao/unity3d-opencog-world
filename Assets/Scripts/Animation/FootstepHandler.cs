using UnityEngine;
using System.Collections;

public class FootstepHandler : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Classes
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Foot type.
	/// </summary>
	public enum FootType 
	{
		Player,
		Mech,
		Spider
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The audio source.
	/// </summary>
	public AudioSource audioSource;
	
	/// <summary>
	/// The type of the foot.
	/// </summary>
	public FootType footType;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	private PhysicMaterial physicMaterial;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	public void OnCollisionEnter (Collision collisionInfo) 
	{
		physicMaterial = collisionInfo.collider.sharedMaterial;
	}
	
	public void OnFootStep () 
	{
		if (!audioSource.enabled)
		{
			return;
		}
		
		AudioClip sound = null;
		switch (footType) 
		{
		case FootType.Player:
			sound = MaterialImpactManager.GetPlayerFootstepSound (physicMaterial);
			break;
		case FootType.Mech:
			sound = MaterialImpactManager.GetMechFootstepSound (physicMaterial);
			break;
		case FootType.Spider:
			sound = MaterialImpactManager.GetSpiderFootstepSound (physicMaterial);
			break;
		}	
		audioSource.pitch = 0.98f + (new Random().RandomRange(0, 4))*0.01f;
		audioSource.PlayOneShot(sound, 0.8f + (new Random().RandomRange (0, 4))*0.1f);
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
	
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
	
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	

}

