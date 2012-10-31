using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

/**
 *  Class: OCBehaviour
 *  A class that represents actions that OpenCog Avatars can do
 */
public class OCBehaviour : MonoBehaviour {

    protected Dictionary<int, Avatar> interactors = new Dictionary<int, Avatar>();

	Ray ray;
	
	// to store all the names of actions.
	// Remember to add all the action to this list when write a new kind of OCBehaviour
	public List<string> myActionList = new List<string>();
	
	/** GetInteractPoint
     *
     * Returns the Interact Point of the GameObject.
	 * @return Vector3
	 */
	public Vector3 GetInteractPoint(Avatar a) {
		// WARNING : if you are having trouble with weird interacting points,
		// ensure that the detector collider is larger than the physics collider and
		// make sure the physics collider is covering the transform position.
		
		// If there is a collider, we use a ray cast to
		// ensure the default interact point is outside of the actual object
		// NOTE: using Raycast to colliders seemed initially like a good way to do it,
		// but we should probably use the collider's bounds bounding box.
		if (gameObject.collider != null) 
		{
			
			Vector3 src = a.gameObject.transform.position;
			Vector3 dest = gameObject.collider.bounds.center;

			if(a.agentType == "player")
			{
				Vector3 direction = Vector3.Normalize(dest - src);
				return (dest - direction * (gameObject.collider.bounds.extents.x + 	gameObject.collider.bounds.extents.z)/1.4f);
			}
			else
			{

				Ray towardsObject = new Ray(src, dest-src);
				Vector3 towardsObjectVector = (dest-src);
				
				ray = new Ray(src,dest-src);
				RaycastHit hit2 = new RaycastHit();
				
				gameObject.collider.Raycast(ray, out hit2, 10000);
				//Debug.Log( "hit on object " + hit2.transform.gameObject + " was this far:" + hit2.distance);
							
				//Debug.DrawRay (ray.origin, ray.direction * 10, Color.yellow);
				//Debug.Log( "ray is " + ray);
				
				// also take into account the size of player collider.
				Ray rayToAvatar = new Ray(dest,src-dest);
				RaycastHit hit1 = new RaycastHit();
				
				a.gameObject.collider.Raycast(rayToAvatar, out hit1, 10000);
				//Debug.Log( "hit on avatar " + hit1.transform.gameObject + " was this far: " + hit1.distance);
				
				//Debug.Log( "total distance is " + towardsObjectVector.magnitude);
				
				float extraError = 0.4f;
				float distanceToGo = towardsObjectVector.magnitude -
					(2.0f*towardsObjectVector.magnitude - (hit1.distance + hit2.distance))
						- extraError;
				//Debug.Log( "distance to go is " + distanceToGo);
				
				Vector3 vvv = towardsObject.GetPoint(distanceToGo);
				//Debug.Log( "final destination is " + vvv);
				                                                                      
				if (vvv.sqrMagnitude != 0) return vvv;
			}
		}
		return transform.position;

	}
	
	/** GetAllOCB
     * OCB = OCBehaviour
     * @return all the OCBehaviours of the GameObject.
	 */
	public OCBehaviour[] GetAllOCB() {
		return gameObject.GetComponents<OCBehaviour>() as OCBehaviour[];
	}

    /** AddInteractor
     * 
     * Record an interator who is able to interact with this GameObject.
     */
    protected void AddInteractor(Avatar avatar)
    {
        int avatarId = avatar.GetInstanceID();
        if (!interactors.ContainsKey(avatarId))
        {
            interactors.Add(avatarId, avatar);
        }
    }

    /** RemoveInteractor
     * 
     * Remove an interator who is no longer able to interact with this GameObject.
     */
    protected void RemoveInteractor(Avatar avatar)
    {
        int avatarId = avatar.GetInstanceID();
        if (interactors.ContainsKey(avatarId))
        {
            interactors.Remove(avatarId);
        }
    }

    /** RemoveActionFromOtherInteractors
     * 
     * @param thisAvatar avatar that would not be involved in this method, if it is null, then
     * all avatars in the interactor list will be involved.
     * @param actionName the action name to be removed from action manager.
     */
    protected void RemoveActionFromOtherInteractors(Avatar thisAvatar, string actionName)
    {
        foreach (Avatar otherAvatar in interactors.Values)
        {
            // thisAvatar is null meaning that this method will be applied to all avatars.
            if (thisAvatar == null || otherAvatar.GetInstanceID() != thisAvatar.GetInstanceID())
            {
                ActionManager AM = otherAvatar.GetComponent<ActionManager>() as ActionManager;
                AM.removeAction(gameObject.GetInstanceID(), actionName);
            }
        }
    }

    /** AddActionToOtherInteractors
     * 
     * @param thisAvatar avatar that would not be involved in this method, if it is null, then
     * all avatars in the interactor list will be involved.
     * @param action the action summary to be appended to action manager.
     */
    protected void AddActionToOtherInteractors(Avatar thisAvatar, ActionSummary action)
    {		
        foreach (Avatar otherAvatar in interactors.Values)
        {
            // thisAvatar is null meaning that this method will be applied to all avatars.
            if (thisAvatar == null || otherAvatar.GetInstanceID() != thisAvatar.GetInstanceID())
            {
                ActionManager AM = otherAvatar.GetComponent<ActionManager>() as ActionManager;
                AM.addAction(action);
            }
        }
    }
	
	public static GameObject findObjectByInstanceId(int instanceID)
	{
		GameObject objects = GameObject.Find("Objects");
		foreach (Transform child in objects.transform) 
		{
			if (child.gameObject.GetInstanceID() == instanceID)
				return child.gameObject;
		}
		
		return null;
	}

}
