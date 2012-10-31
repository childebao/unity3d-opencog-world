using UnityEngine;
using System.Collections;

/// <summary>
/// Post effects.
/// </summary>
public class PostEffects : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
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
	/// Checks the shader and create material.
	/// </summary>
	/// <returns>
	/// The shader and create material.
	/// </returns>
	/// <param name='s'>
	/// S.
	/// </param>
	/// <param name='m2Create'>
	/// M2 create.
	/// </param>
	public static Material CheckShaderAndCreateMaterial(Shader s, Material m2Create) 
	{
		if (m2Create && m2Create.shader == s) 
			return m2Create;
			
		if (!s) { 
			Debug.LogWarning("PostEffects: missing shader for " + m2Create.ToString ());
			return null;
		}
		
		if(!s.isSupported) {
			Debug.LogWarning ("The shader " + s.ToString () + " is not supported");
			return null;
		}
		else {
			m2Create = new Material (s);	
			m2Create.hideFlags = HideFlags.DontSave;		
			return m2Create;
		}
	}
		
	/// <summary>
	/// Checks the support.
	/// </summary>
	/// <returns>
	/// The support.
	/// </returns>
	/// <param name='needDepth'>
	/// If set to <c>true</c> need depth.
	/// </param>
	public static bool CheckSupport (bool needDepth) 
	{		
		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures) {
			Debug.Log ("Disabling image effect as this platform doesn't support any");
			return false;
		}	
		
		if(needDepth && !SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth)) {
			Debug.Log ("Disabling image effect as depth textures are not supported on this platform.");
			return false;
		}
		
		return true;
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



