using UnityEngine;
using System.Collections;

public class ClawController : OCBehaviour {
	
	// By default we search a gameobject called "TheClaw"
	public Claw theClaw;
	// By default the panel is a child transform with name "Panel"
	public GameObject panel = null;
	
	private ActionSummary stopClawAction;
	private ActionSummary startClawAction;
	
	public Material stopMaterial;
	public Material startMaterial;
	
	// Use this for initialization
	void Start () {
		if (theClaw == null)
			theClaw = GameObject.Find("TheClaw").GetComponent<Claw>();
		if (panel == null)
			panel = transform.FindChild("Panel").gameObject;
	
		AnimSummary animS = new AnimSummary("pickup");
        PhysiologicalEffect effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		stopClawAction = new ActionSummary(this,"StopClaw", animS, effect, true);
		
		animS = new AnimSummary("pickup");
        effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		startClawAction = new ActionSummary(this, "StartClaw", animS, effect, true);
		
		panel.renderer.material = startMaterial;
		
		myActionList.Add("StartClaw");
		myActionList.Add("StopClaw");
		
	}

	
	/**
     * This is called when the Avatar approaches the object
	 */
	public void AddAction(Avatar avatar)
    {
		
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		if (!theClaw.isClawActive) {
			AM.addAction(startClawAction);
		} else {
			AM.addAction(stopClawAction);
		}
	}
	
	/**
     * This is called when the Avatar moves away from the object
	 */
	public void RemoveAction(Avatar avatar)
    {
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "StopClaw");
		AM.removeAction(gameObject.GetInstanceID(), "StartClaw");
	}
	
	public void StopClaw(Avatar a) {
		theClaw.DisableTheClaw();
		panel.renderer.material = startMaterial;
		
		ActionManager AM = a.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "StopClaw");
		AM.addAction(startClawAction);
		
	}
	
	public void StartClaw(Avatar a) {
		theClaw.RestartTheClaw();
		panel.renderer.material = stopMaterial;
		
		ActionManager AM = a.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "StartClaw");
		AM.addAction(stopClawAction);
	}
}
