using UnityEngine;
using System.Collections;

public class Fire_Projectile : MonoBehaviour {
	
	public float ProjectileSpeed = 0.0f;
	public float MaxDistance = 100.0f;
	
	public Vector4 moveVector = Vector4.zero;
	private float distanceMoved = 0.0f;
	private Transform myTransform;
	
	// Use this for initialization
	void Start () 
	{
		myTransform = transform;
		
		RaycastHit hit;
        Ray ray = Camera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
		
		moveVector = ray.direction.normalized;
		//moveVector = -GameObject.Find("Player").transform.forward.normalized;
		//moveVector = transform.up;
		
		myTransform.Translate(moveVector * ProjectileSpeed * Time.deltaTime);
	}
	
	// Update is called once per frame
	void Update () 
	{
		myTransform.Translate(moveVector * ProjectileSpeed * Time.deltaTime);
	
		distanceMoved += ProjectileSpeed * Time.deltaTime;
		
		if(distanceMoved > MaxDistance)
		{
			Destroy(gameObject);
		}
	}
}
