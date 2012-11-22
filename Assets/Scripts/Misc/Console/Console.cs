using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;

using Embodiment;

/// <summary>
/// To be subclassed by commands added to the Console.
/// </summary>
public abstract class ConsoleCommand : MonoBehaviour
{
    public void Start() {
        Console.get().addCommand(this);
    }
    /// <summary>
    /// Run the command, whatever it may be and return the result as a string.
    /// </summary>
    abstract public string run(ArrayList arguments);
    /// <summary>
    /// return the command signature.
    /// An ordered list of KeyValuePair<Type,int>... the int is the number of
    /// that type (0 == unlimited).
    /// </summary>
    abstract public ArrayList getSignature();

    abstract public string getName();
}

/// <summary>
/// This class defines a console class that allows a player to issue commands
/// and communicate (by text) with OpenCog avatars.
///
/// This controller will create a command window in the screen, which can be
/// enabled or disabled with the tilde/backquote key.
///
/// Commands should be placed within the tree on the hierarchy to be
/// automatically added, or they should be added using AddCommand()
/// </summary>
public class Console : MonoBehaviour
{
    static private Console cs;
    static public Console get() {
        return cs;
    }
	
	private HUD theHUD;

    public void Awake () {
		
		theHUD = GameObject.Find("HUD").GetComponent<HUD>();
		
		Input.eatKeyPressOnTextFieldFocus  = false;
        cs = this;
    }

    /// the skin the console will use
	public GUISkin mySkin;
    /// style for my commands
    public GUIStyle commandStyle;
    /// Whether the console is currently visible
	public bool showChat = false;
    /// Whether we are currently showing completion options
    private bool showCompletionOptions = false;
    private ArrayList completionPossibilities;

    /// Keep a record of all commands as <command name string, CommandObject>
    /// pairs
    private Hashtable commandTable = new Hashtable();

    /// default command if no leading /
    public string defaultCommand;
	
	public class ConsoleEntry
	{
        public enum Type { COMMAND, RESULT, SAY, ERROR };
        public Type type;
        public DateTime t; // the time that the command was executed
        public string commandName; // The command
		public string receiver; // should be replaced by Avatar class
		public string sender;   // should be replaced by Avatar class
		public string msg = "";
		public bool mine = true; // Judge if the message is mine.
	}
	
    /// Whether the console is expanding/appearing
	enum movement { APPEAR, DISAPPEAR, NONE };
    private movement movementState = movement.NONE;

    /// The current contents of the input field
	private string inputField = "";
    
    /// A history of input for up arrow support
	private LinkedList<string> inputHistory;
    /// Current inputHistory position
    private LinkedListNode<string> inputHistoryCurrent = null;

    /// Scroll back for the history
	private Vector2 scrollPosition;
    /// All the console entries ...
	private ArrayList entries;
	
    /// The dimensions on the console window
	//private Rect window = new Rect( (Screen.width/2 - 150), Screen.height - 210, 300, 200);
    private float height;
    /// Y changes as the console slides in
    private float currentYPos;
	private Rect window;
	
    private string textInput;
    private bool callInputBox;
    private string messages;
	
	// The player object - for stealing and resuming mouse control.
	//public Player player;

	// Troy: action callback using delegate, would be invoked in ActionManager.
    public void notifyActionComplete(ActionResult ar) {
		string actionName = "None";
		if (ar.action != null)
			actionName = ar.action.actionName;
		string avatarName = ar.avatar.gameObject.name;
		if (ar.description.Length > 0) {
			AddConsoleEntry("[" + avatarName + "::" + actionName + "] " + ar.description, null, ConsoleEntry.Type.RESULT);
		} else {
			AddConsoleEntry("[" + avatarName + "::" + actionName + "] completed.", null, ConsoleEntry.Type.RESULT);
		}
		
    }
	
	private void CloseChatWindow()
	{
        // we leave the showChat as true as it will get switched off once
        // the disappearing movement is complete
		//this.showChat = true;
        // Set movement state to start disappearing
        this.movementState = movement.DISAPPEAR;
        // Re-enable the character controller for player movement
		
		//@TODO:set new character controller
        theHUD.setCharacterControl(true);
	}
	
	private void FocusControl ()
	{
		GUI.FocusControl("CommandArea");
	}
	
	private void GlobalChatWindow (int id) 
	{
		// Begin a scroll view. All rects are calculated automatically - 
	    // it will use up any available screen space and make sure contents flow correctly.
	    // This is kept small with the last two parameters to force scrollbars to appear.
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
	
        lock(entries)
        {
            foreach (ConsoleEntry entry in entries)
            {
                GUILayout.BeginHorizontal();
                // Here, we format things slightly differently for each
                // ConsoleEntry type.
                commandStyle.wordWrap = true;
                if ( entry.type == ConsoleEntry.Type.ERROR )
                {
                    commandStyle.normal.textColor = Color.red;
                    GUILayout.Label (entry.msg,commandStyle);
                    GUILayout.FlexibleSpace ();
                }
                else if ( entry.type == ConsoleEntry.Type.SAY )
                {
                    commandStyle.normal.textColor = Color.black;
                    GUILayout.Label ("> ");
                    commandStyle.normal.textColor = Color.green;
                    GUILayout.Label (entry.msg,commandStyle);
                    GUILayout.FlexibleSpace ();
                }
                else if ( entry.type == ConsoleEntry.Type.COMMAND )
                {
                    commandStyle.normal.textColor = Color.black;
                    GUILayout.Label ("> ");
                    commandStyle.normal.textColor = Color.blue;
                    GUILayout.Label (entry.msg,commandStyle);
                    GUILayout.FlexibleSpace ();
                }
                else if ( entry.type == ConsoleEntry.Type.RESULT )
                {
                    commandStyle.normal.textColor = Color.black;
                    GUILayout.Label (entry.msg,commandStyle);
                    GUILayout.FlexibleSpace ();
                }
                
                GUILayout.EndHorizontal();
                
            }
        }
		// End the scrollview we began above.
	    GUILayout.EndScrollView ();
		
        // Remove backquote character from text input as this
        // enables/disables the the console
        char chr = Event.current.character;
        if ( chr == '`' ) { Event.current.character = '\0'; }
		GUI.SetNextControlName("CommandArea");
		this.inputField = GUILayout.TextField(this.inputField);
		
		GUI.DragWindow();
	}
	
	private void AddConsoleEntry (string str, string sender, ConsoleEntry.Type type)
	{
        if (str == null) return; // No message, ignore it

        // Create console entry
		ConsoleEntry entry = new ConsoleEntry();
        entry.t = DateTime.Now;
		entry.sender = sender;
		entry.msg = str;
		if (sender == null) {
            entry.mine = true;
        } else entry.mine = false;
        entry.type = type;

        // Add to list
        lock(entries) {
            entries.Add(entry);
        }
		
        // Prune oldest entries
		if (entries.Count > 50) entries.RemoveAt(0);

        // Ensure we are at the bottom...
        // TODO Do this in a non-brittle way... i.e. find actual maximum value
		scrollPosition.y = 1000000;	
	}

    public void AddSpeechConsoleEntry(string content, string sender, string listener)
    {
        string str = "[" + sender + " -> " + listener + "]: " + content;
        AddConsoleEntry(str, sender, ConsoleEntry.Type.RESULT);
    }

    private bool TabComplete(string context)
    {
        if (! context.StartsWith("/")) return false;
        ArrayList tokens = splitCommandLine(context);
        
        ArrayList possibilities = new ArrayList();
        // Find possible completions

        if (possibilities.Count > 1) {
            // Only show completions if there are more than one...
            showCompletionOptions = true;
            completionPossibilities = possibilities;
        } else if (possibilities.Count == 1) {
            // just complete the token
            //inputField = contextExceptLastToken + " " + possibilities[0];
            showCompletionOptions = false;
            completionPossibilities = null;
        } else {
            // no possible completions
            
        }
        return true;
    }
	
	private void ProcessConsoleLine(string text)
	{
        if (text == "") return;
        if (text.StartsWith("/")) {
			AddConsoleEntry(text, null, ConsoleEntry.Type.COMMAND);
            string cmdline = text.Remove(0,1); // remove leading slash
            ArrayList args = splitCommandLine(cmdline);
            if (args != null) {
                string cmd = args[0] as string;
                // check if we recognise this command
                if (!commandTable.Contains(cmd)) {
                    // we don't know about the command
                    AddConsoleEntry("error: unknown command " + (string) cmd, null, ConsoleEntry.Type.ERROR);
                } else {
                    ConsoleCommand cc = commandTable[cmd] as ConsoleCommand;
                    args.RemoveAt(0); // remove actual command name
                    string result = cc.run(args);
                    AddConsoleEntry(result, null, ConsoleEntry.Type.RESULT);
                }
            }
        } else if (defaultCommand != null) {
            // assume the input should be sent to whatever is the default
            // command (usually just chat)
            ConsoleCommand cc = commandTable[defaultCommand] as ConsoleCommand;
			if(cc == null)
			{
				Debug.LogError("CommandTable: " + commandTable.ToString() + " Default Command:" + defaultCommand);
			}
			
            ArrayList args = splitCommandLine(text);
            if (args != null && cc != null) {
                AddConsoleEntry(text, null, ConsoleEntry.Type.SAY);
                string result = cc.run(args);
                AddConsoleEntry(result, null, ConsoleEntry.Type.RESULT);
            }
        }
        inputHistory.AddFirst(text);
        inputHistoryCurrent = null;
	}

    public ArrayList splitCommandLine(string command) {
		string text = command.TrimEnd('\n');
        string[] words = text.Split(' ');

        ArrayList joined = new ArrayList();
        // join elements surrounded by quotes
        bool speechOpen = false;
        int speechStartIndex = 0;
        for (int i=0; i < words.Length; i++) {
			// ignore double spaces
			if (words[i].Length == 0) continue;
            if (speechOpen) {
                if (words[i][words[i].Length - 1] == '\"') {
                    // Allow escaped speech marks at the start of words
                    if (words[i][words[i].Length - 2] == '\\') continue;
                    // Otherwise this is a terminating speech mark
                    speechOpen = false;
                    string temp = "";
                    for (int j=speechStartIndex; j <= i; j++) {
                        temp += words[j];
                        // Add space between words within string
                        temp += " ";
                    }
                    // Remove trailing space
                    temp = temp.Substring(0,temp.Length-1);
                    joined.Add(temp.Trim('\"'));
                }
            }
            else if (words[i][0] == '\"') {
                if (words[i][words[i].Length-1] == '\"') {
                    joined.Add(words[i].Trim('\"'));
                    continue;
                }
                // Found an opening speech mark
                speechStartIndex = i;
                speechOpen = true;
            } else {
                // just add tokens otherwise
                joined.Add(words[i].Trim('\"'));
            }
        }
        if (speechOpen) {
            // unterminated string...
            AddConsoleEntry("error: no matching \" character.", null, ConsoleEntry.Type.ERROR);
            return null;
        }
        return joined;
    }

    public void addCommand(ConsoleCommand cc) {
        commandTable[cc.getName()] = cc;
    }

    public void removeCommand(string cmdName) {
        commandTable.Remove(cmdName);
    }
	
    // Use this for initialization
    void Start()
    {
		Input.eatKeyPressOnTextFieldFocus = false;
		
		height = Screen.height * 0.30f;
		//player = (GameObject.FindWithTag("Player") as GameObject).GetComponent<Player>();
        // Initialise position
        currentYPos = -height;
        window = new Rect( 0, currentYPos, Screen.width, height);
        // If user has made console visible using public property then ensure
        // it starts appearing
        if (showChat) this.movementState = movement.APPEAR;
		
		ActionManager.globalActionCompleteEvent += new ActionCompleteHandler(notifyActionComplete);

        // Initialise support
		entries = new ArrayList();
        inputHistory = new LinkedList<string>();
        if (defaultCommand == null || defaultCommand == "") {
            if (commandTable.Contains("say")) {
                defaultCommand = "say";
            }
        }
        completionPossibilities = new ArrayList();

        // add history of commands here... good way of storing test cases
        inputHistory.AddFirst("/do Avatar self MoveToObject \"Soccer Ball\"");
		inputHistory.AddFirst("/do Avatar \"Soccer Ball\" Kick 3000");
		inputHistory.AddFirst("/do Avatar \"Soccer Ball\" PickUp");
		inputHistory.AddFirst("/do Avatar \"Soccer Ball\" Drop");
        inputHistory.AddFirst("/list Avatar");
		inputHistory.AddFirst("/load npc");
    }
	
	public bool isActive()
	{
		if (this.movementState != movement.DISAPPEAR && showChat)
			return true;
		return false;
	}

    void Update()
    {

		
		height = Screen.height * 0.30f;

		if (Input.GetKeyDown(KeyCode.BackQuote)) {
            // If the window is already visible and isn't disappearing...
            if (showChat && this.movementState != movement.DISAPPEAR) {
                CloseChatWindow();
            } else {
                showChat = true;
                theHUD.setCharacterControl(false);
                
                this.movementState = movement.APPEAR;
            }
            
        }
		
		// Below this line is only relevent when console is active
		// ----------
		if (!this.isActive()) return;
//		if(Event.curren && Event.current.type == EventType.KeyDown) 
//			Debug.Log("Event.current =" + Event.current.ToString());
		if(Input.GetKeyDown(KeyCode.Return))
		{
			Debug.Log("Input Field Length = " + inputField.Length.ToString() + ", Name of Focused Control = " + GUI.GetNameOfFocusedControl());
		}
		
        if (Input.GetKeyDown(KeyCode.Return) && 
                inputField.Length > 0)// &&
                //GUI.GetNameOfFocusedControl() == "CommandArea")
		{
			this.ProcessConsoleLine(inputField);
			inputField = ""; // blank input field
            inputHistoryCurrent = null; // reset current position in input history
		}
        // Implement input history using up/down arrow
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (inputHistoryCurrent == null) {
                // TODO save current output so that we can push down to restore
                // previously written text
                inputHistoryCurrent = inputHistory.First;
            } else if (inputHistoryCurrent.Next != null) {
                inputHistoryCurrent = inputHistoryCurrent.Next;
            }
            if (inputHistoryCurrent != null)
                inputField = inputHistoryCurrent.Value;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (inputHistoryCurrent != null && inputHistoryCurrent.Previous != null) {
                inputHistoryCurrent = inputHistoryCurrent.Previous;
            }
            if (inputHistoryCurrent != null)
                inputField = inputHistoryCurrent.Value;
        }
        if (showChat) theHUD.setCharacterControl(false);
    }


    void OnGUI()
    {
		Input.eatKeyPressOnTextFieldFocus = false;
		
		if(mySkin) {
            commandStyle = mySkin.label;
            commandStyle.normal.textColor = Color.blue;
            GUI.skin = mySkin;
        }
		
		if (showChat) 
		{
            int movementSpeed = 10;
            if (this.movementState == movement.APPEAR) {
                currentYPos = currentYPos+movementSpeed;
                if (currentYPos >= 0) {
                    currentYPos = 0;
                    this.movementState = movement.NONE;
                }
            } else if (this.movementState == movement.DISAPPEAR) {
                currentYPos = currentYPos-movementSpeed;
                if (currentYPos <= -height) {
                    currentYPos = -height;
                    this.movementState = movement.NONE;
                    this.showChat = false;
                }
            }
            window = new Rect( 0, currentYPos, Screen.width, height);
			window = GUI.Window(1, window, GlobalChatWindow, "Command");
            GUI.FocusWindow(0);
            if (this.movementState != movement.DISAPPEAR) {
				
                GUI.FocusWindow(1);
                FocusControl();
            }
		}
		
		
	}

    void OnApplicationQuit()
    {

    }
}




/*
public class DebugConsole
{
    private string ConsoleText = "";
    private bool displayConsole = false;
    public bool DisplayConsole
    {
        get { return displayConsole; }
        set
        {
            displayConsole = value;
            if (!DisplayConsole)
            {
                ConsoleText = "";
                PreviousCommandIndex = -1;
            }
        }
    }
 
    private List<string> PreviousCommands = new List<string>();
    private int PreviousCommandIndex = -1;
 
    private string AutoCompleteBase = "";
    private List<string> AutoCompleteOptions = new List<string>();
    private int AutoCompleteOptionsIndex = -1;
 
    private ConsoleCommands.ConsoleCommand GetCommand(string CommandText)
    {
        foreach (ConsoleCommands.ConsoleCommand Command in ConsoleCommands.Commands)
        {
            if (Command.CommandText.Equals(CommandText, StringComparison.CurrentCultureIgnoreCase))
            {
                return Command;
            }
        }
        return null;
    }
 
    private void ExecuteCommand(string CommandText)
    {
        CommandText = CommandText.Trim();
        PreviousCommands.Add(CommandText);
 
        string[] SplitCommandText = CommandText.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
 
        ConsoleCommands.ConsoleCommand Command = GetCommand(SplitCommandText[0]);
        if (Command != null)
        {
            Command.Callback(SplitCommandText);
        }
    }
 
    private void AutoComplete()
    {
        string AutoCompleteText = AutoCompleteBase.Trim().ToLower();
 
        if (AutoCompleteOptionsIndex < 0)
        {
            AutoCompleteOptions.Clear();
            foreach (ConsoleCommands.ConsoleCommand Command in ConsoleCommands.Commands)
            {
                if (Command.CommandText.ToLower().StartsWith(AutoCompleteText))
                {
                    AutoCompleteOptions.Add(Command.CommandText);
                }
            }
            AutoCompleteOptions.Sort();
 
            if (AutoCompleteOptions.Count > 0)
            {
                AutoCompleteOptionsIndex = 0;
                PreviousCommandIndex = -1;
            }
        }
        else
        {
            if (AutoCompleteOptions.Count > 0)
            {
                AutoCompleteOptionsIndex = (AutoCompleteOptionsIndex + 1) % AutoCompleteOptions.Count;
            }
            else
            {
                AutoCompleteOptionsIndex = -1;
            }
        }
 
        if (AutoCompleteOptionsIndex >= 0)
        {
            ConsoleText = AutoCompleteOptions[AutoCompleteOptionsIndex];
        }
    }
 
    private void ClearAutoComplete()
    {
        AutoCompleteBase = "";
        AutoCompleteOptions.Clear();
        AutoCompleteOptionsIndex = -1;
    }
 
    public void OnGUI()
    {
        if (DisplayConsole)
        {
            string BaseText = ConsoleText;
            if (PreviousCommandIndex >= 0)
            {
                BaseText = PreviousCommands[PreviousCommandIndex];
            }
 
            Event CurrentEvent = Event.current;
            if ((CurrentEvent.isKey) &&
                (!CurrentEvent.control) &&
                (!CurrentEvent.shift) &&
                (!CurrentEvent.alt))
            {
                bool isKeyDown = (CurrentEvent.type == EventType.KeyDown);
                if (isKeyDown)
                {
                    if (CurrentEvent.keyCode == KeyCode.Return || CurrentEvent.keyCode == KeyCode.KeypadEnter)
                    {
                        ExecuteCommand(BaseText);
                        DisplayConsole = false;
                        return;
                    }
 
                    if (CurrentEvent.keyCode == KeyCode.UpArrow)
                    {
                        if (PreviousCommandIndex <= -1)
                        {
                            PreviousCommandIndex = PreviousCommands.Count - 1;
                            ClearAutoComplete();
                        }
                        else if(PreviousCommandIndex > 0)
                        {
                            PreviousCommandIndex--;
                            ClearAutoComplete();
                        }
                        return;
                    }
 
                    if (CurrentEvent.keyCode == KeyCode.DownArrow)
                    {
                        if (PreviousCommandIndex == PreviousCommands.Count - 1)
                        {
                            PreviousCommandIndex = -1;
                            ClearAutoComplete();
                        }
                        else if (PreviousCommandIndex >= 0)
                        {
                            PreviousCommandIndex++;
                            ClearAutoComplete();
                        }
                        return;
                    }
 
                    if (CurrentEvent.keyCode == KeyCode.Tab)
                    {
                        if (AutoCompleteBase.Length == 0)
                        {
                            AutoCompleteBase = BaseText;
                        }
                        AutoComplete();
                        return;
                    }
                }
            }
 
            GUI.SetNextControlName("ConsoleTextBox");
            Rect TextFieldRect = new Rect(0.0f, (float)Screen.height - 20.0f, (float)Screen.width, 20.0f);
 
            string CommandText = GUI.TextField(TextFieldRect, BaseText);
            if (PreviousCommandIndex == -1)
            {
                ConsoleText = CommandText;
            }
            if (CommandText != BaseText)
            {
                ConsoleText = CommandText;
                PreviousCommandIndex = -1;
                ClearAutoComplete();
            }
            GUI.FocusControl("ConsoleTextBox");
        }
    }
}
 
public static class ConsoleCommands
{
    public delegate void Command(string[] Params);
 
    public class ConsoleCommand
    {
        public ConsoleCommand(string CommandText, string HelpText, Command Callback)
        {
            this.CommandText = CommandText;
            this.HelpText = HelpText;
            this.Callback = Callback;
        }
 
        public string CommandText;
        public string HelpText;   // Currently not used
        public Command Callback;
    }
 
    public static ConsoleCommand[] Commands = new ConsoleCommand[] {
    // Fill this with your commands:
        new ConsoleCommand("MyCommand", "This is the help text for MyCommand", MyCommandDelegate),
    };
 
    public static void MyCommandDelegate(string[] Params)
  {
    // Do stuff
  }
}
 
public class DebugManager : MonoBehaviour
{
    private DebugConsole DebugConsole = null;
 
    public bool ConsoleEnabled
    {
        get { return (DebugConsole != null) ? DebugConsole.DisplayConsole : false; }
    }
 
  void Awake()
  {
        DebugConsole = new DebugConsole();
  }
 
  void Update()
  {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (DebugConsole.DisplayConsole)
            {
                DebugConsole.DisplayConsole = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            DebugConsole.DisplayConsole = true;
        }
  }
 
  void OnGUI()
  {
    DebugConsole.OnGUI();
  }
}
*/