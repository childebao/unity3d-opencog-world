using UnityEngine;
using Embodiment;
using System.Collections;

// Every gameobject that can be cooked should include this component
// A foodstuff has 4 states: raw, becooking,cooked,overcooked

public enum FOODSTATE
{
	RAW,
	BECOOKING,
	COOKED,
	OVERCOOKED
}

public class FoodStuff : OCBehaviour {
	
	// these belows should be assigned in Unity editor
	public FOODSTATE foodState = FOODSTATE.RAW;
	public float needCookedTime = 8.0f ; // how long does it need to be cooked
	public float overCookedTime = 15.0f; // how long does it need to become over cooked
	public GameObject prefabRawMesh = null;	
	public GameObject prefabBecookingMesh = null;	
	public GameObject prefabCookedMesh = null;	
	public GameObject prefabOverCookedMesh = null;	
	// aboves should be assigned in Unity editor
	
	public float hasBeenCookedTime = 0.0f; // how long has been cooked 
	private bool beingCooked = false; // is now under cooked?
	private float lastTimeUpdate = 0.0f; // The time begin to cook
	
	private GameObject myMesh = null;

	// Use this for initialization
	void Start () 
	{
		MeshUpdate();
		StateChangesRegister.RegisterState(gameObject, this,"foodState" );
	}
	
	public bool isBeingCookedNow()
	{
		return beingCooked;
	}
	
	public void BeginToCook()
	{
		if (beingCooked)
			return;
		
		beingCooked = true;
		foodState = FOODSTATE.BECOOKING;
		lastTimeUpdate = Time.time;
		MeshUpdate();
	}
	
	public void StopToCook()
	{
		// if is not being cooked , log error
		if (!beingCooked)
		{
			Debug.LogError("Error: It is not being cooked! Cannot stop cook!");
			return;
		}

		StateUpdate();
		beingCooked = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (beingCooked)
		{
			hasBeenCookedTime += Time.time - lastTimeUpdate;
			lastTimeUpdate = Time.time;
			StateUpdate();	
		}
	}
	
	private void StateUpdate()
	{
		if ( (hasBeenCookedTime > overCookedTime) && (foodState != FOODSTATE.OVERCOOKED) )
		{
			foodState = FOODSTATE.OVERCOOKED;
			MeshUpdate();
		}
		else if ((hasBeenCookedTime > needCookedTime) && (foodState != FOODSTATE.OVERCOOKED) &&(foodState != FOODSTATE.COOKED ))
		{
			foodState = FOODSTATE.COOKED;
			MeshUpdate();
		}
	}
	
	private void MeshUpdate()
	{
		GameObject myOldMesh = myMesh;
		switch (foodState)
		{
		case FOODSTATE.RAW:
			myMesh = Instantiate(prefabRawMesh) as GameObject;
			break;
		case FOODSTATE.BECOOKING:
			myMesh = Instantiate(prefabBecookingMesh) as GameObject;
			break;
		case FOODSTATE.COOKED:
			myMesh = Instantiate(prefabCookedMesh) as GameObject;
			break;
		case FOODSTATE.OVERCOOKED:
			myMesh = Instantiate(prefabOverCookedMesh) as GameObject;
			break;
		default:
			myMesh = Instantiate(prefabRawMesh) as GameObject;
			break;
		}
		
		if (myOldMesh != null)
		{
			DestroyObject(myOldMesh);
		}
		
		myMesh.transform.parent = gameObject.transform;
		myMesh.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
	}
}
