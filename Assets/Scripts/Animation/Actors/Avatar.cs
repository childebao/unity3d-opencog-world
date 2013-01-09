using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Embodiment;

public class Avatar: Interactor {
    
    enum MoveDirection{
        Forward = 0,
        Backward = 1,
        Left = 2,
        Right = 3
    }
    

    public float Speed = 0;
    public float RotateSpeed = 0;
    public GameObject TempTarget;
    
    public GameObject _inventory = null;
    private int inventoryId = 0;
	
	
    public GameObject inventory
    {
        get {
            return _inventory;
        }
        set {
            _inventory = value;
        }
    }
    
    //Movement
    private Vector3 movement = Vector3.zero;
    private Vector3 rotateDir = Vector3.zero;

    // list of gameobjects to be notifies of avatars completing/failing actions
    private LinkedList<MonoBehaviour> actionResultListeners = new LinkedList<MonoBehaviour>();

    protected bool moving = false;
    protected bool rotating = false;
    private bool useAngle = false;

    private float OriginalAngle = 0;
    private float TurnAngle = 0;

    private GameObject LastTargetObj;
    //private Vector3 LastDestPos = Vector3.zero;

    private ActionSummary moveToObjectAction;
    private ActionSummary moveToCoordAction;
    private ActionSummary rotateToAction;
    private ActionSummary jumpUpAction;
    private ActionSummary jumpTowardAction;
    private ActionSummary jumpForwardAction;
    private ActionSummary sayAction;

    private ActionSummary moveBackwardAction;
    private ActionSummary moveForwardAction;
    private ActionSummary rotateLeftAction;
    private ActionSummary rotateRightAction;

    private ActionSummary buildBlockAction;
	private ActionSummary buildBlockAtPositionAction;
    private ActionSummary destroyBlockAction;
    private ActionSummary trickyLearnAction;
    
    private ActionSummary currentAction = null;
    
	private OCConnector connector;
	// World game object will be used in building/destroying blocks,
	// collision detection.
	private WorldGameObject worldGameObject;
	
    // Define two dictionaries to monitor the action updating state in 
	// action manager, each time when an action is added to or removed 
	// from the manager, the state should be reported to OAC to mark 
	// the action as either available or unavailable.
	// Currently, only action name, action actor, action target(if any)
	// will be reported. If you want to have a new feature in OAC, just add 
	// extra information by yourself.
	private Dictionary<ActionKey, ActionSummary> addedActions = new Dictionary<ActionKey, ActionSummary>();
	private Dictionary<ActionKey, ActionSummary> removedActions = new Dictionary<ActionKey, ActionSummary>();
	
    virtual public string agentType
    {
        get {
            return "avatar";
        }
    }
    
    void  Start (){
    	connector = GetComponent<OCConnector>() as OCConnector;
    	worldGameObject = GameObject.Find("World").GetComponent<WorldGameObject>() as WorldGameObject;
        AM = GetComponent<ActionManager>() as ActionManager;
		
		ActionManager.registerActionMap("walk", "MoveToCoordinate");
		ActionManager.registerActionMap("go_to_object", "MoveToObject");
        ActionManager.registerActionMap("turn", "RotateTo");
        ActionManager.registerActionMap("jump_up", "JumpUp");
        ActionManager.registerActionMap("jump_toward", "JumpToward");
        ActionManager.registerActionMap("jump_forward", "JumpForward");
        ActionManager.registerActionMap("say", "Say");
        ActionManager.registerActionMap("lick", "TurnOnBatterySwitch");
        ActionManager.registerActionMap("step_backward", "MoveBackward");
        ActionManager.registerActionMap("step_forward", "MoveForward");
        ActionManager.registerActionMap("rotate_left", "RotateLeft");
        ActionManager.registerActionMap("rotate_right", "RotateRight");
        ActionManager.registerActionMap("rotate", "RotateToByITween");
        ActionManager.registerActionMap("build_block", "BuildBlockInFrontWithOffset");
		ActionManager.registerActionMap("build_block_At_Position", "BuildBlockAtPosition");
        ActionManager.registerActionMap("destroy_block", "DestroyBlockInFront");

        ActionManager.registerActionMap("eat", "Consume", ActionSource.EXTERNAL);
        ActionManager.registerActionMap("grab", "PickUp", ActionSource.EXTERNAL);

        moveToObjectAction = new ActionSummary(this, "MoveToObject", new AnimSummary(), 
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW), false);
        moveToObjectAction.usesCallback = true;

        moveToCoordAction = new ActionSummary(this, "MoveToCoordinate", new AnimSummary(), 
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW), false);
        moveToCoordAction.usesCallback = true;

        rotateToAction = new ActionSummary(this, "RotateToByITween", new AnimSummary(), 
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW), false);
        rotateToAction.usesCallback = true;

        jumpUpAction = new ActionSummary(this, "JumpUp", new AnimSummary(),
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW), false);
        jumpUpAction.usesCallback = true;

        jumpTowardAction = new ActionSummary(this, "JumpToward", new AnimSummary(),
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.MEDIUM), false);
        jumpTowardAction.usesCallback = true;
        
		jumpForwardAction = new ActionSummary(this, "JumpForward", new AnimSummary(),
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.MEDIUM), false);
        jumpForwardAction.usesCallback = true;

        sayAction = new ActionSummary(this, "Say", new AnimSummary(),
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.NONE), false);
        sayAction.usesCallback = false;

        trickyLearnAction = new ActionSummary(this, "TurnOnBatterySwitch", new AnimSummary(),
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.NONE), false);
        trickyLearnAction.usesCallback = true;

        moveBackwardAction = new ActionSummary(this, "MoveBackward", new AnimSummary(), 
                                               new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW), false);
        moveBackwardAction.usesCallback = true;

        moveForwardAction = new ActionSummary(this, "MoveForward", new AnimSummary(),
                                               new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW), false);
        moveForwardAction.usesCallback = true;

        rotateLeftAction = new ActionSummary(this, "RotateLeft", new AnimSummary(),
                                               new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW), false);
        rotateLeftAction.usesCallback = true;

        rotateRightAction = new ActionSummary(this, "RotateRight", new AnimSummary(),
                                               new PhysiologicalEffect(PhysiologicalEffect.CostLevel.LOW), false);
        rotateRightAction.usesCallback = true;

        buildBlockAction = new ActionSummary(this, "BuildBlockInFrontWithOffset", new AnimSummary("createBlock"),
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.HIGH), false);
        buildBlockAction.usesCallback = false; 
		
		buildBlockAtPositionAction = new ActionSummary(this, "BuildBlockAtPosition", new AnimSummary("createBlock"),
                                                new PhysiologicalEffect(PhysiologicalEffect.CostLevel.HIGH), false);
        buildBlockAtPositionAction.usesCallback = false; 
		
		destroyBlockAction = new ActionSummary(this, "DestroyBlockInFront", new AnimSummary("destroyBlockM"),
		                                        new PhysiologicalEffect(PhysiologicalEffect.CostLevel.HIGH), false);
		destroyBlockAction.usesCallback = false;
		
        AM.addAction(moveToCoordAction);
        AM.addAction(moveToObjectAction);
        AM.addAction(rotateToAction);
        AM.addAction(jumpUpAction);
        AM.addAction(jumpTowardAction);
        AM.addAction(sayAction);
        AM.addAction(trickyLearnAction);
        AM.addAction(moveBackwardAction);
        AM.addAction(moveForwardAction);
        AM.addAction(rotateLeftAction);
        AM.addAction(rotateRightAction);
        AM.addAction(buildBlockAction);
		AM.addAction(buildBlockAtPositionAction);
		AM.addAction(destroyBlockAction);
		AM.addAction(jumpForwardAction);
        /*AM.addAction(this,"MoveToCoordinate");
        AM.addAction(this,"MoveForward");
        AM.addAction(this,"Stop");
        AM.addAction(this,"RotateTo");
        AM.addAction(this,"LookAt")*/; // This LookAt can NO LONGER be used as an action. It is used internally instead. 
        
        Animation animation = gameObject.GetComponentInChildren<Animation>();
        // Make all animations in this character play at half speed
        foreach ( AnimationState state in animation) {
            state.speed = 1.0f;
        }

    }	

    void Update (){
        if(moving) Move();

        if(rotating) {
            if(useAngle)
                Rotate(TurnAngle);
            else
                Rotate(90);
        }

        // Check if the inventory is changed.
        bool inventoryChange = false;
        if (inventory != null)
        {
            if (inventory.GetInstanceID() != inventoryId)
            {
                inventoryChange = true;
                inventoryId = inventory.GetInstanceID();
            }
        }
        else
        {
            // The inventory is already null, but the last inventory id is still
            // dirty, which means inventory changed.
            if (inventoryId != 0)
            {
                inventoryChange = true;
                inventoryId = 0;
            }
        }
        // If the inventory changed, recalculate the height of spinner if any.
        if (inventoryChange)
        {
            Transform spinner = transform.FindChild("SelectionSpinner");
            if (spinner)
            {
                spinner.SendMessage("calcObjectHeight");
            }
        }
		
		// Send updated actions to OAC, if any.
		lock (addedActions)
		{
			if (addedActions.Count > 0)
			{
				List<ActionSummary> addedList = new List<ActionSummary>(addedActions.Values);
				if (connector != null)
					connector.sendActionAvailability(addedList, true);
				addedActions.Clear();
			}
		}
		
		lock (removedActions)
		{
			if (removedActions.Count > 0)
			{
				List<ActionSummary> removedList = new List<ActionSummary>(removedActions.Values);
				if (connector != null)
					connector.sendActionAvailability(removedList, false);
				removedActions.Clear();
			}
		}
		

    }

    // -------------------------------------
    // These two methods are used internally for non-iTween based movement
    // from frame to frame
    private void Move () {
		
        transform.Translate(movement * Speed * Time.deltaTime);
    }

    private void Rotate (float turnAngle, ActionCompleteHandler h = null) {
        transform.Rotate(rotateDir * RotateSpeed * Time.deltaTime);
        float DeltaAngle = Mathf.Abs(transform.eulerAngles.y - OriginalAngle);

        if(DeltaAngle > turnAngle) {
            rotating = false;
			ActionResult ar = new ActionResult(rotateToAction, ActionResult.Status.SUCCESS, this , null, "I did a rotation!");	
            if (h != null) h(ar); //new ActionResult(a,ActionResult.Status.SUCCESS,this));
        }
    }
    // -------------------------------------

    public bool isMoving (){
        return moving;
    }

    public bool isRotating (){
        return rotating;
    }
    
    // --- Actions
    
    public void MoveToCoordinate(Vector3 Destination, float LookTime = 1.0f, ActionCompleteHandler callback = null) {
         
        if (currentAction == null) currentAction = moveToCoordAction;
        
        //LastDestPos = Destination;
        //Debug.LogWarning("Destination is " + Destination);
        // Pass null as we don't want to report the completion of this
        // subaction
        LookAt(Destination,LookTime);

        Vector3 Dest = Destination;
        // Overwrite the y position (height)
        Dest.y = transform.position.y;
		
        Animation anim = gameObject.GetComponentInChildren<Animation>();
        DisableNormalAnimation();
        Debug.LogWarning("playing move start");
        anim.Play("walk");
        StartCoroutine(RestoreNormalAnimation(anim["walk"].length));

        // Wrap parameters for collision detection while moving.
        Hashtable onupdateparams = new Hashtable();
 		if (callback != null)
		{
        	onupdateparams.Add("callback", callback);
        	onupdateparams.Add("dest", Dest);
		
        	iTween.MoveTo(gameObject, iTween.Hash("position", Dest,
                     "speed" , Speed,
                     "easetype" , "linear",
                     "axis", "y",
                     "onupdate", "_MoveToCollisionDetect", "onupdateparams", onupdateparams,
                     "oncomplete" , "_ReachDestCoordinate", "oncompleteparams", callback)
                 );
		}
		else
		{
        	onupdateparams.Add("dest", Dest);
		
        	iTween.MoveTo(gameObject, iTween.Hash("position", Dest,
                    "speed" , Speed,
                    "easetype" , "linear",
                    "axis", "y",
                    "onupdate", "_MoveToCollisionDetect", "onupdateparams", onupdateparams,
                    "oncomplete" , "_ReachDestCoordinate")
                );
		}
        moving = true;
    }
    
	/// <summary>
	/// This collision detecting function is executed in every frame when doing
	/// an iTween MoveTo action.
	/// </summary>
	private void _MoveToCollisionDetect(object paramVals)
	{/*
        bool isDestInFront = false;
        Hashtable paramMap = (Hashtable)paramVals;
        // Calculate the angle from current position to destination.
        Vector3 dest = (Vector3)paramMap["dest"];

        if(Vector3.Equals(dest, transform.position)) return;

        Vector3 destDir = dest - transform.position;
        float angle = Vector3.Angle(transform.forward, destDir);

        isDestInFront = (angle < 45.0f);

        Debug.LogError("Current position: " + transform.position + " Dest position: " + dest + " Target angle: " + angle);
		// Get the block that the avatar is standing on.
        Vector3 front = gameObject.transform.position;

        // Get the direction that the avatar facing towards, by default it is facing towards Z-Axis.
        Vector3 eulerAngle = transform.rotation.eulerAngles;

        float zFront = 1.2f * (float)Math.Cos((eulerAngle.y / 180) * Math.PI);
        float xFront = 1.2f * (float)Math.Sin((eulerAngle.y / 180) * Math.PI);
        
		front.x += xFront;
		front.y += 1;
		front.z += zFront;

        IntVect frontPoint = new IntVect((int)front.x, (int)front.z, (int)front.y);
		
		if (worldGameObject)
		{
            // If the avatar is heading towards the destination in forward direction and there's a
            // block in front, then stop moving.
			if (isDestInFront && 
                worldGameObject.WorldData.GetBlock((uint)frontPoint.X, (uint)frontPoint.Y, (uint)frontPoint.Z).Type != BlockType.Air)
			{
				// if the block in front is only one block high, it can jump on it
				if (worldGameObject.WorldData.GetBlock((uint)frontPoint.X, (uint)frontPoint.Y, (uint)frontPoint.Z + 1).Type == BlockType.Air)
				{
					// jump on it
					
					JumpToward(new Vector3(frontPoint.X,frontPoint.Z + 1.0f ,frontPoint.Y),null);
				}
				else
				{
					Debug.LogError("Block is detected in front.");
					iTween.Stop();
					
					// Send an action failure feedback.
					ActionResult ar = new ActionResult(moveToCoordAction, ActionResult.Status.FAILURE, this , null, 
					                                   "Failed to move to destination because of obstacle in front");
	                ActionCompleteHandler h = (ActionCompleteHandler)paramMap["callback"];
	                if (h != null) h(ar);
				}
			}
		}
		*/
	}
    
    public void DisableNormalAnimation() {
        Debug.LogWarning("disable normal");
        Animator xxx = gameObject.GetComponentInChildren<Animator>();
        xxx.StopNormal();
    }
    
    public IEnumerator RestoreNormalAnimation(float afterDuration) {
        yield return new WaitForSeconds(afterDuration);
        Animator xxx = gameObject.GetComponentInChildren<Animator>();
        Debug.LogWarning("restore normal after " + afterDuration + " seconds.");
        xxx.PlayNormal();
    }
    
    public void _ReachDestCoordinate(ActionCompleteHandler h) {
        moving = false;
        DisableNormalAnimation();
        Animation anim = gameObject.GetComponentInChildren<Animation>();
        //anim.Play("move_end");
        //StartCoroutine(RestoreNormalAnimation(anim["move_end"].length));

		ArrayList pp = new ArrayList();
        pp.Add(gameObject.transform.position);
        ActionResult ar = new ActionResult(moveToCoordAction, ActionResult.Status.SUCCESS, this , pp, "I moved to (" + 
		                                   gameObject.transform.position.x + ", " +
		                                   gameObject.transform.position.y + ", " + 
		                                   gameObject.transform.position.z + ")" );
		                                   
        notifyListeners(ar,h);
		
    }

    public void MoveToObject(ActionTarget targetToMoveTo, float LookTime = 1.0f, ActionCompleteHandler callback = null) 
	{
		GameObject theObject = OCBehaviour.findObjectByInstanceId(targetToMoveTo.id);
		if (theObject == null)
		{
			Debug.Log("MoveToObject: The object is null!");
			
			ArrayList pp = new ArrayList();
        	pp.Add(targetToMoveTo);
			pp.Add(1.0f);
			ActionResult ar = new ActionResult(moveToObjectAction, ActionResult.Status.FAILURE, this, pp,
			                                  "Tried to move to an unexisting object");
			OCActionScheduler scheduler = gameObject.GetComponent<OCActionScheduler>() as OCActionScheduler;
        	callback += scheduler.actionComplete;
			notifyListeners(ar,callback);
			return;
		}
		
        LastTargetObj = theObject;
        if (currentAction == null) currentAction = moveToObjectAction;
        
        Vector3 Dest;
        
        if(theObject.tag == "OCObject") 
		{
            OCBehaviour OCB = theObject.GetComponent<OCBehaviour>() as OCBehaviour;
			if (OCB == null)
			{
				Vector3 src = gameObject.transform.position;
				Vector3 dest = theObject.collider.bounds.center;
				Vector3 direction = Vector3.Normalize(dest - src);
				Dest = dest - direction * (theObject.collider.bounds.extents.x + theObject.collider.bounds.extents.z)/2.0f;
			}
			else
				Dest = OCB.GetInteractPoint(this);
		}    
        else 
		{
            // Need to get the size of the destination object...
            Dest = theObject.transform.position;
        }
        // Overwrite the y position (height)
        Dest.y = transform.position.y;
        
		if (agentType == "player")
		{

	        Animation anim = gameObject.GetComponentInChildren<Animation>();
        	DisableNormalAnimation();
        	Debug.LogWarning("playing player move start");
        	anim.Play("walk");
        	StartCoroutine(RestoreNormalAnimation(anim["walk"].length));
	        iTween.MoveTo(gameObject, iTween.Hash("position", Dest,
	                    "looktarget", Dest, 
	                    "speed" , 2.0f,
	                    "easetype" , "linear",
	                    "axis", "y",
	                    "oncomplete" , "_ReachDestObject", "oncompleteparams",callback)
	                );
		}
		else
		{
	        //Debug.Log("Moving to " + Target + " at " + Dest);
	        DisableNormalAnimation();
	        Animation anim = gameObject.GetComponentInChildren<Animation>();
	        Debug.LogWarning("playing robot move start");
	        //anim.Play("move_start");
	        //StartCoroutine(RestoreNormalAnimation(anim["move_start"].length));
	        anim.Play("walk");
			StartCoroutine(RestoreNormalAnimation(anim["walk"].length));
	        iTween.MoveTo(gameObject, iTween.Hash("position", Dest,
	                    "looktarget", Dest, 
	                    "speed" , Speed,
	                    "easetype" , "linear",
	                    "axis", "y",
	                    "oncomplete" , "_ReachDestObject", "oncompleteparams",callback)
	                );
		}
        moving = true;
    }

    private void _ReachDestObject( ActionCompleteHandler h = null) {
		
		moving = false;
		if (agentType != "player")
		{
	        DisableNormalAnimation();
	       // Animation anim = gameObject.GetComponentInChildren<Animation>();
	        //anim.Play("move_end");
	       // StartCoroutine(RestoreNormalAnimation(anim["move_end"].length));
		}
		else
		{
			((Player)this).isAutoWalking = false;
		}
		
		if (LastTargetObj != null)
			LookAt(LastTargetObj.transform.position, 1f);

		ArrayList pp = new ArrayList();
		ActionTarget targetToMoveTo = new ActionTarget(LastTargetObj.GetInstanceID(), EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
        pp.Add(targetToMoveTo);
		pp.Add(1.0f);
        ActionResult ar = new ActionResult(moveToObjectAction, ActionResult.Status.SUCCESS, this , pp, "I moved to the " + targetToMoveTo.id);
        notifyListeners(ar,h);
		
	    LastTargetObj = null;
        
    }

    private void LookAt (Vector3 LookTarget, float LookTime = 1.0f) {
        Vector3 LookPos = LookTarget;
        if (LookTarget - transform.position == Vector3.zero) {
            // Don't try to look in a zero length direction.
			
			// ?
        }
        LookPos.y = transform.position.y;

        iTween.LookTo(gameObject, iTween.Hash("looktarget", LookPos, "time", LookTime));
 
        
    }

    public void notifyListeners(ActionResult ar, ActionCompleteHandler h) {
        if (currentAction == null) 
			return;
        if (h != null && ar != null)
        	h(ar);
		/*else
			Debug.LogError("Avatar: notifyListeners should not have null ActionResult or null ActionCompleteHandler!");*/
        currentAction = null;
    }

    /// These commands run indefinitely and are useful for free form navigation
    
    void StrafeLeft() {
        moving = true;
        movement = Vector3.left;
    }

    void StrafeRight() {
        moving = true;
        movement = Vector3.right;
    }

    public void MoveForward() {
        moving = true;
        movement = Vector3.forward;
    }

    public void MoveBackward() {
        moving = true;
        movement = Vector3.back;
    }

    public void RotateLeft() {
        OriginalAngle = transform.eulerAngles.y;
        rotating = true;
        rotateDir = new Vector3(0,-1,0);
    }

    public void RotateRight() {
        OriginalAngle = transform.eulerAngles.y;
        rotating = true;
        rotateDir = new Vector3(0,1,0);
    }

	/// <summary>
	/// If called by OAC, use "RotateToByITween" method instead, which will invoke
	/// a callback function in the end.
	/// </summary>
    public void RotateTo (float Angle, Vector3 rotateDir) {

        if(rotateDir == new Vector3(0,-1,0))
            RotateLeft();
        if(rotateDir == new Vector3(0,1,0))
            RotateRight();

        useAngle = true;
        TurnAngle = Angle;
        OriginalAngle = transform.eulerAngles.y;
    }
    
	/// <summary>
	/// Implementation of RotateTo by iTween, will trigger a callback to notify OAC
	/// about the action status.
	/// </summary>
	public void RotateToByITween (float angle, ActionCompleteHandler h = null)
	{
        if (currentAction == null) currentAction = rotateToAction;
        
		if (angle < 0)
		{
			// If angle is negative, means to turn left.
			angle += 360;
		}
		
		Vector3 eulerAngle = new Vector3(0, transform.eulerAngles.y + angle, 0);
		
		iTween.RotateTo(gameObject, iTween.Hash("rotation", eulerAngle,
                    "speed", 35.0f,
                    "oncomplete", "_ReachRotateAngle", "oncompleteparams", h)
                );
	}
	
	private void _ReachRotateAngle( ActionCompleteHandler h = null) {
		ActionResult ar = new ActionResult(rotateToAction, ActionResult.Status.SUCCESS, this , null, "I did a rotation");
        notifyListeners(ar,h);
	}
	
    public void JumpUp(ActionCompleteHandler h)
    {
        currentAction = jumpUpAction;
        Vector3 dest = gameObject.transform.position;
        dest.y += 1.5f;
		
		Hashtable paras = new Hashtable(); 
		paras.Add("myString", "a string"); 
		
        iTween.MoveTo(gameObject, iTween.Hash("position", dest,
                    "speed", Speed,
                    "easetype", "linear",
                    "oncomplete", "_onJumpUpComplete", "oncompleteparams", h)
                );
        
    }
	
	private void _onJumpUpComplete(ActionCompleteHandler h)
	{
		if (h == null)
			return;
		       
        ActionResult ar = new ActionResult(jumpUpAction, ActionResult.Status.SUCCESS, this , null, "I did a jump up!");
		
	}


    public void JumpToward(Vector3 Destination, ActionCompleteHandler h)
    {
        currentAction = jumpTowardAction;
        Vector3 dest = Destination;
        //dest.y = gameObject.transform.position.y + Destination.y;
		
		/* TODO: need a new calculate way
		// Check if the destination is already occupied by a block.
		IntVect destPoint = new IntVect((int)dest.x, (int)dest.z, (int)dest.y);
        
        if (worldGameObject)
		{
			if (worldGameObject.WorldData.GetBlock((uint)destPoint.X, (uint)destPoint.Y, (uint)destPoint.Z).Type != BlockType.Air)
			{
				ActionResult ar = new ActionResult(jumpTowardAction, ActionResult.Status.FAILURE, this , null, "I can not jump towards destination!");
				h(ar);
				return;
			}
		}
		*/
        LookAt(dest, 0.5f);
        iTween.MoveTo(gameObject, iTween.Hash("position", dest,
            "speed", Speed,
            "easetype", "linear",
            "oncomplete", "_onJumpTowardComplete", "oncompleteparams", h)
        );
	  
    }
    
	public void JumpForward(float height, ActionCompleteHandler h)
	{
		currentAction = jumpForwardAction;
		Vector3 dest = gameObject.transform.position;
		
		// Get the direction that the avatar facing towards, by default it is facing towards Z-Axis.
        Vector3 eulerAngle = transform.rotation.eulerAngles;

        float zFront = 1.0f * (float)Math.Cos((eulerAngle.y / 180) * Math.PI);
        float xFront = 1.0f * (float)Math.Sin((eulerAngle.y / 180) * Math.PI);
		
		dest.x += xFront;
		dest.y += height;
		dest.z += zFront;
		
		// Check if the destination is already occupied by a block.
		IntVect destPoint = new IntVect((int)dest.x, (int)dest.z, (int)dest.y+1);
		
        if (worldGameObject)
		{
			if (worldGameObject.WorldData.GetBlock((int)destPoint.X, (int)destPoint.Y, (int)destPoint.Z).Type != BlockType.Air)
			{
				ActionResult ar = new ActionResult(jumpForwardAction, ActionResult.Status.FAILURE, this , null, "I can not jump forward!");
				h(ar);
				return;
			}
		}
		
		LookAt(dest, 2.5f);

        iTween.MoveTo(gameObject, iTween.Hash("position", dest,
                    "speed", Speed,
                    "easetype", "linear",
                    "oncomplete", "_onJumpTowardComplete", "oncompleteparams", h)
                );	
	}
	
	private void _onJumpTowardComplete(ActionCompleteHandler h)
	{
		if (h == null)
			return;
					
        ActionResult ar = new ActionResult(jumpTowardAction, ActionResult.Status.SUCCESS, this , null, "I did a jump toward!"); 
        notifyListeners(ar,h);
		
	}	
	
    public void Say(string content, string listener = "Player", ActionCompleteHandler h = null)
    {
        if (currentAction == null) currentAction = sayAction;
        if (listener.Trim() == "") listener = "Player";
        GameObject console = GameObject.Find("Console") as GameObject;
        if (console)
        {
            Console consoleScript = console.GetComponent("Console") as Console;
            consoleScript.AddSpeechConsoleEntry(content, gameObject.name, listener);
        }
        else
        {
            Debug.LogError("Can not find console instance.");
        }

        DialogInstance dialog = gameObject.GetComponent<DialogInstance>() as DialogInstance;
        if (dialog != null)
        {
            dialog.LoadDialog(content);
        }
    }

    ///--------------------------------
    ///
    
    /// Stop all movement
    public void Stop() {
        rotating = false;
        moving = false;
    }

    public bool putInInventory(GameObject go) {
        if (inventory != null) return false;
        // put this object in the avatars inventory
        inventory = go;
        // Perform action
        go.rigidbody.isKinematic = true;
        go.collider.isTrigger = true;
		
        if (agentType == "player") {
            // place the object under the hand transform
          	GameObject HandObject = GameObject.Find("l_WristJ");
			go.transform.parent = HandObject.transform;	
            go.transform.localPosition = new Vector3(0.0f,0.0f,0.0f); 
        } else {
            // Hold the inventory above the avatar instead of its right hand.
            // If there is a spinner marking the avatar is selected, we should recalculate
            // the height of spinner.
            Transform spinner = transform.FindChild("SelectionSpinner");
            float objectHeight = VerticalSizeCalculator.getHeight(gameObject.transform, spinner);
            go.transform.parent = gameObject.transform;
            Vector3 goSize = Vector3.up * go.collider.bounds.size.y;
            go.transform.position = gameObject.transform.position + goSize + (Vector3.up * 1.01f * objectHeight);
            go.transform.localRotation = Quaternion.identity;
            
            // Add the particle effect tractor beam
            setTractorBeamState(true);
        }
        // reset any actions...
        go.SendMessage("AddAction",this);
		
		PutOnAbleObject putOn = go.GetComponent<PutOnAbleObject>();
		if (putOn != null)
			putOn.SendMessage("OnBeHeld");
		
        return true;
    }
    
    public bool removeFromInventory(GameObject go) {
        if (inventory == null) return false;
        
        inventory = null;
        if (go != null) {
            // Change properties
            go.transform.parent = GameObject.Find("Objects").transform;
            go.rigidbody.isKinematic = false;
            go.collider.isTrigger = false;
            go.rigidbody.useGravity = true;
            // reset any actions...
            go.SendMessage("RemoveAction",this); 
            go.SendMessage("AddAction",this); 
        }
        setTractorBeamState(false);
		
        PutOnAbleObject putOn = go.GetComponent<PutOnAbleObject>();
		if (putOn != null)
			putOn.SendMessage("OnNotBeHeld", this);
        return true;
    }
    
    public void setTractorBeamState(bool state) {
        if (state) {
            GameObject beam = (GameObject) Instantiate(Resources.Load("tractor beam particles"));
            beam.name = "beam";
            beam.transform.parent = transform;
            beam.transform.localRotation = Quaternion.identity;
            beam.transform.localPosition = Vector3.zero;
            beam.transform.localScale = Vector3.one;
        } else {
            // Remove the particle effect tractor beam
            Transform b = transform.FindChild("beam");
            if (b != null) Destroy(b.gameObject);
        }
    }

    /// <summary>
    /// Build a block in front of the avatar with a given vertical offset.
    /// </summary>
    /// <param name="offset">Vertical offset to decide the altitude of the block</param>
    /// <param name="blockTypeStr">
    /// Decide what kind of block we are going to build, "TopSoil" by default.
    /// </param>
    /// <param name="h"></param>
    public void BuildBlockInFrontWithOffset(float offset, string blockTypeStr = "TopSoil", ActionCompleteHandler h = null)
    {
    	if (!worldGameObject) {
    		Debug.LogError("World game object is not available, can not build a block.");
			return;
		}
        if (currentAction == null) currentAction = buildBlockAction;
        // Get the block that the avatar is standing on.
        Vector3 myPosition = gameObject.transform.position;
        IntVect standingBlock = new IntVect((int)myPosition.x, (int)myPosition.z, (int)myPosition.y);

        // Correct the position of avatar to the center of its current standing on block.
        // So that the avatar will not compass the block that is to be built.
        transform.position = new Vector3(standingBlock.X + 0.5f, transform.position.y, standingBlock.Y + 0.5f);

        // Get the direction that the avatar facing towards, by default it is facing towards Z-Axis.
        Vector3 eulerAngle = transform.rotation.eulerAngles;

        float zFront = 1.0f * (float)Math.Cos((eulerAngle.y / 180) * Math.PI);
        float xFront = 1.0f * (float)Math.Sin((eulerAngle.y / 180) * Math.PI);

        Vector3 buildPoint = gameObject.transform.position;

        buildPoint.x += xFront;
        buildPoint.y += offset;
        buildPoint.z += zFront;

        // Translate the build point to integer coordinate.
        IntVect blockBuildPoint = new IntVect((int)buildPoint.x, (int)buildPoint.z, (int)buildPoint.y);

    	BlockType blockType = BlockType.Stone;
		if (Block.StringToTypeMap.ContainsKey(blockTypeStr))
		{
			blockType = Block.StringToTypeMap[blockTypeStr];
		}

        worldGameObject.world.GenerateBlockAt(blockBuildPoint, blockType);
    }
	

	public void BuildBlockAtPosition(IntVect blockBuildPoint, string blockType = "TopSoil", ActionCompleteHandler h = null)
    {
    	if (!worldGameObject) {
    		Debug.LogError("World game object is not available, can not build a block.");
			return;
		}
		
		LookAt(new Vector3(blockBuildPoint.X,blockBuildPoint.Z,blockBuildPoint.Y));
	    if (currentAction == null) currentAction = buildBlockAtPositionAction;
		
		IntVect blockBuildPointtest = new IntVect((int)blockBuildPoint.X, (int)blockBuildPoint.Z, (int)blockBuildPoint.Y);

    	BlockType type = BlockType.Stone;
		if (Block.StringToTypeMap.ContainsKey(blockType))
		{
			type = Block.StringToTypeMap[blockType];
		}

        worldGameObject.world.GenerateBlockAt(blockBuildPoint, BlockType.Lava);
	
    }

	
    public void DestroyBlockInFront(float offset)
    {
    	if (!worldGameObject) {
			Debug.LogError("World game object is not available, can not destroy a block.");
			return;
		}
		if (currentAction == null) currentAction = destroyBlockAction;
		// Get the block that the avatar is standing on.
        Vector3 myPosition = gameObject.transform.position;
        IntVect standingBlock = new IntVect((int)myPosition.x, (int)myPosition.z, (int)myPosition.y);

        // Correct the position of avatar to the center of its current standing on block.
        // So that the avatar will not compass the block that is to be built.
        transform.position = new Vector3(standingBlock.X + 0.5f, transform.position.y, standingBlock.Y + 0.5f);
		
        // Get the direction that the avatar facing towards.
        Vector3 eulerAngle = transform.rotation.eulerAngles;

        float zFront = 1.0f * (float)Math.Cos((eulerAngle.y / 180) * Math.PI);
        float xFront = 1.0f * (float)Math.Sin((eulerAngle.y / 180) * Math.PI);

        Vector3 destroyPoint = gameObject.transform.position;

        destroyPoint.x += xFront;
        destroyPoint.y += offset;
        destroyPoint.z += zFront;

        IntVect blockDestroyPoint = new IntVect((int)destroyPoint.x, (int)destroyPoint.z, (int)destroyPoint.y);
		
		// Set the block 
        worldGameObject.WorldData.SetBlockLightWithRegeneration((int)blockDestroyPoint.X, (int)blockDestroyPoint.Y, (int)blockDestroyPoint.Z, 255);
    }
	
	public void notifyActionAdded(ActionSummary action)
	{
		ActionKey ak = action.getKey();
		if (!addedActions.ContainsKey(ak))
		{
			addedActions.Add(ak,action);
		}
	}
	
	public void notifyActionRemoved(ActionSummary action)
	{
		ActionKey ak = action.getKey();
		if (!removedActions.ContainsKey(ak))
		{
			removedActions.Add(ak,action);
		}
	}

    #region Evil trikcy functions
    // A tricky function to make avatar look smart enough to learn
    // something. Just used for the video.
    public void TurnOnBatterySwitch(ActionCompleteHandler h = null)
    {
        GameObject batterySwitch = GameObject.Find("BatterySwitch");
        if (batterySwitch == null) return;
		ActionTarget theObj = new ActionTarget(batterySwitch.GetInstanceID(), EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
        MoveToObject(theObj, 1.0f, triggerBatterySpawner);
    }

    void triggerBatterySpawner(ActionResult ar)
    {
        GameObject batterySwitch = GameObject.Find("BatterySwitch");
        if (batterySwitch == null) return;
        OCActionScheduler scheduler = gameObject.GetComponent<OCActionScheduler>() as OCActionScheduler;
        this.AM.doAction(batterySwitch.GetInstanceID(), "SpawnObject", null, scheduler.actionComplete);
    }
    #endregion
}
