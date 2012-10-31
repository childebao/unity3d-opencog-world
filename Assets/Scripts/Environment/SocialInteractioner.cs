using UnityEngine;
using System.Collections;

public class SocialInteractioner : OCBehaviour {
		
    private ActionSummary touchAction;  // touch me, depend on the force you use, the npc would consider as pat, push or hit. 
	private ActionSummary kissAction; // kiss me 
	private ActionSummary hugAction;  // hug me
	
	
	// Use this for initialization
	void Start () {
	        
       	AnimSummary animS = new AnimSummary();
        PhysiologicalEffect effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		touchAction = new ActionSummary(this,"Touch", animS, effect, true);		
		touchAction.usesCallback = true;
		myActionList.Add("Touch");
		
		kissAction = new ActionSummary(this,"Kiss", animS, effect, true);		
		kissAction.usesCallback = true;
		myActionList.Add("Kiss");
		
		hugAction = new ActionSummary(this,"Hug", animS, effect, true);		
		hugAction.usesCallback = true;
		myActionList.Add("Hug");		
		
	}
	
	
    public void AddAction(Avatar avatar)
    {
        ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
        AM.addAction(touchAction);
        AM.addAction(kissAction);
        AM.addAction(hugAction);
		

    }
    
    public void RemoveAction(Avatar avatar){
        ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
        AM.removeAction(gameObject.GetInstanceID(), "Touch");
        AM.removeAction(gameObject.GetInstanceID(), "Kiss");
        AM.removeAction(gameObject.GetInstanceID(), "Hug");		
    }

	private IEnumerator applyTouch(Avatar a, float force , ActionCompleteHandler completionCallback=null)
	{
		// if the avatar is a player, we get the force from the HUG force Panel
		if (a.gameObject.tag == "Player")
		{
			Player player = a as Player;
			HUD theHud = player.getTheHUD();
			if (theHud == null )
			{
				Debug.LogError("The player's HUD is null! --in touch action");
				yield break;
			}
			yield return StartCoroutine(theHud.gettingForceFromHud(this));
			
			force = theHud.getCurrentForceVal();
		}
			
		// Touch an avatar will add a force to it, and it maybe push on the forward direction of the toucher
		// if the avatar will be pushed forward , depends on the force and the mass of avatar
		
		Rigidbody avatarRigid = gameObject.GetComponent<Rigidbody>() as Rigidbody;
		float mass = avatarRigid.mass;
		
		// caculate the relative force
		float relforce = force / 20.0f / mass;
		Vector3 direction = a.transform.forward;
		
		if (relforce > 1.0f)
		{
			Vector3 dest = gameObject.transform.position;
	   	    
			dest.x += direction.x * relforce ;
			dest.y += 0.0f;
			dest.z += direction.z * relforce ;	
        	iTween.MoveTo(gameObject , iTween.Hash("position", dest,
		                                  "speed",relforce,
		                                  "easetype",iTween.EaseType.easeInOutCubic));
			
		}
		
        Debug.Log( a.gameObject.name + "touch " + gameObject.name + " with force = " + force + " direction=" + direction);
		
		if (completionCallback != null) {
            ArrayList pp = new ArrayList();
            pp.Add(force);

            ActionResult ar = new ActionResult(touchAction, ActionResult.Status.SUCCESS, a, pp, a.gameObject.name + " touched " +  gameObject.name);
            completionCallback(ar);
        }
	}
	
	public void Touch (Avatar a, float force =300.0f , ActionCompleteHandler completionCallback=null) {
	   
		StartCoroutine(applyTouch(a,force,completionCallback));
        
	}
	
	public void Kiss (Avatar a, ActionCompleteHandler completionCallback=null) {
		
		// todo: we need a kiss animation
		Debug.Log( a.gameObject.name + "kiss " + gameObject.name );
		
	}
	
	public void Hug (Avatar a, ActionCompleteHandler completionCallback=null) {
		
		// todo: we need a hug animation
		Debug.Log( a.gameObject.name + "hug " + gameObject.name);
		
	}
}
