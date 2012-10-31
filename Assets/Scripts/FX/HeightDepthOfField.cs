using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Height Depth of Field")]

/// <summary>
/// Height depth of field.
/// </summary>
public class HeightDepthOfField : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Classes
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Dof quality setting.
	/// </summary>
	public enum DofQualitySetting 
	{
		OnlyBackground = 1,
		BackgroundAndForeground = 2,
	}
	
	/// <summary>
	/// Dof resolution.
	/// </summary>
	public enum DofResolution
	{
		High = 2,
		Medium = 3,
		Low = 4,	
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The resolution.
	/// </summary>
	public DofResolution resolution  = DofResolution.High;
	
	/// <summary>
	/// The object focus.
	/// </summary>
	public Transform objectFocus = null;
	 
	/// <summary>
	/// The max blur spread.
	/// </summary>
	public float maxBlurSpread = 1.55f;
	
	/// <summary>
	/// The foreground blur extrude.
	/// </summary>
	public float foregroundBlurExtrude = 1.055f;
	
	/// <summary>
	/// The smoothness.
	/// </summary>
	public float smoothness = 1.0f;
	   
	/// <summary>
	/// The visualize.
	/// </summary>
	public bool visualize = false;
   
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The dof blur shader.
	/// </summary>
	private Shader dofBlurShader;
	
	/// <summary>
	/// The dof blur material.
	/// </summary>
	private Material dofBlurMaterial = null;	
	
	/// <summary>
	/// The dof shader.
	/// </summary>
	private Shader dofShader;
	
	/// <summary>
	/// The dof material.
	/// </summary>
	private Material dofMaterial = null;
	
	/// <summary>
	/// The height of the width over.
	/// </summary>
	private float widthOverHeight = 1.25f;
	
	/// <summary>
	/// The size of the one over base.
	/// </summary>
	private float oneOverBaseSize = 1.0f / 512.0f;
	
	/// <summary>
	/// The camera near.
	/// </summary>
	private float cameraNear = 0.5f;
	
	/// <summary>
	/// The camera far.
	/// </summary>
	private float cameraFar = 50.0f;
	
	/// <summary>
	/// The camera fov.
	/// </summary>
	private float cameraFov = 60.0f;	
	
	/// <summary>
	/// The camera aspect.
	/// </summary>
	private float cameraAspect = 1.333333f;
	
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
		if (!dofBlurShader)
			dofBlurShader = Shader.Find("Hidden/BlurPassesForDOF");
		if (!dofShader)
			dofShader = Shader.Find("Hidden/HeightDepthOfField");	
	}
	
	/// <summary>
	/// Creates the materials.
	/// </summary>
	public void CreateMaterials () 
	{		
		if (!dofBlurMaterial)
			dofBlurMaterial = PostEffects.CheckShaderAndCreateMaterial (dofBlurShader, dofBlurMaterial);
		if (!dofMaterial)
			dofMaterial = PostEffects.CheckShaderAndCreateMaterial (dofShader, dofMaterial);           
	}
	
	/// <summary>
	/// Supported this instance.
	/// </summary>
	public bool Supported () 
	{
		return (PostEffects.CheckSupport (true) && dofBlurShader.isSupported && dofShader.isSupported);
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
	public void OnDisable () {
		if (dofBlurMaterial) {
			DestroyImmediate (dofBlurMaterial);
			dofBlurMaterial = null;	
		}	
		if (dofMaterial) {
			DestroyImmediate (dofMaterial);
			dofMaterial = null;	
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
		
		widthOverHeight = (1.0f * source.width) / (1.0f * source.height);
		oneOverBaseSize = 1.0f / 512.0f;		
		
		cameraNear = camera.nearClipPlane;
		cameraFar = camera.farClipPlane;
		cameraFov = camera.fieldOfView;
		cameraAspect = camera.aspect;
	
		Matrix4x4 frustumCorners = Matrix4x4.identity;		
		Vector4 vec;
		Vector3 corner;
		float fovWHalf = cameraFov * 0.5f;
		Vector3 toRight = camera.transform.right * cameraNear * Mathf.Tan (fovWHalf * Mathf.Deg2Rad) * cameraAspect;
		Vector3 toTop = camera.transform.up * cameraNear * Mathf.Tan (fovWHalf * Mathf.Deg2Rad);
		Vector3 topLeft = (camera.transform.forward * cameraNear - toRight + toTop);
		float cameraScaleFactor = topLeft.magnitude * cameraFar/cameraNear;	
			
		topLeft.Normalize();
		topLeft *= cameraScaleFactor;
	
		Vector3 topRight = (camera.transform.forward * cameraNear + toRight + toTop);
		topRight.Normalize();
		topRight *= cameraScaleFactor;
		
		Vector3 bottomRight = (camera.transform.forward * cameraNear + toRight - toTop);
		bottomRight.Normalize();
		bottomRight *= cameraScaleFactor;
		
		Vector3 bottomLeft = (camera.transform.forward * cameraNear - toRight - toTop);
		bottomLeft.Normalize();
		bottomLeft *= cameraScaleFactor;
				
		frustumCorners.SetRow (0, topLeft); 
		frustumCorners.SetRow (1, topRight);		
		frustumCorners.SetRow (2, bottomRight);
		frustumCorners.SetRow (3, bottomLeft);	
		
		dofMaterial.SetMatrix ("_FrustumCornersWS", frustumCorners);
		dofMaterial.SetVector ("_CameraWS", camera.transform.position);			
		
		Transform t;
		if (!objectFocus)
			t = camera.transform;
		else
			t = objectFocus.transform;
																		
		dofMaterial.SetVector ("_ObjectFocusParameter", new Vector4 (	
					t.position.y - 0.25f, t.localScale.y * 1.0f / smoothness, 1.0f, objectFocus ? objectFocus.collider.bounds.extents.y * 0.75f : 0.55f));
	       		
		dofMaterial.SetFloat ("_ForegroundBlurExtrude", foregroundBlurExtrude);
		dofMaterial.SetVector ("_InvRenderTargetSize", new Vector4 (1.0f / (1.0f * source.width), 1.0f / (1.0f * source.height),0.0f,0.0f));
		
		int divider = 1;
		if (resolution == DofResolution.Medium)
			divider = 2;
		else if (resolution >= DofResolution.Medium)
			divider = 3;
		
		RenderTexture hrTex = RenderTexture.GetTemporary (source.width, source.height, 0); 
		RenderTexture mediumTexture = RenderTexture.GetTemporary (source.width / divider, source.height / divider, 0);    
		RenderTexture mediumTexture2 = RenderTexture.GetTemporary (source.width / divider, source.height / divider, 0);    
		RenderTexture lowTexture = RenderTexture.GetTemporary (source.width / (divider * 2), source.height / (divider * 2), 0);     
		
		source.filterMode = FilterMode.Bilinear;
		hrTex.filterMode = FilterMode.Bilinear;   
		lowTexture.filterMode = FilterMode.Bilinear;     
		mediumTexture.filterMode = FilterMode.Bilinear;
		mediumTexture2.filterMode = FilterMode.Bilinear;
		
	    // background (coc -> alpha channel)
	   	CustomGraphicsBlit (null, source, dofMaterial, 3);		
	   		
	   	// better downsample (should actually be weighted for higher quality)
	   	mediumTexture2.DiscardContents();
	   	Graphics.Blit (source, mediumTexture2, dofMaterial, 6);	
				
		Blur (mediumTexture2, mediumTexture, 1, 0, maxBlurSpread * 0.75f);			
		Blur (mediumTexture, lowTexture, 2, 0, maxBlurSpread);			
	    	      		
		// some final calculations can be performed in low resolution 		
		dofBlurMaterial.SetTexture ("_TapLow", lowTexture);
		dofBlurMaterial.SetTexture ("_TapMedium", mediumTexture);							
		Graphics.Blit (null, mediumTexture2, dofBlurMaterial, 2);
		
		dofMaterial.SetTexture ("_TapLowBackground", mediumTexture2); 
		dofMaterial.SetTexture ("_TapMedium", mediumTexture); // only needed for debugging		
								
		// apply background defocus
		hrTex.DiscardContents();
		Graphics.Blit (source, hrTex, dofMaterial, visualize ? 2 : 0); 
		
		// foreground handling
		CustomGraphicsBlit (hrTex, source, dofMaterial, 5); 
		
		// better downsample and blur (shouldn't be weighted)
		Graphics.Blit (source, mediumTexture2, dofMaterial, 6);					
		Blur (mediumTexture2, mediumTexture, 1, 1, maxBlurSpread * 0.75f);	
		Blur (mediumTexture, lowTexture, 2, 1, maxBlurSpread);	
		
		// some final calculations can be performed in low resolution		
		dofBlurMaterial.SetTexture ("_TapLow", lowTexture);
		dofBlurMaterial.SetTexture ("_TapMedium", mediumTexture);							
		Graphics.Blit (null, mediumTexture2, dofBlurMaterial, 2);	
		
		if (destination != null)
		    destination.DiscardContents ();
		    
		dofMaterial.SetTexture ("_TapLowForeground", mediumTexture2); 
		dofMaterial.SetTexture ("_TapMedium", mediumTexture); // only needed for debugging	   
		Graphics.Blit (source, destination, dofMaterial, visualize ? 1 : 4);	
		
		RenderTexture.ReleaseTemporary (hrTex);
		RenderTexture.ReleaseTemporary (mediumTexture);
		RenderTexture.ReleaseTemporary (mediumTexture2);
		RenderTexture.ReleaseTemporary (lowTexture);
	}	
	
	/// <summary>
	/// Blur the specified from, to, iterations, blurPass and spread.
	/// flat blur
	/// </summary>
	/// <param name='from'>
	/// From.
	/// </param>
	/// <param name='to'>
	/// To.
	/// </param>
	/// <param name='iterations'>
	/// Iterations.
	/// </param>
	/// <param name='blurPass'>
	/// Blur pass.
	/// </param>
	/// <param name='spread'>
	/// Spread.
	/// </param>
	public void Blur (RenderTexture from, RenderTexture to, int iterations, int blurPass, float spread) 
	{
		RenderTexture tmp = RenderTexture.GetTemporary (to.width, to.height, 0);
		
		if (iterations < 2) {
			dofBlurMaterial.SetVector ("offsets", new Vector4 (0.0f, spread * oneOverBaseSize, 0.0f, 0.0f));
			tmp.DiscardContents ();
			Graphics.Blit (from, tmp, dofBlurMaterial, blurPass);
		
			dofBlurMaterial.SetVector ("offsets", new Vector4 (spread / widthOverHeight * oneOverBaseSize,  0.0f, 0.0f, 0.0f));		
			to.DiscardContents ();
			Graphics.Blit (tmp, to, dofBlurMaterial, blurPass);	 	
		} 
		else {	
			dofBlurMaterial.SetVector ("offsets", new Vector4 (0.0f, spread * oneOverBaseSize, 0.0f, 0.0f));
			tmp.DiscardContents ();
			Graphics.Blit (from, tmp, dofBlurMaterial, blurPass);
			
			dofBlurMaterial.SetVector ("offsets", new Vector4 (spread / widthOverHeight * oneOverBaseSize,  0.0f, 0.0f, 0.0f));		
			to.DiscardContents ();
			Graphics.Blit (tmp, to, dofBlurMaterial, blurPass);	 
		
			dofBlurMaterial.SetVector ("offsets", new Vector4 (spread / widthOverHeight * oneOverBaseSize,  spread * oneOverBaseSize, 0.0f, 0.0f));		
			tmp.DiscardContents ();
			Graphics.Blit (to, tmp, dofBlurMaterial, blurPass);	
		
			dofBlurMaterial.SetVector ("offsets", new Vector4 (spread / widthOverHeight * oneOverBaseSize,  -spread * oneOverBaseSize, 0.0f, 0.0f));		
			to.DiscardContents ();
			Graphics.Blit (tmp, to, dofBlurMaterial, blurPass);	
		}
		
		RenderTexture.ReleaseTemporary (tmp);
	}
	
	/// <summary>
	/// Customs the graphics blit.
	/// used for noise
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
	/// <param name='passNr'>
	/// Pass nr.
	/// </param>
	public void CustomGraphicsBlit (RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr) 
	{
		RenderTexture.active = dest;
		       
		fxMaterial.SetTexture ("_MainTex", source);	        
	        	        
		GL.PushMatrix ();
		GL.LoadOrtho ();	
	    	
		fxMaterial.SetPass (passNr);	
		
	    GL.Begin (GL.QUADS);
							
		GL.MultiTexCoord2 (0, 0.0f, 0.0f); 
		GL.Vertex3 (0.0f, 0.0f, 3.0f); // BL
		
		GL.MultiTexCoord2 (0, 1.0f, 0.0f); 
		GL.Vertex3 (1.0f, 0.0f, 2.0f); // BR
		
		GL.MultiTexCoord2 (0, 1.0f, 1.0f); 
		GL.Vertex3 (1.0f, 1.0f, 1.0f); // TR
		
		GL.MultiTexCoord2 (0, 0.0f, 1.0f); 
		GL.Vertex3 (0.0f, 1.0f, 0.0f); // TL
		
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



