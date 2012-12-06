using UnityEngine;
using System.Collections;
using Embodiment;

public class StoveButton : OCBehaviour {
	
	private GameObject theCookFire = null; 
	private GameObject theStove = null;
	public GameObject prefabCookFire = null; // should be assigned in Unity editor
	
	private ActionSummary turnOnAction;
    private ActionSummary turnOffAction;
	
	public bool isTurnOn = false;
	
	// Use this for initialization
	void Start () {
		
		theStove = GameObject.Find("cookTop");
		
		AnimSummary animS = new AnimSummary("destroyBlockM");
        PhysiologicalEffect effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		turnOnAction = new ActionSummary(this,"TurnOnStove", animS, effect, true);
		
		animS = new AnimSummary("destroyBlockM");
        effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
		turnOffAction = new ActionSummary(this, "TurnOffStove", animS, effect, true);
		
		myActionList.Add("TurnOnStove");
		myActionList.Add("TurnOffStove");
		
		StateChangesRegister.RegisterState(gameObject, this,"isTurnOn" );
	
	}
	
	/**
     * This is called when the Avatar approaches the object
	 */
	public void AddAction(Avatar avatar)
    {
		
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		if (!isTurnOn) {
			AM.addAction(turnOnAction);
		} else {
			AM.addAction(turnOffAction);
		}
	}
	
	/**
     * This is called when the Avatar moves away from the object
	 */
	public void RemoveAction(Avatar avatar)
    {
		ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "TurnOnStove");
		AM.removeAction(gameObject.GetInstanceID(), "TurnOffStove");
	}
	
	public void TurnOnStove(Avatar a) {
		
		isTurnOn = true;
		if (theCookFire == null)
		{
			theCookFire = Instantiate(prefabCookFire) as GameObject;
			theCookFire.transform.parent = theStove.transform;
			theCookFire.transform.localPosition = new Vector3(0.0f, 0.4f, 0.0f);
		}
		
		ActionManager AM = a.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "TurnOnStove");
		AM.addAction(turnOffAction);
		
	    Debug.Log("Turn on the stove fire! ");
		
	}
	
	public void TurnOffStove(Avatar a) {
		
		isTurnOn = false;
		if (theCookFire != null)
		{
			DestroyObject(theCookFire);
		}
		
		ActionManager AM = a.GetComponent<ActionManager>() as ActionManager;
		AM.removeAction(gameObject.GetInstanceID(), "TurnOffStove");
		AM.addAction(turnOnAction);
		
		// Notify all the foodstuff in pan on the stove if there is
		FoodStuff[] foods = theStove.GetComponentsInChildren<FoodStuff>();
		foreach (FoodStuff food in foods) 
		{
			food.StopToCook();
		}
		
		Debug.Log("Turn off the stove fire! ");
	}
	
	public bool getIsTurnOn()
	{
		return isTurnOn;
	}
		
	// Update is called once per frame
	void Update () 
	{
		if (isTurnOn)
		{
			// Notify all the foodstuff in pan on the stove if there is
			// must check every frame because food can be put in the pan after the stove fire turns on
			FoodStuff[] foods = theStove.GetComponentsInChildren<FoodStuff>();
			foreach (FoodStuff food in foods) 
			{
				food.BeginToCook();
			}
		}
	}
	
}
