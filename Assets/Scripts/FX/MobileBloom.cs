using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Mobile Bloom")]

/// <summary>
/// Mobile bloom.
/// </summary>
public class MobileBloom : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The intensity.
	/// </summary>
	public float intensity = 0.5f;
	
	/// <summary>
	/// The color mix.
	/// </summary>
	public Color colorMix = Color.white;
	
	/// <summary>
	/// The color mix blend.
	/// </summary>
	public float colorMixBlend = 0.25f;
	
	/// <summary>
	/// The agony tint.
	/// </summary>
	public float agonyTint = 0.0f;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The bloom shader.
	/// </summary>
	private Shader bloomShader;
	
	/// <summary>
	/// The apply.
	/// </summary>
	private Material apply = null;
	
	/// <summary>
	/// The rt format.
	/// </summary>
	private RenderTextureFormat rtFormat = RenderTextureFormat.Default;
	
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
		if (!bloomShader)
			bloomShader = Shader.Find("Hidden/MobileBloom");
	}
	
	/// <summary>
	/// Creates the materials.
	/// </summary>
	public void CreateMaterials () 
	{		
		if (!apply) {
			apply = new Material (bloomShader);	
			apply.hideFlags = HideFlags.DontSave;
		}           
	}
	
	/// <summary>
	/// Raises the damage event.
	/// </summary>
	public void OnDamage () 
	{
		agonyTint = 1.0f;	
	}
	
	/// <summary>
	/// Supported this instance.
	/// </summary>
	public bool Supported () 
	{
		return (SystemInfo.supportsImageEffects && SystemInfo.supportsRenderTextures && bloomShader.isSupported);
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
		rtFormat = SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.RGB565) ? RenderTextureFormat.RGB565 : RenderTextureFormat.Default;
		return true;
	}
	
	/// <summary>
	/// Raises the disable event.
	/// </summary>
	public void OnDisable () 
	{
		if (apply) {
			DestroyImmediate (apply);
			apply = null;	
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
	
		agonyTint = Mathf.Clamp01 (agonyTint - Time.deltaTime * 2.75f);
			
		RenderTexture tempRtLowA = RenderTexture.GetTemporary (source.width / 4, source.height / 4, (int)rtFormat);
		RenderTexture tempRtLowB = RenderTexture.GetTemporary (source.width / 4, source.height / 4, (int)rtFormat);
		
		// prepare data
		
		apply.SetColor ("_ColorMix", colorMix);
		apply.SetVector ("_Parameter", new Vector4 (colorMixBlend * 0.25f,  0.0f, 0.0f, 1.0f - intensity - agonyTint));	
		
		// downsample & blur
		
		Graphics.Blit (source, tempRtLowA, apply, agonyTint < 0.5f ? 1 : 5);
		Graphics.Blit (tempRtLowA, tempRtLowB, apply, 2);
		Graphics.Blit (tempRtLowB, tempRtLowA, apply, 3);
		
		// apply
		
		apply.SetTexture ("_Bloom", tempRtLowA);
		Graphics.Blit (source, destination, apply, QualityManager.quality > QualityManager.Quality.Medium ? 4 : 0);
		
		RenderTexture.ReleaseTemporary (tempRtLowA);
		RenderTexture.ReleaseTemporary (tempRtLowB);
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



