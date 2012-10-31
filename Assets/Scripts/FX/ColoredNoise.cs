using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Noise")]

/// <summary>
/// Colored noise.
/// </summary>
public class ColoredNoise : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The global noise amount.
	/// </summary>
	public float globalNoiseAmount = 0.075f;
	
	/// <summary>
	/// The global noise amount on damage.
	/// </summary>
	public float globalNoiseAmountOnDamage = -6.0f;
	
	/// <summary>
	/// The local noise amount.
	/// </summary>
	public float localNoiseAmount = 0.0f;
	
	/// <summary>
	/// The noise texture.
	/// </summary>
	public Texture2D noiseTexture;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The noise shader.
	/// </summary>
	private Shader noiseShader;
	
	/// <summary>
	/// The noise.
	/// </summary>
	private Material noise = null;
	
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
		FindShaders ();
		CheckSupport ();
		CreateMaterials ();	
	}
	
	/// <summary>
	/// Finds the shaders.
	/// </summary>
	public void FindShaders () 
	{	
		if (!noiseShader)
			noiseShader = Shader.Find("Hidden/ColoredNoise");
	}
	
	/// <summary>
	/// Creates the materials.
	/// </summary>
	public void CreateMaterials () 
	{		
		if (!noise) {
			noise = new Material (noiseShader);	
			noise.hideFlags = HideFlags.DontSave;
		}           
	}
	
	/// <summary>
	/// Supported this instance.
	/// </summary>
	public bool Supported () 
	{
		return (SystemInfo.supportsImageEffects && SystemInfo.supportsRenderTextures && noiseShader.isSupported);
	}
	
	/// <summary>
	/// Checks the support.
	/// </summary>
	/// <returns>
	/// The support.
	/// </returns>
	public bool CheckSupport () 
	{
		if (!Supported ()) {
			enabled = false;
			return false;
		}	
		return true;
	}
	
	/// <summary>
	/// Raises the disable event.
	/// </summary>
	public void OnDisable () 
	{
		if (noise) {
			DestroyImmediate (noise);
			noise = null;	
		}
	}
	
	/// <summary>
	/// Raises the render image event.
	/// </summary>
	/// <param name='source'>
	/// Source.
	/// </param>
	/// <param name='destination'>
	/// Destination.
	/// </param>
	public void OnRenderImage (RenderTexture source, RenderTexture destination) 
	{		
	#if UNITY_EDITOR
		FindShaders ();
		CheckSupport ();
		CreateMaterials ();	
	#endif
							
		noise.SetFloat ("_NoiseAmount", globalNoiseAmount + localNoiseAmount * Mathf.Sign (globalNoiseAmount));
		noise.SetTexture ("_NoiseTex", noiseTexture);	
	
		DrawNoiseQuadGrid (source, destination, noise, noiseTexture, 0);
	}

	/// <summary>
	/// Helper to draw a screenspace grid of quads with random texture coordinates
	/// </summary>
	/// <param name='source'>
	/// Source.
	/// </param>
	/// <param name='dest'>
	/// Destination.
	/// </param>
	/// <param name='fxMaterial'>
	/// Fx material.
	/// </param>
	/// <param name='noise'>
	/// Noise.
	/// </param>
	/// <param name='passNr'>
	/// Pass nr.
	/// </param>
	static public void DrawNoiseQuadGrid (RenderTexture source, RenderTexture dest, Material fxMaterial, Texture2D noise, int passNr) 
	{
		RenderTexture.active = dest;
		
		#if UNITY_XBOX360
		    var tileSize : float = 128.0f;
		#else
		    float tileSize = 64.0f;
		#endif
		
		float subDs = (1.0f * source.width) / tileSize;
	       
		fxMaterial.SetTexture ("_MainTex", source);	        
	                
		GL.PushMatrix ();
		GL.LoadOrtho ();	
			
		float aspectCorrection = (1.0f * source.width) / (1.0f * source.height);
		float stepSizeX = 1.0f / subDs;
		float stepSizeY = stepSizeX * aspectCorrection; 
	   	float texTile = tileSize / (noise.width * 1.0f);
	    	    	
		fxMaterial.SetPass (passNr);	
		
	    GL.Begin (GL.QUADS);
	    
	   	for(float x1 = 0.0f; x1 < 1.0f; x1 += stepSizeX) 
		{
	   		for(float y1 = 0.0f; y1 < 1.0f; y1 += stepSizeY) 
			{
	   			float tcXStart = new Random().RandomRange(0,200)/100.0f - 1.0f;
	   			float tcYStart = new Random().RandomRange(0,200)/100.0f - 1.0f;
	   			float texTileMod = Mathf.Sign (new Random().RandomRange(0,200)/100.0f - 1.0f);
							
			    GL.MultiTexCoord2 (0, tcXStart, tcYStart); 
			    GL.Vertex3 (x1, y1, 0.1f);
			    GL.MultiTexCoord2 (0, tcXStart + texTile * texTileMod, tcYStart); 
			    GL.Vertex3 (x1 + stepSizeX, y1, 0.1f);
			    GL.MultiTexCoord2 (0, tcXStart + texTile * texTileMod, tcYStart + texTile * texTileMod); 
			    GL.Vertex3 (x1 + stepSizeX, y1 + stepSizeY, 0.1f);
			    GL.MultiTexCoord2 (0, tcXStart, tcYStart + texTile * texTileMod); 
			    GL.Vertex3 (x1, y1 + stepSizeY, 0.1f);
	   		}
	   	}
	    	
		GL.End ();
	    GL.PopMatrix ();
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



