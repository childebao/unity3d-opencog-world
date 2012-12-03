// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Reflection;
using Embodiment;

/**
 * A class that will allow an avatar to pick up an object that has this behaviour.
 */
public class Picker : OCBehaviour {
	
    private ActionSummary pickupAction;
    private ActionSummary dropAction;
	
	public Avatar holder = null; // the avatar currently hold me

	void Start() {
		
		AnimSummary animPickup = new AnimSummary("createBlock");
        PhysiologicalEffect pickupEffect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		pickupAction = new ActionSummary(this,"PickUp", animPickup, pickupEffect, true);
		pickupAction.usesCallback = true;
		
		AnimSummary animDrop = new AnimSummary("doSomething");
        PhysiologicalEffect dropEffect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		dropAction = new ActionSummary(this, "Drop", animDrop, dropEffect, true);
		dropAction.usesCallback = true;
		if(rigidbody == null) {
			Debug.LogError("Pickup behaviour requires a rigidbody!");
		}
		
		myActionList.Add("PickUp");
		myActionList.Add("Drop");
		
		StateChangesRegister.RegisterState(gameObject, this,"holder" );
		
	}
	
	public Avatar getHolder()
	{
		return holder;
	}
	
	/**
     * This is called when the Avatar approaches the object
	 */
	public void AddAction(Avatar avatar)
    {
		Avatar holder = null;
        foreach (Avatar a in interactors.Values)
        {
            if (a.inventory == gameObject)
            {
                holder = a;
            }
        }
		
        AddInteractor(avatar);

		if (holder != null) {
            ActionManager AM = holder.GetComponent<ActionManager>() as ActionManager;
			AM.addAction(dropAction);
            RemoveActionFromOtherInteractors(holder, "PickUp");
		} else {
			// if the avatar is stand above the object to pick up, It's not allow to pick up
			if (DoesAvatarStepOnObject(avatar, gameObject))
			{
				RemoveInteractor(avatar);
				return;
			}
			
            AddActionToOtherInteractors(null, pickupAction);
		}

        
	}
	
	/**
     * This is called when the Avatar moves away from the object
	 */
	public void RemoveAction(Avatar avatar)
    {
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "Drop");
		AM.removeAction(gameObject.GetInstanceID(), "PickUp");

        RemoveInteractor(avatar);
	}
	
	/**
     * Let the Avatar pick up this GameObject.
	 */
	public void PickUp (Avatar a, ActionCompleteHandler completionCallback=null) {
		
		// if the avatar is stand above the object to pick up, It's not allow to pick up
		if (DoesAvatarStepOnObject(a, gameObject))
		{
			Debug.LogWarning("You are standing above the object, you cannot pick up it!");
			return;
		}
		ActionManager AM = a.GetComponent<ActionManager>() as ActionManager;
		if (a.putInInventory(gameObject)) {
			AM.removeAction(gameObject.GetInstanceID(), "PickUp");
			// if there are something in being in cooking, notify to stop cook,because it has been pick up
			FoodStuff[] foods = gameObject.GetComponentsInChildren<FoodStuff>();
			foreach (FoodStuff food in foods) 
			{
				if (food.isBeingCookedNow())
					food.StopToCook();
			}
        	// Report to callback 
			if (completionCallback != null) {
        		ActionResult ar = new ActionResult(pickupAction, ActionResult.Status.SUCCESS, a, null, "Picked up object");
				holder = a;
				completionCallback(ar);
			}
		} else {
			// we already are holding something, send failure!
			if (completionCallback != null) {
        		ActionResult ar = new ActionResult(pickupAction, ActionResult.Status.FAILURE, a, null,
				                                   "Already have " + a.inventory + " in inventory!");
				completionCallback(ar);
				return;
			}
		}
	}
	
	/**
     * Let the Avatar put down this GameObject.
	 */
	public void Drop(Avatar a, ActionCompleteHandler completionCallback=null) {
		ActionManager AM = a.GetComponent<ActionManager>() as ActionManager;
		// Whether we hold an object or not, we will be getting rid of the drop action...
		AM.removeAction(gameObject.GetInstanceID(), "Drop");
		if (a.removeFromInventory(gameObject)) {
			// Report to callback 
			holder = null;
			if (completionCallback != null) {
        		ActionResult ar = new ActionResult(dropAction, ActionResult.Status.SUCCESS, a, null, "Dropped object");
				completionCallback(ar);
			}
		} else { 
			// we are not holding anything, send failure!
			if (completionCallback != null) {
        		ActionResult ar = new ActionResult(dropAction, ActionResult.Status.FAILURE, a, null,
				                                   "Nothing in inventory!");
				completionCallback(ar);
				return;
			}
		}
	}	
	
	void Update () 
	{
		if (holder == null)
		{
			// if the avatar step on this object, it cannot pick up it, remove the pickup action
			foreach (Avatar a in interactors.Values)
	        {
				if (DoesAvatarStepOnObject(a, gameObject))
				{
					ActionManager AM = a.GetComponent<ActionManager>() as ActionManager;
	            	AM.removeAction(gameObject.GetInstanceID(), "PickUp");
				}
	        }
		}
	}
	
	public bool DoesAvatarStepOnObject(Avatar a, GameObject obj)
	{
		// only for player
		if (a.agentType != "player")
			return false;
		
		float dis_x = gameObject.transform.position.x - a.gameObject.transform.position.x;
		float dis_z = gameObject.transform.position.z - a.gameObject.transform.position.z;
		float dis = dis_x * dis_x + dis_z * dis_z;
		float r = (gameObject.collider.bounds.size.x/4.0f) * (gameObject.collider.bounds.size.x/4.0f);
		if (dis < r  )
			return true;
		else 
			return false;
	}
}
