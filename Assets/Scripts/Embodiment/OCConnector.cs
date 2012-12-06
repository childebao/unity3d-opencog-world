using UnityEngine;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Text;
using System.IO;
using System.Reflection;
using ProtoBuf;
using Embodiment;

///
///  OCConnector exists as a component of virtual avatar(NPC).
///  Each NPC in the game can communicate with OC embodiment system
///  and feedback human player by using this connector.
///           
///  OCConnector reuses some methods from the original PetaverseProxy,
///  but is intended to be designed with a more flexible and distributed
///  architecture. Each agent is autonomous and has it's own connection
///  to the Router.
///
public class OCConnector : NetworkElement
{
    public String settingsFilename = "Assets/Configuration/embodiment.config";

    #region Private Variables
    private WorldGameObject world;

    // Call action scheduler component to schedule and execute actions
    // in the actions list.
    private OCActionScheduler actionScheduler;
    
    // Create a customize log instance which uses Unity3D API.
    private Logger log = Logger.getInstance();
    
    // Const string for load command feedback.
    private readonly string SUCCESS_LOAD = "SUCCESS LOAD";
    private readonly string SUCCESS_UNLOAD = "SUCCESS UNLOAD";

    // Basic attributes of this avatar.
    private string myBaseId;    /** For example "NPC" */
    private string myId;        /** For example "AVATAR_NPC" */
    private string myBrainId;   /** For example "OAC_NPC" */
    private string myName;		/** Not yet been used now */
    private string myType;		/** "pet" type by default. */
    private string myTraits;	/** "princess" by default. */
    
    // Flag to check if the OAC to this avatar is alive.
    private bool isOacAlive = false;

    // Define master's(owner's) information.
    private string masterId;
    private string masterName;

    // The queue used to store the messages to be sent to OAC.
    private List<Message> messagesToSend = new List<Message>();
    
    // The lock object used to sync the atomic sequence: 
    // - get a timestamp, build and enqueue a message
    private System.Object messageSendingLock = new System.Object();
    
    // Timer to send message in a given interval: 
    // We can implement a timer by using unity API - FixedUpdate().
    private float messageSendingTimer = 0.0f;		/**< The timer used to control message sending. */
    private float messageSendingInterval = 0.1f;	/**< The interval to send messages in the queue. */

    
    // the name of the current map
	private string mapName;
	
    // The size of map.
    private uint globalPositionOffsetX;
	private uint globalPositionOffsetY;
	private uint globalPositionOffsetZ;
	
    // Map global X beginning position.
    private int globalPositionX;

    // Map global Y beginning position.
    private int globalPositionY;
	
	// Map global Z beginning position.
    private int globalPositionZ;

    // Floor height in the minecraft-like world.
    private int globalFloorHeight;

    // Flag to check if a valid map info(which should contain the info of 
    // the avatar itself) has been sent to OAC as a "handshake".
    private bool isFirstSentMapInfo;
    
    // The list of actions that I am going to perform.
    // The action plans are received from OAC.
    private LinkedList<MetaAction> actionsList;
    
    // The counter for actions executed. This counter is cycled.
    // Therefore, repetition is allowed. 
    private long actionTicket;
    
    // The lock used to avoid duplicated action tickets.
    private System.Object actionTicketLock = new System.Object();
    

    // Currently selected demand name.
    private HashSet<long> ticketsToDrop;
    
    // The map from ticket to action, each action has its unique ticket.
    private Dictionary<long, MetaAction> ticketToActionMap;
    
    // The map from ticket to action plan, each action plan contains a sequence
    // of actions.
    private Dictionary<long, string> ticketToPlanIdMap;

    // The action plan id that is being performed currently.
    private string currentPlanId;

    // Currently selected demand name 
    private string currentDemandName;

    // Store the feeling value of this avatar.
    private Dictionary<string, float> feelingValueMap;

    // Store the demand value of this avatar.  
    private Dictionary<string, float> demandValueMap;

    // Other OC agents percepted. Record the pairs of their unity object id and brain id.
    private Dictionary<int, string> perceptedAgents;
	
	private int stateChangeActionCount = 0;
	private int disappearActionCount = 0;
	private int appearActionCount = 0;
	private int moveActionCount = 0;
	
    #endregion

    private int msgCount = 0;
    /**
     * Check if this avatar has been initialized already.
     * (e.g. established the connection to router.)
     *
     * @return true if initialized 
     */
    public bool IsInit()
    {
        return this.isOacAlive;
    }

    /**
     * Send messages in the message queue. This method is called in
     * each message sending interval. For now, it is called in FixedUpdate()
     * method.
     */
    public void sendMessages()
    {
        if (!this.isOacAlive)
            return;

        if (this.messagesToSend.Count > 0)
        {
            List<Message> localMessagesToSend;
            // copy messages to a local queue and clear the global sending queue.
            lock (this.messageSendingLock) {
                localMessagesToSend = new List<Message>(this.messagesToSend);
                this.messagesToSend.Clear();
            } // lock

            foreach (Message message in localMessagesToSend) {
                // Check if router and destination is available. If so, send the message. 
                // otherwise just ignore the message
                string routerId = config.get("ROUTER_ID", "ROUTER");
                if (!isElementAvailable(routerId) ) {
                    log.Warn("Router not available. Discarding message to '" +
                                 message.To + "' of type '" + message.Type + "'.");
                    continue;
                }
                if (!isElementAvailable(message.To) ) {
                    log.Warn("Destination not available. Discarding message to '" +
                                 message.To + "' of type '" + message.Type + "'.");
                    continue;
                }
                if (sendMessage(message)) {
                    log.Debugging("Message from '" + message.From + "' to '" +
                                 message.To + "' of type '" + message.Type + "'.");
                } else {
                    log.Warn("Error sending message from '" + message.From + "' to '" +
                                 message.To + "' type '" + message.Type + "'.");
                }
            } // foreach
        }
        
    }
    
    /**
     * Return a current time stamp with a specific format.
     */
    public static string getCurrentTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
    }

    /**
     * Process messages that are delivered by router, overriding the method in
     * NetworkElement.
     *
     * @param message A message to be handled.
     *
     * @return false if not an exit command. (this is obsolete in this unity game, but
     * it is OK to keep it)
     */
    public override bool processNextMessage(Message message)
    {
        log.Debugging(message.getPlainTextRepresentation());
    
        if (message.Type == Message.MessageType.FEEDBACK)
        {
            // e.g. we can append the information in the console.
            log.Error("Feedback " + message.getPlainTextRepresentation());
        }
        else if (message.getPlainTextRepresentation().StartsWith(SUCCESS_LOAD))
        {
            // Format: SUCCESS LOAD NetworkElement_id avatar_id
            char[] separator = { ' ' };
            string[] tokens = message.getPlainTextRepresentation().Split(separator, StringSplitOptions.RemoveEmptyEntries);
            
            string neId = tokens[2];
            log.Info("Successfully loaded '" + neId + "'.");
            isOacAlive = true;
        }
        else if (message.getPlainTextRepresentation().StartsWith(SUCCESS_UNLOAD))
        {
            char[] separator = {' '};
            string[] tokens = message.getPlainTextRepresentation().Split(separator, StringSplitOptions.RemoveEmptyEntries);

            // Format: SUCCESS UNLOAD NetworkElement_id avatar_id
            string neId = tokens[2];
            log.Info("Successfully unloaded '" + neId + "'.");
            isOacAlive = false;
        }
        else
        {
            // Get the plain text of this message(in XML format) and parse it.
            if (isOacAlive)
            {
                // Parse the message only when oac is ready.
                parseXML(message.getPlainTextRepresentation());
            }
        }
        return false;
    }

    /**
     * Parse the xml information contained in a message.
     *
     * @param xmlText xml text to be parsed
     */
    private void parseXML(string xmlText)
    {
        XmlDocument document = new XmlDocument();
        try
        {
            document.Load(new StringReader(xmlText));
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
        parseDOMDocument(document);
    }

    /**
     * Parse a DOM document.
     * 
     * @param document XML document to be parsed
     */
    private void parseDOMDocument(XmlDocument document)
    {
        // Handles action-plans 
        XmlNodeList list = document.GetElementsByTagName(EmbodimentXMLTags.ACTION_PLAN_ELEMENT);
        for (int i = 0; i < list.Count; i++)
        {
            parseActionPlanElement((XmlElement)list.Item(i));
        }

        // Handles emotional-feelings       
        XmlNodeList feelingsList = document.GetElementsByTagName(EmbodimentXMLTags.EMOTIONAL_FEELING_ELEMENT);
        for (int i = 0; i < feelingsList.Count; i++)
        {
            parseEmotionalFeelingElement((XmlElement)feelingsList.Item(i));
        }
        
        // Handle psi-demand
        XmlNodeList demandsList = document.GetElementsByTagName(EmbodimentXMLTags.PSI_DEMAND_ELEMENT); 
        for (int i = 0; i< demandsList.Count; i++)
        {
            parsePsiDemandElement((XmlElement)demandsList.Item(i)); 
        }
		
		// Handle single-action-command
	    XmlNodeList singleActionList = document.GetElementsByTagName(EmbodimentXMLTags.SINGLE_ACTION_COMMAND_ELEMENT); 
        for (int i = 0; i< singleActionList.Count; i++)
        {
            parseSingleActionElement((XmlElement)singleActionList.Item(i)); 
        }
    }

    private void parseEmotionalFeelingElement(XmlElement element)
    {
        string avatarId = element.GetAttribute(EmbodimentXMLTags.ENTITY_ID_ATTRIBUTE);

        // Parse all feelings and add them to a map.
        XmlNodeList list = element.GetElementsByTagName(EmbodimentXMLTags.FEELING_ELEMENT);
        for (int i = 0; i < list.Count; i++)
        {
            XmlElement feelingElement = (XmlElement)list.Item(i);
            string feeling = feelingElement.GetAttribute(EmbodimentXMLTags.NAME_ATTRIBUTE);
            float value = float.Parse(feelingElement.GetAttribute(EmbodimentXMLTags.VALUE_ATTRIBUTE));

            // group all feelings to be updates only once
            feelingValueMap[feeling] = value;

            log.Debugging("Avatar[" + this.myId + "] -> parseEmotionalFeelingElement: Feeling '" + feeling + "' value '" + value + "'.");
        }

        // Update feelings of this avatar.
        updateEmotionFeelings();
    }
    
    private void parsePsiDemandElement(XmlElement element)
    {
        string avatarId = element.GetAttribute(EmbodimentXMLTags.ENTITY_ID_ATTRIBUTE);

        // Parse all demands and add them to a map.
        XmlNodeList list = element.GetElementsByTagName(EmbodimentXMLTags.DEMAND_ELEMENT);
        for (int i = 0; i < list.Count; i++)
        {
            XmlElement demandElement = (XmlElement)list.Item(i);
            string demand = demandElement.GetAttribute(EmbodimentXMLTags.NAME_ATTRIBUTE);
            float value = float.Parse(demandElement.GetAttribute(EmbodimentXMLTags.VALUE_ATTRIBUTE));

            // group all demands to be updated only once
            demandValueMap[demand] = value;

            log.Debugging("Avatar[" + this.myId + "] -> parsePsiDemandElement: Demand '" + demand + "' value '" + value + "'.");
        }
    }	
	
	// this kind of message is from the opencog, and it's not a completed plan, 
	// but a single action that current the robot want to do
	private void parseSingleActionElement(XmlElement element)
	{
		Avatar oca = gameObject.GetComponent<Avatar>() as Avatar;
		if (oca == null)
			return;
		
	    string actionName = element.GetAttribute(EmbodimentXMLTags.NAME_ATTRIBUTE);
        if (actionName == "BuildBlockAtPosition")
		{
			int x = 0,y = 0,z = 0;
			XmlNodeList list = element.GetElementsByTagName(EmbodimentXMLTags.PARAMETER_ELEMENT);
	        for (int i = 0; i < list.Count; i++)
	        {
	            XmlElement paraElement = (XmlElement)list.Item(i);
				string paraName = paraElement.GetAttribute(EmbodimentXMLTags.NAME_ATTRIBUTE);
				if (paraName == "x")
					x = int.Parse(paraElement.GetAttribute(EmbodimentXMLTags.VALUE_ATTRIBUTE));
				else if (paraName == "y")
					y = int.Parse(paraElement.GetAttribute(EmbodimentXMLTags.VALUE_ATTRIBUTE));
				else if (paraName == "z")
					z = int.Parse(paraElement.GetAttribute(EmbodimentXMLTags.VALUE_ATTRIBUTE));
				
	        }
			IntVect blockBuildPoint = new IntVect(x, y, z);
			
			oca.BuildBlockAtPosition(blockBuildPoint);
		}
		else if (actionName == "MoveToCoordinate")
		{
			int x = 0,y = 0,z = 0;
			XmlNodeList list = element.GetElementsByTagName(EmbodimentXMLTags.PARAMETER_ELEMENT);
	        for (int i = 0; i < list.Count; i++)
	        {
	            XmlElement paraElement = (XmlElement)list.Item(i);
				string paraName = paraElement.GetAttribute(EmbodimentXMLTags.NAME_ATTRIBUTE);
				if (paraName == "x")
					x = int.Parse(paraElement.GetAttribute(EmbodimentXMLTags.VALUE_ATTRIBUTE));
				else if (paraName == "y")
					y = int.Parse(paraElement.GetAttribute(EmbodimentXMLTags.VALUE_ATTRIBUTE));
				else if (paraName == "z")
					z = int.Parse(paraElement.GetAttribute(EmbodimentXMLTags.VALUE_ATTRIBUTE));
				
	        }
			Vector3 vec = new Vector3(x,z,y);
			oca.MoveToCoordinate(vec);
		}
 
	}
	

    /**
     * TODO This function is supposed to receive emotion information from OpenPsi
     * and update local avatar's emotion.(e.g. facial expression?)
     */
    private void updateEmotionFeelings()
    {
        OCEmotionalExpression emotionalExpression = gameObject.GetComponent<OCEmotionalExpression>() as OCEmotionalExpression;
        emotionalExpression.showEmotionExpression(this.FeelingValueMap);   

    }
    
    /**
     * Parse action plan and append the result into action list.
     *
     * @param element meta action in xml format
     */
    private void parseActionPlanElement(XmlElement element)
    {
		Debug.Log("/////////////// In Parse Action Plan Element! //////////////////");
		
        // Get the action performer id.
        string avatarId = element.GetAttribute(EmbodimentXMLTags.ENTITY_ID_ATTRIBUTE);
        if (avatarId != this.myBrainId)
        {
            // Usually this would not happen.
            log.Warn("Avatar[" + this.myId + "]: This action plan is not for me.");
            return;
        }
 
        // Cancel current action and clear old action plan in the list.
        if (this.actionsList.Count > 0)
        {
            log.Warn("Stop all current actions");
            cancelAvatarActions();
        }
        
        // Update current plan id and selected demand name.
        this.currentPlanId = element.GetAttribute(EmbodimentXMLTags.ID_ATTRIBUTE);
        this.currentDemandName = element.GetAttribute(EmbodimentXMLTags.DEMAND_ATTRIBUTE); 
        
        XmlNodeList list = element.GetElementsByTagName(EmbodimentXMLTags.ACTION_ELEMENT);
        LinkedList<MetaAction> actionPlan = new LinkedList<MetaAction>();
        for (int i = 0; i < list.Count; i++)
        {
            MetaAction avatarAction = MetaAction.Factory((XmlElement)list.Item(i), true);

            actionPlan.AddLast(avatarAction);
        }

        lock (this.actionsList)
        {
            this.actionsList = actionPlan;
        }
        // Start to perform an action in front of the action list.
        this.actionScheduler.SendMessage("receiveActionPlan", actionPlan);
        //processNextAvatarAction();
    }
    
    /**
     * Cancel current action and clear actions from previous action plan.
     */
    private void cancelAvatarActions()
    {
        if(this.actionsList.Count > 0)
            lock (this.actionsList)
            {
                this.actionsList.Clear();
            }
        
        // Ask action scheduler to stop all current actions.
        this.actionScheduler.SendMessage("cancelCurrentActionPlan");
        this.sendActionStatus(this.currentPlanId, false);
    }

    /**
     * Send the physiological information of this avatar to OAC with a tick message
     * for OAC to handle it.
     * This method would be invoked by physiological model.
     */
    private void sendAvatarSignalsAndTick(Dictionary<string, double> physiologicalInfo)
    {
        string timestamp = getCurrentTimestamp();
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);
        
        // (currently this is avatar-signal, but should be changed...)
        XmlElement avatarSignal = (XmlElement)root.AppendChild(doc.CreateElement("avatar-signal"));
        avatarSignal.SetAttribute("id", this.myBrainId);
        
        avatarSignal.SetAttribute("timestamp", timestamp);
        
        // Append all physiological factors onto the message content.
        foreach (string factor in physiologicalInfo.Keys)
        {
            // <physiology-level name="hunger" value="0.3"/>   
            XmlElement p = (XmlElement)avatarSignal.AppendChild(doc.CreateElement("physiology-level"));
            p.SetAttribute("name", factor);
            p.SetAttribute("value", physiologicalInfo[factor].ToString(CultureInfo.InvariantCulture.NumberFormat));
        }
        
        string xmlText = BeautifyXmlText(doc);
        //log.Debugging("OCConnector - sendAvatarSignalsAndTick: " + xmlText);
            
        // Construct a string message.
        StringMessage message = new StringMessage(this.myId, this.myBrainId, xmlText);

        lock (messageSendingLock)
        {
            // Add physiological information to message sending queue.
            messagesToSend.Add(message);

            // Send a tick message to make OAC start next cycle.
            if (bool.Parse(config.get("GENERATE_TICK_MESSAGE")))
            {
                Message tickMessage = new TickMessage(this.myId, this.myBrainId);
                messagesToSend.Add(tickMessage);
            }
        }
    }

    public void handleActionResult(ActionResult ar, MetaAction action)
    {
        bool result = (ar.status == ActionResult.Status.SUCCESS ? true : false);
        
        // Send action status to my brain.
        this.sendActionStatus(this.currentPlanId, action, result);
        //Debug.LogWarning("Action plan " + this.currentPlanId + " sequence " + action.Sequence + " status sent.");
        lock (this.actionsList)
        {
            this.actionsList.Remove(action);
        }
    }
    
    public void handleOtherAgentActionResult(ActionResult ar)
    {
        // don't report actions that game from us.
        // don't report actions without an action summary (these are from trying
        // to do non-existant actions).
        if (ar.avatar == gameObject.GetComponent<Avatar>() || ar.action == null) {
            //Debug.LogWarning("skipping action result from " + ar.avatar);
            return;
        }
		
        // the corresponding process within OpenCog's embodiment system is in PAI::processAgentActionWithParameters
		
        string timestamp = getCurrentTimestamp();
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);

        XmlElement agentSignal = (XmlElement) root.AppendChild(doc.CreateElement("agent-signal"));
        agentSignal.SetAttribute("id", ar.avatar.gameObject.GetInstanceID().ToString());
        agentSignal.SetAttribute("type", ar.avatar.agentType);
        agentSignal.SetAttribute("timestamp", timestamp);
        XmlElement actionElement = (XmlElement)agentSignal.AppendChild(doc.CreateElement(EmbodimentXMLTags.ACTION_ELEMENT));
        
		// note that the name and the action-instance-name are different
		// ie: name = kick , while action-instance-name = kick2342
		actionElement.SetAttribute("name", ar.action.actionName);
		actionElement.SetAttribute("action-instance-name", ar.actionInstanceName);
		
		bool result = (ar.status == ActionResult.Status.SUCCESS ? true : false);
		actionElement.SetAttribute("result-state", "true"); //successful or failed
		if (ar.action.objectID == gameObject.GetInstanceID()) {
			actionElement.SetAttribute("target", this.myBrainId);
		} else {
			actionElement.SetAttribute("target", ar.action.objectID.ToString());
		}
		
		// currently we only process the avatar and ocobject type, other types in EmbodimentXMLTages can is to be added when needed.
		// if you add other types such as BLOCK_OBJECT_TYPE, you should also modify PAI::processAgentActionWithParameters in opencog
		String targetType = ar.action.actionObject.tag;
		if (targetType == "OCA" || targetType == "Player")// it's an avatar
			actionElement.SetAttribute("target-type", EmbodimentXMLTags.AVATAR_OBJECT_TYPE);
		else if (targetType == "OCObject") // it's an object
			actionElement.SetAttribute("target-type",EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
		else
			Debug.LogError("Error target type: " + targetType + " in action: " + ar.action.actionName);
				 
		// we can only process the parameter type defined in class ActionParamType both in opencog and unity
		// currently they are : boolean, int, float, string, vector, rotation, entity
		// also see opencog/opencog/embodiment/control/perceptionActionInterface/BrainProxyAxon.xsd
        ArrayList paramList = ar.parameters;
        
       if (paramList != null) {
			
			int i;
			if (targetType == "OCA" || targetType == "Player")
			{
				if (ar.action.objectID == ar.avatar.gameObject.GetInstanceID())
					i = -1;
				else 
					i = 0;
			}
            else
			{
				i = 0;
			}
            foreach (System.Object obj in paramList)
            {
                XmlElement param = (XmlElement)actionElement.AppendChild(doc.CreateElement("param"));
				
				// the first param in pinfo is usually the avator does this action, so we just skip it
				string paratype = obj.GetType().ToString();
				if (paratype == "System.Int32") // it's a int
				{
					param.SetAttribute("type", "int");
					param.SetAttribute("name", ar.action.pinfo[i+1].Name);
					param.SetAttribute("value", obj.ToString());
				}
				else if (paratype == "System.Single") // it's a float
				{
					param.SetAttribute("type", "float");
					param.SetAttribute("name", ar.action.pinfo[i+1].Name);
					param.SetAttribute("value", obj.ToString());
				}
				else if (paratype == "System.Boolean") // it's a bool
				{
					param.SetAttribute("type", "boolean");
					param.SetAttribute("name", ar.action.pinfo[i+1].Name);
				
					param.SetAttribute("value", obj.ToString().ToLower());
				}
				else if (paratype == "System.String")// it's a string
				{
					param.SetAttribute("type", "string");
					param.SetAttribute("name", ar.action.pinfo[i+1].Name);
					param.SetAttribute("value", obj.ToString());
				}
				// it's an entity, we only process the ActionTarget, 
				// if your parameter is an Entiy, please change it into ActionTarget type first
				else if (paratype == "ActionTarget") 
				{
					param.SetAttribute("type", "entity");
					param.SetAttribute("name", ar.action.pinfo[i+1].Name);
					XmlElement entityElement = (XmlElement)param.AppendChild(doc.CreateElement(EmbodimentXMLTags.ENTITY_ELEMENT));
					ActionTarget entity = obj as ActionTarget;
					entityElement.SetAttribute(EmbodimentXMLTags.ID_ATTRIBUTE, entity.id.ToString());
					entityElement.SetAttribute(EmbodimentXMLTags.TYPE_ATTRIBUTE, entity.type);
					
					// currently it seems not use of OWNER_ID_ATTRIBUTE and OWNER_NAME_ATTRIBUTE, we just skip them
				}
				else if ( paratype == "UnityEngine.Vector3") // it's an vector
				{   
					Vector3 vec = (Vector3)obj ;
					param.SetAttribute("type", "vector");
					param.SetAttribute("name", ar.action.pinfo[i+1].Name);
					XmlElement vectorElement = (XmlElement)param.AppendChild(doc.CreateElement(EmbodimentXMLTags.VECTOR_ELEMENT));
					vectorElement.SetAttribute(EmbodimentXMLTags.X_ATTRIBUTE, vec.x.ToString());
					vectorElement.SetAttribute(EmbodimentXMLTags.Y_ATTRIBUTE, vec.y.ToString());
					vectorElement.SetAttribute(EmbodimentXMLTags.Z_ATTRIBUTE, vec.z.ToString());
					
				}
				// todo: we don't have a rotation type
				else 
				{
					// we can only process the type define in ActionParamType
					continue;
				}
				              
                i++;                
            }
        }

        Message message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));
        
        log.Warn("sending action result from " + ar.avatar + "\n" + BeautifyXmlText(doc));
        
        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }
    }
	
	
	// When isAppear is true, it's an appear action, if false, it's a disappear action 
	public void handleObjectAppearOrDisappear(String objectID,String objectType, bool isAppear)
	{
		if (objectID == gameObject.GetInstanceID().ToString())
			return;
		
	    string timestamp = getCurrentTimestamp();
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);
		
        XmlElement agentSignal = (XmlElement) root.AppendChild(doc.CreateElement("agent-signal"));
        agentSignal.SetAttribute("id", objectID);
		
		String targetType;

		if (objectType == "OCA" || objectType == "Player")// it's an avatar
			targetType = EmbodimentXMLTags.AVATAR_OBJECT_TYPE;
		else // it's an object
			targetType = EmbodimentXMLTags.ORDINARY_OBJECT_TYPE;		
		
        agentSignal.SetAttribute("type", "object");
        agentSignal.SetAttribute("timestamp", timestamp);
        XmlElement actionElement = (XmlElement)agentSignal.AppendChild(doc.CreateElement(EmbodimentXMLTags.ACTION_ELEMENT));
        
		// note that the name and the action-instance-name are different
		// ie: name = kick , while action-instance-name = kick2342
		if (isAppear)
		{
			actionElement.SetAttribute("name", "appear");
			actionElement.SetAttribute("action-instance-name", "appear"+ (++appearActionCount).ToString());
		}
		else
		{
			actionElement.SetAttribute("name", "disappear");
			actionElement.SetAttribute("action-instance-name", "disappear"+ (++disappearActionCount).ToString());			
		}

		actionElement.SetAttribute("result-state", "true"); 
		
		actionElement.SetAttribute("target", objectID);
		actionElement.SetAttribute("target-type",targetType);		
		
        Message message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));
        
        log.Debugging("sending state change of " + objectID + "\n" + BeautifyXmlText(doc));
        
        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }		
		
	}
	
	public void sendMoveActionDone (GameObject obj,Vector3 startPos, Vector3 endPos)
	{
		if (obj == gameObject)
			return;
		
        string timestamp = getCurrentTimestamp();
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);

        XmlElement agentSignal = (XmlElement) root.AppendChild(doc.CreateElement("agent-signal"));
        agentSignal.SetAttribute("id", obj.GetInstanceID().ToString());
        
        agentSignal.SetAttribute("timestamp", timestamp);
        XmlElement actionElement = (XmlElement)agentSignal.AppendChild(doc.CreateElement(EmbodimentXMLTags.ACTION_ELEMENT));
        
		// note that the name and the action-instance-name are different
		// ie: name = kick , while action-instance-name = kick2342
		actionElement.SetAttribute("name", "move");
		actionElement.SetAttribute("action-instance-name", "move" + (++moveActionCount));
		actionElement.SetAttribute("result-state", "true"); //successful or failed
		
		actionElement.SetAttribute("target", obj.GetInstanceID().ToString());

		String targetType = obj.tag;
		if (targetType == "OCA" || targetType == "Player")// it's an avatar
		{
			agentSignal.SetAttribute("type", EmbodimentXMLTags.AVATAR_OBJECT_TYPE);
			actionElement.SetAttribute("target-type", EmbodimentXMLTags.AVATAR_OBJECT_TYPE);
		}
		else// it's an object
		{
			agentSignal.SetAttribute("type", EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
			actionElement.SetAttribute("target-type",EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
		}
	
        XmlElement paramOld = (XmlElement)actionElement.AppendChild(doc.CreateElement("param"));
		paramOld.SetAttribute("name", "startPosition");
        XmlElement paramNew = (XmlElement)actionElement.AppendChild(doc.CreateElement("param"));
		paramNew.SetAttribute("name", "endPosition");
		
		paramOld.SetAttribute("type", "vector");
		XmlElement oldVectorElement = (XmlElement)paramOld.AppendChild(doc.CreateElement(EmbodimentXMLTags.VECTOR_ELEMENT));
		oldVectorElement.SetAttribute(EmbodimentXMLTags.X_ATTRIBUTE, startPos.x.ToString());
		oldVectorElement.SetAttribute(EmbodimentXMLTags.Y_ATTRIBUTE, startPos.y.ToString());
		oldVectorElement.SetAttribute(EmbodimentXMLTags.Z_ATTRIBUTE, startPos.z.ToString());
		
		paramNew.SetAttribute("type", "vector");
		XmlElement newVectorElement = (XmlElement)paramNew.AppendChild(doc.CreateElement(EmbodimentXMLTags.VECTOR_ELEMENT));
		newVectorElement.SetAttribute(EmbodimentXMLTags.X_ATTRIBUTE, endPos.x.ToString());
		newVectorElement.SetAttribute(EmbodimentXMLTags.Y_ATTRIBUTE, endPos.y.ToString());
		newVectorElement.SetAttribute(EmbodimentXMLTags.Z_ATTRIBUTE, endPos.z.ToString());		
		
		Message message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));
        
        log.Debugging("sending move action result: \n" + BeautifyXmlText(doc));
        
        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }
	}
	
	// Send all the existing states of object to opencog when the robot is loaded 
	// it will be processed in opencog the same as opencog::pai::InitStateInfo()
	public void sendExistingStates(GameObject obj, String stateName, String valueType, System.Object stateValue)
	{
		string timestamp = getCurrentTimestamp();
		System.Diagnostics.Debug.Assert(obj != null && stateName != "" && valueType != "");
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);
		
		string id = obj.GetInstanceID().ToString();
				
        XmlElement StateSignal = (XmlElement) root.AppendChild(doc.CreateElement("state-info"));
        StateSignal.SetAttribute("object-id", id);

		if (obj.tag == "OCA" || obj.tag == "Player")// it's an avatar
			StateSignal.SetAttribute("object-type", EmbodimentXMLTags.AVATAR_OBJECT_TYPE);
		else // it's an object
			StateSignal.SetAttribute("object-type",EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);

        StateSignal.SetAttribute("state-name", stateName);
		StateSignal.SetAttribute("timestamp", timestamp);
		
		XmlElement valueElement = (XmlElement)StateSignal.AppendChild(doc.CreateElement("state-value"));
						
		if (valueType == "System.Int32") // it's a int
		{
			valueElement.SetAttribute("type", "int");
			valueElement.SetAttribute("value",stateValue.ToString());
		}
		else if (valueType == "System.Single") // it's a float
		{
			valueElement.SetAttribute("type", "float");
			valueElement.SetAttribute("value",stateValue.ToString());			
		}
		else if (valueType == "System.Boolean") // it's a bool
		{
			valueElement.SetAttribute("type", "boolean");
			valueElement.SetAttribute("value",stateValue.ToString().ToLower());
		}
		else if (valueType == "System.String")// it's a string
		{
			valueElement.SetAttribute("type", "string");
			valueElement.SetAttribute("value", stateValue as string);
			
		}
		else if (valueType == "UnityEngine.GameObject") 
		{
			valueElement.SetAttribute("type", "entity");
			XmlElement entityElement = (XmlElement)valueElement.AppendChild(doc.CreateElement(EmbodimentXMLTags.ENTITY_ELEMENT));
			MakeEntityElement(stateValue as GameObject, entityElement);
		}
		else if ( valueType == "UnityEngine.Vector3") // it's an vector
		{   
			valueElement.SetAttribute("type", "vector");
			Vector3 vec = (Vector3)stateValue ;
			XmlElement vectorElement = (XmlElement)valueElement.AppendChild(doc.CreateElement(EmbodimentXMLTags.VECTOR_ELEMENT));
			vectorElement.SetAttribute(EmbodimentXMLTags.X_ATTRIBUTE, vec.x.ToString());
			vectorElement.SetAttribute(EmbodimentXMLTags.Y_ATTRIBUTE, vec.y.ToString());
			vectorElement.SetAttribute(EmbodimentXMLTags.Z_ATTRIBUTE, vec.z.ToString());
		}
		
		// todo: we don't have a rotation type
		else 
		{
			// we can only process the type define in ActionParamType
			log.Warn("Unexcepted type: " + valueType + " in OCConnector::handleObjectStateChange!" );
			return;
		}


        Message message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));
        
        log.Debugging("sending state change of " + obj + "\n" + BeautifyXmlText(doc));
        
        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }		
	}	
	
	// we handle object state change as an "stateChange" action, and send it to the opencog via "agent-signal"
	// it will be processed in opencog the same as handleOtherAgentActionResult
	public void handleObjectStateChange(GameObject obj, String stateName, String valueType, System.Object oldValue, System.Object newValue,String blockId = "")
	{
		if (obj == gameObject)
			return;
		
        string timestamp = getCurrentTimestamp();
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);
		
		String id;
		if (blockId != "")
			id = blockId;
		else
			id = obj.GetInstanceID().ToString();
			
		String targetType;
		if (blockId != "")
			targetType = EmbodimentXMLTags.ORDINARY_OBJECT_TYPE;
		else
		    targetType = obj.tag;
		
        XmlElement agentSignal = (XmlElement) root.AppendChild(doc.CreateElement("agent-signal"));
        agentSignal.SetAttribute("id", id);
        
        agentSignal.SetAttribute("timestamp", timestamp);
        XmlElement actionElement = (XmlElement)agentSignal.AppendChild(doc.CreateElement(EmbodimentXMLTags.ACTION_ELEMENT));
        
		// note that the name and the action-instance-name are different
		// ie: name = kick , while action-instance-name = kick2342
		actionElement.SetAttribute("name", "stateChange");
		actionElement.SetAttribute("action-instance-name", "stateChange"+ (++stateChangeActionCount).ToString());

		actionElement.SetAttribute("result-state", "true"); 
		
		actionElement.SetAttribute("target", id);
		
		
		if (targetType == "OCA" || targetType == "Player")// it's an avatar
		{
			agentSignal.SetAttribute("type", EmbodimentXMLTags.AVATAR_OBJECT_TYPE);
			actionElement.SetAttribute("target-type", EmbodimentXMLTags.AVATAR_OBJECT_TYPE);
		}
		else // it's an object
		{
			agentSignal.SetAttribute("type", EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
			actionElement.SetAttribute("target-type",EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
		}
			
        XmlElement paramStateName = (XmlElement)actionElement.AppendChild(doc.CreateElement("param"));
		paramStateName.SetAttribute("name", "stateName");
		paramStateName.SetAttribute("type", "string");
		paramStateName.SetAttribute("value",stateName );

        XmlElement paramOld = (XmlElement)actionElement.AppendChild(doc.CreateElement("param"));
		paramOld.SetAttribute("name", "OldValue");
        XmlElement paramNew = (XmlElement)actionElement.AppendChild(doc.CreateElement("param"));
		paramNew.SetAttribute("name", "NewValue");
		
				
		if (valueType == "System.Int32") // it's a int
		{
			paramOld.SetAttribute("type", "int");
			paramOld.SetAttribute("value",oldValue.ToString());
			
			paramNew.SetAttribute("type", "int");
			paramNew.SetAttribute("value",newValue.ToString());
		}
		else if (valueType == "System.Single") // it's a float
		{
			paramOld.SetAttribute("type", "float");
			paramOld.SetAttribute("value",oldValue.ToString());
			
			paramNew.SetAttribute("type", "float");
			paramNew.SetAttribute("value",newValue.ToString());
		}
		else if (valueType == "System.Boolean") // it's a bool
		{
			paramOld.SetAttribute("type", "boolean");
			paramOld.SetAttribute("value",oldValue.ToString().ToLower());
			
			paramNew.SetAttribute("type", "boolean");
			paramNew.SetAttribute("value",newValue.ToString().ToLower());
		}
		else if (valueType == "System.String")// it's a string
		{
			paramOld.SetAttribute("type", "string");
			paramOld.SetAttribute("value", oldValue as string);
			
			paramNew.SetAttribute("type", "string");
			paramNew.SetAttribute("value", newValue as string);
		}
		else if (valueType == "UnityEngine.GameObject") 
		{
			paramOld.SetAttribute("type", "entity");
			XmlElement oldEntityElement = (XmlElement)paramOld.AppendChild(doc.CreateElement(EmbodimentXMLTags.ENTITY_ELEMENT));
			MakeEntityElement(oldValue as GameObject, oldEntityElement);
			
			paramNew.SetAttribute("type", "entity");
			XmlElement newEntityElement = (XmlElement)paramNew.AppendChild(doc.CreateElement(EmbodimentXMLTags.ENTITY_ELEMENT));
			MakeEntityElement(newValue as GameObject, newEntityElement);
		}
		else if ( valueType == "UnityEngine.Vector3") // it's an vector
		{   
			paramOld.SetAttribute("type", "vector");
			Vector3 vec = (Vector3)oldValue ;
			XmlElement oldVectorElement = (XmlElement)paramOld.AppendChild(doc.CreateElement(EmbodimentXMLTags.VECTOR_ELEMENT));
			oldVectorElement.SetAttribute(EmbodimentXMLTags.X_ATTRIBUTE, vec.x.ToString());
			oldVectorElement.SetAttribute(EmbodimentXMLTags.Y_ATTRIBUTE, vec.y.ToString());
			oldVectorElement.SetAttribute(EmbodimentXMLTags.Z_ATTRIBUTE, vec.z.ToString());
			
			paramNew.SetAttribute("type", "vector");
			vec = (Vector3)newValue ;
			XmlElement newVectorElement = (XmlElement)paramNew.AppendChild(doc.CreateElement(EmbodimentXMLTags.VECTOR_ELEMENT));
			newVectorElement.SetAttribute(EmbodimentXMLTags.X_ATTRIBUTE, vec.x.ToString());
			newVectorElement.SetAttribute(EmbodimentXMLTags.Y_ATTRIBUTE, vec.y.ToString());
			newVectorElement.SetAttribute(EmbodimentXMLTags.Z_ATTRIBUTE, vec.z.ToString());			
			
		}
		// todo: we don't have a rotation type
		else 
		{
			// we can only process the type define in ActionParamType
			log.Warn("Unexcepted type: " + valueType + " in OCConnector::handleObjectStateChange!" );
		}


        Message message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));
        
        log.Debugging("sending state change of " + obj + "\n" + BeautifyXmlText(doc));
        
        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }		
	}
	
	private void MakeEntityElement(GameObject obj, XmlElement entityElement)
	{

		if (obj == null)
		{
			entityElement.SetAttribute(EmbodimentXMLTags.ID_ATTRIBUTE, "null");
			entityElement.SetAttribute(EmbodimentXMLTags.TYPE_ATTRIBUTE, EmbodimentXMLTags.UNKNOWN_OBJECT_TYPE);
		}
		else
		{
			String targetType = obj.tag;
			if (targetType == "OCA" || targetType == "Player")// it's an avatar
			{
				entityElement.SetAttribute(EmbodimentXMLTags.ID_ATTRIBUTE, obj.GetInstanceID().ToString());
				entityElement.SetAttribute(EmbodimentXMLTags.TYPE_ATTRIBUTE, EmbodimentXMLTags.AVATAR_OBJECT_TYPE);
			}
			else // it's an object
			{
				entityElement.SetAttribute(EmbodimentXMLTags.ID_ATTRIBUTE, obj.GetInstanceID().ToString());
				entityElement.SetAttribute(EmbodimentXMLTags.TYPE_ATTRIBUTE, EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
			}
		}
	}

    private XmlElement makeXMLElementRoot(XmlDocument doc)
    {
        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", ""));

        // Create the root element named "oc:embodiment-msg"
        XmlElement root = (XmlElement)doc.AppendChild(doc.CreateElement("oc", "embodiment-msg", "http://www.opencog.org/brain"));
        XmlAttribute schemaLocation = doc.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
        schemaLocation.Value = "http://www.opencog.org/brain BrainProxyAxon.xsd";
        root.SetAttributeNode(schemaLocation);
        return root;
    }
    
    /**
     * Creates and store an action status message to be sent to an OAC.
     * This message represents the status of all actions of the given planId.
     * This method will create a agent-signal xml message like:
     * 
     * (currently this is avatar-signal, but should be changed...)
     * <agent-signal id="..." timestamp="...">
     * <action name="..." plan-id="..." sequence="..." status="..."/>
     * </agent-signal>
     * 
     * @param planId plan id
     * @param action avatar action
     * @param success action result
     */
    private void sendActionStatus(string planId, MetaAction action, bool success)
    {
        string timestamp = getCurrentTimestamp();
        // Create a xml document
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);

        XmlElement avatarSignal = (XmlElement)root.AppendChild(doc.CreateElement("avatar-signal"));
        avatarSignal.SetAttribute("id", this.myBrainId);
        avatarSignal.SetAttribute("timestamp", timestamp);
        XmlElement actionElement = (XmlElement)avatarSignal.AppendChild(doc.CreateElement(EmbodimentXMLTags.ACTION_ELEMENT));
        actionElement.SetAttribute(EmbodimentXMLTags.ACTION_PLAN_ID_ATTRIBUTE, planId);
        actionElement.SetAttribute(EmbodimentXMLTags.SEQUENCE_ATTRIBUTE, action.Sequence.ToString());
        actionElement.SetAttribute("name", action.Name);
        actionElement.SetAttribute("status", success ? "done" : "error");

        StringMessage message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));

        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }
    }
    
    /**
     * Creates and store an action status message to be sent to an OAC.
     * This message represents the status of all actions of the given planId.
     * This method will create a agent-signal xml message like:
     * 
     * (currently this is avatar-signal, but should be changed...)
     * <agent-signal id="..." timestamp="...">
     * <action name="..." plan-id="..." sequence="..." status="..."/>
     * </agent-signal>
     * 
     * @param planId plan id
     * @param success action result
     */
    private void sendActionStatus(string planId, bool success)
    {
        string timestamp = getCurrentTimestamp();
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);

        XmlElement avatarSignal = (XmlElement)root.AppendChild(doc.CreateElement("avatar-signal"));
        avatarSignal.SetAttribute("id", this.myBrainId);
        avatarSignal.SetAttribute("timestamp", timestamp);
        XmlElement actionElement = (XmlElement)avatarSignal.AppendChild(doc.CreateElement(EmbodimentXMLTags.ACTION_ELEMENT));
        actionElement.SetAttribute(EmbodimentXMLTags.ACTION_PLAN_ID_ATTRIBUTE, planId);
        actionElement.SetAttribute("status", success ? "done" : "error");

        StringMessage message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));

        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }
    }
    
    /// <summary>
    ///	Send the availability status of an action to OAC. It is a mechanism to notify
	/// OAC whether or not a specific action can be performed currently.
	/// e.g. When avatar is close enough to a food stuff, it can pick it up, otherwise,
	/// the "pick up" action is not available.
    /// </summary>
    /// <param name="action">
    /// A <see cref="ActionSummary"/>
    /// </param>
    /// <param name="available">
    /// whether or not the action is available.
    /// </param>
	public void sendActionAvailability(List<ActionSummary> actionList, bool available)
	{
		string timestamp = getCurrentTimestamp();
        // Create a xml document
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);

        XmlElement avatarSignal = (XmlElement)root.AppendChild(doc.CreateElement("avatar-signal"));
        avatarSignal.SetAttribute("id", this.myBrainId);
        avatarSignal.SetAttribute("timestamp", timestamp);
        
        // Extract actions from action list
        foreach (ActionSummary action in actionList)
		{
			// Set the action name
	        string ocActionName = ActionManager.getOCActionNameFromMap(action.actionName);
	        
			// check if the method name has a mapping to opencog action name.
			if (ocActionName == null) continue;
			
	        XmlElement actionElement = (XmlElement)avatarSignal.AppendChild(doc.CreateElement(EmbodimentXMLTags.ACTION_AVAILABILITY_ELEMENT));
	        
	        actionElement.SetAttribute("name", ocActionName);
	        
	        // Actions like "walk", "jump" are built-in naturally, they don't need an external target to act on.
	        if (action.objectID != gameObject.GetInstanceID())
			{
		        // Set the action target
		        actionElement.SetAttribute("target", action.objectID.ToString());
		        
		        // Set the action target type
				// currently we only process the avatar and ocobject type, other types in EmbodimentXMLTages can is to be added when needed.
				// if you add other types such as BLOCK_OBJECT_TYPE, you should also modify PAI::processAgentAvailability in opencog
				string targetType = action.actionObject.tag;
				if (targetType == "OCA" || targetType == "Player")// it's an avatar
					actionElement.SetAttribute("target-type", EmbodimentXMLTags.AVATAR_OBJECT_TYPE);
				else if (targetType == "OCObject") // it's an object
					actionElement.SetAttribute("target-type", EmbodimentXMLTags.ORDINARY_OBJECT_TYPE);
				else
					Debug.LogError("Error target type: " + targetType + " in action: " + action.actionName);
			}
							
	        actionElement.SetAttribute("available", available ? "true" : "false");
		}

        StringMessage message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));

        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }
	}

    /**
     *  Add the map-info to message sending queue, yet we use the verb "send" here,
     *  because that's what we intend to do.
     */
    public void sendMapInfoMessage(List<OCObjectMapInfo> newMapInfoSeq, bool isFirstTimePerceptMapObjects= false)
    {
        // No new map info to send.
        if (newMapInfoSeq.Count == 0) return;

        LinkedList<OCObjectMapInfo> localMapInfo = new LinkedList<OCObjectMapInfo>(newMapInfoSeq);

        // If information of avatar itself is in the list, then put it at the first position.
        bool foundAvatarId = false;
        foreach (OCObjectMapInfo objMapInfo in localMapInfo)
        {
            if (objMapInfo.Id.Equals(this.myBrainId))
            {
                localMapInfo.Remove(objMapInfo);
                localMapInfo.AddFirst(objMapInfo);
                foundAvatarId = true;
                break;
            }
        } // foreach
        
        if (!foundAvatarId && this.isFirstSentMapInfo)
        {
            // First <map-info> message should contain information about the OCAvatar itself
            // If it is not there, skip this.
            log.Warn("Skipping first map-info message because it " + 
                     "does not contain info about the avatar itself!");
            return;
        }
            
        StringMessage message = 
            (StringMessage)serializeMapInfo(new List<OCObjectMapInfo>(localMapInfo), "map-info", "map-data",isFirstTimePerceptMapObjects);

        lock (this.messageSendingLock)
        {
            messagesToSend.Add(message);
        } // lock

        // First map info message has been sent.
        this.isFirstSentMapInfo = false;
    }

    /**
     * Add the terrain info message to message sending queue.
     */
    public void sendTerrainInfoMessage(List<OCObjectMapInfo> terrainInfoSeq, bool isFirstTimePerceptTerrain= false)
    {
        // No new map info to send.
        if (terrainInfoSeq.Count == 0) return;

        StringMessage message = 
            (StringMessage)serializeMapInfo(terrainInfoSeq, "terrain-info", "terrain-data",isFirstTimePerceptTerrain);

        lock (this.messageSendingLock)
        {
            messagesToSend.Add(message);
			this.sendMessages();
        } // lock
    }

    /**
     * Function to serialize map info(and terrain info). The map info instance are serialized 
     * by using protobuf-net, which is a fast message compiling tool. Eventually, all
     * map infos will be packed instead of XML. But since the message processing in PAI(server side)
     * distinguish message type by xml tags, we need to construct a simple XML with following format:
     * 
     *      <?xml version="1.0" encoding="UTF-8"?>
     *      <oc:embodiment-msg xsi:schemaLocation="..." xmlns:xsi="..." xmlns:pet="...">
     *          <map(terrain)-info global-position-x="24" global-position-y="24" \
     *              global-position-offset="96" global-floor-height="99" is-first-time-percept-world = "false"(or "true") >
     *              <map(terrain)-data timestamp="...">packed message stream</map(terrain)-data>
     *          </map(terrain)-info>
     *      </oc:embodiment-msg>
     * 
     * @param mapinfoSeq the map-info instance sequence to be serialized.
     * @param messageTag the XML tag of message, currently there are "map-info" and "terrain-info".
     * @param payloadTag the XML tag for wrapping the payload of message, currently there are "map-data"
     * and "terrain-data".
     */
    private Message serializeMapInfo(List<OCObjectMapInfo> mapinfoSeq, string messageTag, string payloadTag, bool isFirstTimePerceptWorld = false)
    {
        string timestamp = getCurrentTimestamp();
        // Create a xml document
        XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);

        // Create a terrain-info element and append to root element.
        XmlElement mapInfo = (XmlElement)root.AppendChild(doc.CreateElement(messageTag));
		mapInfo.SetAttribute("map-name", this.mapName);
        mapInfo.SetAttribute("global-position-x", this.globalPositionX.ToString());
        mapInfo.SetAttribute("global-position-y", this.globalPositionY.ToString());
		mapInfo.SetAttribute("global-position-z", this.globalPositionZ.ToString());
        mapInfo.SetAttribute("global-position-offset-x", this.globalPositionOffsetX.ToString());
		mapInfo.SetAttribute("global-position-offset-y", this.globalPositionOffsetY.ToString());
		mapInfo.SetAttribute("global-position-offset-z", this.globalPositionOffsetZ.ToString());
         mapInfo.SetAttribute("global-floor-height", (this.globalFloorHeight).ToString());
		mapInfo.SetAttribute("is-first-time-percept-world", isFirstTimePerceptWorld.ToString().ToLower());
		mapInfo.SetAttribute("timestamp", timestamp);

        string encodedPlainText;
        using (var stream = new MemoryStream())
        {
            // Serialize the instances into memory stream by protobuf-net
            Serializer.Serialize<List<OCObjectMapInfo>>(stream, mapinfoSeq);
            byte[] binary = stream.ToArray();
            // Encoding the binary in base64 string format in order to transport
            // via NetworkElement.
            encodedPlainText = Convert.ToBase64String(binary);
        }

        XmlElement data = (XmlElement)mapInfo.AppendChild(doc.CreateElement(payloadTag));

        data.InnerText = encodedPlainText;

        return new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));
    }
	
	public void sendFinishPerceptTerrian()
	{
		XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);
		string timestamp = getCurrentTimestamp();
 
        XmlElement signal = (XmlElement) root.AppendChild(doc.CreateElement("finished-first-time-percept-terrian-signal"));
		signal.SetAttribute("timestamp",timestamp );
	
		Message message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));
        
        log.Debugging("sending finished-first-time-percept-terrian-signal: \n" + BeautifyXmlText(doc));
        
        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }
	
	}

    /**
     * Beautify the xml text, which means to add a newline after every node.
     */
    static public string BeautifyXmlText(XmlDocument doc)
    {
        StringBuilder sb = new StringBuilder();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.NewLineChars = "\r\n";
        settings.NewLineHandling = NewLineHandling.Replace;

        XmlWriter writer = XmlWriter.Create(sb, settings);
        doc.Save(writer);
        writer.Close();

        return sb.ToString();
    }
    
    /**
     * Accessor to this avatar's id. (a.k.a AVATAR_xxx)
     */
    public string Id
    {
        get { return this.myId; }
    }
    
    /**
     * Accessor to this avatar's brain id. (a.k.a OAC_xxx)
     */
    public string BrainId
    {
        get { return this.myBrainId; }
    }
    
    /**
     * Accessor to this avatar's name. 
     * Not yet been used.
     */
    public string Name
    {
        get { return this.myName; }
    }
    
    /**
     * Accessor to this avatar master's id.
     */
    public string MasterId
    {
        get { return this.masterId; }
    }
    
    /**
     * Accessor to feelingValueMap
     */
    public Dictionary<string, float> FeelingValueMap
    {
        get { return this.feelingValueMap; }
    }
    
    /**
     * Accessor to demandValueMap 
     */
    public Dictionary<string, float> DemandValueMap
    {
        get { return this.demandValueMap; }
    }

    /**
     * Set the value of given demand
     * 
     * Abstract demands like certainty, competence and affiliation are updated by 
     * OpenCog (PsiDemandUpdaterAgents), which are received by OCConnector::parsePsiDemandElement. 
     * 
     * While other physiological demands like energy, integrity are updated by 
     * OCPhysiologicalModel in unity, which calls this function to update these demand values
     */
    public void SetDemandValue(string demandName, float demandValue)
    {
        this.demandValueMap[demandName] = demandValue; 
    }
    
    /**
     * Accessor to currentDemandName
     */
    public string CurrentDemandName
    {
        get { return this.currentDemandName; } 
    }

    /**
     * To be called when instantiating a new OCAvatar.
     * 
     * @param agentId The id of OCAvatar
     * @param agentName The name of OCAvatar
     * @param agentTraits The traits of OCAvatar
     * @param agentType The type of OCAvatar, "pet" by default
     * @param masterId The human player id who creates the OCAvatar
     * @param masterName The human player name who creates the OCAvatar
     * @param agentPort The local listening port of this OCConnector instance in order to communicate with OpenCog.
     * 
     * @return Result of the initialization action.
     */
    public bool Init(string agentName, string agentTraits, string agentType,
                    string masterId, string masterName)
    {
        // Initialize basic attributes.
        this.myBaseId = agentName;//gameObject.GetInstanceID().ToString();
        this.myId = "AVATAR_" + this.myBaseId;
        this.myBrainId = "OAC_" + this.myBaseId;
        this.myName = agentName;
        this.myType = EmbodimentXMLTags.PET_OBJECT_TYPE;
        this.myTraits = "Princess";
        this.currentDemandName = ""; 
        
        // Load settings from file.
        if (settingsFilename.Length > 0) {
            config.loadFromFile(settingsFilename);
        }
        
        // Initialize NetworkElement
        base.initialize(this.myId);
        
        // Config master's settings.
        this.masterId = masterId;
        this.masterName = masterName;

        // Create action list.
        this.actionsList = new LinkedList<MetaAction>();

        this.isFirstSentMapInfo = true;
        
        // Initialize action ticket.
        lock (this.actionTicketLock)
        {
            this.actionTicket = 0L;
        }
        
        this.ticketToActionMap = new Dictionary<long, MetaAction>();
        this.ticketToPlanIdMap = new Dictionary<long, string>();

        this.feelingValueMap = new Dictionary<string, float>();
        this.demandValueMap = new Dictionary<string, float>(); 
        this.perceptedAgents = new Dictionary<int, string>();

        world = GameObject.Find("World").GetComponent<WorldGameObject>() as WorldGameObject;

        if (world != null)
        {
			mapName = world.loadFromFile;
			
			// If there are chuncks auto generated around the bounday, we should minus this boundary
			if (OCPerceptionCollector.hasBoundaryChuncks)	
			{
				// Calculate the offset of the terrain.
	            this.globalPositionOffsetX = (uint) WorldGameObject.chunkBlocksWidth * (uint)WorldGameObject.chunksWide - 2;
				this.globalPositionOffsetY = (uint)WorldGameObject.chunkBlocksHeight * (uint)WorldGameObject.chunksHigh - 2;
	 
	            // There is an invisible chunk at the edge of the terrain, so we should take count of it.
	            this.globalPositionX = (int)WorldGameObject.chunkBlocksWidth;
	            this.globalPositionY = (int)WorldGameObject.chunkBlocksHeight;
			}
			else
			{
	            // Calculate the offset of the terrain.
	            this.globalPositionOffsetX = (uint) WorldGameObject.chunkBlocksWidth * (uint)WorldGameObject.chunksWide;
				this.globalPositionOffsetY = (uint) WorldGameObject.chunkBlocksHeight * (uint)WorldGameObject.chunksHigh;
				
	            // There is an invisible chunk at the edge of the terrain, so we should take count of it.
	            this.globalPositionX = 0;
	            this.globalPositionY = 0;
			}
			this.globalPositionOffsetZ = (uint) WorldGameObject.chunkBlocksDepth * (uint)WorldGameObject.chunksDeep ;
			this.globalPositionZ = 0;
            // The floor height should be 1 unit larger than the block's z index.
            this.globalFloorHeight = world.WorldData.floor;
        }
        else
        {
			mapName = "unknown_map";
            this.globalPositionOffsetX = 128;
			this.globalPositionOffsetY = 128;
			this.globalPositionOffsetZ = 128;

            this.globalPositionX = 0;
            this.globalPositionY = 0;
			this.globalPositionZ = 0;
            this.globalFloorHeight = 0;
        }
      
        // Get action scheduler component.
        this.actionScheduler = gameObject.GetComponent<OCActionScheduler>() as OCActionScheduler;
        ActionManager.globalActionCompleteEvent += handleOtherAgentActionResult;

        return true;
    }
    
	public IEnumerator connectOAC()
	{
        // First step, connect to the router.
        int timeout = 100;
        while (!base.established && timeout > 0)
        {
            StartCoroutine(base.connect());
            yield return new WaitForSeconds(1.0f);
            timeout--;
        }

        if (timeout == 0)
        {
            log.Error("Breaking");
            yield break;
        }

        // Second step, check if spawner is available to spawn an OAC instance.
        bool isSpawnerAlive = isElementAvailable(config.get("SPAWNER_ID"));
        timeout = 60;
        while (!isSpawnerAlive && timeout > 0)
        {
            log.Info("Waiting for spawner...");
            yield return new WaitForSeconds(1f);
            isSpawnerAlive = isElementAvailable(config.get("SPAWNER_ID"));
            timeout--;
        }

        if (!isSpawnerAlive)
        {
            log.Error("Spawner is not available, OAC can not be launched.");
            yield break;
        }

        // Finally, load the OAC by sending "load agent" command to spawner.
        loadOAC();
        timeout = 100;
        // Wait some time for OAC to be ready.
        while (!isOacAlive && timeout > 0)
        {
            yield return new WaitForSeconds(1f);
            timeout--;
        }
	}
    
    /// <summary>
    /// Load an OAC instance as the brain of this OCAvatar.
    /// </summary>
    private void loadOAC()
    {
        StringBuilder msgCmd = new StringBuilder("LOAD_AGENT ");
        msgCmd.Append(this.myBrainId + WHITESPACE + this.masterId + WHITESPACE);
        msgCmd.Append(this.myType + WHITESPACE + this.myTraits + "\n");

        Message msg = Message.factory(this.myId, 
                                      config.get("SPAWNER_ID"), 
                                      Message.MessageType.STRING, 
                                      msgCmd.ToString());
        sendMessage(msg);
    }
    
    /// <summary>
    /// Unload the OAC instance controlling this avatar when finalizing.
    /// </summary>
    private void unloadOAC()
    {
        StringBuilder msgCmd = new StringBuilder("UNLOAD_AGENT ");
        msgCmd.Append(this.myBrainId + "\n");

        Message msg = Message.factory(this.myId, 
                                      config.get("SPAWNER_ID"), 
                                      Message.MessageType.STRING, 
                                      msgCmd.ToString());
        if (!sendMessage(msg))
        {
            log.Warn("Could not send unload message to spawner.");
        }

        // Wait some time for the message to be sent.
        try {
            Thread.Sleep(500);
        } catch (ThreadInterruptedException e) {
            log.Error("Error putting NetworkElement main thread to sleep. " +
                           e.Message);
        }
    }
    
    /**
     * The interface of dialog system in the game.
     * A player chats with avatar by typing text in the console,
     * then the text would be sent to OAC by this method.
     * 
     * @param text the chat text to be sent.
     * @param source GameObject that sent the communication.
     */
    public void sendSpeechContent(string text, GameObject source)
    {
        text = text.TrimEnd('\n');
        // Don't send a message unless we are initialized to connect to
        // a server
        if (!this.isOacAlive) {
            log.Warn("Avatar[" + this.myId + "]: Received '" + text +
                    "' from player but I am not connected to an OAC.");
            return;
        }
        log.Debugging("Avatar[" + this.myId + "]: Received '" + text + "' from player.");

        // Avoid creating messages if the destination (avatar brain) isn't available 
        if (!isElementAvailable(this.myBrainId))
            return;
        
        StringBuilder speechMsg = new StringBuilder();
        speechMsg.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");

        speechMsg.Append("<oc:embodiment-msg xmlns:oc=\"http://www.opencog.org/brain\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.opencog.org/brain BrainProxyAxon.xsd\">\n");
        speechMsg.Append("<communication source-id=\"");
        speechMsg.Append(source.GetInstanceID());
        speechMsg.Append("\" timestamp=\"");
        speechMsg.Append(getCurrentTimestamp());
        speechMsg.Append("\">");
        speechMsg.Append(text);
        speechMsg.Append("</communication>");
        speechMsg.Append("</oc:embodiment-msg>");
        
        Message message = Message.factory(this.myId, this.myBrainId, Message.MessageType.RAW, speechMsg.ToString());
        
        // Add the message to the sending queue.
        lock (this.messageSendingLock) { 
            this.messagesToSend.Add(message);
        }
    }
    
    /**
     * Save data and exit from embodiment system normally 
     * when finalizing the OCAvatar. 
     */
    public void saveAndExit()
    {
        if (isOacAlive) {
            // TODO save local data of this avatar.
            this.unloadOAC();
        }
        finalize();
    }

    #region Unity API
    /// <summary>
    /// A unity API function inherited from MonoBehavior.
    /// Initialization work would be done in Init() for we need some parameters
    /// for this instance.
    /// </summary>
    void Awake()
    {
    }
    
    void Update()
    {
        // Invoke base network element function to do networking stuffs in 
        // every frame.
        pulse();

        //isOacAlive = isElementAvailable(myBrainId);
    }
    
    /**
     * This method is called by unity system in a fixed frequency,
     * so we can make a timer to do things we want.
     */
    void FixedUpdate()
    {
        this.messageSendingTimer += Time.fixedDeltaTime;
        
        if (this.messageSendingTimer >= this.messageSendingInterval) {
            this.sendMessages();
            this.messageSendingTimer = 0.0f;
        }
    }
    
    /// <summary>
    /// A unity API function inherited from MonoBehavior.
    /// The deconstruction method executed when application quit.
    /// </summary>
    void OnApplicationQuit()
    {
        this.saveAndExit();
    }
    #endregion
	
	public void sendBlockStructure(IntVect startBlock, bool isToRecognize)
	{
	    XmlDocument doc = new XmlDocument();
        XmlElement root = makeXMLElementRoot(doc);
		string timestamp = getCurrentTimestamp();
 
        XmlElement signal = (XmlElement) root.AppendChild(doc.CreateElement("block-structure-signal"));
		if (isToRecognize)
		{
	        signal.SetAttribute("recognize-structure","true" );
	        
	        signal.SetAttribute("startblock-x", startBlock.X.ToString());
			signal.SetAttribute("startblock-y", startBlock.Y.ToString());
			signal.SetAttribute("startblock-z", startBlock.Z.ToString());
		}
		signal.SetAttribute("timestamp",timestamp );
	
		Message message = new StringMessage(this.myId, this.myBrainId, BeautifyXmlText(doc));
        
        log.Debugging("sending block structure signal: \n" + BeautifyXmlText(doc));
        
        lock (messageSendingLock)
        {
            this.messagesToSend.Add(message);
        }
	}
}
