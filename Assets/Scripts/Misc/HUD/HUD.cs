using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Embodiment;

public class HUD : MonoBehaviour
{
    public GameObject selectedAvatar;
//    public Player player;
	private GUITexture reticuleTexture;
	
	public const float minForce = 1.0f; 
	public const float maxForce = 200.0f; 
    private const float WIDTH = 80.0f;
    private const float HEIGHT = 200.0f; 
		
	private bool isShowForcePanel = false;
	private bool isForceIncrease = true; 
	private float lastTimeRenderForcePanel;
	private float currentForce = 1.0f;
	private SocialInteractioner soInter = null;
	private Texture2D barTex, bgTex;
	
	// the skin the interact GUI will use
    public GUISkin mySkin;
    public bool showGUI = true;
    public bool interactMenuVisible = false;
	
	// For things selected with a click
    public Transform selectedIndicator;
    // For things that just get highlighted when we are looking at them
    public Transform highlightIndicator;
    
    // if to show the OCObject info 
    public bool showOCObjectInfo = false;
	
	GameObject lastSelected = null;
    GameObject lastHighlighted = null;
	Transform allObjects = null;
	
	public GameObject theMainCamera = null;
    
    // Use this for initialization
    void Start ()
    {	
        Transform reticule = gameObject.transform.Find("Reticule");
        if (reticule == null)
            Debug.LogError("No \"Reticule\" game object found");
        reticuleTexture = reticule.gameObject.GetComponent<GUITexture>();
        reticuleTexture.pixelInset = new Rect (Screen.width/2, Screen.height/2, 16, 16);

        feelingTextureMap = new Dictionary<string, Texture2D>();
        demandTextureMap = new Dictionary<string, Texture2D>(); 
		
		allObjects = GameObject.Find("Objects").transform;
		
		if (theMainCamera == null)
            theMainCamera = GameObject.Find("Main_Camera");
		
		constructForceTexture();
    }

    // Update is called once per frame
    void Update ()
    {
		// While the interact button is pressed, display the interaction GUI
        // but only if the console isn't enabled
        if(Console.get() != null && !Console.get().isActive() && Input.GetKey(KeyCode.E)) {
            showGUI = true;
        } else {
            showGUI = false;
        } 
		
		// if the forcePanel is shown currently, we get the input to forcePanel,
		// not process others until the left mouse button released 
		if (IsShowForcePanel())
		{
			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				hideForcePanel();
			}
			return;
		}
		
		// If the console is not active, then a right click selects any object in the center
        // of the screen
        Vector3 thePoint = new Vector3(Camera.main.pixelWidth/2,Camera.main.pixelHeight/2,0);
        if(Input.GetKeyDown(KeyCode.Mouse0)) {
            if(Console.get().isActive()) {
                // If the console is active, then a right click selects an object whereever the cursor is
                thePoint = Input.mousePosition;
            }
        }
		
		// Determine what the character is looking at
        Ray ray = Camera.main.ScreenPointToRay (thePoint);
        // Draw a debug ray in the scene view
        // Increase the ray length from 10 to 40 because when use the Third Person view, 10 is not enough to hit the object looked at 
        Debug.DrawRay (ray.origin, ray.direction * 40, Color.yellow);
        
        // Dray a debug ray to show the player's forward direction
        Debug.DrawRay (gameObject.transform.position , gameObject.transform.forward * 15, Color.green);
        
        // If something gets hit, change the material so it appears selected
        RaycastHit hit = new RaycastHit();
		if (Physics.Raycast (ray.origin, ray.direction, out hit, 40)) 
		{
            //Debug.Log("raycast hit: " + hit.transform.gameObject);
            GameObject o = hit.transform.gameObject; 
            // Don't highlight chunks from the gameworld made of cubes
            if (o.tag != "Chunk" && o.transform.IsChildOf(allObjects)) {
                // if renderer is null this is probably an avatar or something else special
                // otherwise we assume it's a proper
                if (lastHighlighted != o) {
                    // only change things if we are looking at something different
					//@TODO: Readd the spinner stuff
                    removeSpinner();
                    //Debug.Log("Changed highlighted object to " + o.name);
                    addSpinner(o);
					
                }
				if(Input.GetKeyDown(KeyCode.Mouse1) && !Console.get().isActive() && lastHighlighted!=null)
				{
					//isAutoWalking = true;
					ArrayList args = new ArrayList();
					ActionTarget theObj = new ActionTarget(lastHighlighted.GetInstanceID(), EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
					args.Add(theObj);
					args.Add(1.0f);
					
					ActionManager am = selectedAvatar.GetComponent<ActionManager>();
					
					am.doAction(gameObject.GetInstanceID(),"MoveToObject",args,actionComplete);
				}
          	} else {
                removeSpinner();
            }
			
			// If the gameobject is an avatar then make it the currently selected avatar
            // for the information shown on the HUD
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                var avatar = o.GetComponent("Avatar");
                if (avatar != null)
                {
                    // o is Avatar or Player object
                    if (selectedAvatar != avatar)
                    {
                        Debug.Log("Selecting avatar " + o.name);
                    }
                    else
                    {
                        // current selected avatar has already been selected at last round.
                        return;
                    }
                    selectedAvatar = avatar.gameObject;
                    removeSelectedSpinner();
                    removeSpinner();
                    addSelectedSpinner(o);

                } else {
//                    if (selectedAvatar != null) {
//                        Debug.Log("Selecting none (object was=" + o.name +")");
//                        removeSelectedSpinner();
//                    }
//                    selectedAvatar = null;
                    
                }
            }
        } else {
            // Also select no avatar if there was no object
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                removeSelectedSpinner();
            }
            removeSpinner();
        }
		
        // Ideally we'd put this somewhere that responds to resolution changes.
        reticuleTexture.pixelInset = new Rect (Screen.width/2, Screen.height/2, 16, 16); 
        
    }
    
    private Vector2 scrollPosition = new Vector2(0.0f,0.0f);
    void ActionWindow (int id)
    {
        scrollPosition = GUILayout.BeginScrollView (scrollPosition);
        GUIStyle commandStyle = new GUIStyle();

        // Use the selected avatar, or use the player object if no avatar selected
        GameObject avatar = selectedAvatar;
//        if (selectedAvatar == null)	{ avatar = player; }
        if (avatar == null) { 
			
			// End the scrollview we began above.
        	GUILayout.EndScrollView();
			return; 
		
		}
        
		ActionManager am = avatar.GetComponent<ActionManager>();
		
        lock(am.currentActions) {
        foreach (ActionKey akey in am.currentActions.Keys) {
            ActionSummary a = am.currentActions[akey] as ActionSummary;
            if (a.playerVisible) {
                commandStyle.normal.textColor = Color.green;
            } else {
                commandStyle.normal.textColor = Color.white;				
            }
            GUILayout.Label (a.actionObject.name + ":" + a.actionName,commandStyle);
                
        }
        }
        // End the scrollview we began above.
        GUILayout.EndScrollView();
    }

    // the skin panel will use
    public GUISkin panelSkin;
    
    // style for label
    private GUIStyle boxStyle;
    
    // A map from feeling names to textures. The texture needs to be created dynamically
    // whenever a new feeling is added.
    private Dictionary<string, Texture2D> feelingTextureMap;
    
    // A map from demand names to textures. The texture needs to be created dynamically
    // whenever a new demandis added.
    private Dictionary<string, Texture2D> demandTextureMap;	
    
    // We need to initialize the feeling to texture map at the first time of obtaining the
    // feeling information.
    private bool isFeelingTextureMapInit = false;
    
    // We need to initialize the demand to texture map at the first time of obtaining the
    // demand information.
    private bool isDemandTextureMapInit = false;
    
    private OCConnector connector;

    private bool showPsiPanel = false;
    private Rect panel;
    private Vector2 panelScrollPosition;
    private float feelingBoxWidth;
    private float demandBoxWidth; 

    private void ShowPsiPanel()
    {
        showPsiPanel = true;
    }

    private void HidePsiPanel()
    {
        showPsiPanel = false;
    }	
    
    /**
     * Retrieve feelings stored in OCConnector and draw bars in the panel 
     */
    private void showFeelings()
    {
        Dictionary<string, float> feelingValueMap = connector.FeelingValueMap;
        
        //panelScrollPosition = GUILayout.BeginScrollView(scrollPosition);
        feelingBoxWidth = panel.width * 0.58f;
        
        // Display feeling levels
        if (feelingValueMap.Count == 0 ) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Waiting for feeling update...", GUILayout.MaxWidth(panel.width));
            GUILayout.EndHorizontal();
        } else {
            lock (feelingValueMap)
            {
                foreach (string feeling in feelingValueMap.Keys)
                {
                    float value = feelingValueMap[feeling];
                    // Dark color is for a contrast to the actual feeling level
                    Color dark = new Color(0,0,0,0.6f);

                    // Just use green for now. Perhaps later we should
                    // have a configurable color for each feeling
                    Color c = new Color(0, 255, 0, 0.6f);
					
					// Remove old bar texture and create a new one, because each frame, 
					// the unity will rearrange size and position of everytning shown on the screen
					if (feelingTextureMap.ContainsKey(feeling))
						Destroy(feelingTextureMap[feeling]);
					feelingTextureMap[feeling] = constructBarTexture(value,(int)feelingBoxWidth,c,dark);
					
	                // Set the texture of background.
	                boxStyle.normal.background = feelingTextureMap[feeling];
				
					// Show the label and bar for the feeling
	                GUILayout.BeginHorizontal();
	                GUILayout.Label(feeling + ": ", panelSkin.label, GUILayout.MaxWidth(panel.width * 0.4f));
					GUILayout.Box("", boxStyle, GUILayout.Width(feelingBoxWidth), GUILayout.Height(16));
	                GUILayout.EndHorizontal();
	            }
				
				GUILayout.Space(16f);
				
	            // We only need to initialize the map at the first time.
	            if (!isFeelingTextureMapInit) isFeelingTextureMapInit = true;
	        }// lock
		}// if			
	}
	
	/**
	 * Retrieve demands stored in OCConnector and draw bars in the panel 
	 */	
	private void showDemands()
	{
        Dictionary<string, float> demandValueMap = connector.DemandValueMap;
        string currentDemandName = connector.CurrentDemandName;  		
        
        demandBoxWidth = panel.width * 0.58f;
                
        // Display demand satisfactions (i.e. truth values)
        if (demandValueMap.Count == 0 ) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Waiting for demand update...", GUILayout.MaxWidth(panel.width));
            GUILayout.EndHorizontal();
        } else {
            lock (demandValueMap)
            {
                foreach (string demand in demandValueMap.Keys)
                {
                    float value = demandValueMap[demand];
                    // Dark color is for a contrast to the actual feeling level
                    Color dark = new Color(0,0,0,0.6f);

                    // Just use blue for now. Perhaps later we should
                    // have a configurable color for each demand					
                    Color c = (currentDemandName==demand)? new Color(255, 0, 0, 0.6f): 
                                                           new Color(0, 0, 255, 0.6f);
                    
                    if (demandTextureMap.ContainsKey(demand))
                        Destroy(demandTextureMap[demand]);
                    demandTextureMap[demand] = constructBarTexture(value,(int)demandBoxWidth,c,dark);
                    
                    // Set the texture of background.
                    boxStyle.normal.background = demandTextureMap[demand];
                    
                    // Draw the label and bar for the demand
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(demand + ": ", panelSkin.label, GUILayout.MaxWidth(panel.width * 0.4f));
                    GUILayout.Box("", boxStyle, GUILayout.Width(demandBoxWidth), GUILayout.Height(16));
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.Space(16f); 
                
                // We only need to initialize the map at the first time.
                if (!isDemandTextureMapInit) isDemandTextureMapInit = true;
            }// lock
        }// if
    }
    
    private void PsiPanel(int id)
    {		
        // Set box style based on skin
        if (panelSkin != null)
            boxStyle = panelSkin.box;
        else
            boxStyle = new GUIStyle();
        
        GUILayout.BeginVertical();
        
        this.showDemands(); 
        this.showFeelings(); 	
        
        GUILayout.EndVertical();
    }
    
    Texture2D constructBarTexture(float x, int width, Color main, Color background)
    {
        Texture2D t = new Texture2D(width, 16);
        int i = 0;
        for (; i < width * x; i++)
            for (int j = 0; j < 16; j++)
                t.SetPixel(i, j, main);
        for (; i < width; i++)
            for (int j = 0; j < 16; j++)
                t.SetPixel(i, j, background);
        t.Apply();
        return t;
    }
	
	public bool IsShowForcePanel()
	{
		return isShowForcePanel;
	}
	
	public IEnumerator gettingForceFromHud(SocialInteractioner currentSoInter)
	{
		if (currentSoInter == null)
		{
			Debug.LogError("To show the force panel but the SocialInteractioner is null!");
			yield break;
		}
		soInter = currentSoInter;
		showForcePanel();
		while(true)
		{
			yield return new WaitForSeconds(0.1f);
			if (!isShowForcePanel)
				break;
		}
		yield break;
	}
	
	private void  showForcePanel()
	{
		isShowForcePanel = true;
		isForceIncrease = true;
		currentForce = 1.0f;
		lastTimeRenderForcePanel = Time.time;	
	}
	
	
	public void hideForcePanel()
	{
		isShowForcePanel = false;
	}
	
	public float getCurrentForceVal()
	{
		return currentForce;
	}
	
	
	private void constructForceTexture()
	{
		float w = WIDTH;
		float h = HEIGHT;
		float top = Screen.height/2;
		float left = Screen.width/2;
		
		barTex = new Texture2D((int)w,(int)h);
		
		for (int i = 0; i<w;i++)
			for(int j =0; j<h;j++)
				barTex.SetPixel(i,j,new Color(((float)j)/h,1.0f - ((float)j)/h ,0.2f,1.0f));
		
		barTex.Apply();

		bgTex = new Texture2D((int)w,(int)h);
				
		for (int i = 0; i<w;i++)
			for(int j =0; j<h;j++)
				bgTex.SetPixel(i,j,new Color(0.14f,0.15f,0.16f,0.7f));
		
		bgTex.Apply();
	}
	
	private void displayForcePanel(float currentforce)
	{
		//Color bgColor = GUI.backgroundColor;
		float w = WIDTH;
		float h = HEIGHT;
		float top = Screen.height/2;
		float left = Screen.width/2;
		
		GUI.DrawTexture(new Rect(left,top ,w, h ), bgTex);
		GUI.Label(new Rect (left,top + 20.0f ,60.0f,20.0f), maxForce.ToString());
		GUI.Label(new Rect (left,top + (h - 20.0f) ,60.0f,20.0f), minForce.ToString());
		float forceHeight = currentforce/maxForce * (h - 40.0f);
		
		GUI.DrawTexture(new Rect(left+20.0f,top+(h - 20.0f - forceHeight) ,w/2,forceHeight),barTex,ScaleMode.ScaleAndCrop);
		GUI.Label(new Rect (left+ 30.0f,top + h/2 ,100.0f,20.0f),"Force=" + currentforce.ToString());
		
		
	}
	
	public float resetHeight = 1.0f;
    
    void addSelectedSpinner(GameObject o) {
        Transform t = (Transform) Instantiate(selectedIndicator);
        t.parent = o.transform;
        t.name = "SelectionSpinner";
        lastSelected = o;
    }
    
    void removeSelectedSpinner() {
        GameObject o = lastSelected;
        if (o == null) return;
        Debug.Log("Remove spinner from object " + o);
        Transform t = o.transform.FindChild("SelectionSpinner");
        Debug.Log("Found transform t=" + t);
        if (t != null) GameObject.Destroy(t.gameObject);
        lastSelected = null;
    }
    
    void addSpinner(GameObject o) {
        Transform t = (Transform) Instantiate(highlightIndicator);
        t.parent = o.transform;
        t.name = "HighlightSpinner";
        lastHighlighted = o;
        if (o.tag == "OCObject")
        {
            OCBehaviour ocb = o.GetComponent("OCBehaviour") as OCBehaviour;
            showOCObjectInfo = true;
        }
    }
    
    void removeSpinner() {
        GameObject o = lastHighlighted;
        if (o == null) return;
        Debug.Log("Remove spinner from object " + o);
        Transform t = o.transform.FindChild("HighlightSpinner");
        Debug.Log("Found transform t=" + t);
        if (t != null) GameObject.Destroy(t.gameObject);
        if (o.tag == "OCObject")
        {
            OCBehaviour ocb = o.GetComponent("OCBehaviour") as OCBehaviour;
            showOCObjectInfo = false;
        }
        lastHighlighted = null;
    }
	
	public void setCharacterControl(bool t) {
        // Enable/disable the ability
        GameObject playerObject = selectedAvatar;
        bool success = false;
        if (playerObject) {
            CharacterGridMotor cm = playerObject.GetComponent("CharacterGridMotor") as CharacterGridMotor;
            if (cm) {
                cm.enabled = t;
                success = true;
            }
//            MonoBehaviour mouseLookX = playerObject.GetComponent("MouseLook") as MonoBehaviour;
//            MonoBehaviour mouseLookY = playerObject.transform.Find("Main_Camera").GetComponent("MouseLook") as MonoBehaviour;
//            mouseLookX.enabled = t;
//            mouseLookY.enabled = t;

        }
        if (!success) {
            Debug.LogError("No GameObject with 'Player' tag to steal control from");
        }
        Screen.lockCursor = t;
        Screen.showCursor = !t;
    }
    
    void OnGUI ()
    {
		
		
        // left inset, bottom right 
        int inset = 3;
        int width = 200;
        Rect window = new Rect( inset, Screen.height - Screen.height/3 - inset, width, Screen.height/3 );
        
        if (selectedAvatar == null)	{
            GUILayout.Window(2, window, ActionWindow, "Your Action List");
        } 
        else 
        {
            GUILayout.Window(2, window, ActionWindow, selectedAvatar.transform.name + "'s Action List");

            // Psi (feeling, demand etc.) panel controlling section
            if (selectedAvatar.tag == "OCA")
            {
                connector = selectedAvatar.GetComponent<OCConnector>() as OCConnector;
                if (connector != null)
                    ShowPsiPanel();
                // If the avatar has no connector it's a puppet Avatar controlled by the console only
                if (panelSkin != null)
                    GUI.skin = panelSkin;
            }
            else
            {
                HidePsiPanel();
                connector = null;
            }
            
            if (showPsiPanel)
            {
                if (connector != null)
                {
                    float theWidth = Screen.width * 0.25f;
                    float theHeight = Screen.height / 3;
                    panel = new Rect(Screen.width - theWidth - inset, Screen.height - theHeight - inset, theWidth, theHeight);
                    GUILayout.Window(3, panel, PsiPanel, selectedAvatar.transform.name + "'s Psi States Panel",
                                     GUILayout.MinWidth (theWidth), GUILayout.MinHeight (theHeight));
                }
            }
            
        }// if

        WorldGameObject world = GameObject.Find("World").GetComponent<WorldGameObject>();
        GUIStyle theStyle = new GUIStyle();
		// try to indicate the type of block we have stored
        theStyle.normal.background = world.storedBlockTexture;
        GUI.Box(new Rect(8, 8, 40, 40), "");
        GUI.Box(new Rect(12,12,32,32), "", theStyle);
		
		// the force is looping between minForce and maxForce, util the mouse button released
		if (isShowForcePanel)
		{
			float currentTime = Time.time;
			float deltaForce = (currentTime - lastTimeRenderForcePanel)* maxForce;
			if (isForceIncrease)
			{
				currentForce += deltaForce;
				if (currentForce > maxForce)
				{
					currentForce = maxForce;
					isForceIncrease = false;
				}
			}
			else
			{
				currentForce -= deltaForce;
				if (currentForce < minForce)
				{
					currentForce = minForce;
					isForceIncrease = true;
				}
			}
			
			displayForcePanel(currentForce);
			
			lastTimeRenderForcePanel = currentTime;
			
			return;
		}
		
        if (showGUI) {
            if (!interactMenuVisible) setCharacterControl(false);
            // Show the available actions in a radial menu
            
            // Find some variables of interest so that we don't calculate
            // them for every action.
            // centre of the screen:
            float centre_x = Camera.main.pixelWidth/2;
            float centre_y = Camera.main.pixelHeight/2;

            // filter out actions that shouldn't be player visible
			ActionManager am = selectedAvatar.GetComponent<ActionManager>();
            var playerActions = from x in am.currentActions.Keys.Cast<ActionKey>()
                                where (am.currentActions[x] as ActionSummary).playerVisible
                                select (am.currentActions[x] as ActionSummary);

            // how far to split action buttons
            float degreeSplit = (Mathf.PI / 2.0f) / playerActions.Count();
            
            int i = 0;
            
            // radius from centre
            float radius = 100.0f;
            
            // Hacky copy of the ActionSummary list to avoid out of sync exceptions
            List<ActionSummary> playerActionsCopy = new List<ActionSummary>();
            foreach (ActionSummary action in playerActions) {
                playerActionsCopy.Add(action);
            }
            ///
            foreach (ActionSummary action in playerActionsCopy) {
                //GameObject actionObject = action.actionObject;
                // Start at 45 degrees and rotate
                float theta = (Mathf.PI / 4.0f) + (i * degreeSplit);
                float x = centre_x + (Mathf.Sin(theta) * radius);
                float y = centre_y - (Mathf.Cos(theta) * radius);
                if (GUI.Button (new Rect (x,y,180,20), action.actionObject.name + ":" + action.actionName)) {
                    //Debug.Log("you clicked on " + action.actionName);
                    
                    am.doAction(action.getKey(), new ArrayList(), actionComplete);
                }
                i++;
                
            }

            interactMenuVisible = true;
        } else if (interactMenuVisible) {
            // interactMenuVisible is used to avoid calling this unnecesarily  
            //@TODO:Set new character controller
			setCharacterControl(true);
            interactMenuVisible = false;
        } else if (showOCObjectInfo)
        {
            if (lastHighlighted == null)
                return;
            if (lastHighlighted.tag != "OCObject")
                return;
            
            Color _textColor = GUI.contentColor;
            GUI.contentColor = Color.yellow;
            
            float x = Camera.main.pixelWidth/2 + 20;
            float y = Camera.main.pixelHeight/2 + 20;

            GUI.Label(new Rect(x, y, 200, 20), lastHighlighted.name + " : " + lastHighlighted.GetInstanceID());
            
            OCBehaviour[] ocbs =  lastHighlighted.GetComponents<OCBehaviour>();
            
            if (ocbs.Length != 0)
            {
                y += 5.0f;
                
                GUI.contentColor = Color.grey;
                y += 23.0f;
                GUI.Label(new Rect (x,y,200,20), "Action list:");
                GUI.contentColor = Color.green;
                foreach(OCBehaviour ocb in ocbs)
                {
                    foreach(string actionName in ocb.myActionList)
                    {
                        y += 23.0f;
                        GUI.Label(new Rect (x,y,200,20), actionName);
                        
                    }
                    
                }
            }
            GUI.contentColor = _textColor;
        }
		else
		{
			//@TODO:Set new character controller
			setCharacterControl(true);
		}
    }
	
	void actionComplete(ActionResult ar) 
    {
        Debug.Log("Action " + ar.action.actionName + " complete");
    }
    
}

