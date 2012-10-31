using UnityEngine;
using System.Collections;

public class SpawnSwitch : OCBehaviour {

    // What type of object should we spawn?
    public GameObject templateObject;
    
    private ActionSummary spawnAction;
    
    public Vector3 spawnLocation;
    
    private Transform objectsRoot;
    
    // Use this for initialization
    void Start () {
        if (templateObject == null)
            Debug.LogError("SpawnSwitch has no template object!");
    
        AnimSummary animS = new AnimSummary();
        PhysiologicalEffect effect = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW);
        spawnAction = new ActionSummary(this,"SpawnObject", animS, effect, true);
        objectsRoot = GameObject.Find("Objects").transform;
				
		myActionList.Add("SpawnObject");
        
    }

    /**
     * This is called when the Avatar approaches the object
     */
    public void AddAction(Avatar avatar)
    {
        ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
        AM.addAction(spawnAction);
    }
    
    /**
     * This is called when the Avatar moves away from the object
     */
    public void RemoveAction(Avatar avatar)
    {
        ActionManager AM = avatar.GetComponent<ActionManager>() as ActionManager;
        AM.removeAction(gameObject.GetInstanceID(), "SpawnObject");
    }
    
    
    public void SpawnObject(Avatar a)
    {
        GameObject newObject = Instantiate(templateObject) as GameObject;
        newObject.transform.position = spawnLocation;
        newObject.transform.parent = objectsRoot;
        
    }
}
