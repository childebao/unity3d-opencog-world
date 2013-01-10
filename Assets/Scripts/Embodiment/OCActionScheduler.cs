using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Embodiment;

/**
 * There are some actions that avatar obtains from certain objects, e.g. 
 * "Consume" is an action that avatar learns from food or water. Then such
 * actions should get a target as its parameter when they are required by OpenCog.
 * ActionTarget is such a class to record the target object information.
 */
public class ActionTarget
{
    private int targetId;
    private string targetType;

    public ActionTarget(int id, string type)
    {
        targetId = id;
        targetType = type;
    }

    public int id
    {
        get { return this.targetId; }
    }

    public string type
    {
        get { return this.targetType; }
    }
}

public class OCActionScheduler : MonoBehaviour
{
	// Avatar component of this OCAvatar.
	private ActionManager AM;
    private OCConnector connector;
	private Avatar AV;

    public LinkedList<MetaAction> actionList = new LinkedList<MetaAction>();
    public MetaAction currentAction = null;
	private static float TimeOutSeconds = 15.0f;
	private static float ActionIntervalSeconds = 2.0f;
	private float currentActionBeginTime = 0.0f;
	
	public void executeAction(MetaAction action)
	{
        string actionName = action.Name;
        
		// Check if action name is registered as built-in or external one.
        if (ActionManager.builtinActionMap.ContainsKey(actionName))
        {
            string methodName = ActionManager.builtinActionMap[actionName];
            ArrayList args = action.Parameters;
            this.AM.doAction(gameObject.GetInstanceID(), methodName, args, actionComplete);
		}
        else if(ActionManager.externalActionMap.ContainsKey(actionName))
        {
            string methodName = ActionManager.externalActionMap[actionName];
            // Retrieve action parameters.
            ArrayList args = action.Parameters;
            ActionTarget target = null;
            foreach (System.Object arg in args)
            {
                if (arg.GetType() == typeof(ActionTarget))
                {
                    target = (ActionTarget)arg;
                    break;
                }
            }
            if (target == null)
            {
                Debug.LogError("OCActionScheduler - executeAction: target missing for " + actionName);
                // Action target is null, notify failure.
                actionComplete(new ActionResult(null, ActionResult.Status.FAILURE, AV, args));
                return;
            }
            this.AM.doAction(target.id, methodName, args, actionComplete);
        }
        else
        {
            Debug.LogError("OCActionScheduler - executeAction: " + actionName
			               + " is not registered.");
            // Action is not registered, notify failure.
            actionComplete(new ActionResult(null, ActionResult.Status.FAILURE, AV, action.Parameters));
        }
		
		currentActionBeginTime = Time.time;
	}

    public void actionComplete(ActionResult ar)
    {
        if (this.currentAction == null) return;
		if (this.connector != null)
        	this.connector.handleActionResult(ar, this.currentAction);
        this.currentAction = null;
		
    }

    public void receiveActionPlan(LinkedList<MetaAction> actionPlan)
    {
        cancelCurrentActionPlan();
        lock (this.actionList)
        { this.actionList = actionPlan; }

        // Following code is for visual debugging of pathfinding. 
      List<Vector3> path = extractPath(actionPlan);
        if (path.Count == 0) return;
        VisualPathDebugger visualDebugger = GameObject.Find("World").GetComponent<VisualPathDebugger>() as VisualPathDebugger;
        if (visualDebugger)
            visualDebugger.SendMessage("DrawPath", path);
            
    }
	
	public void cancelCurrentActionPlan()
	{
        lock (this.actionList)
        {
            this.actionList.Clear();
        }
        this.currentAction = null;
    }
	
    /// <summary>
    /// For debugging purpose, extract all locations to go in the action plan.
    /// </summary>
    /// <param name="actions"></param>
    /// <returns></returns>
    private List<Vector3> extractPath(LinkedList<MetaAction> actions)
    {
        List<Vector3> path = new List<Vector3>();
        foreach (MetaAction action in actions)
        {
			Vector3 pos = new Vector3(0.0f,0.0f,0.0f);
            ArrayList parameters = action.Parameters;
            foreach (System.Object param in parameters)
            {
                if (param.GetType() == typeof(Vector3))
                {
					path.Add((Vector3)param);
                }
            }
        }
        return path;
    }
	
	void Awake()
	{
		this.AV = GetComponent<Avatar>() as Avatar;
        this.AM = GetComponent<ActionManager>() as ActionManager;
		this.connector = gameObject.GetComponent("OCConnector") as OCConnector;
	}
	
	void Update()
	{
        // Check if there is an running actvion.
        if (this.currentAction != null) 
		{
		//	if (Time.time - currentActionBeginTime > TimeOutSeconds)
		//		cancelCurrentActionPlan();
			
			return;
		}
		
		// we don't want the agent execute actions too quickly,
		// especaill when build two blocks successively, will get minecrafe bugs
		if (Time.time - currentActionBeginTime < ActionIntervalSeconds)
			return;
		
        // If action plan hasn't been finished, pick up one from the head
        // and execute it.
        if (this.actionList != null&& this.actionList.Count > 0)
        {
            lock (this.actionList)
            {
                this.currentAction = this.actionList.First.Value;
                this.actionList.RemoveFirst();
            }
            executeAction(this.currentAction);
        }
	}

}


