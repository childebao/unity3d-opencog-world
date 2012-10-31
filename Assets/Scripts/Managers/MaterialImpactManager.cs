using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class MaterialImpact
{
	public PhysicMaterial physicMaterial;
	
	public AudioClip[] playerFootstepSounds;
	
	public AudioClip[] mechFootstepSounds;
	
	public AudioClip[] spiderFootstepSounds;
	
	public AudioClip[] bulletHitSounds;
}

public class MaterialImpactManager : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The materials.
	/// </summary>
	
	public MaterialImpact[] materials;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The dict.
	/// </summary>
	private static System.Collections.Generic.Dictionary<PhysicMaterial, MaterialImpact> dict;
	
	/// <summary>
	/// The default mat.
	/// </summary>
	private static MaterialImpact defaultMat;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	public void Awake () 
	{
		defaultMat = materials[0];
		
		dict = new System.Collections.Generic.Dictionary<PhysicMaterial, MaterialImpact> ();
		for (int i = 0; i < materials.Length; i++) {
			dict.Add (materials[i].physicMaterial, materials[i]);
		}
	}
	
	/// <summary>
	/// Gets the player footstep sound.
	/// </summary>
	/// <returns>
	/// The player footstep sound.
	/// </returns>
	/// <param name='mat'>
	/// Mat.
	/// </param>
	static public AudioClip GetPlayerFootstepSound (PhysicMaterial mat) 
	{
		MaterialImpact imp = GetMaterialImpact (mat);
		return GetRandomSoundFromArray(imp.playerFootstepSounds);
	}
	
	/// <summary>
	/// Gets the mech footstep sound.
	/// </summary>
	/// <returns>
	/// The mech footstep sound.
	/// </returns>
	/// <param name='mat'>
	/// Mat.
	/// </param>
	static public AudioClip GetMechFootstepSound (PhysicMaterial mat) 
	{
		MaterialImpact imp = GetMaterialImpact (mat);
		return GetRandomSoundFromArray(imp.mechFootstepSounds);
	}
	
	/// <summary>
	/// Gets the spider footstep sound.
	/// </summary>
	/// <returns>
	/// The spider footstep sound.
	/// </returns>
	/// <param name='mat'>
	/// Mat.
	/// </param>
	static public AudioClip GetSpiderFootstepSound (PhysicMaterial mat) 
	{
		MaterialImpact imp = GetMaterialImpact (mat);
		return GetRandomSoundFromArray(imp.spiderFootstepSounds);
	}
	
	/// <summary>
	/// Gets the bullet hit sound.
	/// </summary>
	/// <returns>
	/// The bullet hit sound.
	/// </returns>
	/// <param name='mat'>
	/// Mat.
	/// </param>
	static public AudioClip GetBulletHitSound (PhysicMaterial mat) 
	{
		MaterialImpact imp = GetMaterialImpact (mat);
		return GetRandomSoundFromArray(imp.bulletHitSounds);
	}
	
	/// <summary>
	/// Gets the material impact.
	/// </summary>
	/// <returns>
	/// The material impact.
	/// </returns>
	/// <param name='mat'>
	/// Mat.
	/// </param>
	static public MaterialImpact GetMaterialImpact (PhysicMaterial mat) 
	{
		if (mat && dict.ContainsKey (mat))
			return dict[mat];
		return defaultMat;
	}
	
	/// <summary>
	/// Gets the random sound from array.
	/// </summary>
	/// <returns>
	/// The random sound from array.
	/// </returns>
	/// <param name='audioClipArray'>
	/// Audio clip array.
	/// </param>
	static public AudioClip GetRandomSoundFromArray (AudioClip[] audioClipArray) 
	{
		if (audioClipArray.Length > 0)
		{
			int index = new Random().RandomRange (0, audioClipArray.Length - 1);
			//Debug.Log("GetRandomSoundFromArray: " + audioClipArray.Length.ToString() + ", " + index.ToString());
			return audioClipArray[index];
		}
		return null;
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

