using UnityEngine;
using System.Collections;
using System.Reflection;

public class Ball : OCBehaviour {
    //A class that will allow an avatar to kick an object that has this behaviour.
    //
    private ActionSummary kickAction; 
    
    private ActionSummary throwAction; 

    void  Start (){
        // Right now we don't have kick animation!!
        AnimSummary animS = new AnimSummary("throw");
        PhysiologicalEffect effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
        kickAction = new ActionSummary(this, "Kick", animS, effect, true);
        kickAction.usesCallback = true;
        
        animS = new AnimSummary("throw");
        effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
        throwAction = new ActionSummary(this, "Throw", animS, effect, true);
        throwAction.usesCallback = true;
		
		myActionList.Add("throw");
		myActionList.Add("Kick");
    
        if(rigidbody == null) {
            Debug.Log("Ball action requires a rigidbody!");
        }
    }

    public void AddAction(Avatar avatar)
    {
        ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
        AM.addAction(kickAction);
        if (avatar.inventory == gameObject) {
            AM.addAction(throwAction);
        }
    }
    
    public void RemoveAction(Avatar avatar){
        ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
        AM.removeAction(gameObject.GetInstanceID(), "Kick");
        AM.removeAction(gameObject.GetInstanceID(), "Throw");
    }

    public void Kick (Avatar a, float force = 600.0f, Vector3? torque = null, ActionCompleteHandler completionCallback=null)
    {
        a.removeFromInventory(gameObject);
        Vector3 addPoint;
        Vector3 direction = a.transform.forward;

        // slight incline
        direction.y = 1.5f;
        
        addPoint.x = direction.x * force;
        addPoint.y = direction.y * force;
        addPoint.z = direction.z * force;
        
        rigidbody.AddForce(addPoint);

        Vector3 t = torque ?? Vector3.zero;
        rigidbody.AddTorque(t);
        
        Debug.Log("Kick with force = " + force + " direction=" + direction + " torque=" + t);

        // Report to callback 
        if (completionCallback != null) {
            ArrayList pp = new ArrayList();
            pp.Add(force);
            pp.Add(t);
            ActionResult ar = new ActionResult(kickAction, ActionResult.Status.SUCCESS, a, pp, "I did a kick!");
            completionCallback(ar);
        }
    }
    
    public void Throw (Avatar a, float force = 500.0f, ActionCompleteHandler completionCallback=null)
    {
        a.removeFromInventory(gameObject);
        
        Vector3 addPoint;
        // If we need the Body transform it should be retrieved from the avatar
        Vector3 direction = a.transform.forward;

		if (a.GetType() == typeof(Player) ) {
			if (((Player)a).isNowFPview) 
			{	// if it's in the First person view, the ball is to throw from the camera
				Vector3 thePoint = new Vector3(Camera.main.pixelWidth/2,Camera.main.pixelHeight/2,0);
				Ray ray = Camera.main.ScreenPointToRay (thePoint);
				direction = ray.direction;
				gameObject.transform.position = ray.origin;
			}	
			else if (a.RightHand != null)		
			{	// when in a third person view
				// if this object is held on the righthand of avatar, then the throwing direction will be wrong,
				// so we have to move it to the center point of avatar before throw it
				float height = VerticalSizeCalculator.getHeight(a.transform, null) / 2.0f;
				Vector3 goSize = Vector3.up * gameObject.collider.bounds.size.y;
				gameObject.transform.position = a.transform.position + goSize + Vector3.up * 1.01f * height;
			}
		}
			
		addPoint.x = direction.x * force;
		addPoint.y = direction.y * force;
		addPoint.z = direction.z * force;
		rigidbody.AddForce(addPoint);

        Debug.Log("Throw with force = " + force + " direction=" + direction);

        // Report to callback 
        if (completionCallback != null) {
            ArrayList pp = new ArrayList();
            pp.Add(force);
            ActionResult ar = new ActionResult(kickAction, ActionResult.Status.SUCCESS, a, pp, "I did a throw!");
            completionCallback(ar);
        }
    }
    
}
