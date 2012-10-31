using UnityEngine;
using System.Collections;

public class PushableObject  : OCBehaviour  {
	

	public float weight = 100.0f;
	private ActionSummary pushForwardAction;
	
	// Use this for initialization
	void Start () {

		AnimSummary animS = new AnimSummary();
        PhysiologicalEffect effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		pushForwardAction = new ActionSummary(this,"PushForward", animS, effect, true);		
		myActionList.Add("PushForward");
	
	}
	
	
	/**
     * This is called when the Avatar approaches the object
	 */
	public void AddAction(Avatar avatar)
    {
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		AM.addAction(pushForwardAction);
	}
	
	/**
     * This is called when the Avatar moves away from the object
	 */
	public void RemoveAction(Avatar avatar)
    {
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "PushForward");
	}
	
	public void PushForward(Avatar a, float force = 100.0f) {
	   
		Vector3 dest = gameObject.transform.position;
   	    Vector3 direction = a.transform.forward;

		dest.x += direction.x * force / weight;
		dest.y += 0.0f;
		dest.z += direction.z * force / weight;	
		
		iTween.MoveTo(gameObject , iTween.Hash("position", dest,
		                                  "speed",1,
		                                  "easetype","linear"));
		
	}

}
