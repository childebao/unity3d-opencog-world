using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

/// <summary>
/// Render fog plane.
/// </summary>
public class RenderFogPlane : MonoBehaviour
{
	//////////////////////////////////////////////////
	
	#region Public Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The camera for ray.
	/// </summary>
	public Camera cameraForRay;
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
		
	#region Private Member Data
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// The frustum corners.
	/// </summary>
	private Matrix4x4 frustumCorners;
	
	/// <summary>
	/// The CAMER a_ ASPEC t_ RATI.
	/// </summary>
	private float CAMERA_ASPECT_RATIO = 1.333333f;
	
	/// <summary>
	/// The CAMER a_ NEA.
	/// </summary>
	private float CAMERA_NEAR;
	
	/// <summary>
	/// The CAMER a_ FA.
	/// </summary>
	private float CAMERA_FAR;
	
	/// <summary>
	/// The CAMER a_ FO.
	/// </summary>
	private float CAMERA_FOV;
	
	/// <summary>
	/// The mesh.
	/// </summary>
	private Mesh mesh;
	
	/// <summary>
	/// The uv.
	/// </summary>
	private Vector2[] uv = new Vector2[4];
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Public Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Raises the enable event.
	/// </summary>
	public void OnEnable () 
	{			
		renderer.enabled = true;
		
		if (!mesh)
			mesh = GetComponent<MeshFilter>().sharedMesh;
				
		// write indices into uv's for fast world space reconstruction		
				
		if (mesh) { 
			uv[0] = new Vector2 (1.0f, 1.0f); // TR
			uv[1] = new Vector2 (0.0f, 0.0f); // TL
			uv[2] = new Vector2 (2.0f, 2.0f); // BR
			uv[3] = new Vector2 (3.0f, 3.0f); // BL
			mesh.uv = uv;
		}	
		
		if (!cameraForRay)
			cameraForRay = Camera.main;	
	}
	
	/// <summary>
	/// Raises the disable event.
	/// </summary>
	public void OnDisable () 
	{
		renderer.enabled = false;
	}
	
	/// <summary>
	/// Supported this instance.
	/// </summary>
	public bool Supported () 
	{
		return (renderer.sharedMaterial.shader.isSupported && SystemInfo.supportsImageEffects && SystemInfo.supportsRenderTextures && SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth));
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public void Update () 
	{	
		if (EarlyOutIfNotSupported ()) {
			enabled = false;
			return;
		}
		if (!renderer.enabled)
			return;	
		
		frustumCorners = Matrix4x4.identity;
			
		Ray ray;
		Vector4 vec;
		Vector3 corner;
		
		CAMERA_NEAR = cameraForRay.nearClipPlane;
		CAMERA_FAR = cameraForRay.farClipPlane;
		CAMERA_FOV = cameraForRay.fieldOfView;
		CAMERA_ASPECT_RATIO = cameraForRay.aspect;
	
		float fovWHalf = CAMERA_FOV * 0.5f;
		
		Vector3 toRight = cameraForRay.transform.right * CAMERA_NEAR * Mathf.Tan (fovWHalf * Mathf.Deg2Rad) * CAMERA_ASPECT_RATIO;
		Vector3 toTop = cameraForRay.transform.up * CAMERA_NEAR * Mathf.Tan (fovWHalf * Mathf.Deg2Rad);
	
		Vector3 topLeft = (cameraForRay.transform.forward * CAMERA_NEAR - toRight + toTop);
		float CAMERA_SCALE = topLeft.magnitude * CAMERA_FAR/CAMERA_NEAR;
		
		// correctly place transform first
	
		transform.localPosition.Set(transform.localPosition.x, transform.localPosition.y, CAMERA_NEAR + 0.0001f);
		transform.localScale.Set( (toRight * 0.5f).magnitude, 1.0f, (toTop * 0.5f).magnitude);
		transform.localRotation.eulerAngles.Set(270.0f, 0.0f, 0.0f);
	
		// write view frustum corner "rays"
		
		topLeft.Normalize();
		topLeft *= CAMERA_SCALE;
	
		Vector3 topRight = (cameraForRay.transform.forward * CAMERA_NEAR + toRight + toTop);
		topRight.Normalize();
		topRight *= CAMERA_SCALE;
		
		Vector3 bottomRight = (cameraForRay.transform.forward * CAMERA_NEAR + toRight - toTop);
		bottomRight.Normalize();
		bottomRight *= CAMERA_SCALE;
		
		Vector3 bottomLeft = (cameraForRay.transform.forward * CAMERA_NEAR - toRight - toTop);
		bottomLeft.Normalize();
		bottomLeft *= CAMERA_SCALE;
				
		frustumCorners.SetRow (0, topLeft); 
		frustumCorners.SetRow (1, topRight);		
		frustumCorners.SetRow (2, bottomRight);
		frustumCorners.SetRow (3, bottomLeft);
								
		renderer.sharedMaterial.SetMatrix ("_FrustumCornersWS", frustumCorners);
		renderer.sharedMaterial.SetVector ("_CameraWS", cameraForRay.transform.position);
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
	
	#region Private Member Functions
	
	//////////////////////////////////////////////////
	
	/// <summary>
	/// Earlies the out if not supported.
	/// </summary>
	/// <returns>
	/// The out if not supported.
	/// </returns>
	private bool EarlyOutIfNotSupported () {
		if (!Supported ()) {
			enabled = false;
			return true;
		}	
		return false;
	}
	
	//////////////////////////////////////////////////
	
	#endregion
	
	//////////////////////////////////////////////////
}



