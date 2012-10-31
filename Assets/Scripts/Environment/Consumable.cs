using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Consumable : OCBehaviour {
	
	private ActionSummary consumeAction;
	
	// Use this for initialization
	void Start () {
		// Right now we don't have consume animation!!
		AnimSummary animS = new AnimSummary("doSomething");
        PhysiologicalEffect effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
	
        Config config = Config.getInstance();
		
		// increase the energy
		effect.energyIncrease += config.getFloat("EAT_ENERGY_INCREASE");
		// decrease the hunger
		effect.changeFactors["hunger"] = -effect.energyIncrease;
		// increase the poo urgency
		effect.changeFactors["poo_urgency"] = config.getFloat("EAT_POO_INCREASE");
		
		consumeAction = new ActionSummary(this, "Consume", animS, effect, true);
		
		myActionList.Add("Consume");
	}
	
	public void AddAction(Avatar avatar)
    {
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		AM.addAction(consumeAction);

		AddInteractor(avatar);
	}
	
	public void RemoveAction(Avatar avatar){
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "Consume");
        if (interactors.ContainsKey(avatar.GetInstanceID()))
            interactors.Remove(avatar.GetInstanceID());

        RemoveInteractor(avatar);
	}
	
	public void Consume(Avatar a) 
	{
        List<int> interactorIds = new List<int>();
        interactorIds.AddRange(interactors.Keys);
        foreach (int id in interactorIds)
        {
		    gameObject.SendMessage("RemoveAction", interactors[id]);
        }
		if (a.inventory == gameObject) {
			a.setTractorBeamState(false);
			// if inventory then play animation and wait till complete
			animation.CrossFade("consume",0.1f);
			StartCoroutine(removeAfter(animation["consume"].length));
		} else {
			a.setTractorBeamState(false);
			// otherwise, just make the object disappear
			Destroy(gameObject);
		}
	}
	
	public IEnumerator removeAfter(float seconds) {
		yield return new WaitForSeconds(seconds);
		Destroy(gameObject);
	}
	
	
	
}
