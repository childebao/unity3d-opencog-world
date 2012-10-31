using UnityEngine;
using System.Collections;
using Embodiment;
// this object can be put on  the top of  other object
// for example: a pan can be put on a stove
// note: this gameobject should be a trigger

[RequireComponent (typeof (BoxCollider))]

public class PutOnAbleObject : OCBehaviour {
	
	/// Set what objects this object can be put on
    public string[] PutOnObjectNames; 
	private ActionSummary putOnAction;
	private GameObject theObjToUnder = null; // currently, I can be put on theObjToUnder, but I have not been put on it yet
	public GameObject theObjUnder = null; // I currently am put on theObjUnder
	private bool isBeHeld = false;

	// Use this for initialization
	void Start () {

		AnimSummary animPickup = new AnimSummary("pickup");
        PhysiologicalEffect pickupEffect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		putOnAction = new ActionSummary(this,"PutOn", animPickup, pickupEffect, true);
		putOnAction.usesCallback = true;
		myActionList.Add("PutOn");
		StateChangesRegister.RegisterState(gameObject, this,"theObjUnder" );
		
	}
	
	public GameObject getTheObjUnder()
	{
		return theObjUnder;
	}
	
	public void OnBeHeld()
	{
		// when I am held by an avatar,
		// then I can be put on other the top of other objects
		isBeHeld = true;
		theObjUnder = null;
		
	}
	
	private void OnNotBeHeld(Avatar formerHolder)
	{
		// When I am not be held by an avatar any more,				
		RemoveAction(formerHolder);
		isBeHeld = false;
		
	}
	
	/**
     * This is called when the Avatar approaches the object
	 */
	public void AddAction(Avatar avatar)
    {
		//ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		//AM.addAction(putOnAction);
	}
	
	/**
     * This is called when the Avatar moves away from the object
	 */
	public void RemoveAction(Avatar avatar)
    {
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "PutOn");
	}

	
	void Update ()
	{
		// if I am currently held by an avatar, 
		if (isBeHeld && theObjToUnder == null)
		{	
			GameObject[] objs = GameObject.FindGameObjectsWithTag("OCObject");
		    // if there is an object in PutOnObjectNames 
			foreach(GameObject enteredObj in objs)
			{
			    foreach(string child in PutOnObjectNames) 
				{
					if (enteredObj.name == child)
					{
						// if this object is nearby
						float distance = Vector3.Distance(enteredObj.transform.position,gameObject.transform.position);
						if ( distance < 2.0)
						{   
							// add the "put me on the enteredObj" action to the avatar
							Picker picker = gameObject.GetComponent<Picker>();
							Avatar holder = picker.getHolder();
							if (holder == null)
								return;
							
							ActionManager AM = holder.GetComponent<ActionManager>() as ActionManager;
							AM.addAction(putOnAction);
							
							theObjToUnder = enteredObj;
							Debug.Log("There is a " + child +"nearby.");
							break;
						}
					}
				}
			}
		}
		else if (isBeHeld && theObjToUnder != null)
		{
			// if the object is not near by any more, remove the "put on" action from the holder
			float distance = Vector3.Distance(theObjToUnder.transform.position,gameObject.transform.position);
			if (distance > 2.0)
			{
				Picker picker = gameObject.GetComponent<Picker>();
				Avatar holder = picker.getHolder();
				if (holder == null)
					return;
				
				RemoveAction(holder);
				theObjToUnder = null;
			}
		}
	
	}

	// I am to be put on the top of the Object theObjUnder 
	public void PutOn(Avatar a, ActionTarget theTargetToUnder = null, ActionCompleteHandler completionCallback=null) 
	{
	   
		if (theObjToUnder == null) 
		{
			Debug.LogError("There is not a suitable place to put on.");
			return;
		}
		
		if (theObjUnder != null)
		{
			Debug.LogError("This object has already been put on other object.");
			return;
		}
		// remove me from my holder
		a.removeFromInventory(gameObject);
		ActionManager AM = a.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "Drop");
		
        Vector3 objectSize = Vector3.up * (theObjToUnder.collider.bounds.size.y / 2.0f);
        gameObject.transform.parent = theObjToUnder.transform;
        Vector3 mySize = Vector3.up * (gameObject.collider.bounds.size.y / 2.0f);
        gameObject.transform.position = theObjToUnder.transform.position + mySize + objectSize;
        gameObject.transform.localRotation = Quaternion.identity;
        
		gameObject.rigidbody.isKinematic = true;
        gameObject.collider.isTrigger = true;

 	
		theObjUnder = theObjToUnder;
		theObjToUnder = null;
		
		RemoveAction(a);
		
		// if the my pos is now far away for pick up,
		// then remove the "pickup" action from the avatar
		float distance = Vector3.Distance(a.transform.position, gameObject.transform.position);

		if (distance > 2.0)
		{
			AM.removeAction(gameObject.GetInstanceID(), "PickUp");
			// and must remove the "PickUp" action of all the objects currently attach to this gameObject
			foreach (Transform child in transform)
			{
				Picker pi = child.GetComponent<Picker>() as Picker;
				if (pi != null)
				{
					AM.removeAction(pi.gameObject.GetInstanceID(), "PickUp");
				}
			}
		}
		
		if (completionCallback != null) 
		{
	        ArrayList pp = new ArrayList();
	        pp.Add(new ActionTarget(theObjUnder.GetInstanceID(), EmbodimentXMLTags.ORDINARY_OBJECT_TYPE));
			
	        ActionResult ar = new ActionResult(putOnAction, ActionResult.Status.SUCCESS, a, pp, a.gameObject.name + " put " +  gameObject.name + " on " + theObjUnder.name);
	        completionCallback(ar);
   	 	}

	}
}
