using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]

/// <summary>
/// ShaderDatabase
/// knows and eventually "cooks" shaders in the beginning of the game (see CookShaders),
/// also knows some tricks to hide the frame buffer with white and/or black planes
/// to hide loading artefacts or shader cooking process
/// </summary>
public class ShaderDatabase : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The shaders.
	/// </summary>
	public Shader[] shaders;
	
	/// <summary>
	/// The cook shaders on mobiles.
	/// </summary>
	public bool cookShadersOnMobiles = true;
	
	/// <summary>
	/// The cook shaders cover.
	/// </summary>
	public Material cookShadersCover;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The cook shaders object.
	/// </summary>
	private GameObject cookShadersObject;
	
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
	#if UNITY_IPHONE || UNITY_ANDROID
		Screen.sleepTimeout = 0.0f;
	
		if (!cookShadersOnMobiles)
			return;
			
		if (!cookShadersCover.HasProperty ("_TintColor"))
			Debug.LogWarning ("Dualstick: the CookShadersCover material needs a _TintColor property to properly hide the cooking process", transform);
		
		CreateCameraCoverPlane ();
		cookShadersCover.SetColor ("_TintColor", Color (0.0,0.0,0.0,1.0));
	#endif
	}
	
	/// <summary>
	/// Creates the camera cover plane.
	/// </summary>
	/// <returns>
	/// The camera cover plane.
	/// </returns>
	public GameObject CreateCameraCoverPlane ()  
	{
		cookShadersObject = GameObject.CreatePrimitive (PrimitiveType.Cube);
		cookShadersObject.renderer.material = cookShadersCover;	
		cookShadersObject.transform.parent = transform;
		cookShadersObject.transform.localPosition = Vector3.zero;
		cookShadersObject.transform.localPosition.Set
			( 
			  cookShadersObject.transform.localPosition.x
			, cookShadersObject.transform.localPosition.y
			, cookShadersObject.transform.localPosition.z + 1.55f
			);
		cookShadersObject.transform.localRotation = Quaternion.identity;
		cookShadersObject.transform.localEulerAngles.Set
			( 
			  cookShadersObject.transform.localEulerAngles.x
			, cookShadersObject.transform.localEulerAngles.y
			, cookShadersObject.transform.localEulerAngles.z + 180.0f
			);
		cookShadersObject.transform.localScale = Vector3.one *1.5f;	
		cookShadersObject.transform.localScale.Set
			( 
			  cookShadersObject.transform.localScale.x
			, cookShadersObject.transform.localScale.y
			, cookShadersObject.transform.localScale.z * 1.6f
			);	
		
		return cookShadersObject;		
	}
	
	/// <summary>
	/// Whites the out.
	/// </summary>
	public IEnumerator WhiteOut () 
	{
		CreateCameraCoverPlane ();
		Material mat  = cookShadersObject.renderer.sharedMaterial;
		mat.SetColor ("_TintColor", new Color (1.0f, 1.0f, 1.0f, 0.0f));	
		
		yield return null;
		
		Color c  = new Color (1.0f, 1.0f, 1.0f, 0.0f);
		while (c.a < 1.0) {
			c.a += Time.deltaTime * 0.25f;
			mat.SetColor ("_TintColor", c);
			yield return null;
		}
				
		DestroyCameraCoverPlane ();
	}
	
	/// <summary>
	/// Whites the in.
	/// </summary>
	/// <returns>
	/// The in.
	/// </returns>
	public IEnumerator WhiteIn () 
	{	
		CreateCameraCoverPlane ();
		Material mat = cookShadersObject.renderer.sharedMaterial;
		mat.SetColor ("_TintColor", new Color (1.0f, 1.0f, 1.0f, 1.0f));	
		
		yield return null;
		
		Color c = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		while (c.a > 0.0) {
			c.a -= Time.deltaTime * 0.25f;
			mat.SetColor ("_TintColor", c);
			yield return null;
		}
				
		DestroyCameraCoverPlane ();
	}
	
	/// <summary>
	/// Destroies the camera cover plane.
	/// </summary>
	public void DestroyCameraCoverPlane () 
	{
		if (cookShadersObject)
			DestroyImmediate (cookShadersObject);	
		cookShadersObject = null;
	}
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	public void Start () 
	{	
	#if UNITY_IPHONE || UNITY_ANDROID	
		if (cookShadersOnMobiles)
			yield CookShaders ();	
	#endif
	}
	
	/// <summary>
	/// this function is cooking all shaders to be used in the game. 
	/// it's good practice to draw all of them in order to avoid
	/// triggering in game shader compilations which might cause evil
	/// frame time spikes
	/// currently only enabled for mobile (iOS and Android) platforms
	/// </summary>
	/// <returns>
	/// The shaders.
	/// </returns>
	public IEnumerator CookShaders () 
	{
		if (shaders.GetLength(0) > 0) 
		{
			Material m = new Material (shaders[0]);
			GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
		
			cube.transform.parent = transform;
			cube.transform.localPosition = Vector3.zero;
			cube.transform.localPosition.Set
				( 
				  cube.transform.localPosition.x
				, cube.transform.localPosition.y
				, cube.transform.localPosition.z + 4.0f
				);
				
			yield return null;	
			
			foreach (Shader s in shaders) 
			{
				if (s) {
					m.shader = s;
					cube.renderer.material = m;
				}
				yield return null;
			}
						 
			Destroy (m);
			Destroy (cube);
			
			yield return null;
			Color c = Color.black;
			c.a = 1.0f;
			while (c.a>0.0f) 
			{
				c.a -= Time.deltaTime*0.5f;
				cookShadersCover.SetColor ("_TintColor", c);
				yield return null;
			}
		}
	
		DestroyCameraCoverPlane ();
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

