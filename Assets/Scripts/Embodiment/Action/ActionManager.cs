using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public struct ActionKey {
    
    public readonly int objectID;
  	public readonly string actionName;

    public ActionKey (int id, string name) {
        objectID = id;
        actionName = name;
    }

}

public class ActionSummary : ICloneable {
    // All actions must expect a callback as their first argument
    // This callback should be MonoBehaviour that has a "actionComplete" method of the form:
    // void actionComplete(ActionResult result);

    public int objectID;
	public string actionName;
	public AnimSummary animSummary;
    public PhysiologicalEffect phyEffect;

    public ActionKey getKey()
    {
        return new ActionKey(objectID,actionName);
    }

    // parameter info
    public ParameterInfo[] pinfo;
    //--- This binding stuff isn't currently being used
    // these enforce certain parameters in pinfo to be bound
    public pbinding[] bindings;
    // the pbinding for:
    // -free is an accessable action parameter
    // -actor is the Avatar script calling the action
    // -direction is the direction between the object and the actor transform
	public enum pbinding { FREE = 0, ACTOR = 1, DIRECTION = 2 };

    // whether the player can see this action type (false for things like movement and looking at things)
    public bool playerVisible;
    public bool usesCallback;

    public GameObject actor;
	public GameObject actionObject;
    public Type componentType;
	
	public ActionSummary(OCBehaviour ocb, string name, AnimSummary animS, PhysiologicalEffect effect = null, bool visible = true)
	{
		MethodInfo methodInfo = ocb.GetType().GetMethod(name);
		pinfo = methodInfo.GetParameters();
        init(ocb, name, pinfo, animS, effect, visible);
	}
	
	public ActionSummary(Avatar ocb, string name, AnimSummary animS, PhysiologicalEffect effect = null, bool visible = true)
	{
		MethodInfo methodInfo = ocb.GetType().GetMethod(name);
		pinfo = methodInfo.GetParameters();
        init(ocb, name, pinfo, animS, effect, visible);
	}
	
	public void init(OCBehaviour ocb, string name, ParameterInfo[] parameters, AnimSummary animS, PhysiologicalEffect effect = null, bool visible = true)
    {
		actionObject = ocb.gameObject;
		objectID = ocb.gameObject.GetInstanceID();
		componentType = ocb.GetType();
		actionName = name;
		pinfo = parameters;
		animSummary = animS;
        phyEffect = effect;
		playerVisible = visible;
        this.usesCallback = false;
        validateAction();
        setupParameterBindings();
	}

	public void init(Avatar a, string name, ParameterInfo[] parameters, AnimSummary animS, PhysiologicalEffect effect = null, bool visible = true)
    {
		actionObject = a.gameObject;
		objectID = a.gameObject.GetInstanceID();
		componentType = typeof(Avatar);
		actionName = name;
		pinfo = parameters;
		animSummary = animS;
        phyEffect = effect;
		playerVisible = visible;
        this.usesCallback = false;
        validateAction();
        setupParameterBindings();
	}

    public void setupParameterBindings()
    {
        bindings = new pbinding[pinfo.Length];
        for (int i = 0; i < bindings.Length; i++) {
            bindings[i] = pbinding.FREE;
        }
    }
	
    /* Do we need a way to bind parameters that shouldn't be freely accessable
     * to callers? E.g. the Avatar doing the action is often passed, but the
     * OpenCog shouldn't be able to fill this in with an Avatar it doesn't
     * control
     * public void bindActor(GameObject _actor)
    {
        actor = _actor;
    }

    public object[] getBoundArray()
    {
        System.Diagnostics.Debug.Assert(actor != null);
        object[] boundP = new object[pinfo.Length];
        for (int i=0; i < pinfo.Length; i++) {
            if (bindings[i] == pbinding.FREE) boundP[i] = null;
            else if (bindings[i] == pbinding.ACTOR) boundP[i] = actor.GetComponent("Avatar");
            else if (bindings[i] == pbinding.DIRECTION) boundP[i] = calculateAngle();
            boundP = pinfo[i];
        }
    }*/

    public bool validateAction()
    {
        // check whether the parameter info has a callback as last parameter
        int numParams = pinfo.Length;
        // ensure that the first parameter is the avatar performing the action
        if (componentType != typeof(Avatar)) {
            //Debug.Log("pinfo[0].ParameterType: " + pinfo[0].ParameterType);
            if (pinfo[0].ParameterType != typeof(Avatar)) {
                throw new ArgumentException();
            }
        }
        if (numParams > 1) {
            // check that it accepts a MonoBehaviour callback as first slot
            if (pinfo[numParams-1].ParameterType.IsSubclassOf(typeof(MonoBehaviour))
                || pinfo[numParams-1].ParameterType == typeof(MonoBehaviour))
                this.usesCallback = true;
        }

        return true;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
    
}

public class ActionResult {
	public static int actionCount = 0;
	public string actionInstanceName;
	public enum Status { NONE = 0, SUCCESS = 1, FAILURE = 2, NOT_AVAILABLE = 3, EXCEPTION = 255 };
    public ActionSummary action; // details of the Action
    public Avatar avatar; // the avatar that did the action
    public Status status = Status.NONE;
    public string description;
	public ArrayList parameters;
	
	public ActionResult(ActionSummary As,Status St, Avatar Av, ArrayList _parameters = null, string des = "")
    {
		actionInstanceName  = As.actionName + (++ actionCount);
		action = As;
		status = St;
		avatar = Av;
		description = des;
		parameters = _parameters;
	}
}

/**
 * To register action event listeners in your own class, make sure to two steps have been done:
 *   1. Your callback function should have the same format as the delegate defined below.
 *     e.g. void onActionComplete (ActionResult ar);
 *   2. If you want the callback to be invoked for all events of a specific ActionManager instance(say, "AM").
 *      You can register the callback by doing:
 *         AM.actionCompleteEvent += myActionCompleteHandler;
 *      Otherwise, if you want to invoke it globally, then do:
 *         ActionManager.globalActionCompleteEvent += myActionCompleteHandler;
 *      (myActionCompleteHandler has to match the signature of the ActionCompleteHandler delegate)
 */
public delegate void ActionCompleteHandler(ActionResult ar);

public enum ActionSource { BUILTIN, EXTERNAL }

//[RequireComponent (typeof (Avatar))]
public class ActionManager : MonoBehaviour {
	
	private Avatar myAvatar;
	private Animator animator;
	private GameObject OCObjects;
    private OCPhysiologicalModel phyModel;
	
    public Hashtable currentActions;
    public Hashtable rootActionTable = new Hashtable();
    
    
	// The reason to maintain following two maps:
	// OpenCog sends an action named "walk", but in unity end,
    // the "walk" action might be performed by invoking "MoveToCoordinate" 
	// or "MoveToObject" methods.
    // Therefore, we need to map from action name to method name.
    
    // Avatar's built-in actions.
	public static Dictionary<string, string> builtinActionMap = new Dictionary<string, string>();
    // The actions that avatar learns from external environment.
	public static Dictionary<string, string> externalActionMap = new Dictionary<string, string>();
    
    ArrayList listeners  = new ArrayList();
	
	// Whenever an action completes, we call this event
	public event ActionCompleteHandler actionCompleteEvent;
	
	// A global event which fires when ANY event is completed.
	// For example, the Console may register a callback to this event. 
	public static event ActionCompleteHandler globalActionCompleteEvent;
	
	bool isPlayer = false;
	
	void Start()
    {
		myAvatar = gameObject.GetComponent<Avatar>() as Avatar;
		isPlayer = GetComponent<Player>() != null;
        phyModel = GetComponent<OCPhysiologicalModel>() as OCPhysiologicalModel;
		
		foreach(Transform child in transform)
		{
			animator = child.GetComponent<Animator>() as Animator;
			if(animator != null)
				break;
		}
		if(animator == null)
		{
			//@TODO: Decide if refactoring the code to use an Animator class is a good idea...
			//Debug.LogError("Error : \"Animator\" is not attached to the 3D model");
		}
		currentActions = rootActionTable;
		
		// the defaultActionCompleteHandler is always fired, and invokes the global event for completed actions.
		actionCompleteEvent += defaultActionCompleteHandler;
	}

	/// <summary>
	/// Register an mapping from the name of action to the name of a method that implements the action. An action
	/// source should be given to classify the action.
	/// e.g. "walk" is a built-in action that avatar naturally has, and the method in Unity3D to implement walking
	///      is "MoveToCoordinate", then it should be registered by invoking:
	/// 		registerActionMap("walk", "MoveToCoordinate", ActionSource.BUILTIN);
	/// 	 "pick_up" is an action that avatar has only when it is close enough to an object that is pickable.
	/// 	 Then it should be registered as:
	///			registerActionMap("pick_up", "PickUp", ActionSource.EXTERNAL);
	/// </summary>
	public static void registerActionMap(string actionName, string methodName, ActionSource src = ActionSource.BUILTIN)
	{
	    if (src == ActionSource.BUILTIN)
        {
        	if (!builtinActionMap.ContainsKey(actionName))
	            builtinActionMap.Add(actionName, methodName);
        }
        else
        {
        	if (!externalActionMap.ContainsKey(actionName))
	            externalActionMap.Add(actionName, methodName);
        }
	}
	
	/// <summary>
	/// Obtains an action name from the method name. Say, the action that is associated to 
	/// method "MoveToCoordinate" is "walk".
	/// </summary>
	public static string getOCActionNameFromMap(string methodName)
	{
		if (builtinActionMap.ContainsValue(methodName))
		{
			foreach (KeyValuePair<string, string> pair in builtinActionMap)
			{
				if (pair.Value == methodName) return pair.Key;
			}
		}
		else if (externalActionMap.ContainsValue(methodName))
		{
			foreach (KeyValuePair<string, string> pair in externalActionMap)
			{
				if (pair.Value == methodName) return pair.Key;
			}
		}
		
		return null;
	}
	
    public void addAction(ActionSummary action)
    {
        ActionKey al = action.getKey();
		if(!rootActionTable.Contains(al)) {
			// Each time an action is added, notify the avatar.
			if (myAvatar != null) {
				myAvatar.SendMessage("notifyActionAdded", action.Clone());
			}
			rootActionTable.Add(al,action);
		}
		//Debug.Log("Add action " + action.actionName + " (actions size is " + rootActionTable.Count + ")");
	}

    public void removeAction(int objectID, string methodName)
    {
		//GameObject OCObject = OCOR.GetOCObject(objectID);
        ActionKey al = new ActionKey(objectID,methodName);
		if (rootActionTable.Contains(al)) {
			// Each time an action is removed, notify the avatar.
			ActionSummary action = getActionSummary(objectID,methodName);
            myAvatar.SendMessage("notifyActionRemoved", action);
            rootActionTable.Remove(al);
        }
        //Debug.Log("Remove action " + al.actionName + " (actions size is " + rootActionTable.Count + ")");
	}
	
	// push a list as the current set of actions that can be done.
    // give error if there is already a list pushed on top of the rootList
    // notify any action list listeners
    void pushActionTable(Hashtable actions)
    {
		if(currentActions == rootActionTable) {
			currentActions = new Hashtable();
		} else {
			Debug.LogError("Temporary action list already exists in ActionManager");
		}
		foreach(ArrayList key in actions) {
			currentActions.Add(key,actions[key]);
		}
	}

    // revert back to the root list
    // fail silently if the root list is already active
    // notify any action list listeners
    void revertActionList()
    {
		currentActions = rootActionTable;
	}

    // Just summarise whether the root list == currentActions
    public bool isRootActionListActive()
    {
		if(currentActions == rootActionTable) return true;
		else return false;
	}
	
	public ActionSummary getActionSummary(int objectID, string methodName)
    {
        ActionSummary a = currentActions[new ActionKey(objectID,methodName)] as ActionSummary;
        if (a == null) {
            Debug.LogWarning("No ActionSummary for objid: " + objectID + " method: " + methodName);
			//OCObjectRepository.get().DumpToLog();
            return null;
        }
        return a.Clone() as ActionSummary;
	}
	
	/// <summary>
	/// The callback system for completing events is somewhat complex, but flexible enough to account
	/// for a wide range of behaviours.
	/// Some of the complexity comes from the ActionManager not forcing actions to support completion
	/// callbacks (for actions that complete straight away) instead the ActionManager takes care of
	/// notifying listeners for completion events.
	/// The other optional part is that the caller to doAction may or may not provide their own callback
	/// which also needs to be notified on completion.
	/// 
	/// </summary>
	public IEnumerator doAction(ActionSummary action, ArrayList p, ActionCompleteHandler completionCallback) 
    {
		if (action == null) {
			// If we are trying to do a null action (this happens when trying to use the
			// other variants of doAction), then we just want to respond with failure.
			if (completionCallback == null)
				completionCallback = new ActionCompleteHandler(defaultLocalActionCompleteHandler);
			else 
				completionCallback += defaultLocalActionCompleteHandler;
			ActionResult ar = new ActionResult(null, ActionResult.Status.NOT_AVAILABLE, myAvatar, p,
			                                  "Tried to perform a non-existent action");
			completionCallback(ar);
			yield break;
		}
        string methodName = action.actionName;
		GameObject actionObject = action.actionObject;
        MonoBehaviour behaviour;
        
        behaviour = actionObject.GetComponent(action.componentType) as MonoBehaviour;
        if (behaviour == null) {
            Debug.LogError("Couldn't get monobehaviour for action "
                    + methodName + " of componentType "
                    + action.componentType);
            yield break;
        }

        ParameterInfo[] pinfo = action.pinfo;
		object[] args = new object[pinfo.Length];
        int i=0;
        int pmod=0;
        // If the action is not a direct avatar action, then it will receive
        // the avatar doing the action as the first argument
		if (action.componentType != typeof(Avatar)) {
            args[0] = myAvatar;
            i=1;
            pmod=-1; // skip back one because p won't have avatar
        }
        for(; i < pinfo.Length; i++) {
            // last element of the parameters[] p array is reserved for callback
            if (action.usesCallback && (i >= (pinfo.Length - 1)) ) continue;
            
            // ensure that any exceptions due to bad parameter types are caught
            if(i < p.Count && p[i+pmod] != null) {
                if(pinfo[i].ParameterType == p[i+pmod].GetType() || p[i+pmod].GetType().IsSubclassOf(pinfo[i].ParameterType) ) {
                    args[i] = p[i+pmod];
                } else {
                    args[i] = null;
                    Debug.LogError("Error : Type mismatch!" +
                           " expected: " + pinfo[i].ParameterType.ToString() + 
                           " got: " + p[i+pmod].GetType().ToString());
					yield break;
                }
            }
            if (args[i] == null) {
                if(pinfo[i].IsOptional) {
                    args[i] = pinfo[i].DefaultValue;
                } else {
                    Debug.LogError("Error : Missing required parameter" +
                           " i of type " + pinfo[i].ParameterType.ToString());
					yield break;
                }
            }
            //Debug.Log(args[i] + " got: " + args[i].GetType().ToString() + " expected: " + pinfo[i].ParameterType.ToString());
		}
		// Set up the callback delegate to 
		if (completionCallback == null)
			completionCallback = new ActionCompleteHandler(defaultLocalActionCompleteHandler);
		else 
			completionCallback += defaultLocalActionCompleteHandler;
		
		// Add callback to end of parameter array
		if (action.usesCallback)
		{
            //Debug.Log("Set completion callback for " + action.actionName);
            args[i-1] = completionCallback;
		}
		
		// look up action
		MethodInfo methodInfo = behaviour.GetType().GetMethod(methodName);
        if (methodInfo == null) {
            Debug.LogError("Couldn't get methodInfo for methodname " + methodName);
			yield break;
        }
		
		// Play the animation, but not if a human player
		if (!isPlayer) {
			AnimSummary animS = action.animSummary;
			if(animS.FirstAnim != null)
			{
				animator.StopNormal();
				animator.animation.Play(animS.FirstAnim);
				//yield return animator.animation.WaitForAnim(action.animSummary.FirstAnim);
				float waitTime = animator.animation[animS.FirstAnim].length;
				yield return new WaitForSeconds(waitTime);
			}
			if(animS.NewIdleAnim != null)
			{
				animator.SetIdleAnim(animS.NewIdleAnim);
			}
			if(animS.NewWalkAnim != null)
			{
				animator.SetWalkAnim(animS.NewWalkAnim);
			}
			animator.PlayNormal(); //Restore to idle state
		}
		
		//Call the action
		try
		{
			methodInfo.Invoke(behaviour, args);
    	}
		catch (TargetInvocationException e)
		{
		    Exception ex = e.InnerException; // ex now stores the original exception
			Debug.LogError(ex);
			yield break;
		}
		if (!action.usesCallback) {
			//Debug.Log("Action doesn't use callback, so starting action completion pipeline manually");
			
			// If the action doesn't do a callback to say it's complete,
			// then we manually have to start the completion event pipeline
			ActionResult ar = new ActionResult(action, ActionResult.Status.SUCCESS, myAvatar, p);
			completionCallback(ar);
		}

        // Update the physiological model after we perform the action.
        if (phyModel != null)
            phyModel.SendMessage("processPhysiologicalEffect", action.phyEffect);
		
		// initiate and wait for the second part of the animation if the action
		// has one
		if (!isPlayer) {
			AnimSummary animS = action.animSummary;
			if(animS.SecondAnim != null)
			{
				animator.StopNormal();
				animator.animation.Play(animS.SecondAnim);
				// We don't wait for it to complete, it'll complete on it's own.
				//float waitTime = animator.animation[animS.SecondAnim].length;
				//yield return new WaitForSeconds(waitTime);
			}
		}
		
	}
	
    public void doAction(string objectName, string methodName, ArrayList p, ActionCompleteHandler callback = null) 
    {
        // Only gets the first object found by name...
		GameObject OCObject = OCObjectRepository.get().GetObjectByName(objectName);
		doAction(OCObject.GetInstanceID(), methodName, p, callback);
    }
	
	public void doAction(ActionKey ak, ArrayList p, ActionCompleteHandler callback = null) 
    {
		doAction(ak.objectID, ak.actionName, p, callback);
	}

    public void doAction(int objectID, string methodName, ArrayList p, ActionCompleteHandler callback = null) 
    {
		ActionSummary asummary = getActionSummary(objectID, methodName);
        StartCoroutine(doAction(asummary, p, callback));
    }
	
	public void defaultLocalActionCompleteHandler(ActionResult ar) 
	{
		// This method passes any single action completion events to the ActionManager-wide event listeners.
		
		//Debug.Log("calling default local action complete handler");
		if (actionCompleteEvent != null) {
			actionCompleteEvent(ar);
		}
	}
	public void defaultActionCompleteHandler(ActionResult ar) 
	{
		// This method passes any ActionManager completion events to the global event handler.
		
		//Debug.Log("calling default action complete handler");
		if (globalActionCompleteEvent != null) {
			globalActionCompleteEvent(ar);
		}
	}
	
    public ParameterInfo[] getActionSignature(int objectID, string methodName) 
    {
        // Look up action, and if it exists return the parameter info. Their types and whether they are optional.
        // C# System.Reflection already has object types to do this.
        return (currentActions[new ActionKey(objectID,methodName)] as ActionSummary).pinfo;
    }

    public void registerActionListListener(MonoBehaviour callback) 
    {
        // check that the callback object has actionAdded and actionRemoved method using System.Reflection
        // actionAdded expects a ActionSummary
        // actionRemoved expects an objectID and methodName
        // add the callback to the listeners list;
    }

    public void unregisterActionListListener(MonoBehaviour callback) 
    {
        // remove the callback to the listeners list;
    }
}
