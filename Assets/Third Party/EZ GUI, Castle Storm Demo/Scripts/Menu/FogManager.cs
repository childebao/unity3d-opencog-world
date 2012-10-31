using UnityEngine;
using System.Collections;

public class FogManager : MonoBehaviour 
{
	public GameObject fogParent;
	public float speed;
	
	Transform[] parts;	// Array of fog particle transforms
	float[] rots;		// Rotations for each fog particle
	bool[] dirs;		// Rotation directions for each fog particle
	SpriteRoot[] sprts;	// References to each fog particle's sprite

	// Use this for initialization
	void Start () 
	{
		// Initialize our arrays:
		parts = new Transform[fogParent.transform.childCount];
		rots = new float[parts.Length];
		dirs = new bool[parts.Length];
		sprts = new SpriteRoot[parts.Length];
		
		int i=0;
		
		// Get values for all our arrays, searching each fog
		// particle that is the child of the fog parent.
		foreach(Transform t in fogParent.transform)
		{
			dirs[i] = UnityEngine.Random.Range(0,2) == 1;
			parts[i] = t;
			rots[i] = UnityEngine.Random.Range(0,180);
			sprts[i] = (SpriteRoot) t.gameObject.GetComponent(typeof(SpriteRoot));
			++i;
		}
	}
		
	// Update is called once per frame
	void Update () 
	{
		float coef;
		
		// Loop through every fog particle, checking to see if it is
		// outside of our hard-coded bounds.  If it has exited our
		// bounding area, "loop" the particle back to the other side
		// of the bounding area.  Also rotate and fade the fog particles
		// in/out slowly.
		for(int i=0; i<parts.Length; ++i)
		{
			if(dirs[i])
				coef = 1f;
			else
				coef = -1f;
			
			rots[i] += speed * Time.deltaTime * coef;
			parts[i].localEulerAngles = new Vector3(0,0,rots[i]);
			
			if(parts[i].position.z < -10f)
			{
				parts[i].position = parts[i].position + Vector3.forward * 21f;
			}
			else if(parts[i].position.z > 11f)
			{
				parts[i].position = parts[i].position - Vector3.forward * 21f;
			}
			
			if(parts[i].position.x > 12)
			{
				parts[i].position = parts[i].position - Vector3.right * 24f;
			}
			else if(parts[i].position.x < -12)
			{
				parts[i].position = parts[i].position + Vector3.right * 24f;
			}

			float alpha = (1f + Mathf.Sin(rots[i]/10f))/2f;
			if(parts[i].position.z > 6.1f)
				alpha = Mathf.Clamp(alpha, 0, 1f - (parts[i].position.z-6f) / 5f);
			
			sprts[i].SetColor(new Color(0.5f, 0.5f, 0.5f, alpha));
		}
	}
}
